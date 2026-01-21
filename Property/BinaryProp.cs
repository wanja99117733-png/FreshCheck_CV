using FreshCheck_CV.Core;
using FreshCheck_CV.Models;
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
            cbMode.Items.Add(BinaryMode.GrayScale);

            // Threshold 기본값

            rangeTrackbar.Minimum = 0;
            rangeTrackbar.Maximum = 255;
            rangeTrackbar.ValueLeft = 60;
            rangeTrackbar.ValueRight = 150;

            // Invert 기본값
            chkInvert.Checked = false;

            // Adaptive UI를 아직 안 만들었다면, 일단 Adaptive는 선택만 가능하게 두고
            // blockSize/C는 BinaryOptions 기본값(31, 5)을 쓰게 됩니다.
        }

        private void HookEvents()
        {
            // 값 바꾸면 즉시 적용(원치 않으면 아래 3줄을 지우고 버튼 클릭에만 ApplyFromUi 호출)
            cbMode.SelectedIndexChanged += (s, e) => ApplyFromUi();
            rangeTrackbar.RangeChanged += (s, e) => ApplyFromUi();
            chkInvert.CheckedChanged += (s, e) => ApplyFromUi();

            // 버튼 적용 방식이면 예:
            // btnApply.Click += (s, e) => ApplyFromUi();
        }

        private void ApplyFromUi()
        {
            int left = rangeTrackbar.ValueLeft;
            int right = rangeTrackbar.ValueRight;

            int minValue = Math.Min(left, right);
            int maxValue = Math.Max(left, right);

            // “좌/우가 교차되면 반전” 규칙을 쓰고 싶으면 아래처럼:
            // bool invert = left > right;
            // 체크박스를 우선으로 쓰려면 아래처럼:
            bool invert = chkInvert.Checked;

            var options = new BinaryOptions
            {
                Mode = BinaryMode.GrayScale,
                MinValue = minValue,
                MaxValue = maxValue,
                Invert = invert
            };

            Global.Inst.InspStage.ApplyBinary(options);
        }
    }
}