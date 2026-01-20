using FreshCheck_CV.Core;
using FreshCheck_CV.Core.FreshCheck_CV.Core.Models;
using FreshCheck_CV.Models.FreshCheck_CV.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

    namespace FreshCheck_CV.Property
    {
        public partial class BinaryProp : UserControl
        {
            public BinaryProp()
            {
                InitializeComponent();
                InitUi();
                HookEvents();
            }

            private void InitUi()
            {
                // 모드 콤보박스에 enum을 넣되, RGB 모드는 일단 제외
                cbMode.Items.Clear();
                cbMode.Items.Add(BinaryMode.GrayThreshold);
                cbMode.Items.Add(BinaryMode.Otsu);
                cbMode.Items.Add(BinaryMode.Adaptive);

                cbMode.SelectedItem = BinaryMode.GrayThreshold;

                // Threshold 기본값
                tbThreshold.Minimum = 0;
                tbThreshold.Maximum = 255;
                tbThreshold.Value = 128;

                // Invert 기본값
                chkInvert.Checked = false;

                // Adaptive UI를 아직 안 만들었다면, 일단 Adaptive는 선택만 가능하게 두고
                // blockSize/C는 BinaryOptions 기본값(31, 5)을 쓰게 됩니다.
            }

            private void HookEvents()
            {
                // 값 바꾸면 즉시 적용(원치 않으면 아래 3줄을 지우고 버튼 클릭에만 ApplyFromUi 호출)
                cbMode.SelectedIndexChanged += (s, e) => ApplyFromUi();
                tbThreshold.Scroll += (s, e) => ApplyFromUi();
                chkInvert.CheckedChanged += (s, e) => ApplyFromUi();

                // 버튼 적용 방식이면 예:
                // btnApply.Click += (s, e) => ApplyFromUi();
            }

            private void ApplyFromUi()
            {
                if (cbMode.SelectedItem == null)
                    return;

                BinaryMode mode = (BinaryMode)cbMode.SelectedItem;

                BinaryOptions opt = new BinaryOptions
                {
                    Mode = mode,
                    Invert = chkInvert.Checked,
                    Threshold = tbThreshold.Value
                    // AdaptiveBlockSize / AdaptiveC는 UI가 없으므로 기본값(31,5) 사용
                };

                Global.Inst.InspStage.ApplyBinary(opt);
            }
        }
    }