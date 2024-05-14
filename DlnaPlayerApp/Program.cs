using log4net.Config;
using System;
using System.Windows.Forms;

namespace DlnaPlayerApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new frmMain());
        }
    }
}
