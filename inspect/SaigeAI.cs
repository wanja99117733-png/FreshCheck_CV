using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaigeVision.Net.V2;
using SaigeVision.Net.V2.Segmentation;
using SaigeVision.Net.Core.V2;

namespace FreshCheck_CV.Inspect
{
    public enum AIEngineType
    {
        Segmentation,        // 배경 제거용 (Cu_seg.saigeseg)
        ScratchSegmentation  // 스크래치 검사용 (Cucumber_Scratch_Det.saigeseg)
    }

    public class SaigeAI : IDisposable
    {
        // 엔진별 독립적인 인스턴스 유지
        private SegmentationEngine _segEngine = null;
        private SegmentationEngine _scratchSegEngine = null;

        // 결과값 저장용
        private SegmentationResult _segResult = null;
        private SegmentationResult _scratchSegResult = null;

        private Bitmap _inspImage = null;

        public SaigeAI() { }

        

        /// <summary>
        /// 모델 로드 - 기존 엔진을 무조건 지우지 않고 해당 타입만 교체합니다.
        /// </summary>
        public void LoadEngine(string modelPath, AIEngineType engineType)
        {
            switch (engineType)
            {
                case AIEngineType.Segmentation:
                    // 기존 배경제거 엔진이 있다면 해당 엔진만 메모리 해제
                    if (_segEngine != null) { _segEngine.Dispose(); _segEngine = null; }
                    LoadSegEngine(modelPath);
                    break;

                case AIEngineType.ScratchSegmentation:
                    // 기존 스크래치 엔진이 있다면 해당 엔진만 메모리 해제
                    if (_scratchSegEngine != null) { _scratchSegEngine.Dispose(); _scratchSegEngine = null; }
                    LoadScratchSegEngine(modelPath);
                    break;
            }
        }

        private void LoadSegEngine(string modelPath)
        {
            _segEngine = new SegmentationEngine(modelPath, 0); // GPU 0번 사용
            SegmentationOption option = _segEngine.GetInferenceOption();
            option.CalcTime = false;
            option.CalcObject = true;
            option.CalcMask = false;
            option.CalcObjectAreaAndApplyThreshold = true;
            option.CalcObjectScoreAndApplyThreshold = true;
            _segEngine.SetInferenceOption(option);
        }

        private void LoadScratchSegEngine(string modelPath)
        {
            _scratchSegEngine = new SegmentationEngine(modelPath, 0); // 동일 GPU 0번에 함께 로드
            SegmentationOption option = _scratchSegEngine.GetInferenceOption();
            option.CalcTime = false;
            option.CalcObject = true;
            option.CalcMask = true;
            option.CalcObjectAreaAndApplyThreshold = true;
            option.CalcObjectScoreAndApplyThreshold = true;
            // 필요시 스코어 임계값 설정
            option.ObjectScoreThresholdPerClass[1] = 0.5;
            option.ObjectAreaThresholdPerClass[1] = 2000;
            _scratchSegEngine.SetInferenceOption(option);
        }

        /// <summary>
        /// 원하는 엔진 타입을 지정하여 검사를 수행합니다.
        /// </summary>
        public bool InspAIModule(Bitmap bmpImage, AIEngineType type)
        {
            if (bmpImage == null) return false;
            _inspImage = bmpImage;
            SrImage srImage = new SrImage(bmpImage);

            if (type == AIEngineType.Segmentation)
            {
                if (_segEngine == null) return false;
                _segResult = _segEngine.Inspection(srImage);
            }
            else if (type == AIEngineType.ScratchSegmentation)
            {
                if (_scratchSegEngine == null) return false;
                _scratchSegResult = _scratchSegEngine.Inspection(srImage);
            }

            return true;
        }

        // 배경제거 결과 이미지 생성 로직
        public Bitmap GetResultImage()
        {
            if (_inspImage == null || _segResult == null) return null;

            Bitmap resultImage = _inspImage.Clone(new Rectangle(0, 0, _inspImage.Width, _inspImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            DrawSegResult(_segResult.SegmentedObjects, resultImage);
            return resultImage;
        }

        private void DrawSegResult(SegmentedObject[] segmentedObjects, Bitmap bmp)
        {
            if (segmentedObjects == null || bmp == null) return;
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle fullRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                using (var allObjPath = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    foreach (var obj in segmentedObjects)
                    {
                        var contour = obj?.Contour?.Value;
                        if (contour == null || contour.Count < 3) continue;
                        allObjPath.AddPolygon(contour.ToArray());
                    }

                    if (allObjPath.PointCount < 3) { g.FillRectangle(Brushes.Black, fullRect); return; }
                    using (Region outside = new Region(fullRect))
                    {
                        outside.Exclude(allObjPath);
                        g.FillRegion(Brushes.Black, outside);
                    }
                }
            }
        }

        public SegmentationResult GetScratchResult() => _scratchSegResult;

        public void Dispose()
        {
            _segEngine?.Dispose();
            _scratchSegEngine?.Dispose();
        }
    }
}