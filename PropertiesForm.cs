using FreshCheck_CV.Dialogs;
using FreshCheck_CV.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace FreshCheck_CV

{
    //#3_CAMERAVIEW_PROPERTY#3 속성창에 사용할 타입 선언
    public enum PropertyType
    {
        InspOption,
        Monitor
    }

    //#2_DOCKPANEL#4 PropertiesForm 클래스 는 도킹 가능하도록 상속을 변경

    //public partial class PropertiesForm: Form
    public partial class PropertiesForm : DockContent
    {
        //#3_CAMERAVIEW_PROPERTY#4 속성탭을 관리하기 위한 딕셔너리
        Dictionary<string, TabPage> _allTabs = new Dictionary<string, TabPage>();


        private bool _isAuthorized = false; // 인증 여부 저장
        private readonly string _adminPassword = "1234"; // 설정할 비밀번호


        public PropertiesForm()
        {
            InitializeComponent();

            //TabControl (Filter / Binary) 다크화
            tabPropControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabPropControl.ItemSize = new Size(78, 30);
            tabPropControl.DrawItem += TabPropControl_DrawItem;

            tabPropControl.Paint += TabPropControl_Paint;

            // 탭 페이지(내용 영역) 배경
            foreach (TabPage page in tabPropControl.TabPages)
            {
                page.BackColor = Color.FromArgb(30, 34, 40);
                page.ForeColor = Color.White;
            }

            this.BackColor = Color.FromArgb(35, 38, 45);

            tabPropControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabPropControl.DrawItem += TabPropControl_DrawItem;

            //#3_CAMERAVIEW_PROPERTY#7 속성 탭을 초기화
            LoadOptionControl(PropertyType.InspOption);
            LoadOptionControl(PropertyType.Monitor);

            tabPropControl.Selecting += TabPropControl_Selecting;
        }

        //#3_CAMERAVIEW_PROPERTY#6 속성탭이 있다면 그것을 반환하고, 없다면 생성
        private void LoadOptionControl(PropertyType propType)
        {
            string tabName = propType.ToString();

            // 이미 있는 TabPage인지 확인
            foreach (TabPage tabPage in tabPropControl.TabPages)
            {
                if (tabPage.Text == tabName)
                    return;
            }

            // 딕셔너리에 있으면 추가
            if (_allTabs.TryGetValue(tabName, out TabPage page))
            {
                tabPropControl.TabPages.Add(page);
                return;
            }

            // 새로운 UserControl 생성
            UserControl _inspProp = CreateUserControl(propType);
            if (_inspProp == null)
                return;

            // 새 탭 추가
            TabPage newTab = new TabPage(tabName)
            {
                Dock = DockStyle.Fill
            };
            _inspProp.Dock = DockStyle.Fill;
            newTab.Controls.Add(_inspProp);
            tabPropControl.TabPages.Add(newTab);
            tabPropControl.SelectedTab = newTab; // 새 탭 선택

            _allTabs[tabName] = newTab;
        }

        //#3_CAMERAVIEW_PROPERTY# 5 속성 탭을 생성하는 메서드
        private UserControl CreateUserControl(PropertyType propType)
        {
            UserControl curProp = null;
            switch (propType)
            {
                case PropertyType.InspOption:
                    BinaryProp blobProp = new BinaryProp();
                    curProp = blobProp;
                    break;
                case PropertyType.Monitor:
                    curProp = new Property.InspectionMonitorProp();
                    break;
                default:
                    MessageBox.Show("유효하지 않은 옵션입니다.");
                    return null;
            }
            return curProp;
        }
        private void TabPropControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tab = sender as TabControl;
            Graphics g = e.Graphics;
            Rectangle rect = tab.GetTabRect(e.Index);

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color backColor = selected
                ? Color.FromArgb(50, 55, 65)
                : Color.FromArgb(35, 38, 45);

            using (SolidBrush brush = new SolidBrush(backColor))
                g.FillRectangle(brush, rect);

            TextRenderer.DrawText(
                g,
                tab.TabPages[e.Index].Text,
                tab.Font,
                rect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }
        //탭 색상변경
        private void TabPropControl_Paint(object sender, PaintEventArgs e)
        {
            TabControl tab = sender as TabControl;

            // 탭 전체 헤더 영역 크기 계산
            Rectangle headerRect = new Rectangle
            (
                0,
                0,
                tab.Width,
                tab.ItemSize.Height + 2
            );

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(35, 38, 45)))
            {
                e.Graphics.FillRectangle(brush, headerRect);
            }
        }

        private void TabPropControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // 사용자가 "InspOption" 탭(여기서는 'Scratch' 또는 'Mold' 등)을 클릭했을 때
            // 스크린샷 기준으로 "InspOption" 텍스트를 체크합니다.
            if (e.TabPage != null && e.TabPage.Text == "InspOption")
            {
                // 이미 인증되었다면 통과
                if (_isAuthorized) return;

                // 비밀번호 확인 창 띄우기
                if (ShowPasswordDialog() == _adminPassword)
                {
                    _isAuthorized = true; // 인증 성공
                }
                else
                {
                    //MessageBox.Show("등록되지 않은 관리자입니다.", "인증 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CustomMessageBoxForm.Show("등록되지 않은 관리자입니다.", "인증 실패"); // 다크모드 메시지 폼
                    e.Cancel = true; // 탭 이동 취소
                }
            }
        }

        // 간단한 비밀번호 입력용 다이얼로그
        private string ShowPasswordDialog()
        {
            using (var pwdForm = new Dialogs.PasswordCheckForm())
            {
                if (pwdForm.ShowDialog() == DialogResult.OK)
                {
                    return pwdForm.Password;
                }
            }
            return string.Empty;
        }

        public void SelectMonitorTab()
        {
            TabPage monitorTab;
            _allTabs.TryGetValue("Monitor", out monitorTab);
            tabPropControl.SelectedTab = monitorTab;
        }

    }
}