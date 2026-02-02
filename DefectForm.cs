using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{
    public partial class DefectForm : DockContent
    {
        private const int ThumbW = 80;
        private const int ThumbH = 55;



        private const int CardPadding = 4;     // 패딩도 절반 권장
        private const int CardW = ThumbW + CardPadding * 2;

        // 라벨 높이도 절반 정도로 줄이기(원하면 14~18 사이)
        private const int LabelH = 34;
        private const int CardH = ThumbH + LabelH + CardPadding * 2 + 3;

        private readonly Color _cardBack = Color.FromArgb(28, 32, 38);
        private readonly Color _cardBorder = Color.FromArgb(60, 60, 60);

        // 선택은 "확실히" 흰색 + 굵게
        private readonly Color _cardSelectedBack = Color.FromArgb(35, 38, 44);
        private readonly Color _cardSelectedBorder = Color.White;

        private const int MaxItems = 4;
        private Panel _selectedCard;

        public DefectForm()
        {
            InitializeComponent();
        }

        public void AddDefectImage(Bitmap overlayBitmap, string title, string savedPath)
        {
            if (overlayBitmap == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddDefectImage(overlayBitmap, title, savedPath)));
                return;
            }

            // 썸네일 생성 + 원본 Dispose
            Bitmap thumb = new Bitmap(overlayBitmap, new Size(ThumbW, ThumbH));
            overlayBitmap.Dispose();

            // 카드(컨테이너)
            var container = new Panel
            {
                Width = CardW,
                Height = CardH,
                BackColor = _cardBack,
                Margin = new Padding(6),
                Cursor = Cursors.Hand
            };

            container.Paint += Card_Paint; // 테두리 그리기

            // 이미지
            var pic = new PictureBox
            {
                Width = ThumbW,
                Height = ThumbH,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = thumb,
                Cursor = Cursors.Hand
            };

            // 가운데 정렬(컨테이너 기준)
            // 상단 패딩 + (컨테이너 폭-썸네일 폭)/2
            int picX = (container.Width - pic.Width) / 2;
            int picY = CardPadding;
            pic.Location = new Point(picX, picY);

            // 라벨
            var lbl = new Label
            {
                AutoSize = false,
                Width = container.Width - CardPadding * 2,
                Height = LabelH,
                Left = CardPadding,
                Top = pic.Bottom + 3,
                ForeColor = Color.Gainsboro,
                Text = title ?? string.Empty,
                Cursor = Cursors.Hand,

                TextAlign = ContentAlignment.TopLeft
            };
            lbl.Font = new Font(lbl.Font.FontFamily, 8.0f, lbl.Font.Style);

            // 1클릭: 선택 표시(카드, 이미지, 라벨 어디를 눌러도 선택)
            EventHandler onSelect = (s, e) => SelectCard(container);
            container.Click += onSelect;
            pic.Click += onSelect;
            lbl.Click += onSelect;

            // 2클릭: 이미지 열기(중복 1회만!)
            MouseEventHandler onOpen = (s, e) =>
            {
                if (e.Button != MouseButtons.Left)
                    return;

                if (!string.IsNullOrWhiteSpace(savedPath) && System.IO.File.Exists(savedPath))
                {
                    System.Diagnostics.Process.Start(savedPath);
                }
            };

            container.MouseDoubleClick += onOpen;
            pic.MouseDoubleClick += onOpen;
            lbl.MouseDoubleClick += onOpen;

            container.Controls.Add(pic);
            container.Controls.Add(lbl);

            panleDefects.Controls.Add(container);

            // 오래된 항목 제거(메모리 안정화)
            while (panleDefects.Controls.Count > MaxItems)
            {
                Control old = panleDefects.Controls[0];
                panleDefects.Controls.RemoveAt(0);

                if (ReferenceEquals(old, _selectedCard))
                    _selectedCard = null;

                var oldPic = old.Controls.OfType<PictureBox>().FirstOrDefault();
                if (oldPic?.Image != null)
                {
                    oldPic.Image.Dispose();
                    oldPic.Image = null;
                }

                old.Dispose();
            }
        }

        private void SelectCard(Panel card)
        {
            if (card == null)
                return;

            if (_selectedCard != null && !_selectedCard.IsDisposed)
            {
                _selectedCard.BackColor = _cardBack;
                _selectedCard.Invalidate();
            }

            _selectedCard = card;
            _selectedCard.BackColor = _cardSelectedBack;
            _selectedCard.Invalidate();
        }

        // 패널 테두리 직접 그리기(선택 강조)
        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var card = sender as Panel;
            if (card == null)
                return;

            bool selected = ReferenceEquals(card, _selectedCard);

            Color border = selected ? _cardSelectedBorder : _cardBorder;
            int thickness = selected ? 3 : 1;

            using (var pen = new Pen(border, thickness))
            {
                Rectangle r = card.ClientRectangle;
                r.Width -= 1;
                r.Height -= 1;

                // 굵기 때문에 안쪽으로 살짝 당겨서 그리면 더 깔끔
                r.Inflate(-thickness / 2, -thickness / 2);
                e.Graphics.DrawRectangle(pen, r);
            }
        }
    }
}