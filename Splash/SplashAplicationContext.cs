using FreshCheck_CV.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV
{
    internal sealed class SplashApplicationContext : ApplicationContext
    {
        private readonly SplashForm _splashForm;
        private CancellationTokenSource _cts;

        public SplashApplicationContext()
        {
            _cts = new CancellationTokenSource();

            _splashForm = new SplashForm();

            // ✅ 핵심: 메시지 루프가 유지되도록 스플래시를 MainForm으로 등록
            MainForm = _splashForm;

            _splashForm.FormClosed += OnSplashClosed;

            // Shown 이후 다음 tick에서 초기화 시작(페인트 보장)
            _splashForm.Shown += (s, e) =>
            {
                _splashForm.BeginInvoke(new Action(async () =>
                {
                    await RunInitializeAndSwitchToMainAsync().ConfigureAwait(true);
                }));
            };

            _splashForm.Show();
        }

        private async Task RunInitializeAndSwitchToMainAsync()
        {
            try
            {
                // 페인트 타이밍 확보
                await Task.Delay(50).ConfigureAwait(true);

                var progress = new Progress<InitProgress>(p => _splashForm.UpdateProgress(p));

                int startTick = Environment.TickCount;

                await AppInitializer.InitializeAsync(progress, _cts.Token).ConfigureAwait(true);

                // 너무 빨리 끝나면 최소 표시 시간 확보(선택)
                int elapsed = Environment.TickCount - startTick;
                int minMs = 800;
                if (elapsed < minMs)
                    await Task.Delay(minMs - elapsed).ConfigureAwait(true);

                var mainForm = new MainForm();
                mainForm.FormClosed += OnMainFormClosed;

                // ✅ 핵심: 메인 폼으로 교체한 다음 스플래시를 닫아야 앱이 유지됨
                MainForm = mainForm;

                mainForm.Show();
                _splashForm.Close();
            }
            catch (OperationCanceledException)
            {
                ExitThread();
            }
            catch (Exception ex)
            {
                FreshCheck_CV.Dialogs.CustomMessageBoxForm.Show($"시스템 실행에 실패하였습니다.\r\n\n{ex.ToString()}", "FC 초기화 오류");
                //MessageBox.Show(ex.ToString(), "FC 초기화 오류");
                ExitThread();
            }
        }

        private void OnMainFormClosed(object sender, FormClosedEventArgs e)
        {
            try { Global.Inst.Dispose(); } catch { }
            ExitThread();
        }

        private void OnSplashClosed(object sender, FormClosedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            // 스플래시가 사용자에 의해 닫힌 경우 메인폼이 아직 없을 수 있음
            if (MainForm == _splashForm)
                ExitThread();
        }
    }
}