using FreshCheck_CV.Splash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FreshCheck_CV.InitProgress;

namespace FreshCheck_CV
{
    internal sealed class Stage_PrepareDirectories : IStartupStage
    {
        public string Name { get { return "폴더 준비"; } }
        public int StageWeightPercent { get { return 15; } }

        public async Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct)
        {
            int minMs = 900;

            var cps = new StartupProgressHelper.Checkpoint[]
            {
                new StartupProgressHelper.Checkpoint(  0, 10, "폴더 확인 시작"),
                new StartupProgressHelper.Checkpoint(250, 35, "Config 폴더 준비 중..."),
                new StartupProgressHelper.Checkpoint(550, 70, "Log/Temp 폴더 준비 중..."),
                new StartupProgressHelper.Checkpoint(750, 90, "폴더 준비 마무리 중..."),
            };

            await StartupProgressHelper.RunWithMinimumDurationAsync(
                work: () =>
                {
                    // 실제 작업(원래 하던 Directory.CreateDirectory들)
                    Directory.CreateDirectory(context.ConfigPath);
                    Directory.CreateDirectory(context.LogPath);
                    Directory.CreateDirectory(context.TempPath);
                    return Task.CompletedTask;
                },
                progress: progress,
                ct: ct,
                minDurationMs: minMs,
                checkpoints: cps).ConfigureAwait(true);
        }
    }
}
