using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FreshCheck_CV.Util
{
    internal enum SquareResizeMode
    {
        Stretch,
        Letterbox,
        CenterCrop
    }

    internal static class ImageResizeHelper
    {
        /// <summary>
        /// 입력 이미지를 size x size(예: 640x640)로 변환합니다.
        /// </summary>
        public static Bitmap ToSquare(Bitmap src, int size, SquareResizeMode mode = SquareResizeMode.Letterbox)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            // 결과 비트맵 생성(속도/호환성 위해 24bpp 사용)
            Bitmap dst = new Bitmap(size, size, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(dst))
            {
                // 속도 우선 세팅 (정확도/품질 우선이면 HighQualityBicubic로 변경 가능)
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low;
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                // 배경(패딩 영역) 검정
                g.Clear(Color.Black);

                Rectangle destRect;

                if (mode == SquareResizeMode.Stretch)
                {
                    // 무조건 꽉 채움(왜곡 가능)
                    destRect = new Rectangle(0, 0, size, size);
                }
                else if (mode == SquareResizeMode.Letterbox)
                {
                    // 비율 유지 + 남는 영역 패딩
                    float scale = Math.Min((float)size / src.Width, (float)size / src.Height);
                    int newW = Math.Max(1, (int)Math.Round(src.Width * scale));
                    int newH = Math.Max(1, (int)Math.Round(src.Height * scale));

                    int padX = (size - newW) / 2;
                    int padY = (size - newH) / 2;

                    destRect = new Rectangle(padX, padY, newW, newH);
                }
                else // CenterCrop
                {
                    // 비율 유지 + 꽉 채운 뒤 중앙만 남기기(크롭)
                    float scale = Math.Max((float)size / src.Width, (float)size / src.Height);
                    int newW = Math.Max(1, (int)Math.Round(src.Width * scale));
                    int newH = Math.Max(1, (int)Math.Round(src.Height * scale));

                    int offsetX = (size - newW) / 2;
                    int offsetY = (size - newH) / 2;

                    destRect = new Rectangle(offsetX, offsetY, newW, newH);
                }

                g.DrawImage(src, destRect);
            }

            return dst;
        }
    }
}
