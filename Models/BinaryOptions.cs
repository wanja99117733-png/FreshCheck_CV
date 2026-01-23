using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Models
{
        public sealed class BinaryOptions
        {
            public ShowBinaryMode ShowMode { get; set; } = ShowBinaryMode.HighlightRed;

            public bool Invert { get; set; } = false;

            // 기준색(BGR): 스포이드로 이미지에서 찍어서 설정
            public byte TargetB { get; set; } = 0;
            public byte TargetG { get; set; } = 0;
            public byte TargetR { get; set; } = 0;

            // RangeTrackbar 좌/우를 오차로 사용
            // Left: 아래쪽 허용 오차, Right: 위쪽 허용 오차 (0~255)
            public int TolLow { get; set; } = 0;
            public int TolHigh { get; set; } = 0;

            public void Validate()
            {
                if (TolLow < 0 || TolLow > 255) throw new ArgumentOutOfRangeException(nameof(TolLow));
                if (TolHigh < 0 || TolHigh > 255) throw new ArgumentOutOfRangeException(nameof(TolHigh));
            }

            public void GetLowerUpper(out OpenCvSharp.Scalar lower, out OpenCvSharp.Scalar upper)
            {
                int lb = Clamp(TargetB - TolLow);
                int lg = Clamp(TargetG - TolLow);
                int lr = Clamp(TargetR - TolLow);

                int ub = Clamp(TargetB + TolHigh);
                int ug = Clamp(TargetG + TolHigh);
                int ur = Clamp(TargetR + TolHigh);

                lower = new OpenCvSharp.Scalar(lb, lg, lr);
                upper = new OpenCvSharp.Scalar(ub, ug, ur);
            }

            private static int Clamp(int v)
            {
                if (v < 0) return 0;
                if (v > 255) return 255;
                return v;
            }
        }
    }
//    public sealed class BinaryOptions
//    {
//        public BinaryMode Mode { get; set; } = BinaryMode.GrayScale;

//        // 0~255, B/G/R에 동일 적용
//        public int MinValue { get; set; } = 0;

//        // 0~255, B/G/R에 동일 적용
//        public int MaxValue { get; set; } = 255;

//        public int MinB { get; set; } = 0;
//        public int MaxB { get; set; } = 255;
//        public int MinG { get; set; } = 0;
//        public int MaxG { get; set; } = 255;
//        public int MinR { get; set; } = 0;
//        public int MaxR { get; set; } = 255;

//        public ShowBinaryMode ShowMode { get; set; } = ShowBinaryMode.HighlightRed; // 기본 하이라이트

//        // 마스크 반전 여부
//        public bool Invert { get; set; } = false;

//public void Validate()
//{
//    ValidateRange(MinB, MaxB, nameof(MinB), nameof(MaxB));
//    ValidateRange(MinG, MaxG, nameof(MinG), nameof(MaxG));
//    ValidateRange(MinR, MaxR, nameof(MinR), nameof(MaxR));
//}

//        private static void ValidateRange(int min, int max, string minName, string maxName)
//        {
//            if (min < 0 || min > 255) throw new ArgumentOutOfRangeException(minName);
//            if (max < 0 || max > 255) throw new ArgumentOutOfRangeException(maxName);
//            if (min > max) throw new ArgumentException($"{minName}는 {maxName}보다 클 수 없습니다.");
//        }
//    }
//}
