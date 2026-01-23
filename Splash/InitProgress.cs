using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreshCheck_CV
{
    public readonly struct InitProgress
    {
        public int Percent { get; }
        public string Message { get; }

        public InitProgress(int percent, string message)
        {
            Percent = percent;
            Message = message ?? string.Empty;
        }

        internal interface IStartupStage
        {
            string Name { get; }

            // stageWeightPercent: 전체 0~100 중 이 단계가 차지하는 비중(진행률 계산용)

            int StageWeightPercent { get; }

            Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct);
        }
    }
}
