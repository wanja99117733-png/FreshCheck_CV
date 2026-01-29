using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Defect
{
    public sealed class DefectResult
    {
        public DefectType Type { get; set; } = DefectType.OK;

        public bool IsDefect { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public Bitmap OverlayBitmap { get; set; } = null;

        public double AreaRatio { get; set; } = 0.0;

        public long ElapsedMs { get; set; }


        // 유진형(스크래치 검사 시간)
        // public long ~~
    }
}