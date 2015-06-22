using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using Microsoft.VisualStudio.PlatformUI;

namespace AvoBright.FontStyler
{
    public partial class FontWindow : DialogWindow
    {
        private string customFontFamilyName = "";

        private bool useTextShadowPolyFill = false;

        public FontWindow()
        {
            InitializeComponent();

            backgroundColorPicker.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(backgroundColorPicker_SelectedColorChanged);
            genericFontRadioButton.Checked += new RoutedEventHandler(genericFontRadioButton_Checked);
            genericFontRadioButton.Unchecked += new RoutedEventHandler(genericFontRadioButton_Unchecked);
            fontFamilyComboBox.SelectionChanged += new SelectionChangedEventHandler(fontFamilyComboBox_SelectionChanged);
            fontSizeNumericBox.ValueChanged += new EventHandler(fontSizeNumericBox_ValueChanged);
            fontVariantComboBox.SelectionChanged += new SelectionChangedEventHandler(fontVariantComboBox_SelectionChanged);
            fontWeightComboBox.SelectionChanged += new SelectionChangedEventHandler(fontWeightComboBox_SelectionChanged);
            fontStyleComboBox.SelectionChanged += new SelectionChangedEventHandler(fontStyleComboBox_SelectionChanged);
            fontDecorationComboBox.SelectionChanged += new SelectionChangedEventHandler(fontDecorationComboBox_SelectionChanged);
            fontTransformComboBox.SelectionChanged += new SelectionChangedEventHandler(fontTransformComboBox_SelectionChanged);
            previewTextBox.TextChanged += new TextChangedEventHandler(previewTextBox_TextChanged);

            // even IE version is 9, use polyfill for text shadow
            var winFormBrowser = new System.Windows.Forms.WebBrowser();
            int majorVersion = winFormBrowser.Version.Major;
            if (majorVersion < 10)
            {
                useTextShadowPolyFill = true;
            }

            SetCopyCss();

            SetPreviewSource();
        }

        enum FontFormat
        {
            Unknown,
            TrueType,
            Woff
        }

        private string ColorToString(Color color)
        {
            return string.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, color.A / 255.0);
        }

        private string GenerateBgCss()
        {
            return string.Format("background-color: {0};", ColorToString(backgroundColorPicker.SelectedColor));
        }

        private string GenerateShadowCss(bool cssValueOnly = false)
        {
            int hShadow = hShadowNumericBox.Value;
            int vShadow = vShadowNumericBox.Value;
            int blurRadius = blurRadiusNumericBox.Value;
            Color color = shadowColorPicker.SelectedColor;

            if (cssValueOnly)
            {
                return string.Format("{0}px {1}px {2}px {3}", hShadow, vShadow, blurRadius, ColorToString(color));
            }
            else
            {
                return string.Format("text-shadow: {0}px {1}px {2}px {3};", hShadow, vShadow, blurRadius, ColorToString(color));
            }
            
        }
        
        private string GenerateFontFaceCss(bool isForPreview = true)
        {
            if (genericFontRadioButton.IsChecked.HasValue && genericFontRadioButton.IsChecked.Value)
            {
                return "";
            }

            string fontFilePath = fontFilePathTextBox.Text;

            if (string.IsNullOrEmpty(fontFilePath) || !File.Exists(fontFilePath))
            {
                return "";
            }

            // font name is already quoted
            string fontName = customFontFamilyName;
            Uri fontUri = new Uri(fontFilePathTextBox.Text);
            
            FontFormat fontFormat = FontFormat.Unknown;
            string fontFileExtension = System.IO.Path.GetExtension(fontFilePathTextBox.Text);

            if (!string.IsNullOrEmpty(fontFileExtension))
            {
                fontFormat = GetFontFormat(fontFileExtension);
            }

            string fontFormatStr = fontFormat == FontFormat.Unknown ? "" : "format(\"" + fontFormat.ToString().ToLower() + "\");";

            string path = "";
            string srcType = "url";
            if (isForPreview)
            {
                path = fontUri.AbsoluteUri;
            }
            else
            {
                path = System.IO.Path.GetFileName(fontFilePathTextBox.Text);
            }

            return string.Format("@font-face {{ font-family: {0}; src: {1}(\"{2}\") {3} }}", fontName, srcType, path, fontFormatStr);
        }

        private FontFormat GetFontFormat(string fontFileExtension)
        {
            if (fontFileExtension.StartsWith("."))
            {
                fontFileExtension = fontFileExtension.Substring(1);
            }

            fontFileExtension = fontFileExtension.ToLower();

            switch (fontFileExtension)
            {
                case "ttf":
                    return FontFormat.TrueType;
                case "woff":
                    return FontFormat.Woff;
                default:
                    return FontFormat.Unknown;
            }
        }

        private string GenerateTextColorCss()
        {
            return string.Format("color: {0};", ColorToString(fontColorPicker.SelectedColor));
        }

        private string GenerateFontCss()
        {
            // font: font-style font-variant font-weight font-size/line-height font-family
            string fontStyle = ((TextBlock)fontStyleComboBox.SelectedItem).Text.ToLower();
            string fontVariant = ((TextBlock)fontVariantComboBox.SelectedItem).Text == "Normal" ? "normal" : "small-caps";
            string fontWeight = ((TextBlock)fontWeightComboBox.SelectedItem).Text.ToLower();
            string fontSize = fontSizeNumericBox.Value.ToString() + "px";

            string fontFamily = "";
            if (genericFontRadioButton.IsChecked.HasValue && genericFontRadioButton.IsChecked.Value)
            {
                switch (fontFamilyComboBox.SelectedIndex)
                {
                    case 0:
                        fontFamily = "serif";
                        break;
                    case 1:
                        fontFamily = "sans-serif";
                        break;
                    case 2:
                        fontFamily = "monospace";
                        break;
                    default:
                        fontFamily = "sans-serif";
                        break;
                }
            }
            else
            {
                fontFamily = customFontFamilyName;
            }

            string result = string.Format("font: {0} {1} {2} {3} {4};", fontStyle, fontVariant, fontWeight, fontSize, fontFamily);
            return result;
        }

        private string GenerateTextDecorationCss()
        {
            string decoration = ((TextBlock)fontDecorationComboBox.SelectedItem).Text.ToLower().Replace(' ', '-');
            return string.Format("text-decoration: {0};", decoration);
        }
        
        private string GenerateTextTransformCss()
        {
            string transform = ((TextBlock)fontTransformComboBox.SelectedItem).Text.ToLower();
            return string.Format("text-transform: {0};", transform);
        }

        private void SetPreviewSource()
        {
            string previewDirPath = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            string previewFilePath = System.IO.Path.Combine(previewDirPath, "preview.html");

            var sourceBuilder = new StringBuilder();
            using (var reader = new StreamReader(new FileStream(previewFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                sourceBuilder.Append(reader.ReadToEnd());
            }

            string allCss = 
                "\t\t" + "body {\n" +
                "\t\t\t" + "padding: 0; margin: 0; height: 50px;\n" +
                "\t\t" + "}\n" +
                "\t\t" + GenerateFontFaceCss() + "\n" +
                "\t\t" + "div {\n" +
                "\t\t\t" + GenerateBgCss() + "\n" +
                "\t\t\t" + GenerateShadowCss() + "\n" +
                "\t\t\t" + GenerateFontCss() + "\n" +
                "\t\t\t" + GenerateTextColorCss() + "\n" +
                "\t\t\t" + GenerateTextDecorationCss() + "\n" +
                "\t\t\t" + GenerateTextTransformCss() + "\n" +
                "\t\t\t" + "text-align: center;\n" +
                "\t\t\t" + "display: block;\n" +
                "\t\t\t" + "line-height: 50px;\n" +
                "\t\t\t" + "margin: 0 auto;\n" +
                "\t\t" + "}\n";

            string body = "\t" + "<div>\n" +
                "\t\t" + previewTextBox.Text + "\n" +
                "\t" + "</div>\n";

            sourceBuilder.Replace("{PREVIEWCSS}", allCss);
            sourceBuilder.Replace("{PREVIEWBODY}", body);
            
            if (useTextShadowPolyFill)
            {
                sourceBuilder.Replace("{TEXTSHADOWSTYLE}", "\t<link rel=\"stylesheet\" href=\"jquery.textshadow.css\" />\n");

                string jqueryFilePath = System.IO.Path.Combine(previewDirPath, "jquery.js");
                string textShadowFilePath = System.IO.Path.Combine(previewDirPath, "jquery.textshadow.min.js");

                string jqueryFileContent = "";
                using (var reader = new StreamReader(new FileStream(jqueryFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    jqueryFileContent = reader.ReadToEnd();
                }

                string textShadowFileContent = "";
                using (var reader = new StreamReader(new FileStream(textShadowFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    textShadowFileContent = reader.ReadToEnd();
                }
                
                string textShadowScript =
                        "\t<script>\n" +
                        "\t\t" + jqueryFileContent + "\n" +
                        "\t</script>\n" +

                        "\t<script>\n" +
                        "\t\t" + textShadowFileContent + "\n" +
                        "\t</script>\n" +

                        "\t<script>\n" +
                        "\t\t$(\'div\').textshadow(\"" + GenerateShadowCss(true) + "\");\n" +
                        "\t\t</script>\n";

                sourceBuilder.Replace("{TEXTSHADOWSCRIPT}", textShadowScript);
            }
            else
            {
                sourceBuilder.Replace("{TEXTSHADOWSTYLE}", "");
                sourceBuilder.Replace("{TEXTSHADOWSCRIPT}", "");
            }

            // need to use temporary, and navigate to it,
            // navigating directly to Html source has problem in loading font file
            string previewFinalFilePath = System.IO.Path.Combine(previewDirPath, "previewfinal.html");
            File.WriteAllText(previewFinalFilePath, sourceBuilder.ToString());
            
            webBrowser.Navigate(new Uri(previewFinalFilePath));
        }

        private void SetCopyCss()
        {
            var builder = new StringBuilder();

            string fontFaceCss = GenerateFontFaceCss(false);
            if (!string.IsNullOrEmpty(fontFaceCss))
            {
                builder.AppendLine(fontFaceCss);
            }

            builder.AppendLine("div {");
            builder.Append('\t').AppendLine(GenerateFontCss());
            builder.Append('\t').AppendLine(GenerateShadowCss());
            builder.Append('\t').AppendLine(GenerateTextDecorationCss());
            builder.Append('\t').AppendLine(GenerateTextTransformCss());
            builder.Append('\t').AppendLine(GenerateBgCss());
            builder.Append('\t').AppendLine(GenerateTextColorCss());
            builder.AppendLine("}");

            cssTextBox.Text = builder.ToString();
        }

        private void fontVariantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void backgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void hShadowNumericBox_ValueChanged(object sender, EventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void vShadowNumericBox_ValueChanged(object sender, EventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void blurRadiusNumericBox_ValueChanged(object sender, EventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void shadowColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(cssTextBox.Text, TextDataFormat.UnicodeText);
        }

        private void chooseFontFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Font Files|*.ttf;*.woff|TTF Files (*.ttf)|*.ttf|WOFF Files (*.woff)|*.woff";
            dialog.FilterIndex = 0;

            bool? result = dialog.ShowDialog(this);
            if (result.HasValue && result.Value == true)
            {
                string filePath = dialog.FileName;

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    fontFilePathTextBox.Text = filePath;
                }
                else
                {
                    fontFilePathTextBox.Text = "Unknown";
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void genericFontRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void genericFontRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(fontFilePathTextBox.Text))
            {
                fontFilePathTextBox.Text = "Unknown";
            }

            SetCopyCss();
            SetPreviewSource();
        }

        private void fontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void fontFilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (File.Exists(fontFilePathTextBox.Text))
            {
                string fontFamilyName = FontFile.GetFontFamilyName(fontFilePathTextBox.Text);
                customFontFamilyName = "\"" + fontFamilyName + "\"";
            }
            else
            {
                customFontFamilyName = "Unknown";
            }

            SetCopyCss();
            SetPreviewSource();
        }

        private void fontSizeNumericBox_ValueChanged(object sender, EventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void fontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void fontWeightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void fontStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void fontDecorationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void fontTransformComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCopyCss();
            SetPreviewSource();
        }

        private void previewTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetPreviewSource();
        }

    }
}
