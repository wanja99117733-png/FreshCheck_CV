using System;
using System.Drawing;
using OpenCvSharp.Extensions;

namespace FreshCheck_CV.Scratch
{
    public static class HighlightRenderer
    {
        public static Bitmap ApplyHighlight(Bitmap srcBitmap, OpenCvSharp.Mat mask, double alpha)
        {
            if (srcBitmap == null)
                throw new ArgumentNullException(nameof(srcBitmap));
            if (mask == null)
                throw new ArgumentNullException(nameof(mask));

            using (OpenCvSharp.Mat src = BitmapConverter.ToMat(srcBitmap))
            using (OpenCvSharp.Mat overlay = new OpenCvSharp.Mat(src.Rows, src.Cols, src.Type(), new OpenCvSharp.Scalar(0, 0, 255))) // BGR Red
            using (OpenCvSharp.Mat blended = new OpenCvSharp.Mat())
            using (OpenCvSharp.Mat result = src.Clone())
            {
                OpenCvSharp.Cv2.AddWeighted(src, 1.0, overlay, alpha, 0.0, blended);

                // 마스크 영역만 빨갛게 덮기
                blended.CopyTo(result, mask);

                return BitmapConverter.ToBitmap(result);
            }
        }

        public static Bitmap RemoveBackground(Bitmap srcBitmap, OpenCvSharp.Mat mask)
        {
            if (srcBitmap == null)
                throw new ArgumentNullException(nameof(srcBitmap));
            if (mask == null)
                throw new ArgumentNullException(nameof(mask));

            using (OpenCvSharp.Mat src = BitmapConverter.ToMat(srcBitmap))
            using (OpenCvSharp.Mat result = new OpenCvSharp.Mat(src.Rows, src.Cols, src.Type(), OpenCvSharp.Scalar.Black))
            {
                src.CopyTo(result, mask);
                return BitmapConverter.ToBitmap(result);
            }
        }
    }
}
