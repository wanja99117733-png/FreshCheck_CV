using FreshCheck_CV.Core;
using FreshCheck_CV.Inspect;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV.Scratch
{
    public partial class ScratchProp : UserControl
    {
        public ScratchProp()
        {
            InitializeComponent();
        }

        private void btnEraseBg_Click(object sender, EventArgs e)
        {
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule; // SaigeAI 인스턴스

            AIEngineType engineType = AIEngineType.Segmentation;
            string modelPath = "D:\\SagieModel\\Cu_seg.saigeseg";
            saigeAI.LoadEngine(modelPath, engineType); // 엔진 연결

            if (saigeAI == null)
            {
                MessageBox.Show("AI 모듈이 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap bitmap = Global.Inst.InspStage.GetCurrentImage(); // 현재 이미지 가져오기
            if (bitmap is null)
            {
                MessageBox.Show("현재 이미지가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            saigeAI.InspAIModule(bitmap); // 검사

            Bitmap resultImage = saigeAI.GetResultImage(); // 배경 삭제된 이미지

            Global.Inst.InspStage.UpdatePreview(resultImage);


            /* 테스트 용도 - S */
            Bitmap curBitmap = Global.Inst.InspStage.GetCurrentImage();
            Bitmap previewBitmap = Global.Inst.InspStage.GetPreviewImage();

            Mat curMat = BitmapConverter.ToMat(curBitmap);
            Mat previewMat = BitmapConverter.ToMat(previewBitmap);

            Cv2.Resize(curMat, curMat, new OpenCvSharp.Size(0, 0), 0.2, 0.2);
            Cv2.Resize(previewMat, previewMat, new OpenCvSharp.Size(0, 0), 0.2, 0.2);

            //Cv2.ImShow("curMat", curMat);
            //Cv2.ImShow("previewMat", previewMat);
            /* 테스트 용도 - E */

            
        }

        // 스크래치 검출 버튼
        private void btnScratchDet_Click(object sender, EventArgs e)
        {
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule;
            if (saigeAI == null) { /* 오류 */ return; }

            // 🔥 1. 배경제거 이미지만 사용 (검사+그리기)
            Bitmap noBgImage = Global.Inst.InspStage.GetPreviewImage();
            if (noBgImage == null)
            {
                MessageBox.Show("먼저 [배경제거] 버튼을 눌러주세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Console.WriteLine($"배경제거 이미지 크기: {noBgImage.Width}x{noBgImage.Height}");

            // 2. 배경제거 이미지로 Scratch 검출+사각형
            string scratchModelPath = "D:\\SagieModel\\Cucumber_Scratch_Det.saigeseg";
            saigeAI.LoadEngine(scratchModelPath, AIEngineType.ScratchSegmentation);

            if (!saigeAI.InspAIModule(noBgImage))  // 🔥 배경제거 이미지로 검출!
            {
                MessageBox.Show("Scratch 검출 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SegmentationResult scratchResult = saigeAI.GetScratchResult();
            Console.WriteLine($"검출된 Scratch 수: {scratchResult?.SegmentedObjects?.Length ?? 0}");

            // 3. 배경제거 이미지에 사각형 그리기
            Global.Inst.InspStage.UpdatePreviewWithScratch(noBgImage, scratchResult);
        }

    }
}
