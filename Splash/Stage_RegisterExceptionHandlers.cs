using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FreshCheck_CV.InitProgress;

namespace FreshCheck_CV
{
    internal sealed class Stage_RegisterExceptionHandlers : IStartupStage
    {
        public string Name => "예외 처리기 등록";
        public int StageWeightPercent => 5;

        public Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct)
        {
            progress.Report(new InitProgress(30, "전역 예외 핸들러 연결 중..."));

            Application.ThreadException -= OnThreadException;
            Application.ThreadException += OnThreadException;

            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            progress.Report(new InitProgress(100, "예외 처리기 등록 완료"));
            return Task.CompletedTask;
        }

        private void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            // Slogger.Write(e.Exception.ToString());
            MessageBox.Show($"예외가 발생했습니다.\r\n\r\n{e.Exception.Message}", "FC", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Slogger.Write(e.ExceptionObject?.ToString() ?? "Unknown unhandled exception");
            MessageBox.Show("처리되지 않은 예외가 발생했습니다.", "FC", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
