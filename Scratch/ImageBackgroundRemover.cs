using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FreshCheck_CV.Scratch
{
    public class ImageBackgroundRemover
    {
        private const int MIN_CONTOUR_AREA = 3000;

        /// <summary>
        /// 이미지에서 오이만 검출하여 배경을 제거합니다.
        /// </summary>
        /// <param name="sourceBitmap">원본 이미지</param>
        /// <returns>배경이 제거된 오이 Bitmap</returns>
        public Bitmap RemoveImageBg(Bitmap sourceBitmap)
        {
            if (sourceBitmap == null)
            {
                throw new ArgumentNullException(nameof(sourceBitmap));
            }

            using (Mat srcMat = BitmapConverter.ToMat(sourceBitmap))
            using (Mat resizedMat = ResizeForProcessing(srcMat))
            using (Mat hsvMat = new Mat())
            {
                Cv2.CvtColor(resizedMat, hsvMat, ColorConversionCodes.BGR2HSV);

                using (Mat cucumberMask = CreateCucumberMask(resizedMat, hsvMat))
                using (Mat refinedMask = RefineMask(cucumberMask))
                {
                    using (Mat grabCutMask = RefineMaskWithGrabCut(resizedMat, refinedMask))
                    using (Mat finalMask = FilterCucumberComponents(grabCutMask, hsvMat))
                    {
                        return ApplyMaskToOriginalImage(srcMat, finalMask);
                    }

                }
            }
        }

        private Mat FilterCucumberComponents(Mat mask, Mat hsvMat)
        {
            // mask: 0/255 바이너리 (GrabCut 이후든 refinedMask든 OK)
            // hsvMat: resizedMat 기준 HSV

            Mat labels = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();

            int n = Cv2.ConnectedComponentsWithStats(
                mask, labels, stats, centroids,
                PixelConnectivity.Connectivity8, MatType.CV_32S);

            Mat result = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.All(0));
            int imgArea = mask.Width * mask.Height;

            // seed(초록) 마스크: 오이일 가능성이 높은 단서
            Mat seedMask = new Mat();
            {
                Scalar greenLower = new Scalar(15, 20, 20);
                Scalar greenUpper = new Scalar(95, 255, 255);
                Cv2.InRange(hsvMat, greenLower, greenUpper, seedMask);

                using (var k = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)))
                    Cv2.MorphologyEx(seedMask, seedMask, MorphTypes.Open, k, iterations: 1);
            }

            // 파라미터(현 상황 기준 추천값)
            int minArea = 2000;                 // 너무 작은 덩어리 제거
            double maxAreaRatio = 0.90;         // ❗0.45는 너무 낮음. 근접/멀티 오이 살리려면 0.85~0.95
            double minSolidity = 0.60;          // 바닥 조각(들쭉날쭉) 제거용
            double minExtent = 0.18;            // area / (w*h) 너무 낮으면 바닥 조각일 확률↑
            int borderPad = 2;                  // 가장자리 판정 여유
            int minGreenOverlap = 15;           // seed 겹침 임계(너무 높이면 bellyrot가 죽음)

            for (int i = 1; i < n; i++)
            {
                int x = stats.Get<int>(i, (int)ConnectedComponentsTypes.Left);
                int y = stats.Get<int>(i, (int)ConnectedComponentsTypes.Top);
                int w = stats.Get<int>(i, (int)ConnectedComponentsTypes.Width);
                int h = stats.Get<int>(i, (int)ConnectedComponentsTypes.Height);
                int area = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);

                if (area < minArea)
                    continue;

                if (area > imgArea * maxAreaRatio)
                    continue;

                bool touchesBorder =
                    (x <= borderPad) || (y <= borderPad) ||
                    (x + w >= mask.Width - 1 - borderPad) ||
                    (y + h >= mask.Height - 1 - borderPad);

                using (Mat compMask = new Mat())
                using (Mat inter = new Mat())
                {
                    // compMask = 현재 라벨만 0/255
                    Cv2.Compare(labels, i, compMask, CmpType.EQ);

                    // 1) seed(초록) 겹침
                    Cv2.BitwiseAnd(compMask, seedMask, inter);
                    int greenOverlap = Cv2.CountNonZero(inter);
                    bool passSeed = greenOverlap >= minGreenOverlap;

                    // 2) 형상 특징(바닥 덩어리 억제)
                    //    - solidity: area / convexHullArea
                    //    - extent: area / (w*h)
                    double extent = (double)area / (w * h);

                    // contour -> hull area 계산
                    Cv2.FindContours(compMask, out OpenCvSharp.Point[][] contours, out _,
                        RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                    if (contours.Length == 0)
                        continue;

                    // 컴포넌트는 보통 1개 contour가 주로 잡힘
                    var c = contours.OrderByDescending(t => Cv2.ContourArea(t)).First();
                    double contourArea = Cv2.ContourArea(c);

                    // 혹시 contourArea가 area보다 너무 작게 나오면(마스크 구멍 등) area 사용
                    double useArea = Math.Max(contourArea, area);

                    OpenCvSharp.Point[] hull = Cv2.ConvexHull(c);
                    double hullArea = Math.Max(Cv2.ContourArea(hull), 1.0);
                    double solidity = useArea / hullArea;

                    bool passShape = (solidity >= minSolidity) && (extent >= minExtent);

                    // - seed가 있으면 shape가 조금 나빠도 살림(오이 표면/조명 때문)
                    // - seed가 거의 없으면(탈색 오이 포함) shape 조건을 더 중요하게 봄
                    if (!passSeed && !passShape)
                        continue;

                    // 가장자리 누수 억제(바닥이 가장자리에서 크게 들어오는 케이스)
                    // 오이가 가장자리에 붙는 정상 케이스도 있으니 “touch + seed없음 + shape나쁨”만 컷
                    if (touchesBorder && !passSeed && solidity < 0.55)
                        continue;

                    // 통과한 컴포넌트 합치기 (여러 개 오이 모두 포함)
                    result.SetTo(Scalar.White, compMask);
                }
            }

            seedMask.Dispose();
            labels.Dispose();
            stats.Dispose();
            centroids.Dispose();

            return result;
        }


        /// <summary>
        /// 처리 속도 향상을 위한 리사이즈
        /// </summary>
        private Mat ResizeForProcessing(Mat srcMat)
        {
            Mat resized = new Mat();
            Cv2.Resize(
                srcMat,
                resized,
                new OpenCvSharp.Size(),
                0.5,
                0.5,
                InterpolationFlags.Linear);

            return resized;
        }

        /// <summary>
        /// HSV 색상 기반 오이 마스크 생성
        /// </summary>
        private Mat CreateCucumberMask(Mat bgrMat, Mat hsvMat)
        {
            if (bgrMat == null)
            {
                throw new ArgumentNullException(nameof(bgrMat));
            }

            if (hsvMat == null)
            {
                throw new ArgumentNullException(nameof(hsvMat));
            }

            // 1) 초록/황록(오이 기본색) 마스크
            Scalar greenLower = new Scalar(18, 25, 25);
            Scalar greenUpper = new Scalar(95, 255, 255);

            // 2) 탈색(하얀/연노랑) 후보 마스크: 과검 줄이려고 V 하한을 좀 올림
            Scalar paleLower = new Scalar(0, 0, 180);
            Scalar paleUpper = new Scalar(180, 55, 255);


            Mat maskGreen = new Mat();
            Mat maskPale = new Mat();
            Mat mergedMask = new Mat();

            Cv2.InRange(hsvMat, greenLower, greenUpper, maskGreen);
            Cv2.InRange(hsvMat, paleLower, paleUpper, maskPale);

            // 작은 잡음 제거(과검 방지용, 약하게)
            using (Mat openKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)))
            {
                Cv2.MorphologyEx(maskGreen, maskGreen, MorphTypes.Open, openKernel, iterations: 1);
                Cv2.MorphologyEx(maskPale, maskPale, MorphTypes.Open, openKernel, iterations: 1);
            }

            Cv2.BitwiseOr(maskGreen, maskPale, mergedMask);

            // green(seed)에서 연결된 영역만 남기기 (bellyrot 과검 방지)
            Mat connectedMask = KeepOnlySeedConnectedRegion(maskGreen, mergedMask);

            maskGreen.Dispose();
            maskPale.Dispose();
            mergedMask.Dispose();

            return connectedMask;
        }

        private Mat KeepOnlySeedConnectedRegion(Mat seedMask, Mat candidateMask)
        {
            if (seedMask == null)
            {
                throw new ArgumentNullException(nameof(seedMask));
            }

            if (candidateMask == null)
            {
                throw new ArgumentNullException(nameof(candidateMask));
            }

            // seed(초록)가 거의 없으면(= 완전 탈색 케이스) 후보 중 가장 큰 덩어리로 fallback
            int seedArea = Cv2.CountNonZero(seedMask);
            if (seedArea < 150)
            {
                return ExtractLargestConnectedComponent(candidateMask);
            }

            Mat prev = new Mat();
            Mat curr = new Mat();
            Mat diff = new Mat();

            seedMask.CopyTo(curr);

            using (Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)))
            {
                for (int i = 0; i < 200; i++)
                {
                    curr.CopyTo(prev);

                    // geodesic dilation: dilate(curr) AND candidateMask
                    Cv2.Dilate(curr, curr, kernel, iterations: 1);
                    Cv2.BitwiseAnd(curr, candidateMask, curr);

                    Cv2.Absdiff(curr, prev, diff);
                    if (Cv2.CountNonZero(diff) == 0)
                    {
                        break;
                    }
                }
            }

            prev.Dispose();
            diff.Dispose();
            return curr; // curr를 반환 (Dispose 금지)
        }

        private Mat ExtractLargestConnectedComponent(Mat mask)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            Mat labels = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();

            int count = Cv2.ConnectedComponentsWithStats(mask, labels, stats, centroids, PixelConnectivity.Connectivity8, MatType.CV_32S);

            int bestLabel = -1;
            int bestArea = 0;

            for (int i = 1; i < count; i++)
            {
                int area = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
                if (area > bestArea)
                {
                    bestArea = area;
                    bestLabel = i;
                }
            }

            Mat result = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.All(0));
            if (bestLabel >= 0)
            {
                using (Mat labelMask = new Mat())
                {
                    Cv2.Compare(labels, bestLabel, labelMask, CmpType.EQ);
                    result.SetTo(Scalar.White, labelMask);
                }
            }

            labels.Dispose();
            stats.Dispose();
            centroids.Dispose();

            return result;
        }



        /// <summary>
        /// Morphology 연산으로 노이즈 제거
        /// </summary>
        private Mat RefineMask(Mat mask)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            Mat refined = new Mat();

            using (Mat kernelClose = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(7, 7)))
            using (Mat kernelOpen = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)))
            {
                // 1) 구멍 메우기
                Cv2.MorphologyEx(mask, refined, MorphTypes.Close, kernelClose, iterations: 1);

                // 2) 작은 노이즈 제거
                Cv2.MorphologyEx(refined, refined, MorphTypes.Open, kernelOpen, iterations: 1);

                // 3) 누락된 하얀 부분을 포함시키기 위한 확장
                //Cv2.Dilate(refined, refined, kernelDilate, iterations: 1);
            }

            return refined;
        }


        /// <summary>
        /// 가장 큰 Contour(오이)만 추출
        /// </summary>
        private Mat ExtractLargestContourMask(Mat mask)
        {
            Cv2.FindContours(
                mask,
                out OpenCvSharp.Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                return mask;
            }

            OpenCvSharp.Point[] largestContour = contours
                .Where(c => Cv2.ContourArea(c) > MIN_CONTOUR_AREA)
                .OrderByDescending(c => Cv2.ContourArea(c))
                .FirstOrDefault();


            Mat resultMask = Mat.Zeros(mask.Size(), MatType.CV_8UC1);

            if (largestContour != null)
            {
                Cv2.DrawContours(
                    resultMask,
                    new[] { largestContour },
                    -1,
                    Scalar.White,
                    -1);
            }

            return resultMask;
        }

        /// <summary>
        /// 원본 이미지에 마스크 적용 (배경 제거)
        /// </summary>
        private Bitmap ApplyMaskToOriginalImage(Mat originalMat, Mat resizedMask)
        {
            Mat mask = new Mat();
            Cv2.Resize(
                resizedMask,
                mask,
                new OpenCvSharp.Size(originalMat.Width, originalMat.Height),
                0,
                0,
                InterpolationFlags.Nearest);

            Mat result = new Mat();
            originalMat.CopyTo(result, mask);

            return BitmapConverter.ToBitmap(result);
        }


        /// <summary>
        /// 범위를 확장하되, 바닥으로 과확장되는 건 GrabCut이 다시 잡아줌
        /// </summary>
        private Mat RefineMaskWithGrabCut(Mat bgrMat, Mat roughMask)
        {
            if (bgrMat == null)
            {
                throw new ArgumentNullException(nameof(bgrMat));
            }

            if (roughMask == null)
            {
                throw new ArgumentNullException(nameof(roughMask));
            }

            // GrabCut 마스크는 0/1/2/3 값 필요
            // 0: Sure BG, 1: Sure FG, 2: Prob BG, 3: Prob FG
            Mat grabMask = new Mat(roughMask.Size(), MatType.CV_8UC1, Scalar.All(2)); // 기본 Prob BG

            // Prob FG = roughMask 흰 영역
            grabMask.SetTo(new Scalar(3), roughMask);

            // Sure FG = roughMask를 살짝 Erode 한 중심부
            using (Mat sureFg = new Mat())
            using (Mat erodeKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(9, 9)))
            {
                Cv2.Erode(roughMask, sureFg, erodeKernel, iterations: 1);
                grabMask.SetTo(new Scalar(1), sureFg);
            }

            // Sure BG = 테두리(가장자리) 영역을 강제로 배경 처리 (누수 방지)
            int border = 8;
            Cv2.Rectangle(grabMask, new Rect(0, 0, grabMask.Width, border), Scalar.All(0), -1);
            Cv2.Rectangle(grabMask, new Rect(0, grabMask.Height - border, grabMask.Width, border), Scalar.All(0), -1);
            Cv2.Rectangle(grabMask, new Rect(0, 0, border, grabMask.Height), Scalar.All(0), -1);
            Cv2.Rectangle(grabMask, new Rect(grabMask.Width - border, 0, border, grabMask.Height), Scalar.All(0), -1);

            using (Mat bgdModel = new Mat())
            using (Mat fgdModel = new Mat())
            {
                Cv2.GrabCut(
                    bgrMat,
                    grabMask,
                    new Rect(),
                    bgdModel,
                    fgdModel,
                    3,
                    GrabCutModes.InitWithMask);
            }

            // 최종 마스크: Sure FG(1) + Prob FG(3)
            Mat finalMask = new Mat(grabMask.Size(), MatType.CV_8UC1, Scalar.All(0));
            using (Mat fg1 = new Mat())
            using (Mat fg3 = new Mat())
            using (Mat fg = new Mat())
            {
                Cv2.Compare(grabMask, new Scalar(1), fg1, CmpType.EQ);
                Cv2.Compare(grabMask, new Scalar(3), fg3, CmpType.EQ);
                Cv2.BitwiseOr(fg1, fg3, fg);

                finalMask.SetTo(Scalar.White, fg);
            }

            grabMask.Dispose();
            return finalMask;
        }

    }

}
