using FreshCheck_CV.Core;
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
using FreshCheck_CV.Dialogs;
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{
    // 현재 참조 WeifenLuo.WinFormsUI.Docking, WeifenLuo.WinFormsUI.Docking.Themes.VS2015
    public partial class MainForm : Form
    {
        private bool _isExitConfirmed;

        //#2_DOCKPANEL#1 DockPanel을 전역으로 선언
        private static DockPanel _dockPanel;

        private readonly List<string> _imageFilePaths = new List<string>();
        private int _currentImageIndex;
        private Timer _imageCycleTimer;
        private bool _isImageCycling;

        private const int IMAGE_CYCLE_INTERVAL_MS = 500;

        public MainForm()
        {

            InitializeComponent();

            FormClosing += MainForm_FormClosing;


            {
                var testMenu = new ToolStripMenuItem("Test");
                var runMoldItem = new ToolStripMenuItem("Run Mold Inspection (temp)");

                runMoldItem.Click += (s, e) =>
                {
                    Global.Inst.InspStage.RunMoldInspectionTemp();
                };

                testMenu.DropDownItems.Add(runMoldItem);

                menuStrip1.Renderer = new DarkMenuRenderer();
                menuStrip1.BackColor = Color.FromArgb(45, 45, 48);
                menuStrip1.ForeColor = Color.White;

                foreach (ToolStripMenuItem topItem in menuStrip1.Items)
                {
                    ApplyDarkStyle(topItem);
                }


                // MenuStrip 다크
                menuStrip1.Renderer = new DarkMenuRenderer();
                menuStrip1.BackColor = Color.FromArgb(45, 45, 48);
                menuStrip1.ForeColor = Color.White;

                _dockPanel = new DockPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(28, 32, 38),
                    Theme = new VS2015DarkTheme()
                };

                Controls.Add(_dockPanel);
                _dockPanel.BringToFront();

                LoadDockingWindows();

            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 이미 확인된 종료면 그냥 통과(무한 팝업 방지)
            if (_isExitConfirmed)
                return;

            // 사용자가 창을 닫는 경우에만 확인창을 띄우고 싶으면 아래 조건 유지
            // (프로세스 종료/윈도우 로그오프 같은 상황에는 방해하지 않기 위함)
            if (e.CloseReason != CloseReason.UserClosing)
                return;

            using (var dlg = new ExitConfirmForm())
            {
                var result = dlg.ShowDialog(this);

                if (result == DialogResult.Yes)
                {
                    _isExitConfirmed = true;
                    // 여기서 Close()를 다시 호출할 필요는 없습니다.
                    // 현재 닫힘 흐름이 그대로 진행됩니다.
                    return;
                }

                // No면 종료 취소
                e.Cancel = true;
            }
        }




        //#2_DOCKPANEL#5 도킹 윈도우를 로드하는 메서드
        private void LoadDockingWindows()
        {
            _dockPanel.AllowEndUserDocking = false;

            _dockPanel.DockBottomPortion = 0.15;
            _dockPanel.DockRightPortion = 0.15;

            // 중앙 메인 이미지
            var cameraWindow = new CameraForm();
            cameraWindow.Show(_dockPanel, DockState.Document);

            // 오른쪽 상단 (Scratch / Mold 설정)
            var propWindow = new PropertiesForm();
            propWindow.Show(_dockPanel, DockState.DockRight);

            // 하단 왼쪽 (RunForm)
            var runWindow = new RunForm();
            runWindow.Show(_dockPanel, DockState.DockBottom);

            // 하단 가운데 (DefectForm)
            var defectWindow = new DefectForm();
            defectWindow.Show(runWindow.Pane, DockAlignment.Right, 0.8);

            // 하단 오른쪽 (ResultForm)
            var resultWindow = new ResultForm();
            resultWindow.Show(defectWindow.Pane, DockAlignment.Right, 0.185);
        }
        // 이미지가 들어 있는 폴더를 선택하는 메서드
        private void LoadImageFolder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "이미지 폴더 선택";
                // 폴더 선택을 취소한 경우 처리 중단
                if (folderDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                // 선택된 폴더에서 이미지 파일 목록 로드
                LoadImageFilesFromFolder(folderDialog.SelectedPath);
            }
        }
        // 선택된 폴더에서 이미지 파일들을 읽어 리스트로 구성
        private void LoadImageFilesFromFolder(string folderPath)
        {
            _imageFilePaths.Clear();

            // 지정된 확장자(.jpg, .png, .bmp)만 필터링
            IEnumerable<string> imageFiles = Directory
                .GetFiles(folderPath)
                .Where(filePath =>
                    filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase));

            _imageFilePaths.AddRange(imageFiles);
            _currentImageIndex = 0;

            if (_imageFilePaths.Count == 0)
            {
                MessageBox.Show("선택한 폴더에 이미지 파일이 없습니다.");
            }
        }
        // 현재 인덱스에 해당하는 이미지를 CameraForm에 표시
        private void LoadNextImage()
        {
            if (_imageFilePaths.Count == 0)
            {
                return;
            }
            // 현재 도킹된 CameraForm을 가져옴
            CameraForm cameraForm = GetDockForm<CameraForm>();
            if (cameraForm == null)
            {
                return;
            }
            // 현재 인덱스의 이미지 파일 로드
            string imagePath = _imageFilePaths[_currentImageIndex];
            cameraForm.LoadImage(imagePath);

            MoveToNextImageIndex();
        }
        // 이미지 인덱스를 순환 구조로 증가시키는 메서드
        private void MoveToNextImageIndex()
        {
            _currentImageIndex++;

            if (_currentImageIndex >= _imageFilePaths.Count)
            {
                _currentImageIndex = 0;
            }
        }
        // 이미지 사이클링용 타이머를 한 번만 생성하는 메서드
        private void EnsureImageCycleTimer()
        {
            if (_imageCycleTimer != null)
            {
                return;
            }

            _imageCycleTimer = new Timer
            {
                Interval = IMAGE_CYCLE_INTERVAL_MS
            };

            _imageCycleTimer.Tick += OnImageCycleTimerTick;
        }
        // 타이머 Tick 이벤트 핸들러
        private void OnImageCycleTimerTick(object sender, EventArgs e)
        {
            LoadNextImage();
        }
        // 이미지 자동 사이클링 정지
        private void StartImageCycle()
        {
            if (_imageFilePaths.Count == 0)
            {
                MessageBox.Show("먼저 이미지 폴더를 선택하세요.");
                return;
            }

            EnsureImageCycleTimer();

            _imageCycleTimer.Start();
            _isImageCycling = true;
        }

        private void StopImageCycle()
        {
            _imageCycleTimer?.Stop();
            _isImageCycling = false;
        }


        //#2_DOCKPANEL#6 쉽게 도킹패널에 접근하기 위한 정적 함수
        //제네릭 함수 사용를 이용해 입력된 타입의 폼 객체 얻기
        public static T GetDockForm<T>() where T : DockContent
        {
            var findForm = _dockPanel.Contents.OfType<T>().FirstOrDefault();
            return findForm;
        }
        // 단일 이미지 파일을 여는 메뉴 이벤트
        private void imageOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CameraForm cameraForm = GetDockForm<CameraForm>();
            if (cameraForm is null)
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "이미지 파일 선택";
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    Global.Inst.InspStage.LoadImage(filePath);

                    // 원본 저장 + 화면 표시를 InspStage에서 한 번에 책임지게 구조화
                }
            }
        }
        // 이미지 폴더 열기 메뉴 이벤트
        private void ImageFolderOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadImageFolder();
        }
        // 이미지 사이클링 시작 메뉴 이벤트
        private void StartCycleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartImageCycle();
        }
        // 이미지 사이클링 정지 메뉴 이벤트
        private void StopCycleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopImageCycle();
        }

        class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }
        }

        class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(63, 63, 70);
            public override Color MenuItemBorder => Color.FromArgb(45, 45, 48);
            public override Color MenuBorder => Color.FromArgb(45, 45, 48);
            public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
        }

        // MenuStrip 및 하위 메뉴 항목에 다크 테마 색상 적용
        private void ApplyDarkStyle(ToolStripMenuItem item)
        {
            item.ForeColor = Color.White;
            item.BackColor = Color.FromArgb(45, 45, 48);

            foreach (ToolStripItem subItem in item.DropDownItems)
            {
                subItem.ForeColor = Color.White;
                subItem.BackColor = Color.FromArgb(45, 45, 48);

                if (subItem is ToolStripMenuItem menuItem)
                {
                    ApplyDarkStyle(menuItem);
                }
            }
        }


    }
}
