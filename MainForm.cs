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
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{
    // 현재 참조 WeifenLuo.WinFormsUI.Docking, WeifenLuo.WinFormsUI.Docking.Themes.VS2015
    public partial class MainForm : Form
    {
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

            //#2_DOCKPANEL#2 DockPanel 초기화
            _dockPanel = new DockPanel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_dockPanel);

            // Visual Studio 2015 테마 적용
            _dockPanel.Theme = new VS2015BlueTheme();

            //#2_DOCKPANEL#6 도킹 윈도우 로드 메서드 호출
            LoadDockingWindows();
        }

        //#2_DOCKPANEL#5 도킹 윈도우를 로드하는 메서드
        private void LoadDockingWindows()
        {
            //도킹해제 금지 설정
            _dockPanel.AllowEndUserDocking = false;

            //메인폼 설정
            var cameraWindow = new CameraForm();
            cameraWindow.Show(_dockPanel, DockState.Document);

            //속성창 추가
            var propWindow = new PropertiesForm();
            propWindow.Show(_dockPanel, DockState.DockRight);
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
                    cameraForm.LoadImage(filePath);
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

    }
}
