using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 예외를 UI로 잡도록 강제
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "FC ThreadException");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show((e.ExceptionObject ?? "Unknown").ToString(), "FC UnhandledException");
            };

            Application.Run(new SplashApplicationContext());
        }
    }
}
