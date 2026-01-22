using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreshCheck_CV.Core;
using WeifenLuo.WinFormsUI.Docking;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FreshCheck_CV
{
    public partial class CameraForm : DockContent
    {
        public CameraForm()
        {
            InitializeComponent();
        }

        //#3_CAMERAVIEW_PROPERTY#1 이미지 경로를 받아 PictureBox에 이미지를 로드하는 메서드
        public void LoadImage(string filePath)
        {
            if (File.Exists(filePath) == false)
                return;

            //#4_IMAGE_VIEWER#6 이미지 뷰어 컨트롤을 사용하여 이미지를 로드
            //picMainview.Image = Image.FromFile(filePath);
            Image bitmap = Image.FromFile(filePath);
            imageViewCtrl.LoadBitmap((Bitmap)bitmap);

            RemoveImageBg();
        }

        private void CameraForm_Resize_1(object sender, EventArgs e)
        {
            int margin = 0;
            imageViewCtrl.Width = this.Width - margin * 2;
            imageViewCtrl.Height = this.Height - margin * 2;

            imageViewCtrl.Location = new System.Drawing.Point(margin, margin);
        }

        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (imageViewCtrl != null)
                imageViewCtrl.LoadBitmap(bitmap);
        }

        public Bitmap GetDisplayImage()
        {
            Bitmap curImage = null;

            if (imageViewCtrl != null)
                curImage = imageViewCtrl.GetCurBitmap();

            return curImage;
        }

        private bool _windowOpened = false; // OpenCV 테스팅 창 열려있는지 여부

        // 작물과 배경 분리하여 배경 제거하는 함수
        public void RemoveImageBg()
        {
            if(_windowOpened)
                Cv2.DestroyAllWindows();

            Bitmap curBitmap = Global.Inst.InspStage.GetCurrentImage();

            // Bitmap 이미지를 Mat 타입으로 변경
            Mat curMat = BitmapConverter.ToMat(curBitmap);
            Cv2.Resize(curMat, curMat, new OpenCvSharp.Size(0, 0), 0.15, 0.15);


            // HSV로 변환
            /* 이유:
            밝은 초록(B=70, G=180, R=70), 어두운 초록(B=40, G=90, R=40)
            이런 식으로 색상과 밝기가 나뉘어지지 않아 다른 색이 들어가버리는 오류가 발생할 수도 있음.
            따라서 색상, 채도, 밝기로 나뉘어지는 HSV를 사용.
            */
            Mat hsv = new Mat();
            Cv2.CvtColor(curMat, hsv, ColorConversionCodes.BGR2HSV);

            // hsvGreen : 초록 오이
            Mat hsvGreen = new Mat();
            Cv2.InRange(hsv, new Scalar(18, 20, 20), new Scalar(95, 255, 255), hsvGreen);

            // hsvPale : 흰 오이
            Mat hsvPale = new Mat();
            Cv2.InRange(hsv, new Scalar(0, 0, 80), new Scalar(179, 80, 255), hsvPale);

            // 초록 마스크를 크게 팽창시켜 "오이 주변 영역"을 만들고,
            // 그 영역 내부에서만 cucumberPale를 인정합니다.
            Mat seed = hsvGreen.Clone();
            Mat kSeed = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(seed, seed, MorphTypes.Close, kSeed, iterations: 1);

            // 초록 주변 허용 영역(팽창). 커질수록 흰 오이 연결이 쉬워지지만 바닥 유입 위험도 증가
            Mat kDilate = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(13, 13));
            Mat seedDilated = new Mat();
            Cv2.Dilate(seed, seedDilated, kDilate);

            Mat paleNearGreen = new Mat();
            Cv2.BitwiseAnd(hsvPale, seedDilated, paleNearGreen);
            /* 오이의 스크래치 영역 추가 - S */
            // 적갈색(상처) HSV 범위
            Mat hsvRed1 = new Mat();
            Mat hsvRed2 = new Mat();

            // Red 영역은 Hue가 양쪽에 걸침
            Cv2.InRange(hsv, new Scalar(0, 50, 50), new Scalar(15, 255, 255), hsvRed1);
            Cv2.InRange(hsv, new Scalar(160, 50, 50), new Scalar(179, 255, 255), hsvRed2);

            Mat hsvRed = new Mat();
            Cv2.BitwiseOr(hsvRed1, hsvRed2, hsvRed);

            Mat seed2 = hsvPale.Clone();
            Mat kSeed2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(seed2, seed2, MorphTypes.Close, kSeed2, iterations: 1);

            Mat kDilate2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(13, 13));
            Mat seedDilated2 = new Mat();
            Cv2.Dilate(seed2, seedDilated2, kDilate2);

            Mat cucumberSeed = new Mat();
            Cv2.BitwiseOr(seedDilated, seedDilated2, cucumberSeed);

            // 붉은 스크래치를 오이 근처에서만 허용
            Mat redNearCucumber = new Mat();
            Cv2.BitwiseAnd(hsvRed, cucumberSeed, redNearCucumber);

            Mat hsvScratch = redNearCucumber.Clone();
            /* 오이의 스크래치 영역 추가 - E */

            // HSV 최종 마스크
            Mat mask = new Mat();
            Cv2.BitwiseAnd(hsvGreen, paleNearGreen, mask);
            Cv2.BitwiseOr(mask, redNearCucumber, mask);

            Mat hsvMask = mask.Clone();

            /* RGB 필터링 - S */
            Mat[] bgr = Cv2.Split(curMat);
            Mat b = bgr[0];
            Mat g = bgr[1];
            Mat r = bgr[2];

            /* RGB 초록 마스크 - S */
            Mat gGtR = new Mat();
            Mat gGtB = new Mat();
            Mat rPlus = new Mat();
            Mat bPlus = new Mat();

            Cv2.Add(r, new Scalar(10), rPlus);
            Cv2.Add(b, new Scalar(10), bPlus);

            Cv2.Compare(g, rPlus, gGtR, CmpType.GT);
            Cv2.Compare(g, bPlus, gGtB, CmpType.GT);

            // G 최소 밝기 조건 (너무 어두운 영역 제거)
            Mat gMin = new Mat();
            Cv2.Threshold(g, gMin, 30, 255, ThresholdTypes.Binary);

            Mat rgbGreen = new Mat();
            Cv2.BitwiseAnd(gGtR, gGtB, rgbGreen);
            Cv2.BitwiseAnd(rgbGreen, gMin, rgbGreen);
            /* RGB 초록 마스크 - E */
            /* RGB 황록 마스크 - S */
            Mat rGtB = new Mat();
            Mat gGtB2 = new Mat();
            Mat bPlus2 = new Mat();

            Cv2.Add(b, new Scalar(20), bPlus2);
            Cv2.Compare(r, bPlus2, rGtB, CmpType.GT);
            Cv2.Compare(g, bPlus2, gGtB2, CmpType.GT);

            Mat rMin = new Mat();
            Cv2.Threshold(r, rMin, 40, 255, ThresholdTypes.Binary);

            Mat rgbYellowGreen = new Mat();
            Cv2.BitwiseAnd(rGtB, gGtB2, rgbYellowGreen);
            Cv2.BitwiseAnd(rgbYellowGreen, rMin, rgbYellowGreen);


            Mat rgbSeed = rgbGreen.Clone();
            Mat rgbKSeed = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(rgbSeed, rgbSeed, MorphTypes.Close, rgbKSeed, iterations: 1);

            Mat rgbKDilate = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(13, 13));
            Mat rgbSeedDilated = new Mat();
            Cv2.Dilate(rgbSeed, rgbSeedDilated, rgbKDilate);

            Cv2.BitwiseAnd(rgbYellowGreen, rgbSeedDilated, rgbYellowGreen);
            /* RGB 황록 마스크 - E */
            /* RGB 아이보리 마스크 - S */
            // 기본 White 조건
            Mat diffRG = new Mat();
            Mat diffRB = new Mat();
            Mat diffGB = new Mat();

            Cv2.Absdiff(r, g, diffRG);
            Cv2.Absdiff(r, b, diffRB);
            Cv2.Absdiff(g, b, diffGB);

            Mat rgOk = new Mat();
            Mat rbOk = new Mat();
            Mat gbOk = new Mat();

            Cv2.Threshold(diffRG, rgOk, 18, 255, ThresholdTypes.BinaryInv);
            Cv2.Threshold(diffRB, rbOk, 22, 255, ThresholdTypes.BinaryInv);
            Cv2.Threshold(diffGB, gbOk, 22, 255, ThresholdTypes.BinaryInv);

            // 밝기 조건
            Mat avg = new Mat();
            Mat bright = new Mat();
            Cv2.AddWeighted(r, 0.33, g, 0.33, 0, avg);
            Cv2.AddWeighted(avg, 1.0, b, 0.33, 0, avg);
            Cv2.Threshold(avg, bright, 90, 255, ThresholdTypes.Binary);

            // 아이보리 색감: R,G가 B보다 약간 큼
            Mat rGtB2 = new Mat();
            Mat gGtB3 = new Mat();
            Mat bPlus3 = new Mat();

            Cv2.Add(b, new Scalar(8), bPlus3);
            Cv2.Compare(r, bPlus3, rGtB2, CmpType.GT);
            Cv2.Compare(g, bPlus3, gGtB3, CmpType.GT);

            // Ivory 후보
            Mat rgbIvory = new Mat();
            Cv2.BitwiseAnd(rgOk, rbOk, rgbIvory);
            Cv2.BitwiseAnd(rgbIvory, gbOk, rgbIvory);
            Cv2.BitwiseAnd(rgbIvory, bright, rgbIvory);
            Cv2.BitwiseAnd(rgbIvory, rGtB2, rgbIvory);
            Cv2.BitwiseAnd(rgbIvory, gGtB3, rgbIvory);
            /* RGB 아이보리 마스크 - E */

            // RGB 최종 마스크
            Mat rgbMask = new Mat();
            Cv2.ImShow("rgbGreen", rgbGreen);
            Cv2.ImShow("rgbYellowGreen", rgbYellowGreen);
            Cv2.ImShow("rgbIvory", rgbIvory);
            Cv2.BitwiseOr(rgbGreen, rgbYellowGreen, rgbMask);
            Cv2.BitwiseOr(rgbMask, rgbIvory, rgbMask);
            // 기존 마스크에 추가
            Cv2.BitwiseOr(mask, rgbMask, mask);
            /* RGB 필터링 - E */

            // 노이즈 제거
            Mat k = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, k, iterations: 1);
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, k, iterations: 2);

            // 오이만 남기기
            Cv2.FindContours(mask, out OpenCvSharp.Point[][] contours, out _,
                RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            Mat finalMask = Mat.Zeros(mask.Size(), MatType.CV_8UC1); // 모든 픽셀값이 0(검정색)인 Mat 생성
            int width = mask.Width;
            int height = mask.Height;

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area < 500) // 너무 작은 노이즈 제거
                    continue;


                // 큰 덩어리면서 가장자리 접촉하면 제외(바닥 제외하기 위함)
                Rect rect = Cv2.BoundingRect(contour);

                bool touchesBorder =
                    rect.X <= 2 || rect.Y <= 2 || rect.Right >= width - 2 || rect.Bottom >= height - 2;

                if (touchesBorder && area < (width * height * 0.2)) // 화면의 일정 비율 + 가장자리 접촉이면 제외
                    continue;

                // 통과한 컨투어는 마스크에 누적(남겨놓을 곳들) 
                Cv2.DrawContours(finalMask, new[] { contour }, -1, Scalar.White, thickness: -1);
            }

            Mat result = new Mat();
            curMat.CopyTo(result, finalMask); // 컬러 이미지에서 마지막으로 마스킹 처리한 이미지에 현재 이미지를 덧씌워서 result로 저장함.

            Cv2.ImShow("curMat", curMat);
            Cv2.ImShow("hsv", hsv);
            Cv2.ImShow("result", result);


            // 테스트 이미지 저장
            //SaveFile("curMat", curMat);
            //SaveFile("hsv", hsv);
            Dictionary<string, Mat> imageList = new Dictionary<string, Mat> ();
            imageList.Add("hsvGreen", hsvGreen);
            imageList.Add("hsvPale", hsvPale);
            imageList.Add("hsvScratch", hsvScratch);
            imageList.Add("hsvMask", hsvMask);
            imageList.Add("rgbMask", rgbMask);
            imageList.Add("mask", mask);
            imageList.Add("finalMask", finalMask);
            imageList.Add("result", result);
            //SaveTestFile(imageList.Values.ToList());
            ShowTestImage(imageList);

            _windowOpened = true;
        }

        // HSV 컬러를 확인하기 위한 캔버스를 띄우는 함수. OpenCV의 HSV는 Scalar를 사용하기 때문에 일반적인 HSV의 값과 다름.
        private void ShowHSVColorCanvas(Scalar hsvColor, string title = "canvas")
        {
            Mat canvas = new Mat(200, 200, MatType.CV_8UC3, hsvColor);
            Cv2.ImShow(title, canvas);
        }

        private void ShowTestImage(Dictionary<string, Mat> imageList)
        {
            foreach(var image in imageList)
            {
                Cv2.ImShow(image.Key, image.Value);
            }
        }


        // 테스트해본 마스크 파일들 저장하는 함수
        private void SaveTestFile(List<Mat> mats)
        {
            string baseDir = @"D:\teamProject\debug";

            // 날짜/시간 폴더명 (파일시스템 안전)
            //string timeFolder = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //string saveDir = Path.Combine(baseDir, timeFolder);
            string saveDir = Path.Combine(baseDir, "02");

            // 폴더 생성 (없으면 자동 생성)
            Directory.CreateDirectory(saveDir);

            string datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            // 파일 경로

            string allImagePath = Path.Combine(saveDir, $"{datetime}_allImage.jpg");

            Mat baseMat = mats[0];

            Mat grayDummy = new Mat(
                baseMat.Rows,
                baseMat.Cols,
                baseMat.Type(),
                new Scalar(128, 128, 128)
            );

            List<Mat> rows = new List<Mat>();

            for (int i = 0; i < mats.Count; i += 3)
            {
                Mat[] row = new Mat[3];

                for (int j = 0; j < 3; j++)
                {
                    int idx = i + j;
                    row[j] = (idx < mats.Count) ? mats[idx] : grayDummy;
                }

                Mat hRow = new Mat();
                Cv2.HConcat(row, hRow);
                rows.Add(hRow);
            }

            Mat result = new Mat();
            Cv2.VConcat(rows, result);
            SaveFile("allMask", result);
        }

        public void SaveFile(string path, Mat mat)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string baseDir = @"D:\teamProject\debug";

                // 날짜/시간 폴더명 (파일시스템 안전)
                string timeFolder = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string saveDir = Path.Combine(baseDir, timeFolder);

                // 폴더 생성 (없으면 자동 생성)
                Directory.CreateDirectory(saveDir);

                Cv2.ImWrite((path + ".jpg"), mat);
            }
        }
    }
}
