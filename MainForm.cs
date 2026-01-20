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

        private List<string> _imageFiles = new List<string>();
        private int _currentIndex = 0;
        private Timer _cycleTimer = null;
        private bool _isCycling = false;

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

        //#2_DOCKPANEL#6 쉽게 도킹패널에 접근하기 위한 정적 함수
        //제네릭 함수 사용를 이용해 입력된 타입의 폼 객체 얻기
        public static T GetDockForm<T>() where T : DockContent
        {
            var findForm = _dockPanel.Contents.OfType<T>().FirstOrDefault();
            return findForm;
        }

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
        private void LoadImageFolder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "이미지 폴더 선택";

                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                _imageFiles = Directory.GetFiles(folderDialog.SelectedPath)
                    .Where(f =>
                        f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _currentIndex = 0;

                if (_imageFiles.Count == 0)
                    MessageBox.Show("이미지 파일이 없습니다.");
            }
        }
    }
}
