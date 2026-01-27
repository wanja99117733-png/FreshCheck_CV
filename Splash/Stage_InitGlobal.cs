using FreshCheck_CV.Core;
using FreshCheck_CV.Splash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FreshCheck_CV.InitProgress;

namespace FreshCheck_CV
{
    internal sealed class Stage_InitGlobal : IStartupStage
    {
        public string Name { get { return "Global 준비"; } }
        public int StageWeightPercent { get { return 15; } }

        public async Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct)
        {
            int minMs = 900;

            var cps = new StartupProgressHelper.Checkpoint[]
            {
                new StartupProgressHelper.Checkpoint(  0, 10, "Global 인스턴스 준비 중..."),
                new StartupProgressHelper.Checkpoint(250, 40, "공용 리소스 연결 중..."),
                new StartupProgressHelper.Checkpoint(600, 80, "모델 로딩 중..."),
            };

            await StartupProgressHelper.RunWithMinimumDurationAsync(
                work: () =>
                {
                    var g = Global.Inst;
                    return Task.CompletedTask;
                },
                progress: progress,
                ct: ct,
                minDurationMs: minMs,
                checkpoints: cps).ConfigureAwait(true);
        }
    }
}
