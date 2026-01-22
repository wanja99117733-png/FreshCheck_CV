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
            public DefectType Type { get; set; } = DefectType.None;

            public bool IsDefect { get; set; } = false;

            // 디버그/표시용 코멘트
            public string Message { get; set; } = string.Empty;

            // 표시용 오버레이
            public Bitmap OverlayBitmap { get; set; } = null;
        }
    }