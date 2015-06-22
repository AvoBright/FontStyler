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
using System.Reflection;

namespace AvoBright.FontStyler
{
    /// <summary>
    /// Interaction logic for NumericBox.xaml
    /// </summary>
    public partial class NumericBox : UserControl
    {
        private int? minValue = null;
        private int? maxValue = null;
        private int currentValue = 0;

        public event EventHandler ValueChanged;

        public int? MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
                if (minValue.HasValue && currentValue < minValue.Value)
                {
                    Value = minValue.Value;
                }
            }
        }

        public int? MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
                if (maxValue.HasValue && currentValue < maxValue.Value)
                {
                    Value = maxValue.Value;
                }
            }
        }

        public int Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                int originalValue = currentValue;

                if (minValue.HasValue && value < minValue.Value)
                {
                    currentValue = minValue.Value;
                }
                else if (maxValue.HasValue && value > maxValue.Value)
                {
                    currentValue = maxValue.Value;
                }
                else
                {
                    currentValue = value;
                }

                NUDTextBox.Text = currentValue.ToString();

                if (originalValue != currentValue)
                {
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, EventArgs.Empty);
                    }
                }
            }
        }


        public NumericBox()
        {
            InitializeComponent();
            NUDTextBox.Text = currentValue.ToString();
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            ++Value;
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            --Value;
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true }); 
            }


            if (e.Key == Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true }); 
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });

            if (e.Key == Key.Down)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (!string.IsNullOrEmpty(NUDTextBox.Text))
            {
                if (!int.TryParse(NUDTextBox.Text, out number))
                {
                    NUDTextBox.Text = currentValue.ToString();
                }
                else
                {
                    Value = number;
                }
            }
            
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

        }
    }
}
