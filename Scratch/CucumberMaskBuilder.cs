using System;
using System.Collections.Generic;

namespace FreshCheck_CV.Scratch
{
    public static class CucumberMaskBuilder
    {
        public static OpenCvSharp.Mat BuildMask(OpenCvSharp.Mat bgr, System.Collections.Generic.List<CucumberColorRule> rules)
        {
            if (bgr == null)
                throw new ArgumentNullException(nameof(bgr));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            OpenCvSharp.Mat includeMask = new OpenCvSharp.Mat(bgr.Rows, bgr.Cols, OpenCvSharp.MatType.CV_8UC1, OpenCvSharp.Scalar.Black);
            OpenCvSharp.Mat excludeMask = new OpenCvSharp.Mat(bgr.Rows, bgr.Cols, OpenCvSharp.MatType.CV_8UC1, OpenCvSharp.Scalar.Black);

            OpenCvSharp.Mat hsv = new OpenCvSharp.Mat();
            try
            {
                OpenCvSharp.Cv2.CvtColor(bgr, hsv, OpenCvSharp.ColorConversionCodes.BGR2HSV);

                for (int i = 0; i < rules.Count; i++)
                {
                    CucumberColorRule rule = rules[i];

                    using (OpenCvSharp.Mat one = BuildFloodFillMask(hsv, rule))
                    {
                        if (rule.IsExclude)
                            OpenCvSharp.Cv2.BitwiseOr(excludeMask, one, excludeMask);
                        else
                            OpenCvSharp.Cv2.BitwiseOr(includeMask, one, includeMask);
                    }
                }

                // include가 비어있으면 결과도 비움
                if (OpenCvSharp.Cv2.CountNonZero(includeMask) == 0)
                    return includeMask; // dispose 하지 말고 반환

                using (OpenCvSharp.Mat notExclude = new OpenCvSharp.Mat())
                using (OpenCvSharp.Mat finalMask = new OpenCvSharp.Mat())
                {
                    OpenCvSharp.Cv2.BitwiseNot(excludeMask, notExclude);
                    OpenCvSharp.Cv2.BitwiseAnd(includeMask, notExclude, finalMask);

                    // 노이즈 정리(가벼운 close)
                    using (OpenCvSharp.Mat k = OpenCvSharp.Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5)))
                    {
                        OpenCvSharp.Cv2.MorphologyEx(finalMask, finalMask, OpenCvSharp.MorphTypes.Close, k, iterations: 2);
                    }

                    // 반환용 clone
                    OpenCvSharp.Mat ret = finalMask.Clone();
                    includeMask.Dispose();
                    excludeMask.Dispose();
                    return ret;
                }
            }
            finally
            {
                hsv.Dispose();

                // includeMask/excludeMask는 위에서 return 경로에 따라 dispose 처리됨
                // (includeMask를 그대로 반환하는 케이스도 있으므로 주의)
            }
        }

        private static OpenCvSharp.Mat BuildFloodFillMask(OpenCvSharp.Mat hsv, CucumberColorRule rule)
        {
            OpenCvSharp.Mat outMask = new OpenCvSharp.Mat(hsv.Rows, hsv.Cols, OpenCvSharp.MatType.CV_8UC1, OpenCvSharp.Scalar.Black);

            // FloodFill용 마스크는 w+2, h+2
            using (OpenCvSharp.Mat ffMask = new OpenCvSharp.Mat(hsv.Rows + 2, hsv.Cols + 2, OpenCvSharp.MatType.CV_8UC1, OpenCvSharp.Scalar.Black))
            using (OpenCvSharp.Mat work = hsv.Clone())
            {
                OpenCvSharp.Point seed = new OpenCvSharp.Point(rule.SeedPoint.X, rule.SeedPoint.Y);

                OpenCvSharp.Scalar loDiff = new OpenCvSharp.Scalar(rule.HueTolerance, rule.SatTolerance, rule.ValTolerance);
                OpenCvSharp.Scalar hiDiff = new OpenCvSharp.Scalar(rule.HueTolerance, rule.SatTolerance, rule.ValTolerance);

                OpenCvSharp.Rect rect;
                int flags = 4 | (255 << 8) | (int)OpenCvSharp.FloodFillFlags.MaskOnly;

                OpenCvSharp.Cv2.FloodFill(
                    image: work,
                    mask: ffMask,
                    seedPoint: seed,
                    newVal: new OpenCvSharp.Scalar(0, 0, 0),
                    rect: out rect,
                    loDiff: loDiff,
                    upDiff: hiDiff,
                    flags: (OpenCvSharp.FloodFillFlags)flags
                );

                // (1,1)~(w,h) 복사
                using (OpenCvSharp.Mat roi = new OpenCvSharp.Mat(ffMask, new OpenCvSharp.Rect(1, 1, hsv.Cols, hsv.Rows)))
                {
                    roi.CopyTo(outMask);
                }
            }

            return outMask;
        }
    }
}
