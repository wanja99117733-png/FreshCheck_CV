using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaigeVision.Net.V2;
using SaigeVision.Net.V2.Detection;
using SaigeVision.Net.V2.IAD;
using SaigeVision.Net.V2.Segmentation;

namespace FreshCheck_CV.Inspect
{
    /*
    #5_SAIGE_SDK# - <<<Saige SDK 모듈 적용>>> 
    1) SaigeVision.Net.Core.V2 참조 추가
    2) SaigeVision.Net.V2 참조 추가
    3) 솔루션 플렛폼 x64 추가
    4) class SaigeAI 코드 구현
    5) 전체 검사를 관리하는 InspStage 클래스 구현
    6) 싱글톤 패턴을 이용하여 전역적으로 접근할 수 있도록 Global 클래스 구현
    7) AIModuleProp UserControl 구현
    8) PropertiesForm에 AIModuleProp UserControl 추가
    */

    public enum AIEngineType
    {
        [Description("Segmentation")]
        Segmentation = 0,
        [Description("ScratchSeg")]
        ScratchSegmentation = 1,
    }

    public class SaigeAI : IDisposable
    {
        AIEngineType _engineType;
        SegmentationEngine _segEngine = null;
        SegmentationResult _segResult = null;

        private SegmentationEngine _scratchSegEngine = null;
        private SegmentationResult _scratchSegResult = null;

        Bitmap _inspImage = null;

        public SaigeAI()
        {
        }

        // 엔진을 로드하는 메서드입니다.
        public void LoadEngine(string modelPath, AIEngineType engineType)
        {
            //GPU에 여러개 모델을 넣을 경우, 메모리가 부족할 수 있으므로, 해제
            DisposeMode();
            _engineType = engineType;

            switch (_engineType)
            {
                case AIEngineType.Segmentation:
                    LoadSegEngine(modelPath);
                    break;
                case AIEngineType.ScratchSegmentation:
                    LoadScratchSegEngine(modelPath);
                    break;
                default:
                    throw new NotSupportedException("지원하지 않는 엔진 타입입니다.");
            }
        }


        public void LoadSegEngine(string modelPath)
        {
            // 검사하기 위한 엔진에 대한 객체를 생성합니다.
            // 인스턴스 생성 시 모데파일 정보와 GPU Index를 입력해줍니다.
            // 필요에 따라 batch size를 입력합니다
            _segEngine = new SegmentationEngine(modelPath, 0);

            // 검사 전 option에 대한 설정을 가져옵니다
            SegmentationOption option = _segEngine.GetInferenceOption();

            /// 추론 API 실행에 소요되는 시간을 세분화하여 출력할지 결정합니다.
            /// `true`로 설정하면 이미지를 읽는 시간, 순수 딥러닝 추론 시간, 후처리 시간을 각각 확인할 수 있습니다.
            /// `false`로 설정하면 추론 API 실행에 소요된 총 시간만을 확인할 수 있습니다.
            /// `true`로 설정하면 전체 추론 시간이 느려질 수 있습니다. 실제 검사 시에는 `false`로 설정하는 것을 권장합니다.
            option.CalcTime = true;
            option.CalcObject = true;
            option.CalcScoremap = false;
            option.CalcMask = false;
            option.CalcObjectAreaAndApplyThreshold = true;
            option.CalcObjectScoreAndApplyThreshold = true;
            option.OversizedImageHandling = OverSizeImageFlags.do_not_inspect;
                
            //option.ObjectScoreThresholdPerClass[1] = 0;
            //option.ObjectScoreThresholdPerClass[2] = 0;

            //option.ObjectAreaThresholdPerClass[1] = 0;
            //option.ObjectAreaThresholdPerClass[2] = 0;

            // option을 적용하여 검사에 대한 조건을 변경할 수 있습니다.
            // 필요에 따라 writeModelFile parameter를 이용하여 모델파일에 정보를 영구적으로 변경할 수 있습니다.
            _segEngine.SetInferenceOption(option);
        }

        public void LoadScratchSegEngine(string modelPath)
        {
            _scratchSegEngine = new SegmentationEngine(modelPath, 0);
            SegmentationOption option = _scratchSegEngine.GetInferenceOption();
            option.CalcTime = false;  // 실제 검사 시 false 권장
            option.CalcObject = true;
            option.CalcScoremap = false;
            option.CalcMask = true;  // 마스크 계산 활성화
            option.CalcObjectAreaAndApplyThreshold = true;
            option.CalcObjectScoreAndApplyThreshold = true;
            option.ObjectScoreThresholdPerClass[0] = 0.5;  // 스크래치 클래스 threshold
            _scratchSegEngine.SetInferenceOption(option);
        }

        // 입력된 이미지에서 IAD 검사 진행
        public bool InspAIModule(Bitmap bmpImage)
        {
            if(bmpImage is null)
            {
                MessageBox.Show("이미지가 없습니다. 유효한 이미지를 입력해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            _inspImage = bmpImage;

            SrImage srImage = new SrImage(bmpImage);

            Stopwatch sw = Stopwatch.StartNew();

            switch (_engineType)
            {
                case AIEngineType.Segmentation:
                    if (_segEngine == null)
                    {
                        MessageBox.Show("엔진이 초기화되지 않았습니다. LoadEngine 메서드를 호출하여 엔진을 초기화하세요.");
                        return false;
                    }
                    // Segmentation 엔진을 이용하여 검사합니다.
                    _segResult = _segEngine.Inspection(srImage);
                    break;
                case AIEngineType.ScratchSegmentation:
                    if (_scratchSegEngine == null)  
                    {
                        MessageBox.Show("엔진이 초기화되지 않았습니다. LoadEngine 메서드를 호출하여 엔진을 초기화하세요.");
                        return false;
                    }
                    _scratchSegResult = _scratchSegEngine.Inspection(srImage); 
                    break;
            }

            //txt_InspectionTime.Text = sw.ElapsedMilliseconds.ToString();
            sw.Stop();

            return true;
        }

        // IADResult를 이용하여 결과를 이미지에 그립니다.
        private void DrawSegResult(SegmentedObject[] segmentedObjects, Bitmap bmp)
        {
            if (segmentedObjects == null || segmentedObjects.Length == 0 || bmp == null)
                return;

            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 전체 영역
                Rectangle fullRect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                // 오브젝트 전체를 합친 Path
                using (GraphicsPath allObjPath = new GraphicsPath(FillMode.Winding))
                {
                    foreach (var prediction in segmentedObjects)
                    {
                        var contour = prediction?.Contour?.Value;
                        if (contour == null || contour.Count < 3)
                            continue;

                        // Outer contour
                        allObjPath.AddPolygon(contour.ToArray());

                        // Inner contours도 Winding 기준으로 같이 채워지게(구멍 "메움" 효과)
                        if (prediction.Contour.InnerValue != null)
                        {
                            foreach (var innerValue in prediction.Contour.InnerValue)
                            {
                                if (innerValue == null || innerValue.Count < 3)
                                    continue;

                                allObjPath.AddPolygon(innerValue.ToArray());
                            }
                        }
                    }

                    // 오브젝트가 하나도 없으면 전체 검정 처리(원하면 return로 바꿔도 됨)
                    if (allObjPath.PointCount < 3)
                    {
                        g.FillRectangle(Brushes.Black, fullRect);
                        return;
                    }

                    // 오브젝트 바깥 영역만 검정으로 덮기
                    using (Region outside = new Region(fullRect))
                    {
                        outside.Exclude(allObjPath);      // 오브젝트 영역 제외
                        g.FillRegion(Brushes.Black, outside); // 바깥만 검정
                    }
                }
            }
        }

        public Bitmap GetResultImage()
        {
            if (_inspImage is null)
                return null;

            Bitmap resultImage = _inspImage.Clone(new Rectangle(0, 0, _inspImage.Width, _inspImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            switch (_engineType)
            {
                case AIEngineType.Segmentation:
                    if (_segResult == null)
                        return resultImage;
                    DrawSegResult(_segResult.SegmentedObjects, resultImage);
                    break;
                case AIEngineType.ScratchSegmentation:
                    return resultImage;
            }

            return resultImage;
        }
        public SegmentationResult GetScratchResult() => _scratchSegResult;

        private void DisposeMode()
        {
            //GPU에 여러개 모델을 넣을 경우, 메모리가 부족할 수 있으므로, 해제
            if (_segEngine != null)
                _segEngine.Dispose();

            if (_scratchSegEngine != null)
                _scratchSegEngine.Dispose();
        }

        #region Disposable

        private bool disposed = false; // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.

                    // 검사완료 후 메모리 해제를 합니다.
                    // 엔진 사용이 완료되면 꼭 dispose 해주세요
                    DisposeMode();
                }

                // Dispose unmanaged managed resources.

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion //Disposable
    }
}
