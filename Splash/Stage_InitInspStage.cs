using FreshCheck_CV.Core;
using FreshCheck_CV.Inspect;
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
            // 모델 2개를 로딩하므로 충분한 최소 대기 시간을 확보합니다.
            int minMs = 3000;

            var cps = new StartupProgressHelper.Checkpoint[]
            {
                new StartupProgressHelper.Checkpoint(   0,  8, "InspStage 준비 중..."),
                new StartupProgressHelper.Checkpoint( 200, 20, "기본 상태 구성 중..."),
                new StartupProgressHelper.Checkpoint( 500, 30, "처리 파이프라인 점검 중..."),
                new StartupProgressHelper.Checkpoint(1000, 55, "AI 배경제거 모델 로딩..."),
                new StartupProgressHelper.Checkpoint(1800, 80, "AI 스크래치 모델 로딩..."),
                new StartupProgressHelper.Checkpoint(2500, 95, "초기화 마무리 중..."),
            };

            await StartupProgressHelper.RunWithMinimumDurationAsync(
                work: async () =>
                {
                    // 1. 기본 인스턴스 초기화 (Global 인스턴스 생성 등)
                    bool ok = Global.Inst.Initialize();
                    if (!ok)
                        throw new InvalidOperationException("InspStage 초기화 실패");

                    // 2. AI 모델 자동 로딩 (상시 로딩을 위해 두 번 호출)
                    //var saigeAI = Global.Inst.InspStage?.AIModule;
                    //if (saigeAI != null)
                    //{
                    //    // 배경제거 모델 로드
                    //    ct.ThrowIfCancellationRequested();
                    //    string bgPath = @"D:\SaigeModel\Cu_seg.saigeseg";
                    //    saigeAI.LoadEngine(bgPath, AIEngineType.Segmentation);

                    //    // 스크래치 모델 로드 (수정된 SaigeAI는 이전 모델을 지우지 않습니다)
                    //    ct.ThrowIfCancellationRequested();
                    //    string scratchPath = @"D:\SaigeModel\Cucumber_Scratch_Det.saigeseg";
                    //    saigeAI.LoadEngine(scratchPath, AIEngineType.ScratchSegmentation);
                    //}

                    // 🔥 오류 해결: async 람다이므로 return Task.CompletedTask가 필요 없습니다.
                },
                progress: progress,
                ct: ct,
                minDurationMs: minMs,
                checkpoints: cps).ConfigureAwait(true);
        }
    }
}