
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
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule;

            if (saigeAI == null)
            {
                MessageBox.Show("AI 모듈이 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap bitmap = Global.Inst.InspStage.GetCurrentImage();
            if (bitmap is null)
            {
                MessageBox.Show("현재 이미지가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 🔥 수정: 어떤 엔진을 사용할지 타입을 명시합니다. (상시 로딩 방식)
            if (saigeAI.InspAIModule(bitmap, AIEngineType.Segmentation))
            {
                Bitmap resultImage = saigeAI.GetResultImage(); // 배경 삭제된 이미지 생성
                Global.Inst.InspStage.UpdatePreview(resultImage);
            }
        }

        // 스크래치 검출 버튼
        private void btnScratchDet_Click(object sender, EventArgs e)
        {
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule;
            if (saigeAI == null) return;

            // 1. 검사 대상: 배경제거된 이미지 (배경이 검정색이어야 스크래치 집중도가 높음)
            Bitmap noBgImage = Global.Inst.InspStage.GetPreviewImage();

            // 2. 출력 대상: 원본 이미지 (사용자가 보기 편하도록)
            Bitmap originalImage = Global.Inst.InspStage.GetCurrentImage();

            if (noBgImage == null)
            {
                MessageBox.Show("먼저 [배경제거] 버튼을 눌러주세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 🔥 수정: 스크래치 전용 엔진으로 검사 수행
            if (!saigeAI.InspAIModule(noBgImage, AIEngineType.ScratchSegmentation))
            {
                MessageBox.Show("Scratch 검출 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SegmentationResult scratchResult = saigeAI.GetScratchResult();

            // 3. 원본 이미지(originalImage) 위에 검출된 결과(scratchResult)의 사각형을 그림
            Global.Inst.InspStage.UpdatePreviewWithScratch(originalImage, scratchResult);
        }
    }
}
