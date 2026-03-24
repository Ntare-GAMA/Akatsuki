using System;
using System.Windows.Forms;

namespace RacingGame
{
    /// <summary>WinForms entry point.</summary>
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.MainMenuForm());
        }
    }
}
