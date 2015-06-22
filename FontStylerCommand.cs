//------------------------------------------------------------------------------
// <copyright file="FontStylerCommand.cs" company="AvoBright">
//     Copyright (c) AvoBright.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AvoBright.FontStyler
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FontStylerCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid MenuGroup = new Guid("eac93291-a181-44ff-b5e2-c81fe2b92d2a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private static FontWindow FontWindow { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontStylerCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private FontStylerCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                CommandID menuCommandID = new CommandID(MenuGroup, CommandId);
                EventHandler eventHandler = this.ShowFontWindow;
                MenuCommand menuItem = new MenuCommand(eventHandler, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FontStylerCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new FontStylerCommand(package);
        }

        /// <summary>
        /// Shows the font window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ShowFontWindow(object sender, EventArgs e)
        {
            if (FontWindow == null)
            {
                FontWindow = new FontWindow();
            }
            FontWindow.ShowModal();
        }
    }
}
