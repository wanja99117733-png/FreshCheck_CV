using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreshCheck_CV.Util;

namespace FreshCheck_CV
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //#14_LOGFORM#1 log4net 설정 파일을 읽어들임
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            SLogger.Write("Logger initialized!", SLogger.LogType.Info);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 예외를 UI로 잡도록 강제
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                FreshCheck_CV.Dialogs.CustomMessageBoxForm.Show($"프로그램 실행에 실패하였습니다.\r\n\r\n{e.Exception.ToString()}", "ThreadException");
                //MessageBox.Show(e.Exception.ToString(), "FC ThreadException");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                FreshCheck_CV.Dialogs.CustomMessageBoxForm.Show($"프로그램 실행에 실패하였습니다.\r\n\r\n{(e.ExceptionObject ?? "Unknown").ToString()}", "FC UnhandledException");
                //FreshCheck_CV.Dialogs.CustomMessageBoxForm.Show("프로그램 실행에 실패하였습니다.", "시스템 오류");
                //MessageBox.Show((e.ExceptionObject ?? "Unknown").ToString(), "FC UnhandledException");
            };

            Application.Run(new SplashApplicationContext());
        }
    }
}
