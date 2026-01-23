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
    internal sealed class Stage_InitInspStage : IStartupStage
    {
        public string Name { get { return "검사 엔진 초기화"; } }
        public int StageWeightPercent { get { return 35; } }

        public async Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct)
        {
            int minMs = 1600;

            var cps = new StartupProgressHelper.Checkpoint[]
            {
                new StartupProgressHelper.Checkpoint(   0,  5, "InspStage 준비 중..."),
                new StartupProgressHelper.Checkpoint( 250, 25, "기본 상태 구성 중..."),
                new StartupProgressHelper.Checkpoint( 400, 60, "처리 파이프라인 점검 중..."),
                new StartupProgressHelper.Checkpoint(800, 90, "초기화 마무리 중..."),
            };

            await StartupProgressHelper.RunWithMinimumDurationAsync(
                work: () =>
                {
                    // 실제 작업(현재는 빠르게 끝나도 됨)
                    bool ok = Global.Inst.Initialize();
                    if (!ok)
                        throw new InvalidOperationException("InspStage 초기화 실패");
                    return Task.CompletedTask;
                },
                progress: progress,
                ct: ct,
                minDurationMs: minMs,
                checkpoints: cps).ConfigureAwait(true);
        }
    }
}