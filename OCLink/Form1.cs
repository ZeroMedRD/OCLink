using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using OCLink;
using System.Data.SqlClient;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        public object MainForm { get; private set; }

        public Form1()
        {
            InitializeComponent();
        }

        class HotKey : IMessageFilter, IDisposable
        {
            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            public static extern UInt32 GlobalAddAtom(String lpString);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);

            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

            IntPtr _hWnd = IntPtr.Zero;
            UInt32 _hotKeyID;
            Keys _hotKey = Keys.None;
            Keys _comboKey = Keys.None;

            public HotKey(IntPtr formHandle, Keys hotKey, Keys comboKey)
            {
                _hWnd = formHandle; //Form Handle, 註冊系統熱鍵需要用到這個
                _hotKey = hotKey; //熱鍵
                _comboKey = comboKey; //組合鍵, 必須設定Keys.Control, Keys.Alt, Keys.Shift, Keys.None以及Keys.LWin等值才有作用

                UInt32 uint_comboKey; //由於API對於組合鍵碼的定義不一樣, 所以我們這邊做個轉換                
                switch (comboKey)
                {
                    case Keys.Alt:
                        uint_comboKey = 0x1;
                        break;
                    case Keys.Control:
                        uint_comboKey = 0x2;
                        break;
                    case Keys.Shift:
                        uint_comboKey = 0x4;
                        break;
                    case Keys.LWin:
                        uint_comboKey = 0x8;
                        break;
                    default: //沒有組合鍵
                        uint_comboKey = 0x0;
                        break;
                }

                _hotKeyID = GlobalAddAtom(Guid.NewGuid().ToString()); //向系統取得一組id
                RegisterHotKey((IntPtr)_hWnd, _hotKeyID, uint_comboKey, (UInt32)hotKey); //使用Form Handle與id註冊系統熱鍵
                Application.AddMessageFilter(this); //使用HotKey類別來監視訊息
            }

            public delegate void HotkeyEventHandler(object sender, HotKeyEventArgs e); //HotKeyEventArgs是自訂事件參數
            public event HotkeyEventHandler OnHotkey; //自訂事件

            const int WM_GLOBALHOTKEYDOWN = 0x312; //當按下系統熱鍵時, 系統會發送的訊息

            public bool PreFilterMessage(ref Message m)
            {
                if (OnHotkey != null && m.Msg == WM_GLOBALHOTKEYDOWN && (UInt32)m.WParam == _hotKeyID) //如果接收到系統熱鍵訊息且id相符時
                {
                    OnHotkey(this, new HotKeyEventArgs(_hotKey, _comboKey)); //呼叫自訂事件, 傳遞自訂參數
                    return true; //並攔截這個訊息, Form將不再接收到這個訊息
                }

                return false;
            }

            private bool disposed = false;

            public void Dispose()
            {
                if (!disposed)
                {
                    UnregisterHotKey(_hWnd, _hotKeyID); //取消熱鍵
                    GlobalDeleteAtom(_hotKeyID); //刪除id
                    OnHotkey = null; //取消所有關聯的事件
                    Application.RemoveMessageFilter(this); //不再使用HotKey類別監視訊息

                    GC.SuppressFinalize(this);
                    disposed = true;
                }
            }

            ~HotKey()
            {
                Dispose();
            }
        }

        public class HotKeyEventArgs : EventArgs
        {
            private Keys _hotKey;
            public Keys HotKey //熱鍵
            {
                get { return _hotKey; }
                private set { }
            }

            private Keys _comboKey;
            public Keys ComboKey //組合鍵
            {
                get { return _comboKey; }
                private set { }
            }

            public HotKeyEventArgs(Keys hotKey, Keys comboKey)
            {
                _hotKey = hotKey;
                _comboKey = comboKey;
            }
        }

        HotKey hotkey1, hotkey2, hotkey3, hotkey4, hotkey5, hotkey6, hotkey7, hotkey8, hotkey9, hotkey10, hotkey11, hotkey12, hotkey13, hotkey14, hotkey15;

        private void Form1_Load(object sender, EventArgs e)
        {
            hotkey1 = new HotKey(this.Handle, Keys.D1, Keys.Alt); //註冊Alt + 1為熱鍵, 如果不要組合鍵請傳Keys.None當參數
            hotkey1.OnHotkey += new HotKey.HotkeyEventHandler(hotkey1to4_OnHotkey); //hotkey1~4共用事件

            //hotkey2 = new HotKey(this.Handle, Keys.D2, Keys.Alt); //註冊Win+1為熱鍵
            //hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey5_OnHotkey); //獨立事件

            // 設定半月功能鍵
            hotkey2 = new HotKey(this.Handle, Keys.D2, Keys.Alt); //註冊Alt + 2 為熱鍵
            hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey2_OnHotkey); //獨立事件

            hotkey3 = new HotKey(this.Handle, Keys.D3, Keys.Alt); //註冊Alt + 3 為熱鍵
            hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkey3_OnHotkey); //獨立事件

            hotkey4 = new HotKey(this.Handle, Keys.D4, Keys.Alt); //註冊Alt + 4 為熱鍵
            hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkey4_OnHotkey); //獨立事件

            hotkey5 = new HotKey(this.Handle, Keys.D5, Keys.Alt); //註冊Alt + 5 為熱鍵
            hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkey5_OnHotkey); //獨立事件

            hotkey6 = new HotKey(this.Handle, Keys.D6, Keys.Alt); //註冊Alt + 6 為熱鍵
            hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkey6_OnHotkey); //獨立事件

            hotkey7 = new HotKey(this.Handle, Keys.D7, Keys.Alt); //註冊Alt + 7 為熱鍵
            hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkey7_OnHotkey); //獨立事件

            hotkey8 = new HotKey(this.Handle, Keys.D8, Keys.Alt); //註冊Alt + 8 為熱鍵
            hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkey8_OnHotkey); //獨立事件

            hotkey9 = new HotKey(this.Handle, Keys.D9, Keys.Alt); //註冊Alt + 9 為熱鍵
            hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件

            hotkey10 = new HotKey(this.Handle, Keys.D0, Keys.Alt); //註冊Alt + 0 為熱鍵
            hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkey10_OnHotkey); //獨立事件

            hotkey11 = new HotKey(this.Handle, Keys.F11, Keys.Alt); //註冊Alt + F11 為熱鍵
            hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkey11_OnHotkey); //獨立事件

            hotkey12 = new HotKey(this.Handle, Keys.F12, Keys.Alt); //註冊Alt + F12 為熱鍵
            hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkey12_OnHotkey); //獨立事件

            hotkey13 = new HotKey(this.Handle, Keys.X, Keys.Alt); //註冊Alt + X 為熱鍵
            hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkey13_OnHotkey); //獨立事件

            hotkey14 = new HotKey(this.Handle, Keys.Q, Keys.Alt); //註冊Alt + Q 為熱鍵
            hotkey14.OnHotkey += new HotKey.HotkeyEventHandler(hotkey14_OnHotkey); //獨立事件

            hotkey15 = new HotKey(this.Handle, Keys.F9, Keys.None); //註冊 F9 為熱鍵
            hotkey15.OnHotkey += new HotKey.HotkeyEventHandler(hotkey15_OnHotkey); //獨立事件

        }
        private void hotkey1to4_OnHotkey(object sender, HotKeyEventArgs e)
        {
            if (!prog_flag)
            {
                this.Show();
                prog_flag = true;
            }
            else
            {
                this.Hide();
                notifyIcon1.ShowBalloonTip(5000);
                prog_flag = false;
            }
            // MessageBox.Show("熱鍵" + e.ComboKey.ToString() + "+" + e.HotKey.ToString() + "被觸發了!", "共用事件");
        }

        private void hotkey2_OnHotkey(object sender, HotKeyEventArgs e)
        {
            //this.Hide();
            //notifyIcon1.ShowBalloonTip(10000);

            openBrowser(2);
        }

        private void hotkey3_OnHotkey(object sender, HotKeyEventArgs e)
        {
            //this.Hide();
            //notifyIcon1.ShowBalloonTip(10000);

            openBrowser(3);
        }

        private void hotkey4_OnHotkey(object sender, HotKeyEventArgs e)
        {
            //this.Hide();
            //notifyIcon1.ShowBalloonTip(10000);

            openBrowser(4);
        }
        void openBrowser(int nCase)
        {


            // 打開愛半月網頁
            string url = string.Empty;
            //string url1 = "?hospital=" + ConfigurationManager.AppSettings["HospID"] + "&dr=" + sDrID.Trim() + "&patientno=" + patientno + "&patientname=" + name + "&patientIdno=" + patientIdno + "&patientBirth=" + patientBirth + "&tel=" + tel;
            string url1 = "?hospital=" + sHospID + "&dr=" + sDrID.Trim() + "&patientno=" + patientno + "&patientname=" + name + "&patientIdno=" + patientIdno + "&patientBirth=" + patientBirth + "&tel=" + tel;

            //MessageBox.Show(url1);

            switch (nCase)
            {
                case 1:
                    break;
                case 2:     // 檢驗病歷                        
                    url = "https://www.weightobserver.com.tw:8443/hisBodyMeasure" + url1 + " https://www.weightobserver.com.tw:8443/hisLaboratory" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 3:     // 檢查報告
                    url = "https://www.weightobserver.com.tw:8443/hisCustomTable" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 4:     // 個管問診
                    url = "https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 5:     // 量測資料
                    url = "https://www.weightobserver.com.tw:8443/hisBodyMeasure" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 6:     // 預約作業
                    url = "https://www.weightobserver.com.tw:8443/his" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 7:     // 檢驗預約
                    url = "https://www.weightobserver.com.tw:8443/hisLabAppoint" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 8:     // 關懷記錄
                    url = "https://www.weightobserver.com.tw:8443/hisWorkRecord" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 9:     // 圖形管理
                    url = "https://www.weightobserver.com.tw:8443/hisImageManage" + url1 + " https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 10:
                    url = "https://www.weightobserver.com.tw:8443/his" + url1;
                    break;
                case 11:
                    break;
                case 12:
                    //string lisurl = "HospId=" + ConfigurationManager.AppSettings["LisID"] + "&LoginId=" + ConfigurationManager.AppSettings["HospID"] + "&LoginPWS=" + ConfigurationManager.AppSettings["LoginPWS"] + "&PID=" + patientIdno;
                    string lisurl = "HospId=" + ConfigurationManager.AppSettings["LisID"] + "&LoginId=" + sHospID + "&LoginPWS=" + ConfigurationManager.AppSettings["LoginPWS"] + "&PID=" + patientIdno;
                    url = "http://59.124.156.61/Reports/Account/Report_List.aspx?" + lisurl;
                    break;
                case 13:
                    break;
                case 14:
                    //url = "http://65.52.165.109:2031/ClinicAppointment?HospID=" + ConfigurationManager.AppSettings["HospID"] + "&PatientIdentity=" + patientIdno + "&PatientName=" + name;
                    url = "http://65.52.165.109:2031/ClinicAppointment?HospID=" + sHospID + "&PatientIdentity=" + patientIdno + "&PatientName=" + name;
                    break;
                case 15:
                    break;
            }

            this.Hide();
            notifyIcon1.ShowBalloonTip(5000);
            prog_flag = false;

            gotoSite(url);

        }
        public void gotoSite(string url)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "chrome.exe";
            proc.StartInfo.Arguments = url + " --new-window --start-maximized";   // --incognito";
            proc.Start();
        }
    }
}
