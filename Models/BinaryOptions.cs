using FreshCheck_CV.Core.FreshCheck_CV.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace FreshCheck_CV.Models
{
    namespace FreshCheck_CV.Core.Models
    {
        // 이진화에 필요한 모든 설정값(파라미터)을 담는 클래스
        // UI(BinaryProp)에서 값을 만들고, BinaryProcessor로 전달하는 용도

        public class BinaryOptions
        {
            // 어떤 이진화 모드를 쓸지
            public BinaryMode Mode { get; set; } = BinaryMode.GrayThreshold;

            #region Gray(흑백) 이진화 옵션
            // 고정 임계값(0~255)
            public int Threshold { get; set; } = 128;
            // 반전 여부 (검정/흰색 뒤집기)
            public bool Invert { get; set; } = false;
            // Adaptive 모드에서 사용하는 블록 크기(홀수, 3 이상)
            public int AdaptiveBlockSize { get; set; } = 31;
            // Adaptive 모드에서 사용하는 보정값(C)
            public int AdaptiveC { get; set; } = 5;
            #endregion

            #region RGB 색상 범위 이진화 옵션 (OpenCV는 BGR 순서)
            // 최소 B 값 (0~255)
            public int MinB { get; set; } = 0;
            // 최대 B 값 (0~255)
            public int MaxB { get; set; } = 255;
            // 최소 G 값 (0~255)
            public int MinG { get; set; } = 0;
            // 최대 G 값 (0~255)
            public int MaxG { get; set; } = 255;
            // 최소 R 값 (0~255)
            public int MinR { get; set; } = 0;
            // 최대 R 값 (0~255)
            public int MaxR { get; set; } = 255;

            #endregion
        }
    }

}
