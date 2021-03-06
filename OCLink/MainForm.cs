﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Tesseract;
using OCLink.Models;
using System.Data.OleDb;
using ZMLib;
using System.Deployment.Application;

namespace OCLink
{
    public partial class MainForm : Form
    {
        public string Server;//住機位置
        public string testhis;//判斷資料庫
        public string function1111;//所選項目
        public bool prog_flag;//顯現視窗判斷
        public string hisid;//醫事機構代碼
        public string hospname;//診所名稱
        public string name;//病人姓名
        public string Birth;//病人生日
        public string tel;//病人電話
        public string Cell;//病人手機
        public string ID;//病人身分證
        public string sHospRowid;//資訊面板
        public string drId = "1111";//取得登錄者帳號
        public string sqlIP;//從form3取值
        string strconn65 = "65.52.165.109,1933";
        string strconnUser = "sa";
        string strconnpw = "I@ntif@t;";
        string strconn23 = "23.97.65.134,1933";
        public string StrValueIP //form3傳值
        {
            set
            {
                sqlIP = value;
            }
        }
        public string sqldatabace;
        public string StrValuedb //form3傳值
        {
            set
            {
                sqldatabace = value;
            }
        }
        public string sqlID;
        public string StrValueid //form3傳值
        {
            set
            {
                sqlID = value;
            }
        }
        public string sqlpw;
        public string StrValuepw //form3傳值
        {
            set
            {
                sqlpw = value;
            }
        }

        #region 設定資料庫連線
        //string strConn = "data source=23.97.65.134,1933;Initial Catalog={0};Integrated Security=False;user id=sa;password=I@ntif@t;";

        #endregion

        string publishVersion;//版本編號
        Stopwatch sw = new System.Diagnostics.Stopwatch();                      //程式計時器
        ArrayList MyMacAddress = new ArrayList();                               //本機地址
        public bool bCheckID = false;                                           //核對身分後確認是否開啟程式
        Dictionary<string, string> dMAC = new Dictionary<string, string>();     //IPMAC位置,診所
        //private his3532040438Entities db_0438 = new his3532040438Entities();    //測試用抓oy的目前用在開蘭那
        ZMClass myClass = new ZMClass();                                        //引用函式庫判斷是否為數字

        class HotKey : IMessageFilter, IDisposable                              //註冊熱鍵程式(一般都不需要更改)
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

            public delegate void HotkeyEventHandler(object sender, HotKeyEventArgs e);      //HotKeyEventArgs是自訂事件參數
            public event HotkeyEventHandler OnHotkey;                                       //自訂事件
            const int WM_GLOBALHOTKEYDOWN = 0x312;                                          //當按下系統熱鍵時, 系統會發送的訊息

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
                _hotKeyID = GlobalAddAtom(Guid.NewGuid().ToString());                       //向系統取得一組id
                RegisterHotKey((IntPtr)_hWnd, _hotKeyID, uint_comboKey, (UInt32)hotKey);    //使用Form Handle與id註冊系統熱鍵
                Application.AddMessageFilter(this);                                         //使用HotKey類別來監視訊息
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (OnHotkey != null && m.Msg == WM_GLOBALHOTKEYDOWN && (UInt32)m.WParam == _hotKeyID)  //如果接收到系統熱鍵訊息且id相符時
                {
                    OnHotkey(this, new HotKeyEventArgs(_hotKey, _comboKey));                            //呼叫自訂事件, 傳遞自訂參數
                    return true;                                                                        //並攔截這個訊息, Form將不再接收到這個訊息
                }
                return false;
            }

            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    UnregisterHotKey(_hWnd, _hotKeyID);     //取消熱鍵
                    GlobalDeleteAtom(_hotKeyID);            //刪除id
                    OnHotkey = null;                        //取消所有關聯的事件
                    Application.RemoveMessageFilter(this);  //不再使用HotKey類別監視訊息

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

        HotKey hotkey1, hotkey2, hotkey3, hotkey4, hotkey5, hotkey6, hotkey7, hotkey8, hotkey9, hotkey10, hotkey11, hotkey12, hotkey13;//這邊就是我註冊的熱鍵(共13個)

        public int nRed = 0, nGreen = 0, nBlue = 0, nResize = 6;
        //public object Pane2 { get; private set; }
        
        [Flags]
        public enum KeyModifiers
        {
            //定義熱鍵值字串(熱鍵值是系統規定的，不能改變)
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }

        public void setdata()
        {
            string strCon1 = "server=" + strconn65 +"; database=OPDBoard;user=" + strconnUser + ";password=" + strconnpw;
            using (SqlConnection conn = new SqlConnection(strCon1))
            {
                conn.Open();
                try
                {
                    string sqlstr = "select * from InfoPanel where IPCBDCode = 'E'";//2021/4/23更改
                    funCommand(sqlstr, conn, 1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString() + "SQL 1 ");
                }
                foreach (string s in MyMacAddress)
                {
                    if (true == (dMAC.ContainsKey(s)))
                    {
                        sHospRowid = dMAC[s];       //透過MacAddress找到對應的診所Rowid
                        bCheckID = true;
                    }
                }
                string strCon2 = "server=" +strconn65 +";database=ZMCMS;user=" +strconnUser +";password=" + strconnpw;
                SqlConnection conn1 = new SqlConnection(strCon2);
                conn1.Open();
                try
                {
                    if (bCheckID == true) //select sh.HospID, sh.HospName from SysHospital sh where sh.HospRowid = '9E10E8FE-9D95-4519-942A-0DCC91C54171'
                    {
                        //string sqlstr4 = "select sh.HospRowid , sh.HospID , sh.HospName , shc.HCRClinicRoom, combo.CBDDescription , combo.CBDDisplayOrder, shc.HCRRowid from SysHospital sh inner join SysHospitalClinicRoom shc on sh.HospRowid = shc.HospRowid left join (select cd.CBDCode,cd.CBDDescription,cd.CBDDisplayOrder from ComboMaster cm inner join ComboDetail cd on cm.CBMRowid = cd.CBMRowid where cm.CBMClass = 'CLINICROOM') as combo on shc.HCRClinicRoom = combo.CBDCode where sh.HospRowid = '" + sHospRowid + "' order by combo.CBDDisplayOrder ";
                        string sqlstr4 = "select sh.HospID, sh.HospName from SysHospital sh where sh.HospRowid = '" + sHospRowid +"'";
                        funCommand(sqlstr4, conn1, 2);//診所名
                    }
                    else
                    {
                        //MessageBox.Show("您的裝置未經授權，請洽相關業務人員\n\n聯絡電話：(03) 4636913");沒有綁MACaddress 跳出
                        //System.Environment.Exit(0); //若認證不過關閉所有程式
                        string mac = MyMacAddress[0].ToString();
                        Form2 f2 = new Form2(mac);
                        f2.FormClosing += new FormClosingEventHandler(f2_FormClosing);//沒有綁MACaddress 跳出序號頁面
                        this.Hide();
                        f2.ShowDialog();
                    }
                }
                catch (Exception ex)  // 使用 Exception
                {
                    MessageBox.Show(ex + "setdata");
                    System.Environment.Exit(0);
                }
            }
        }


        public MainForm()
        {
            InitializeComponent();
            getLocalMacAddress();//抓MAC
            int dwFlag = new int();//判斷網路是否正常
            if (!InternetGetConnectedState(ref dwFlag, 0))
            {
                MessageBox.Show("無法與網際網路連線 請確認網路連線品質是否良好");
                Environment.Exit(0); //關閉所有程式
            }
            setdata();//這個是要抓form標題和安全機制
            hotkey();//一開使預設的熱鍵內容
            this.MaximizeBox = false;//關閉放大紐
            this.MinimizeBox = false;//關閉縮小紐
            prog_flag = true;
            this.TopMost = true;//程式至頂
        }

        private string strValue;
        public string StrValue //mainform form1之間傳值
        {
            set
            {
                strValue = value;
            }
        }
        [DllImport("user32.dll")]
        private static extern Int32 GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern Int32 GetWindowText(Int32 hWnd, StringBuilder lpsb, Int32 count);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName,string lpKeyName,string lpDefault,StringBuilder lpReturnedString,uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]static extern bool WritePrivateProfileString(string lpAppName,string lpKeyName, string lpString, string lpFileName);

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);//用來測試網路是否可以用

        public void getLocalMacAddress()
        {   // 因為電腦中可能有很多的網卡(包含虛擬的網卡)，存入所有的設備號碼
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)   //抓取所有的設備實體位置
            MyMacAddress.Add(nic.GetPhysicalAddress().ToString());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tresize.Visible = false;
            tred.Visible = false;
            tgreen.Visible = false;
            tblue.Visible = false;
            tbResize.Visible = false;
            tbRed.Visible = false;
            tbGreen.Visible = false;
            tbBlue.Visible = false;

            panel1.Visible = false;//把功能版隱藏
            panel2.Visible = false;
            this.Height = 138;

            hotkey1 = new HotKey(this.Handle, Keys.Q, Keys.Alt); //註冊Alt + Q為熱鍵, 如果不要組合鍵請傳Keys.None當參數
            hotkey1.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //功能是程式放大縮小


            if (!Directory.Exists(@"C:\ZMTemp"))//判斷C:\ZMTemp有沒有  沒有就新增
            {
                Directory.CreateDirectory(@"C:\ZMTemp");
            }

            string strCon = @"data source=" + strconn65 +";initial catalog= ZMCMS;user id=" + strconnUser+";password=" + strconnpw;//下方的部分是要抓先前使用者紀錄來保留個人化設定
            using (SqlConnection conn = new SqlConnection(strCon))//連線SQL資料到conn
            {
                try
                {
                    #region 在 Combo 裡取得 F1-F12 的下拉選單需要顯示的功能項目
                    string sql = "select cd.CBDCode,cd.CBDDescription from ComboMaster cm join ComboDetail cd on cm.CBMRowid = cd.CBMRowid where cm.CBMClass = 'OCL' order by cd.CBDDisplayOrder";
                    //SqlCommand cmd = new SqlCommand(sql, conn);//執行SQL來源到cmd
                    //SqlDataReader sqlDataReader = cmd.ExecuteReader();//擷取資料
                    SqlDataAdapter sda = new SqlDataAdapter(sql, conn);
                    //DataSet ds = new DataSet();
                    DataTable dt = new DataTable();       //創一個空間dt             
                    DataTable dt1 = new DataTable();
                    DataTable dt2 = new DataTable();
                    DataTable dt3 = new DataTable();
                    DataTable dt4 = new DataTable();
                    DataTable dt5 = new DataTable();
                    DataTable dt6 = new DataTable();
                    DataTable dt7 = new DataTable();
                    DataTable dt8 = new DataTable();
                    DataTable dt9 = new DataTable();
                    DataTable dt10 = new DataTable();
                    DataTable dt11 = new DataTable();
                    DataTable dt12 = new DataTable();
                    sda.Fill(dt); //創一個空間把sda放到dt裡面 
                    sda.Fill(dt1);
                    sda.Fill(dt2);
                    sda.Fill(dt3);
                    sda.Fill(dt4);
                    sda.Fill(dt5);
                    sda.Fill(dt6);
                    sda.Fill(dt7);
                    sda.Fill(dt8);
                    sda.Fill(dt9);
                    sda.Fill(dt10);
                    sda.Fill(dt11);
                    sda.Fill(dt12);
                    #endregion

                    //以https://social.msdn.microsoft.com/Forums/zh-TW/a3f15a39-c021-4f20-a120-a783a36b67ca/c-29992-getprivateprofilestring-24460-writeprivateprofilestring?forum=233參考
                    //讀取參數
                    #region 設定下拉選單的內容並且若是沒選項目則必需把 Hostkey 給 Dispose
                    #region Hot Key F1 : hotkey2
                    StringBuilder sb = new StringBuilder(500);
                    uint res1 = GetPrivateProfileString("AppName", "F1", "", sb, (uint)sb.Capacity, test);
                    comboBox1.DisplayMember = "CBDDescription";
                    comboBox1.ValueMember = "CBDCode";//這個是要抓選項後面設定的網址
                    comboBox1.DataSource = dt;
                    comboBox1.Text = sb.ToString();
                    if(sb.ToString() =="")
                    {
                        hotkey2.Dispose();//如果沒設定功能會註銷熱鍵以免會跟其他的HIS的快捷鍵衝突
                    }
                    #endregion

                    #region Hot Key F2 : hotkey3
                    uint res2 = GetPrivateProfileString("AppName", "F2", "", sb, (uint)sb.Capacity, test);
                    comboBox2.DisplayMember = "CBDDescription";
                    comboBox2.ValueMember = "CBDCode";
                    comboBox2.DataSource = dt1;
                    comboBox2.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey3.Dispose();
                    }
                    #endregion

                    #region Hot Key F3 : hotkey4
                    uint res3 = GetPrivateProfileString("AppName", "F3", "", sb, (uint)sb.Capacity, test);
                    comboBox3.DisplayMember = "CBDDescription";
                    comboBox3.ValueMember = "CBDCode";
                    comboBox3.DataSource = dt2;
                    comboBox3.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey4.Dispose();
                    }
                    #endregion

                    #region Hot Key F4 : hotkey5
                    uint res4 = GetPrivateProfileString("AppName", "F4", "", sb, (uint)sb.Capacity, test);
                    comboBox4.DisplayMember = "CBDDescription";
                    comboBox4.ValueMember = "CBDCode";
                    comboBox4.DataSource = dt3;
                    comboBox4.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey5.Dispose();
                    }
                    #endregion

                    #region Hot Key F5 : hotkey6
                    uint reg5 = GetPrivateProfileString("AppName", "F5", "", sb, (uint)sb.Capacity, test);
                    comboBox5.DisplayMember = "CBDDescription";
                    comboBox5.ValueMember = "CBDCode";
                    comboBox5.DataSource = dt4;
                    comboBox5.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey6.Dispose();
                    }
                    #endregion

                    #region Hot Key F6 : hotkey7
                    uint reg6 = GetPrivateProfileString("AppName", "F6", "", sb, (uint)sb.Capacity, test);
                    comboBox6.DisplayMember = "CBDDescription";
                    comboBox6.ValueMember = "CBDCode";
                    comboBox6.DataSource = dt5;
                    comboBox6.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey7.Dispose();
                    }
                    #endregion

                    #region Hot Key F7 : hotkey8
                    uint reg7 = GetPrivateProfileString("AppName", "F7", "", sb, (uint)sb.Capacity, test);
                    comboBox7.DisplayMember = "CBDDescription";
                    comboBox7.ValueMember = "CBDCode";
                    comboBox7.DataSource = dt6;
                    comboBox7.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey8.Dispose();
                    }
                    #endregion

                    #region Hot Key F8 : hotkey9
                    uint reg8 = GetPrivateProfileString("AppName", "F8", "", sb, (uint)sb.Capacity, test);
                    comboBox8.DisplayMember = "CBDDescription";
                    comboBox8.ValueMember = "CBDCode";
                    comboBox8.DataSource = dt7;
                    comboBox8.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey9.Dispose();
                    }
                    #endregion

                    #region Hot Key F9 : hotkey10
                    uint reg9 = GetPrivateProfileString("AppName", "F9", "", sb, (uint)sb.Capacity, test);
                    comboBox9.DisplayMember = "CBDDescription";
                    comboBox9.ValueMember = "CBDCode";
                    comboBox9.DataSource = dt8;
                    comboBox9.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey10.Dispose();
                    }
                    #endregion

                    #region Hot Key F10 : hotkey11
                    uint reg10 = GetPrivateProfileString("AppName", "F10", "", sb, (uint)sb.Capacity, test);
                    comboBox10.DisplayMember = "CBDDescription";
                    comboBox10.ValueMember = "CBDCode";
                    comboBox10.DataSource = dt9;
                    comboBox10.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey11.Dispose();
                    }
                    #endregion

                    #region Hot Key F11 : hotkey12
                    uint reg11 = GetPrivateProfileString("AppName", "F11", "", sb, (uint)sb.Capacity, test);
                    comboBox11.DisplayMember = "CBDDescription";
                    comboBox11.ValueMember = "CBDCode";
                    comboBox11.DataSource = dt10;
                    comboBox11.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey12.Dispose();
                    }
                    #endregion

                    #region Hot Key F12 : hotkey13
                    uint reg12 = GetPrivateProfileString("AppName", "F12", "", sb, (uint)sb.Capacity, test);
                    comboBox12.DisplayMember = "CBDDescription";
                    comboBox12.ValueMember = "CBDCode";
                    comboBox12.DataSource = dt11;
                    comboBox12.Text = sb.ToString();
                    if (sb.ToString() == "")
                    {
                        hotkey13.Dispose();
                    }
                    #endregion
                    #endregion

                    #region 依據外部 System.ini 取得 Ctrl、Shift、Alt及其它相關參數並顯示在相對應的View上面
                    uint reg13 = GetPrivateProfileString("AppName", "ctrl", "", sb, (uint)sb.Capacity, test);
                    if ((sb.ToString()) == "True")//判斷是否要勾CTRL等等
                        checkBox1.Checked = true;

                    uint reg14 = GetPrivateProfileString("AppName", "shift", "", sb, (uint)sb.Capacity, test);
                    if ((sb.ToString()) == "True")
                        checkBox2.Checked = true;

                    uint reg15 = GetPrivateProfileString("AppName", "alt", "", sb, (uint)sb.Capacity, test);
                    if (sb.ToString() == "True")
                        checkBox3.Checked = true;

                    uint reg16 = GetPrivateProfileString("AppName", "Server", "", sb, (uint)sb.Capacity, test);
                    comboBox14.Text = sb.ToString();//面板上的主機位置

                    uint reg17 = GetPrivateProfileString("AppName", "His", "", sb, (uint)sb.Capacity, test);
                    comboBox13.Text = sb.ToString();//確認是用哪個HIS

                    uint reg18 = GetPrivateProfileString("AppName", "Resize", "", sb, (uint)sb.Capacity, test);
                    tbResize.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg19 = GetPrivateProfileString("AppName", "R", "", sb, (uint)sb.Capacity, test);
                    tbRed.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg20 = GetPrivateProfileString("AppName", "G", "", sb, (uint)sb.Capacity, test);
                    tbGreen.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg21 = GetPrivateProfileString("AppName", "B", "", sb, (uint)sb.Capacity, test);
                    tbBlue.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg22 = GetPrivateProfileString("AppName", "resize_office", "", sb, (uint)sb.Capacity, test);
                    tresize.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg23 = GetPrivateProfileString("AppName", "r_office", "", sb, (uint)sb.Capacity, test);
                    tred.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg24 = GetPrivateProfileString("AppName", "g_office", "", sb, (uint)sb.Capacity, test);
                    tgreen.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg25 = GetPrivateProfileString("AppName", "b_office", "", sb, (uint)sb.Capacity, test);
                    tblue.Text = sb.ToString();//抓掛號的放大RBG設定

                    uint reg26 = GetPrivateProfileString("AppName", "Enabled", "", sb, (uint)sb.Capacity, test);
                    if(sb.ToString() == "True")
                    {
                        btn_OCR.Enabled = true;//防災機制如果沒截圖OCR紐不給按
                    }
                    
                    uint reg27 = GetPrivateProfileString("AppName", "登入帳號", "", sb, (uint)sb.Capacity, test);
                    drId = sb.ToString();
                    this.Text = this.Text + "_" + drId;

                    uint reg28 = GetPrivateProfileString("AppName", "controll", "", sb, (uint)sb.Capacity, test);
                    comboBox15.Text = sb.ToString();//紀錄視窗放大縮小快捷鍵

                    uint reg29 = GetPrivateProfileString("AppName", "number", "", sb, (uint)sb.Capacity, test);
                    comboBox16.Text = sb.ToString();//紀錄視窗放大縮小快捷鍵
                    UserHotkey();

                    uint reg30 = GetPrivateProfileString("AppName", "展望資料夾位置", "", sb, (uint)sb.Capacity, test);
                    textBox1.Text = sb.ToString();

                    uint reg31= GetPrivateProfileString("AppName", "sqlIP", "", sb, (uint)sb.Capacity, test);
                    sqlIP = sb.ToString();//這是方頂要輸入資料庫登入資料

                    uint reg32 = GetPrivateProfileString("AppName", "sql資料庫", "", sb, (uint)sb.Capacity, test);
                    sqldatabace = sb.ToString();//這是方頂要輸入資料庫登入資料

                    uint reg33= GetPrivateProfileString("AppName", "sql帳號", "", sb, (uint)sb.Capacity, test);
                    sqlID = sb.ToString();//這是方頂要輸入資料庫登入資料

                    uint reg34= GetPrivateProfileString("AppName", "sql密碼", "", sb, (uint)sb.Capacity, test);
                    sqlpw = sb.ToString();//這是方頂要輸入資料庫登入資料

                    uint reg35 = GetPrivateProfileString("AppName", "Enableddb", "", sb, (uint)sb.Capacity, test);
                    if (sb.ToString() == "True")
                    {
                        button3.Enabled = true;//展望切換資料庫鈕
                    }
                    uint reg36 = GetPrivateProfileString("AppName", "REG", "", sb, (uint)sb.Capacity, test);
                    if(sb.ToString() == "")
                    {
                        MessageBox.Show("請設定掛號室截圖路徑 避免連結錯誤");
                    }
                    uint reg37 = GetPrivateProfileString("AppName", "OPD", "", sb, (uint)sb.Capacity, test);
                    uint reg38 = GetPrivateProfileString("AppName", "病歷號", "", sb, (uint)sb.Capacity, test);
                    if(sb.ToString() =="True")
                    {
                        checkBox4.Checked = true;
                    }
                    uint reg39 = GetPrivateProfileString("AppName", "身份證", "", sb, (uint)sb.Capacity, test);
                    if (sb.ToString() == "True")
                    {
                        checkBox6.Checked = true;
                    }
                    if (sb.ToString() == "")
                    {
                        MessageBox.Show("請設定診間截圖路徑 避免連結錯誤");
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SQL error ," + ex.ToString());//回報錯誤
                }
            }
            // 下列RGB參數有一個是空白則一律帶預設值
            //if(tbResize.Text == "" || tbRed.Text == "" || tbGreen.Text == "" || tbBlue.Text =="")
            //{
            //    tbResize.Text = "1";
            //    tbRed.Text = "50";
            //    tbGreen.Text = "50";
            //    tbBlue.Text = "50";
            //}
            //if(tresize.Text == "" || tred.Text == "" || tgreen.Text == "" || tblue.Text == "")
            //{
            //    tresize.Text = "1";
            //    tred.Text = "40";
            //    tgreen.Text = "40";
            //    tblue.Text = "40";
            //}
        }
        
        private bool hotkeycheck = true;//判斷快捷鍵截圖是否要messagebox顯現

        #region hotkey() -- 設並註冊功能鍵
        void hotkey() // 設定功能鍵
        {
            // 設定 F1~F12 Key 的內容
            // 此內容皆在後台做維護,不再由App來做選擇更新,以達一鍵連結的統一性
            // 設定資料皆在 HotKey4OCLink Table 做取得
            // 全部診所皆在此依診所別在此 Table 取得一鍵連結相關資訊

            hotkey2 = new HotKey(this.Handle, Keys.F1, Keys.None); //註冊 F1 為熱鍵
            hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF1); //獨立事件 
            hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey2_OnHotkey); //獨立事件 

            hotkey3 = new HotKey(this.Handle, Keys.F2, Keys.None); //註冊 F2 為熱鍵
            hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF2); //獨立事件
            hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkey3_OnHotkey); //獨立事件

            hotkey4 = new HotKey(this.Handle, Keys.F3, Keys.None); //註冊 F3 為熱鍵
            hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF3); //獨立事件
            hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkey4_OnHotkey); //獨立事件

            hotkey5 = new HotKey(this.Handle, Keys.F4, Keys.None); //註冊 F4 為熱鍵
            hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF4); //獨立事件
            hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkey5_OnHotkey); //獨立事件

            hotkey6 = new HotKey(this.Handle, Keys.F5, Keys.None); //註冊 F5 為熱鍵
            hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF5); //獨立事件
            hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkey6_OnHotkey); //獨立事件

            hotkey7 = new HotKey(this.Handle, Keys.F6, Keys.None); //註冊 F6 為熱鍵
            hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF6); //獨立事件
            hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkey7_OnHotkey); //獨立事件

            hotkey8 = new HotKey(this.Handle, Keys.F7, Keys.None); //註冊F7 為熱鍵
            hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF7); //獨立事件
            hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkey8_OnHotkey); //獨立事件

            hotkey9 = new HotKey(this.Handle, Keys.F8, Keys.None); //註冊 F8 為熱鍵
            hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF8); //獨立事件
            hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件

            hotkey10 = new HotKey(this.Handle, Keys.F9, Keys.None); //註冊 F9 為熱鍵
            hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF9); //獨立事件
            hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkey10_OnHotkey); //獨立事件

            hotkey11 = new HotKey(this.Handle, Keys.F10, Keys.None); //註冊 F10 為熱鍵
            hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF10); //獨立事件
            hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkey11_OnHotkey); //獨立事件

            hotkey12 = new HotKey(this.Handle, Keys.F11, Keys.None); //註冊 F11 為熱鍵
            hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF11); //獨立事件
            hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkey12_OnHotkey); //獨立事件

            hotkey13 = new HotKey(this.Handle, Keys.F12, Keys.None); //註冊 F12 為熱鍵
            hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF12); //獨立事件
            hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
            hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkey13_OnHotkey); //獨立事件
        }
        #endregion

        #region hotkeydispose( -- 清除熱鍵資訊
        void hotkeydispose()//清除熱鍵資訊
        {
            hotkey2.Dispose();
            hotkey3.Dispose();
            hotkey4.Dispose();
            hotkey5.Dispose();
            hotkey6.Dispose();
            hotkey7.Dispose();
            hotkey8.Dispose();
            hotkey9.Dispose();
            hotkey10.Dispose();
            hotkey11.Dispose();
            hotkey12.Dispose();
            hotkey13.Dispose();
        }
        #endregion

        void hotkeycas() // 設定功能鍵(新增Control Alt shift鍵)
        {
            if (bck_box1 == true && bck_box2 ==false && bck_box3 == false)
            {
                hotkey2 = new HotKey(this.Handle, Keys.F1, Keys.Control); //註冊 Control+F1 為熱鍵

                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey2_OnHotkey); //獨立事件

                hotkey3 = new HotKey(this.Handle, Keys.F2, Keys.Control); //註冊 Control+F2 為熱鍵
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF2); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkey3_OnHotkey); //獨立事件

                hotkey4 = new HotKey(this.Handle, Keys.F3, Keys.Control); //註冊 Control+F3 為熱鍵
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF3); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkey4_OnHotkey); //獨立事件

                hotkey5 = new HotKey(this.Handle, Keys.F4, Keys.Control); //註冊 Control+F4 為熱鍵
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF4); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkey5_OnHotkey); //獨立事件

                hotkey6 = new HotKey(this.Handle, Keys.F5, Keys.Control); //註冊 Control+F5 為熱鍵
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF5); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkey6_OnHotkey); //獨立事件

                hotkey7 = new HotKey(this.Handle, Keys.F6, Keys.Control); //註冊 Control+F6 為熱鍵
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF6); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkey7_OnHotkey); //獨立事件

                hotkey8 = new HotKey(this.Handle, Keys.F7, Keys.Control); //註冊 Control+F7 為熱鍵
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF7); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkey8_OnHotkey); //獨立事件

                hotkey9 = new HotKey(this.Handle, Keys.F8, Keys.Control); //註冊 Control+F8 為熱鍵
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF8); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件

                hotkey10 = new HotKey(this.Handle, Keys.F9, Keys.Control); //註冊 Control+F9 為熱鍵
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF9); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkey10_OnHotkey); //獨立事件

                hotkey11 = new HotKey(this.Handle, Keys.F10, Keys.Control); //註冊 Control+F10 為熱鍵
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF10); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkey11_OnHotkey); //獨立事件

                hotkey12 = new HotKey(this.Handle, Keys.F11, Keys.Control); //註冊 Control+F11 為熱鍵
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF11); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkey12_OnHotkey); //獨立事件

                hotkey13 = new HotKey(this.Handle, Keys.F12, Keys.Control); //註冊 Control+F12 為熱鍵
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF12); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkey13_OnHotkey); //獨立事件
            }
            else if (bck_box2 == true && bck_box1 == false && bck_box3 == false)
            {
                hotkey2 = new HotKey(this.Handle, Keys.F1, Keys.Alt); //註冊 Alt+F1 為熱鍵
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey2_OnHotkey); //獨立事件

                hotkey3 = new HotKey(this.Handle, Keys.F2, Keys.Alt); //註冊 Alt+F2 為熱鍵
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF2); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkey3_OnHotkey); //獨立事件

                hotkey4 = new HotKey(this.Handle, Keys.F3, Keys.Alt); //註冊 Alt+F3 為熱鍵
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF3); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkey4_OnHotkey); //獨立事件

                hotkey5 = new HotKey(this.Handle, Keys.F4, Keys.Alt); //註冊 Alt+F4 為熱鍵
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF4); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkey5_OnHotkey); //獨立事件

                hotkey6 = new HotKey(this.Handle, Keys.F5, Keys.Alt); //註冊 Alt+F5 為熱鍵
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF5); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkey6_OnHotkey); //獨立事件

                hotkey7 = new HotKey(this.Handle, Keys.F6, Keys.Alt); //註冊 Alt+F6 為熱鍵
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF6); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkey6_OnHotkey); //獨立事件

                hotkey8 = new HotKey(this.Handle, Keys.F7, Keys.Alt); //註冊 Alt+F7 為熱鍵
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF7); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkey8_OnHotkey); //獨立事件

                hotkey9 = new HotKey(this.Handle, Keys.F8, Keys.Alt); //註冊 Alt+F8 為熱鍵
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF8); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件

                hotkey10 = new HotKey(this.Handle, Keys.F9, Keys.Alt); //註冊 Alt+F9 為熱鍵
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF9); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkey10_OnHotkey); //獨立事件

                hotkey11 = new HotKey(this.Handle, Keys.F10, Keys.Alt); //註冊 Alt+F10 為熱鍵
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF10); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkey11_OnHotkey); //獨立事件

                hotkey12 = new HotKey(this.Handle, Keys.F11, Keys.Alt); //註冊 Alt+F11 為熱鍵
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF11); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkey12_OnHotkey); //獨立事件

                hotkey13 = new HotKey(this.Handle, Keys.F12, Keys.Alt); //註冊 Alt+F12 為熱鍵
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF12); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkey13_OnHotkey); //獨立事件
            }
            else if (bck_box3 == true && bck_box1 == false && bck_box2 == false)
            {
                hotkey2 = new HotKey(this.Handle, Keys.F1, Keys.Shift); //註冊 Shift+F1 為熱鍵
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey2_OnHotkey); //獨立事件

                hotkey3 = new HotKey(this.Handle, Keys.F2, Keys.Shift); //註冊 Shift+F2 為熱鍵
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF2); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkey3_OnHotkey); //獨立事件

                hotkey4 = new HotKey(this.Handle, Keys.F3, Keys.Shift); //註冊 Shift+F3 為熱鍵
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF3); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkey4_OnHotkey); //獨立事件

                hotkey5 = new HotKey(this.Handle, Keys.F4, Keys.Shift); //註冊 Shift+F4 為熱鍵
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF4); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkey5_OnHotkey); //獨立事件

                hotkey6 = new HotKey(this.Handle, Keys.F5, Keys.Shift); //註冊 Shift+F5 為熱鍵
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF5); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件\
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkey6_OnHotkey); //獨立事件

                hotkey7 = new HotKey(this.Handle, Keys.F6, Keys.Shift); //註冊 Shift+F6 為熱鍵
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF6); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkey7_OnHotkey); //獨立事件

                hotkey8 = new HotKey(this.Handle, Keys.F7, Keys.Shift); //註冊 Shift+F7 為熱鍵
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF7); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkey8_OnHotkey); //獨立事件

                hotkey9 = new HotKey(this.Handle, Keys.F8, Keys.Shift); //註冊 Shift+F8 為熱鍵
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF8); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件

                hotkey10 = new HotKey(this.Handle, Keys.F9, Keys.Shift); //註冊 Shift+F9 為熱鍵
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF9); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkey10_OnHotkey); //獨立事件

                hotkey11 = new HotKey(this.Handle, Keys.F10, Keys.Shift); //註冊 Shift+F10 為熱鍵
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF10); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkey11_OnHotkey); //獨立事件

                hotkey12 = new HotKey(this.Handle, Keys.F11, Keys.Shift); //註冊 Shift+F11 為熱鍵
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF11); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkey12_OnHotkey); //獨立事件

                hotkey13 = new HotKey(this.Handle, Keys.F12, Keys.Shift); //註冊 Shift+F12 為熱鍵
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF12); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkey13_OnHotkey); //獨立事件
            }
            else
            {
                hotkey();
            }
        }

        public void funCommand(string sqlstr2, SqlConnection conn1, int ifc)
        {
            SqlCommand cmd = new SqlCommand(sqlstr2, conn1);
            SqlDataReader clinic = cmd.ExecuteReader();
            try
            {
                while (clinic.Read())
                {
                    switch (ifc)//執行項目
                    {
                        case 1:
                            if (clinic["IPMAC"].ToString() != "")
                                dMAC.Add(clinic["IPMAC"].ToString().ToUpper(), clinic["IPHospRowid"].ToString()); //抓取所有資訊面板註冊過的MacAddress
                            break;
                        case 2:
                            hisid = clinic["HospID"].ToString();//醫事機構代碼
                            hospname = clinic["HospName"].ToString();
                            if (ApplicationDeployment.IsNetworkDeployed)
                            {
                                ApplicationDeployment myVersion = ApplicationDeployment.CurrentDeployment;
                                publishVersion = string.Concat(myVersion.CurrentVersion);
                                this.Text = hisid + "_" + hospname + "_" + publishVersion;//診間名稱+醫事機構代碼 + 版本
                            }
                            else
                                this.Text = hisid + "_" + clinic["HospName"].ToString() + "_版本號碼" ;//診間名稱+醫事機構代碼
                            break;
                    }
                }
                clinic.Close();
            }
            catch
            {
                MessageBox.Show("找不到診所及其他資料,請洽系統工程師\n\n聯絡電話：(03) 4636913");
            }
        }

        //private String FindInDictionary()//找到 醫事機構代碼IPHospRowid
        //{
        //    if (true == (dMAC.ContainsKey("IPHospRowid")))
        //    {
        //        return dMAC["IPHospRowid"];
        //    }
        //    else
        //    {
        //        return "Not Found";
        //    }
        //}
        private void hotkey1to4_OnHotkey(object sender, HotKeyEventArgs e)//程式視窗放大縮小
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

        private void hotkeyF1(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox1;//把所選的值放在function1111裡面
        }

        private void hotkeyF2(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox2;
        }

        private void hotkeyF3(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox3;
        }

        private void hotkeyF4(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox4;
        }

        private void hotkeyF5(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox5;
        }

        private void hotkeyF6(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox6;
        }

        private void hotkeyF7(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox7;
        }

        private void hotkeyF8(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox8;
        }

        private void hotkeyF9(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox9;
        }

        private void hotkeyF10(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox10;
        }

        private void hotkeyF11(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox11;
        }

        private void hotkeyF12(object sender, HotKeyEventArgs e)
        {
            function1111 = combobox12;
        }

        private void hotkey2_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox1.SelectedValue.ToString();
            string ur1 = comboBox1.Text.Trim();
            if(testhis =="TECH"&&ur1 =="個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }

        private void hotkey3_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox2.SelectedValue.ToString();
            string ur1 = comboBox2.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }

        }

        private void hotkey4_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox3.SelectedValue.ToString();
            string ur1 = comboBox3.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }

        }

        private void hotkey5_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox4.SelectedValue.ToString();
            string ur1 = comboBox4.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey6_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox5.SelectedValue.ToString();
            string ur1 = comboBox5.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey7_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox6.SelectedValue.ToString();
            string ur1 = comboBox6.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey8_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox7.SelectedValue.ToString();
            string ur1 = comboBox7.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey9_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox8.SelectedValue.ToString();
            string ur1 = comboBox8.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey10_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox9.SelectedValue.ToString();
            string ur1 = comboBox9.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey11_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox10.SelectedValue.ToString();
            string ur1 = comboBox10.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey12_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox11.SelectedValue.ToString();
            string ur1 = comboBox11.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        private void hotkey13_OnHotkey(object sender, HotKeyEventArgs e)
        {
            string ur2 = comboBox12.SelectedValue.ToString();
            string ur1 = comboBox12.Text.Trim();
            if (testhis == "TECH" && ur1 == "個管問診")
            {
                TECHbrowser();
            }
            else
            {
                openBrowser(ur2);//開連結
            }
        }
        void TECHbrowser()
        {
            Process.Start("chrome", "http://www.weightobserver.com.tw:8080/antifat/his?hospital=" + hisid + "&dr=" + drId + "&patientno=" + str1 + "&patientname=" + name + "&patientIdno=" + ID + "&patientBirth=" + Birth + "&tel=" + tel + "&cell=" + Cell + "&strRecno=" + str1);
        }
        void openBrowser(string conn)
        {
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時
            // 打開凌醫網頁
            string ur0 = (hisid != "") ? hisid : "";//判斷是否為空值

            string ur1 = (drId != "") ? drId : "";

            string ur2 = (str1 != "") ? str1 : "";

            string ur3 = (name != "") ? name : "";

            string ur4 = (ID != "") ? ID : "";

            string ur5 = (Birth != "") ? Birth : "";

            string ur6 = (tel != "") ? tel : "";

            string ur7 = (Cell != "") ? Cell : "";


            // output url Format example : 
            // http://www.weightobserver.com.tw:8080/antifat/his?hospital={0}&dr={1}&patientno={2}&patientname={3}&patientIdno={4}&patientBirth={5}&tel={6}&cell={7}
            // 0=醫事機構代碼 1=登錄帳號 2=病歷號 3=名字 4=身分證5=生日 6=家電 7=手機
            string[] names = { ur0, ur1, ur2, ur3, ur4, ur5, ur6, ur7 };//把上面的值放在Names裡面
            string output = names[0] + ", " + names[1] + ", " + names[2] + ", " +//幫他們設定名子
                            names[3] + ", " + names[4] + ", " + names[5] + ", " +
                            names[6] + ", " + names[7];

            string ur = String.Format(conn, names[0], names[1], names[2], names[3], names[4], names[5], names[6], names[7]);//這個在資訊平台一鍵連結功能設定上面會有數字那個數字就是帶剛剛取的值           
                                                                                                                            //string sHospID = names[0];
                                                                                                                            //int iCode = int.Parse("T55");
                                                                                                                            //string sName = names[2];
                                                                                                                            //string sIdNo = String.IsNullOrEmpty(names[3]) ? "" : names[3];
                                                                                                                            //string sBirthday = names[4];
                                                                                                                            //string sTelephone = names[5];
                                                                                                                            //string sMobilePhone = names[6];

            
            if (function1111 != "   ")//當不小心按到空白項目會沒有作用(3個空白的原因是在後台的空白選項是3個空白如果後台改掉這邊就要改)
            {
                if (myClass.IsNumeric(str1))//判斷str1是否為數字
                {
                    this.Hide();
                    notifyIcon1.ShowBalloonTip(5000);
                    prog_flag = false;
                    sw.Stop();//碼錶停止
                    string savetime = sw.Elapsed.TotalMilliseconds.ToString();
                    bool Return = WritePrivateProfileString("AppName", "開連結時間(ms)", savetime, test);
                    Process.Start("chrome", ur);//用chrome開網頁 如過用們預設的瀏覽器會有出錯問題

                    // 儲存使用者按了一鍵連結的相關資訊
                    Save2LogDB(names[0], names[1], names[2], ur);
                }
                else if (function1111 == "凌醫首頁")
                {
                    this.Hide();
                    notifyIcon1.ShowBalloonTip(5000);
                    prog_flag = false;
                    Process.Start("chrome", ur);

                    Save2LogDB(names[0], names[1], names[2], ur);
                }
                else
                {
                    // 截圖內容不正確(不為數字)會開啟這個網址
                    Process.Start("chrome", "http://www.weightobserver.com.tw:8080/antifat/#");

                    Save2LogDB(names[0], names[1], names[2], "http://www.weightobserver.com.tw:8080/antifat/#--截圖內容不正確(不為數字)會開啟這個網址");
                }
                if (ID != checkID || name != checkname)
                {
                    Save2LogDB(names[0], names[1], "100", ur);
                }
            }
        }
      
        private void Save2LogDB(string sHospID, string sLoginUser, string sCode, string sUrl)
        {
            // EF SaveData
            LogDBEntities db_log = new LogDBEntities();

            OCLinkLog oc = new OCLinkLog();

            oc.OCLRowid = Guid.NewGuid().ToString();
            oc.OCLHospID = sHospID;
            oc.OCLLoginUser = sLoginUser;
            oc.OCLCode = sCode;
            oc.OCLUrl = sUrl;
            oc.OCLCreateTime = DateTime.Now;

            db_log.OCLinkLog.Add(oc);
            db_log.SaveChanges();

            return;
        }
        bool btz = true;//判斷診間掛號紐是否按下
        private void buttonS_Click(object sender, EventArgs e)//掛號紐
        {
            panel1.Visible = true;
            panel2.Visible = false;
            btz = true;
            //this.Height = 365;
            buttonS.BackColor = Color.Black;//按鈕按下的顏色
            buttonS.ForeColor = Color.White;
            ButtonZ.BackColor = Color.Gainsboro;
            ButtonZ.ForeColor = Color.Black;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //HotKey.UnregisterHotKey(Handle, 100);
            e.Cancel = true;
            this.Hide();
            notifyIcon1.ShowBalloonTip(5000);//縮小後右下角的提示
            prog_flag = false;
        }

        //快捷鍵按下執行的事件
        private void GlobalKeyProcess()
        {
            this.WindowState = FormWindowState.Minimized;
            Thread.Sleep(200);
            buttonS.PerformClick();
        }

        //重寫。監視系統訊息，呼叫對應方法
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            //如果m.Msg的值為0x0312（我也不知道為什麼是0x0312）那麼表示使用者按下了熱鍵
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    if (m.WParam.ToString().Equals("100"))
                    {
                        GlobalKeyProcess();
                    }

                    //todo  其它熱鍵
                    break;
            }
            // 將系統訊息傳遞自父類的WndProc
            base.WndProc(ref m);
        }
        private void btn_OCR_Click(object sender, EventArgs e)//OCR鈕會問為甚麼不寫在裡面因為我要區分按的OCR和快捷鍵的OCR
        {
            bool Return;
            Return = WritePrivateProfileString("AppName", "Enabled", btn_OCR.Enabled.ToString(), test);
            hotkeycheck = false;
            btn_OCR1(null,null);
        }

        // OCR 測試結果顯示
        string checkID;
        string checkname;
        public string str1;//截圖出來的內容(病歷號)
        public bool GotOCR = false;//是否取得病歷號 
        private void btn_OCR1(object sender, EventArgs e)//這裡取病人資料OCR核心
        {
            str1 = "";
            ID = "";
            tel = "";
            Birth = "";
            Cell = "";
            //if (testhis == "TECH")//方頂取title
            //{
            //    Timer mytimer = new Timer();
            //    mytimer.Tick += new EventHandler(mytimer_Tick);
            //    mytimer.Start();
            //    GotOCR = true;
            //    if (hotkeycheck == false)
            //    {
            //        MessageBox.Show(Regex.Replace(str1, "[^0-9]", ""));
            //        hotkeycheck = true;
            //    }
            //}
            if (GotOCR ==false && function1111 !="凌醫首頁")
            {
                i = 0;
                str1 = GetOCR();//這裡截圖來病歷號
                if (hotkeycheck == false)
                {
                    MessageBox.Show(str1);//要確認截圖結果是否正確
                    hotkeycheck = true;
                }
            }
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時
            //取得病人資料
            if (testhis == "RS" && File.Exists(Server + @":\S\patdb.dbf") && function1111 != "凌醫首頁" && function1111 != "   ")//耀聖 
            {//以vfpoledb的方式取耀聖資料 跟sql取的方式類似
                int nRecno = 0;
                if (myClass.IsNumeric(str1))//判斷是否截圖內容為數字
                {
                    nRecno = Convert.ToInt32(str1);
                    sw.Reset();//碼表歸零
                    sw.Start();//碼表開始計時
                    OleDbConnection conn = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" + Server + @":\S\");    //連接字串
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        sw.Stop();//碼錶停止
                        string savetime1 = sw.Elapsed.TotalMilliseconds.ToString();
                        bool Returnt = WritePrivateProfileString("AppName", "開啟", savetime1, test);//這個是要看程式跑的時間
                        sw.Reset();//碼表歸零
                        sw.Start();//碼表開始計時
                        OleDbCommand cmd = new OleDbCommand(String.Format("select name ,id ,tel, birth  from patdb where recno()={0}", nRecno), conn);//指令跟sql語法一樣
                        OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                        DataTable table = new DataTable();
                        da.Fill(table);
                        sw.Stop();//碼錶停止
                        string savetime2 = sw.Elapsed.TotalMilliseconds.ToString();
                        Returnt = WritePrivateProfileString("AppName", "取資料", savetime2, test);//這個是要看程式跑的時間
                        conn.Close();
                        sw.Reset();//碼表歸零
                        sw.Start();//碼表開始計時
                        foreach (DataRow dr in table.Rows)//取得病人資料
                        {
                            name = dr["name"].ToString().Trim();
                            ID = dr["id"].ToString().Trim();
                            tel = dr["tel"].ToString().Trim();
                            Birth = dr["birth"].ToString().Trim().Replace("A", "10").Replace("B", "11").Replace("C", "12").Replace("D", "13");//為什麼要ReplaceABC因為耀聖民國100年以A為代號民國110為B類推目前只用到民國130年 ex:耀聖生日資料A3 = 民國103年
                            try
                            {
                                Cell = dr["mobil"].ToString().Trim();
                            }
                            catch
                            {
                                Cell = "";
                            }
                        }
                        sw.Stop();//碼錶停止
                        string savetime3 = sw.Elapsed.TotalMilliseconds.ToString();
                        Returnt = WritePrivateProfileString("AppName", "個人資料", savetime3, test);//這個是要看程式跑的時間
                    }
                }
                else
                {
                    MessageBox.Show("擷取資料錯誤,請重新截圖");
                }
            }
            else if (testhis == "VISW" && File.Exists(Server + @":\" + textBox1.Text + @"\CO01M.dbf") && function1111 != "凌醫首頁" && function1111 != "   ")//展望
            {
                if (myClass.IsNumeric(str1))//判斷是否截圖內容為數字
                {
                    OleDbConnection conn = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" + Server + @":\VISW\;Collating Sequence=MACHINE");    //連接字串
                    conn.Open();

                    //string query = ("select * from CO01m where Kcstmr = '" + str1 + "'");
                    OleDbCommand cmd = new OleDbCommand("select * from CO01m where Kcstmr = '" + str1 + "'", conn);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable table = new DataTable();
                    da.Fill(table);

                    conn.Close();

                    foreach (DataRow dr in table.Rows)//取欄位內的病人資料
                    {
                        name = dr["Mname"].ToString().Trim();
                        ID = dr["Mpersonid"].ToString().Trim();
                        tel = dr["Mtelh"].ToString().Trim();
                        Birth = dr["Mbirthdt"].ToString().Trim();
                        try
                        {
                            Cell = dr["Mrec"].ToString().Trim();
                        }
                        catch
                        {
                            Cell = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("擷取資料錯誤,請重新截圖");
                }
            }
            else if (testhis == "TECH" && function1111 != "凌醫首頁" && function1111 != "   ")//方鼎 Sql連接方法 變數是自己打的
            {
                string strCon2 = "server=" + sqlIP +";database=" + sqldatabace + ";user=" + sqlID + ";password=" +sqlpw +";";
                using (SqlConnection conn = new SqlConnection(strCon2))
                {
                    string sqlconn = "";
                    if (checkBox4.Checked == true)
                    {
                        sqlconn = "select * from 病患檔 where 病歷號碼  ='" + str1 + "'";
                    }
                    else if(checkBox6.Checked == true)
                    {
                        sqlconn = "select * from 病患檔 where 身份證字號  like  '%" + str1 + "'";
                    }
                    conn.Open();
                    SqlCommand sda = new SqlCommand(sqlconn, conn);
                    SqlDataReader clinic = sda.ExecuteReader();
                    while (clinic.Read())
                    {
                        name = clinic["姓名"].ToString().Trim();
                        str1 = clinic["病歷號碼"].ToString().Trim();
                        ID = clinic["身份證字號"].ToString().Trim();
                        Birth = clinic["生日"].ToString().Trim();
                        tel = clinic["電話H"].ToString().Trim();
                        if(clinic["電話O"].ToString() !="")
                        {
                            Cell = clinic["電話O"].ToString().Trim();
                        }
                    }
                }
            }
            else if (testhis == "DHA" && File.Exists(Server + @":\DATA\PD011M1.dbf") && function1111 != "凌醫首頁" && function1111 != "   ")//常誠
            {
                if (myClass.IsNumeric(str1))//判斷是否截圖內容為數字
                {
                    OleDbConnection conn = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" + Server + @":\DATA\;Collating Sequence=MACHINE");    //連接字串
                    conn.Open();

                    //string query = ("select * from PD011M1 where Num = '" + str1 + "'");
                    OleDbCommand cmd = new OleDbCommand("select * from PD011M1 where Num = '" + str1 + "'", conn);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable table = new DataTable();
                    da.Fill(table);

                    conn.Close();

                    foreach (DataRow dr in table.Rows)
                    {
                        name = dr["name"].ToString().Trim();
                        ID = dr["id"].ToString().Trim();
                        tel = dr["tel"].ToString().Trim();
                        Birth = String.Format("{0: yyyy/MM/dd}", dr["birth"].ToString().Trim());
                        try
                        {
                            Cell = dr["act_tel"].ToString().Trim();
                        }
                        catch
                        {
                            Cell = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("擷取資料錯誤,請重新截圖");
                }
            }
            else if (testhis == "SC" && File.Exists(Server + @":\SC\Dat\User.dat") && function1111 != "凌醫首頁" && function1111 != "   ")//醫聖
            {
                if (myClass.IsNumeric(str1))//判斷是否截圖內容為數字
                {
                    OleDbConnection conn = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" + Server + @":\SC\Dat;Collating Sequence=MACHINE");    //連接字串
                    conn.Open();

                    //string query = ("select * from User.dat where MEDICAL_NO = '" + str1 + "'");
                    OleDbCommand cmd = new OleDbCommand("select * from User.dat where MEDICAL_NO = '" + str1 + "'", conn);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable table = new DataTable();
                    da.Fill(table);

                    conn.Close();

                    foreach (DataRow dr in table.Rows)
                    {
                        name = dr["NAME"].ToString().Trim();
                        ID = dr["ID"].ToString().Trim();
                        tel = dr["TEL"].ToString().Trim();
                        Birth = dr["BIRTHDAY"].ToString().Trim();
                        try
                        {
                            Cell = dr["TEL2"].ToString().Trim();
                        }
                        catch
                        {
                            Cell = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("擷取資料錯誤,請重新截圖");
                }
            }
            //else if (testhis == "KN")//測試抓oy的his3532040438的資料庫 開蘭*未使用*
            //{
            //    var hospitalList = (from a in db_0438.patient where a.strUserAccount == "'TX" + str1 + "'" select new { a.strDisplayName, a.strIdno, a.strTel, a.strCell }).FirstOrDefault();
            //    name = hospitalList.strDisplayName.Trim();
            //    ID = hospitalList.strIdno.Trim();
            //    tel = hospitalList.strTel.Trim();
            //    if (hospitalList.strCell != "")
            //    {
            //        Cell = hospitalList.strCell.Trim();
            //    }
            //}
            else if (testhis == "MTR" && File.Exists(Server + @"\MTR\DBF\Client.dbf") && function1111 != "凌醫首頁" && function1111 != "   ")//蒙利特(沒測試過但應該沒問題)
            {
                if (myClass.IsNumeric(str1))//判斷是否截圖內容為數字
                {

                    OleDbConnection conn = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" + Server + @":\MTR\DBF;Collating Sequence=MACHINE");    //連接字串
                    conn.Open();

                    //string query = ("select * from client where medical_no = '" + str1 + "'");
                    OleDbCommand cmd = new OleDbCommand("select * from client where medical_no = '" + str1 + "'", conn);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable table = new DataTable();
                    da.Fill(table);

                    conn.Close();

                    foreach (DataRow dr in table.Rows)
                    {
                        name = dr["name"].ToString().Trim();
                        ID = dr["id"].ToString().Trim();
                        tel = dr["tel"].ToString().Trim();
                        Birth = dr["birthday"].ToString().Trim();
                        try
                        {
                            Cell = dr["bigtel"].ToString().Trim();
                        }
                        catch
                        {
                            Cell = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("擷取資料錯誤,請重新截圖");
                }
            }
            string ckeckconn = "server =" + strconn23 + "; database = his" + hisid + "; user = " + strconnUser + "; password = " + strconnpw;
            using (SqlConnection conn = new SqlConnection(ckeckconn))
            {
                string sqlconn = "select top 1* from patient where strUserAccount  like'%" + str1 + "'";
                conn.Open();
                SqlCommand sda = new SqlCommand(sqlconn, conn);
                SqlDataReader clinic = sda.ExecuteReader();//strTel
                while (clinic.Read())
                {
                    checkname = (!String.IsNullOrEmpty(clinic["strname"].ToString().Trim())) ? (clinic["strname"].ToString().Trim()) : "";
                    checkID = (!String.IsNullOrEmpty(clinic["strIdno"].ToString().Trim())) ? (clinic["strIdno"].ToString().Trim()) : "";
                }
            }
            sw.Stop();//碼錶停止
            string savetime = sw.Elapsed.TotalMilliseconds.ToString();
            bool Return = WritePrivateProfileString("AppName", "開資料時間(ms)", savetime, test);
        }
        int i = 0;//強制截圖1次以免無限輪迴造成死當
        private string GetOCR()
        {
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時
            // 依畫面再重新截圖
            int swidth = Screen.PrimaryScreen.Bounds.Width;
            int sheight = Screen.PrimaryScreen.Bounds.Height;
            Bitmap btm = new Bitmap(swidth, sheight); //空圖與螢幕同大小
            Graphics g = Graphics.FromImage(btm); //空圖的畫板
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(swidth, sheight)); //將螢幕內容複製到空圖

            Cutter cutter = new Cutter(btm, btz); //傳送截圖

            if (btz == true &&(!File.Exists(@"C:\ZMTemp\TestFile.TXT"))|| (btz == false && !File.Exists(@"C:\ZMTemp\TestFile_office.TXT")))
            {
                //cutter.FormBorderStyle = FormBorderStyle.None; //截圖全屏，無邊框
                cutter.BackgroundImage = btm; //新的窗體截圖做背景
                cutter.Show();
            }

            // 開始OCR
            //string s = ocr.Recognize(@"C:\Temp\CaptureImage.jpg", -1, startX, startY, width, height, AspriseOCR.RECOGNIZE_TYPE_ALL, AspriseOCR.OUTPUT_FORMAT_PLAINTEXT);
            // 正式環境需把這解開 
            TesseractEngine ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndLstm);//設定語言   英文;
            // 測試時可以把這解開 
            //TesseractEngine ocr = new TesseractEngine(@"D:\ZeroMedRD\OCLink\OCLink\tessdata", "chi_tra", EngineMode.TesseractAndLstm);//設定語言   中英文;
            //ocr = new TesseractEngine("./tessdata", "chi_tra");//設定語言   中文
            //ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);//設定語言   英文

            string sPreventString = String.Empty;
            string str = String.Empty;
            
            //匯入圖片進行識別
            if (btz == true && i < 2)
            {
                i++;
                if (hotkeycheck == false)
                {
                    File.Delete(@"C:\ZMTemp\BWImage.jpg");
                    File.Delete(@"C:\ZMTemp\Preprocess_HighRes.jpg");
                    File.Delete(@"C:\ZMTemp\Preprocess_Resize.jpg");
                }

                //Bitmap bit = new Bitmap(Image.FromFile(@"C:\Temp\CaptureImage.jpg"));
                using (var fs = new FileStream(@"C:\ZMTemp\CaptureImage.jpg",FileMode.Open))
                {
                    var bmp = new Bitmap(fs);
                    fs.Close();
                    Bitmap bit = (Bitmap)bmp.Clone();
                    if (hotkeycheck == false)
                    {
                        if (File.Exists(@"C:\ZMTemp\BWImage.jpg"))
                        {
                            bit = new Bitmap(Image.FromFile(@"C:\ZMTemp\BWImage.jpg"));
                        }
                    }

                    bit = PreprocesImage(bit, false);
                    Page page = ocr.Process(bit);
                    sPreventString = page.GetText().Trim().Replace(" ", "");
                    if(testhis == "TECH")
                    {
                        int i = 0;
                        if (checkBox5.Checked == true)
                        {
                            i = sPreventString.Length - 8;
                            sPreventString = (sPreventString.Length > 8) ? sPreventString.Remove(0, i) : sPreventString;
                        }
                        else
                        {
                            i = sPreventString.Length - 9;
                            sPreventString = (sPreventString.Length > 9) ? sPreventString.Remove(0, i) : sPreventString;

                        }
                    }

                    //if (testhis == "TECH")
                    //{
                    //    string strTxt = "";
                    //    for (int i = 0; i < sPreventString.Length; i++)
                    //    {
                    //        if ((i == 0) && ((sPreventString[i].ToString() == "0") || (sPreventString[i].ToString() == "1") || (sPreventString[i].ToString() == "8") || (sPreventString[i].ToString() == "5") || (sPreventString[i].ToString() == "3")))
                    //        {
                    //            switch (sPreventString[i].ToString())
                    //            {
                    //                case "0":
                    //                    strTxt += "O";
                    //                    break;
                    //                case "1":
                    //                    strTxt += "I";
                    //                    break;
                    //                case "8":
                    //                    strTxt += "B";
                    //                    break;
                    //                case "5":
                    //                    strTxt += "S";
                    //                    break;
                    //                case "3":
                    //                    strTxt += "S";
                    //                    break;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            strTxt += sPreventString[i];
                    //        }
                    //    }
                    //    //string sPreventString2 = "";
                    //    //bool bCK_sPreventString = false;
                    //    //foreach (var test in strTxt)
                    //    //{
                    //    //    if (test.ToString() == " ")
                    //    //    {
                    //    //        bCK_sPreventString = true;
                    //    //    }
                    //    //    if (bCK_sPreventString == false)
                    //    //    {
                    //    //        sPreventString2 += test.ToString();
                    //    //    }
                    //    //}
                    //    str = Regex.Replace(strTxt, "[^A-Za-z0-9]", "");
                    //    str = str.Replace(" ", "").Replace("一", "-").Replace("?", "7");
                    //}
                    //if (testhis != "TECH")
                    //{
                    str = Regex.Replace(sPreventString, "[^A-Za-z0-9]", "");
                    str = str.ToUpper().Replace("O", "0").Replace("L", "1").Replace("I", "1").Replace("S", "5")
                          .Replace("j", "5").Replace("P", "0").Replace(" ", "").Replace("一", "-").Replace("?", "7");
                    str = Regex.Replace(str, "[a-zA-Z]+", "");         // 去除英文
                    str = Regex.Replace(str, "[^0-9][^-][^/]", "");// 保留數字與字符 - /
                    //}
                    str = testhis == "RS" ?str.Length == 6 ? str : str = " ":str;
                    str = testhis == "VISW" ? str.Length == 7 ? str : str = " " : str;//********************
                    page.Dispose();
                    bit.Dispose();
                    ocr.Dispose();
                    
                    if (hotkeycheck == false)
                    {
                       LoadImageList();
                    }
                    if(!myClass.IsNumeric(str)&&hotkeycheck == true)//不為數字在執行一次
                    {
                        btz = false;
                        str = GetOCR();
                        
                    }
                }
            }
            else if (btz == false && i < 2&&str.Length < 5)
            {
                i++;
                if (hotkeycheck == false)
                {
                    File.Delete(@"C:\ZMTemp\BWImage_office.jpg");
                    File.Delete(@"C:\ZMTemp\Preprocess_HighRes_office.jpg");
                    File.Delete(@"C:\ZMTemp\Preprocess_Resize_office.jpg");
                }

                //Bitmap bit = new Bitmap(Image.FromFile(@"C:\Temp\CaptureImage.jpg"));
                using (var fs = new FileStream(@"C:\ZMTemp\CaptureImage_office.jpg", FileMode.Open))
                {
                    var bmp = new Bitmap(fs);
                    fs.Close();
                    Bitmap bit = (Bitmap)bmp.Clone();
                    if (hotkeycheck == false)
                    {
                        if (File.Exists(@"C:\ZMTemp\BWImage_office.jpg"))
                        {
                            bit = new Bitmap(Image.FromFile(@"C:\ZMTemp\BWImage_office.jpg"));
                        }
                    }
                    bit = PreprocesImage(bit, false);
                    Page page = ocr.Process(bit);
                    sPreventString = page.GetText().Trim().Replace(" ", "");
                    if(testhis == "TECH")
                    {
                        int i = 0;
                        if (checkBox5.Checked == true)
                        {
                            i = sPreventString.Length - 8;
                            sPreventString = (sPreventString.Length > 8) ? sPreventString.Remove(0, i) : sPreventString;
                        }
                        else
                        {
                            i = sPreventString.Length - 9;
                            sPreventString = (sPreventString.Length > 9) ? sPreventString.Remove(0, i) : sPreventString;

                        }
                    }
                    //if (testhis == "TECH")
                    //{
                    //    string strTxt = "";
                    //    for (int i = 0; i < sPreventString.Length; i++)
                    //    {
                    //        if ((i == 0) && ((sPreventString[i].ToString() == "0") || (sPreventString[i].ToString() == "1") || (sPreventString[i].ToString() == "8") || (sPreventString[i].ToString() == "5") || (sPreventString[i].ToString() == "3")))
                    //        {
                    //            switch (sPreventString[i].ToString())
                    //            {
                    //                case "0":
                    //                    strTxt += "O";
                    //                    break;
                    //                case "1":
                    //                    strTxt += "I";
                    //                    break;
                    //                case "8":
                    //                    strTxt += "B";
                    //                    break;
                    //                case "5":
                    //                    strTxt += "S";
                    //                    break;
                    //                case "3":
                    //                    strTxt += "S";
                    //                    break;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            strTxt += sPreventString[i];
                    //        }
                    //    }
                    //string sPreventString2 = "";
                    //bool bCK_sPreventString = false;
                    //foreach (var test in strTxt)
                    //{
                    //    if (test.ToString() == " ")
                    //    {
                    //        bCK_sPreventString = true;
                    //    }
                    //    if (bCK_sPreventString == false)
                    //    {
                    //        sPreventString2 += test.ToString();
                    //    }
                    //}
                    //str = Regex.Replace(strTxt, "[^A-Za-z0-9]", "");
                    //str = str.Replace(" ", "").Replace("一", "-").Replace("?", "7");
                    //}
                    //if (testhis != "TECH")
                    //{
                    str = Regex.Replace(sPreventString, "[^A-Za-z0-9]", "");
                    str = str.ToUpper().Replace("O", "0").Replace("L", "1").Replace("I", "1").Replace("S", "5")
                          .Replace("j", "5").Replace("P", "0").Replace(" ", "").Replace("一", "-").Replace("?", "7");
                    str = Regex.Replace(str, "[a-zA-Z]+", "");         // 去除英文
                    str = Regex.Replace(str, "[^0-9][^-][^/]", "");// 保留數字與字符 - /
                    //}
                    str = testhis == "RS" ? str.Length == 6 ? str : str = " " : str;
                    str = testhis == "VISW" ? str.Length == 7 ? str : str = " " : str;//**********************
                    page.Dispose();
                    bit.Dispose(); 
                    ocr.Dispose();

                    if (hotkeycheck == false)
                    {
                        LoadImageList();
                    }
                    if (!myClass.IsNumeric(str) && hotkeycheck == true)
                    {
                        //str = OcrB();//執行時間 1524.9099ms
                        btz = true;//2
                        str = GetOCR();
                    }
                }
            }
            sw.Stop();//碼錶停止
            string savetime = sw.Elapsed.TotalMilliseconds.ToString();
            bool Return = WritePrivateProfileString("AppName", "OCR時間(ms)", savetime, test);//紀錄OCR運行時間
            return (str);
        }
        
        /// <summary>
        /// 圖片顏色區分，剩下白色和黑色
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap PreprocesImage(Bitmap image, bool lReverse)//會抓你設定的RGB跟放大倍率 來辨識圖片提高準確率
        {
            //Color cForeColor = Color.White;
            //Color cBackColor = Color.Black;

            //if (lReverse == true)
            //{
            //    cForeColor = Color.Black;
            //    cBackColor = Color.White;
            //}

            //You can change your new color here. Red,Green,LawnGreen any..
            Color actualColor;

            //make an empty bitmap the same size as scrBitmap
            //if(btz == true)
            //{
            //    nResize = (IsNumeric(tbResize.Text)) ? int.Parse(tbResize.Text.Trim()) : 10;
            //}
            //else
            //{
            //    nResize = (IsNumeric(tresize.Text)) ? int.Parse(tresize.Text.Trim()) : 10;
            //}
            nResize = 5;
            image = ResizeImage(image, image.Width * nResize, image.Height * nResize);
            if(hotkeycheck == false)
            {
                //if(btz == true)
                //{
                    image.Save(@"C:\ZMTemp\Preprocess_Resize.jpg");
                //}
                //else
                //{
                    image.Save(@"C:\ZMTemp\Preprocess_Resize_office.jpg");
                //}
            }
                        
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);
            int type = 2;       //加權平均值法
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    //get the pixel from the scrBitmap image
                    //actualColor = image.GetPixel(i, j);

                    actualColor = image.GetPixel(i, j);
                    int r, g, b, Result = 0;
                    r = actualColor.R;
                    g = actualColor.G;
                    b = actualColor.B;
                    switch (type)
                    {
                        case 0://平均值法
                            Result = ((r + g + b) / 3);
                            break;
                        case 1://最大值法
                            Result = r > g ? r : g;
                            Result = Result > b ? Result : b;
                            break;
                        case 2://加權平均值法
                            Result = ((int)(0.3 * r) + (int)(0.59 * g) + (int)(0.11 * b));
                            break;
                    }
                    newBitmap.SetPixel(i, j, Color.FromArgb(Result, Result, Result));
                }
            }

            newBitmap = ResizeImage(newBitmap, newBitmap.Width, newBitmap.Height);

            if(hotkeycheck ==false)
            {
                if(btz ==true)
                {
                    newBitmap.Save(@"C:\ZMTemp\BWImage.jpg");
                }
                else
                {
                    newBitmap.Save(@"C:\ZMTemp\BWImage_office.jpg");
                }
            }
            return newBitmap;
        }
                
        private void ButtonHotKey_Click(object sender, EventArgs e)//快捷鍵設定鈕
        {
           // this.Height = 335;
            if (panel2.Visible == false)
            {
                panel2.Visible = true;
                panel1.Visible = false;
            }
            else
            {
                panel2.Visible = false;
                panel1.Visible = true;
            }
        }


        string test = @"C:\ZMTemp\System.ini";
        private void ButtonSave_Click(object sender, EventArgs e)//存檔按鈕
        {
            //儲存參數
            bool Return;
            Return = WritePrivateProfileString("AppName", "F1", comboBox1.Text, test);
            Return = WritePrivateProfileString("AppName", "F2", comboBox2.Text, test);
            Return = WritePrivateProfileString("AppName", "F3", comboBox3.Text, test);
            Return = WritePrivateProfileString("AppName", "F4", comboBox4.Text, test);
            Return = WritePrivateProfileString("AppName", "F5", comboBox5.Text, test);
            Return = WritePrivateProfileString("AppName", "F6", comboBox6.Text, test);
            Return = WritePrivateProfileString("AppName", "F7", comboBox7.Text, test);
            Return = WritePrivateProfileString("AppName", "F8", comboBox8.Text, test);
            Return = WritePrivateProfileString("AppName", "F9", comboBox9.Text, test);
            Return = WritePrivateProfileString("AppName", "F10", comboBox10.Text, test);
            Return = WritePrivateProfileString("AppName", "F11", comboBox11.Text, test);
            Return = WritePrivateProfileString("AppName", "F12", comboBox12.Text, test);
            Return = WritePrivateProfileString("AppName", "ctrl", checkBox1.Checked.ToString(), test);
            Return = WritePrivateProfileString("AppName", "shift", checkBox2.Checked.ToString(), test);
            Return = WritePrivateProfileString("AppName", "alt", checkBox3.Checked.ToString(), test);
            Return = WritePrivateProfileString("AppName", "Server", Server , test);
            Return = WritePrivateProfileString("AppName", "His", comboBox13.Text, test);
            Return = WritePrivateProfileString("AppName", "Resize", tbResize.Text, test);
            Return = WritePrivateProfileString("AppName", "R", tbRed.Text, test);
            Return = WritePrivateProfileString("AppName", "G", tbGreen.Text, test);
            Return = WritePrivateProfileString("AppName", "B", tbBlue.Text, test);
            Return = WritePrivateProfileString("AppName", "resize_office", tresize.Text, test);
            Return = WritePrivateProfileString("AppName", "r_office", tred.Text, test);
            Return = WritePrivateProfileString("AppName", "g_office", tgreen.Text, test);
            Return = WritePrivateProfileString("AppName", "b_office", tblue.Text, test);
            Return = WritePrivateProfileString("AppName", "登入帳號", drId, test);
            Return = WritePrivateProfileString("AppName", "controll", comboBox15.Text, test);
            Return = WritePrivateProfileString("AppName", "number", comboBox16.Text, test);
            Return = WritePrivateProfileString("AppName", "展望資料夾位置", textBox1.Text, test);
            Return = WritePrivateProfileString("AppName", "sqlIP", sqlIP, test);
            Return = WritePrivateProfileString("AppName", "sql資料庫", sqldatabace, test);
            Return = WritePrivateProfileString("AppName", "sql帳號", sqlID, test);
            Return = WritePrivateProfileString("AppName", "sql密碼", sqlpw, test);
            Return = WritePrivateProfileString("AppName", "sql密碼", sqlpw, test);
            Return = WritePrivateProfileString("AppName", "病歷號", checkBox4.Checked.ToString(), test);
            Return = WritePrivateProfileString("AppName", "身份證", checkBox6.Checked.ToString(), test);
            UserHotkey();
            if (comboBox1.Text == "   ")
            {
                hotkey2.Dispose();
            }
            else
            {
                hotkey2 = new HotKey(this.Handle, Keys.F1, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey2.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox2.Text == "   ")
            {
                hotkey3.Dispose();
            }
            else
            {
                hotkey3 = new HotKey(this.Handle, Keys.F2, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF2); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey3.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox3.Text == "   ")
            {
                hotkey4.Dispose();
            }
            else
            {
                hotkey4 = new HotKey(this.Handle, Keys.F3, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF3); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey4.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox4.Text == "   ")
            {
                hotkey5.Dispose();
            }
            else
            {
                hotkey5 = new HotKey(this.Handle, Keys.F4, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF4); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey5.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox5.Text == "   ")
            {
                hotkey6.Dispose();
            }
            else
            {
                hotkey6 = new HotKey(this.Handle, Keys.F5, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF5); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey6.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox6.Text == "   ")
            {
                hotkey7.Dispose();
            }
            else
            {
                hotkey7 = new HotKey(this.Handle, Keys.F6, Keys.None);//內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF6); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey7.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox7.Text == "   ")
            {
                hotkey8.Dispose();
            }
            else
            {
                hotkey8 = new HotKey(this.Handle, Keys.F7, Keys.None);//內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF7); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey8.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox8.Text == "   ")
            {
                hotkey9.Dispose();
            }
            else
            {
                hotkey9 = new HotKey(this.Handle, Keys.F8, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF8); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey9.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox9.Text == "   ")
            {
                hotkey10.Dispose();
            }
            else
            {
                hotkey10 = new HotKey(this.Handle, Keys.F9, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF9); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey10.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox10.Text == "   ")
            {
                hotkey11.Dispose();
            }
            else
            {
                hotkey11 = new HotKey(this.Handle, Keys.F10, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF10); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey11.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox11.Text == "   ")
            {
                hotkey12.Dispose();
            }
            else
            {
                hotkey12 = new HotKey(this.Handle, Keys.F11, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF11); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey12.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if (comboBox12.Text == "   ")
            {
                hotkey13.Dispose();
            }
            else
            {
                hotkey13 = new HotKey(this.Handle, Keys.F12, Keys.None); //內容跟新 再做一次選項是否為空值 空值就註銷熱鍵
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkeyF12); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(btn_OCR1); //獨立事件
                hotkey13.OnHotkey += new HotKey.HotkeyEventHandler(hotkey9_OnHotkey); //獨立事件
            }
            if(bck_box1==true || bck_box2==true|| bck_box3 ==true)
            {
                hotkeydispose();
                hotkeycas();
            }
            MessageBox.Show("儲存成功");
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        public bool bck_box1;//判斷有沒有勾選
        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if ((checkBox1.Checked).ToString() == "True")
                bck_box1 = true;
            else
                bck_box1 = false;
            hotkeydispose();//註銷熱鍵
            hotkeycas();//新增熱鍵
        }
        public bool bck_box2;//判斷有沒有勾選
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked.ToString() == "True")
                bck_box2 = true;
            else
                bck_box2 = false;
            hotkeydispose();//註銷熱鍵
            hotkeycas();//新增熱鍵
        }
        public bool bck_box3;//判斷有沒有勾選
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked.ToString() == "True")
                bck_box3 = true;
            else
                bck_box3 = false;
            hotkeydispose();//註銷熱鍵
            hotkeycas();//新增熱鍵
        }

        /// <summary>
        /// 調整圖片大小和對比度
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>

        private Bitmap ResizeImage(Image image, int width, int height)//分析圖片顯現在視窗上
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution * 2);//2,3
            if(hotkeycheck ==false)
            {
                if(btz ==true)
                {
                    image.Save(@"C:\ZMTemp\Preprocess_HighRes.jpg");
                }
                else
                {
                    image.Save(@"C:\ZMTemp\Preprocess_HighRes_office.jpg");
                }
            }
            

            using (var graphics = Graphics.FromImage(destImage))//如果你問我這是什麼我也不知道 我只知道這是處理圖片解析
            {
                
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.Clamp);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void ButtonZ_Click(object sender, EventArgs e)//診間按鈕
        {
            panel1.Visible = true;
            panel2.Visible = false;
            btz = false;
            ButtonZ.BackColor = Color.Black;//按鈕顏色
            ButtonZ.ForeColor = Color.White;
            buttonS.BackColor = Color.Gainsboro;
            buttonS.ForeColor = Color.Black;

            //tbResize.Visible = false;//RGB參數
            //tbRed.Visible = false;
            //tbGreen.Visible = false;
            //tbBlue.Visible = false;
            //tresize.Visible = true;
            //tred.Visible = true;
            //tgreen.Visible = true;
            //tblue.Visible = true;
        }

        public bool IsNumeric(String strNumber)
        {
            Regex NumberPattern = new Regex("[^0-9.-]");
            return !NumberPattern.IsMatch(strNumber);
        }

        private void ButtonHotKey_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void MainForm_Click(object sender, EventArgs e)
        {
            
        }

        public string combobox1;
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            combobox1 = comboBox1.Text;
        }
        public string combobox2;
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox2 = comboBox2.Text;
        }
        public string combobox3;
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox3 = comboBox3.Text;
        }
        public string combobox4;
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox4 = comboBox4.Text;
        }
        public string combobox5;
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox5 = comboBox5.Text;
        }
        public string combobox6;
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox6 = comboBox6.Text;
        }
        public string combobox7;
        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox7 = comboBox7.Text;
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)//耀聖資訊 展望資訊 方鼎資訊 常誠資料 醫聖資訊 開蘭安心 蒙利特
        {
            if(comboBox13.Text == "耀聖資訊")
            {
                testhis = "RS";
                textBox1.Enabled = false;
                button3.Enabled = false;
                checkBox4.Visible = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
            }
            if(comboBox13.Text == "展望資訊")
            {
                testhis = "VISW";
                textBox1.Enabled = true;
                button3.Enabled = false;
                checkBox4.Visible = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
            }
            if (comboBox13.Text == "方鼎資訊")
            {
                testhis = "TECH";
                textBox1.Enabled = false;
                checkBox4.Visible = true;
                checkBox5.Visible = true;
                checkBox6.Visible = true;
                if (sqlIP ==""&&sqldatabace ==""&&sqlID ==""&& sqlpw =="")
                {
                    Form3 lForm = new Form3();
                    lForm.Owner = this;//重要的一步，主要是使Form2的Owner指針指向Form1  
                    lForm.ShowDialog();
                }
                button3.Enabled = true;
                bool Return = WritePrivateProfileString("AppName", "Enableddb", button3.Enabled.ToString(), test);
            }
            if (comboBox13.Text == "常誠資料")
            {
                testhis = "DHA";
                textBox1.Enabled = false;
                button3.Enabled = false;
                checkBox4.Visible = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
            }
            if (comboBox13.Text == "醫聖資訊")
            {
                testhis = "SC";
                textBox1.Enabled = false;
                button3.Enabled = false;
                checkBox4.Visible = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
            }
            if (comboBox13.Text == "開蘭安心")
            {
                testhis = "KN";
                textBox1.Enabled = false;
                button3.Enabled = false;
                checkBox4.Visible = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
            }
            if (comboBox13.Text == "蒙利特")
            {
                testhis = "MTR";
                textBox1.Enabled = false;
                button3.Enabled = false;
                checkBox4.Visible = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
            }
        }

        private void comboBox14_SelectedIndexChanged(object sender, EventArgs e)
        {
            Server = comboBox14.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0); //關閉所有程式
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MainForm_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void 關閉一鍵連結ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)//登入帳號按鈕
        {
            Form1 lForm = new Form1();
            lForm.Owner = this;//重要的一步，主要是使Form2的Owner指標指向Form1
            lForm.String1 = hisid;//設定Form2中string1的值
            lForm.SetValue();//設定Form2中Label1的
            lForm.ShowDialog();
            drId = strValue;//顯示返回的值
            this.Text = hisid + "_" + hospname + "_" + publishVersion + "_" + drId;
        }

        private void comboBox15_SelectedIndexChanged(object sender, EventArgs e)//string ur6 = (tel != "") ?  tel : "";
        {
            
        }
        

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox4.Checked ==true&& checkBox6.Checked ==true)
            {
                checkBox6.Checked = false;
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true && checkBox6.Checked == true)
            {
                checkBox4.Checked = false;
            }
        }
        public string combobox8;
        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox8 = comboBox8.Text;
        }

        private void button3_Click(object sender, EventArgs e)//方頂專用按鈕剛改資料庫按鈕
        {
            Form3 lForm = new Form3();
            lForm.Owner = this;
            lForm.ShowDialog();
        }

        public string combobox9;
        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox9 = comboBox9.Text;
        }
        public string combobox10;
        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox10 = comboBox10.Text;
        }
        public string combobox11;
        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox11 = comboBox11.Text;
        }
        public string combobox12;
        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            combobox12 = comboBox12.Text;
        }
       
        private void buttonC_Click_1(object sender, EventArgs e)//截圖鈕
        {
            btn_OCR.Enabled = true;
            if(btz ==true)//區分是傑掛號還是診間按鈕
            {
                // 要重設座標故先把檔案刪除
                File.Delete(@"C:\ZMTemp\TestFile.TXT");
                if (this.WindowState != FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                    Thread.Sleep(200);
                }
                int swidth = Screen.PrimaryScreen.Bounds.Width;
                int sheight = Screen.PrimaryScreen.Bounds.Height;
                Bitmap btm = new Bitmap(swidth, sheight); //空圖與螢幕同大小
                Graphics g = Graphics.FromImage(btm); //空圖的畫板
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(swidth, sheight)); //將螢幕內容複製到空圖

                Cutter cutter = new Cutter(btm, btz); //傳送截圖

                if (!File.Exists(@"C:\ZMTemp\TestFile.TXT"))
                {
                    //cutter.FormBorderStyle = FormBorderStyle.None; //截圖全屏，無邊框
                    cutter.BackgroundImage = btm; //新的窗體截圖做背景
                    cutter.Show();
                    cutter.TopMost = true;
                }
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                // 要重設座標故先把檔案刪除
                File.Delete(@"C:\ZMTemp\TestFile_office.TXT");
                if (this.WindowState != FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                    Thread.Sleep(200);
                }
                int swidth = Screen.PrimaryScreen.Bounds.Width;
                int sheight = Screen.PrimaryScreen.Bounds.Height;
                Bitmap btm = new Bitmap(swidth, sheight); //空圖與螢幕同大小
                Graphics g = Graphics.FromImage(btm); //空圖的畫板
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(swidth, sheight)); //將螢幕內容複製到空圖

                Cutter cutter = new Cutter(btm, btz); //傳送截圖

                if (!File.Exists(@"C:\ZMTemp\TestFile_office.TXT"))
                {
                    //cutter.FormBorderStyle = FormBorderStyle.None; //截圖全屏，無邊框
                    cutter.BackgroundImage = btm; //新的窗體截圖做背景
                    cutter.Show();
                    cutter.TopMost = true;
                }
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {

        }


        private void LoadImageList()//把解析圖片存在ZMTemp裡面
        {
            if(btz ==true)
            {
                FileStream photo = File.OpenRead(@"C:\ZMTemp\CaptureImage.jpg");
                pictureBox1.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();

                photo = File.OpenRead(@"C:\ZMTemp\Preprocess_HighRes.jpg");
                pictureBox2.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();

                photo = File.OpenRead(@"C:\ZMTemp\Preprocess_Resize.jpg");
                pictureBox3.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();

                photo = File.OpenRead(@"C:\ZMTemp\BWImage.jpg");
                pictureBox4.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();
            }
            else
            {
                FileStream photo = File.OpenRead(@"C:\ZMTemp\CaptureImage_office.jpg");
                pictureBox1.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();

                photo = File.OpenRead(@"C:\ZMTemp\Preprocess_HighRes_office.jpg");
                pictureBox2.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();

                photo = File.OpenRead(@"C:\ZMTemp\Preprocess_Resize_office.jpg");
                pictureBox3.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();

                photo = File.OpenRead(@"C:\ZMTemp\BWImage_office.jpg");
                pictureBox4.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
                photo.Close();
            }
        }
        void f2_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Show();
        }
        //private void mytimer_Tick(object sender, EventArgs e)
        //{
        //    GetCurrentWindow();//取得活動視窗
        //}
        //private void GetCurrentWindow()
        //{
        //    Int32 handle = 0;
        //    StringBuilder sb = new StringBuilder(256);
        //    handle = GetForegroundWindow();
        //    if (GetWindowText(handle, sb, sb.Capacity) > 0)
        //    {
        //        str1 = sb.ToString();
        //    }
        //}
        public void UserHotkey()//設定視窗放大縮小熱鍵值
        {
            if (!String.IsNullOrEmpty(comboBox15.Text) && !String.IsNullOrEmpty(comboBox16.Text))
            {
                if (comboBox15.Text == "Alt")
                {
                    int intNum = Convert.ToInt32(comboBox16.Text);
                    switch (intNum)
                    {
                        case 0:
                            hotkey1 = new HotKey(this.Handle, Keys.D0, Keys.Alt); //註冊Alt + 0為熱鍵
                            break;
                        case 1:
                            hotkey1 = new HotKey(this.Handle, Keys.D1, Keys.Alt); //註冊Alt + 1為熱鍵
                            break;
                        case 2:
                            hotkey1 = new HotKey(this.Handle, Keys.D2, Keys.Alt); //註冊Alt + 2為熱鍵
                            break;
                        case 3:
                            hotkey1 = new HotKey(this.Handle, Keys.D3, Keys.Alt); //註冊Alt + 3為熱鍵
                            break;
                        case 4:
                            hotkey1 = new HotKey(this.Handle, Keys.D4, Keys.Alt); //註冊Alt + 4為熱鍵
                            break;
                        case 5:
                            hotkey1 = new HotKey(this.Handle, Keys.D5, Keys.Alt); //註冊Alt + 5為熱鍵
                            break;
                        case 6:
                            hotkey1 = new HotKey(this.Handle, Keys.D6, Keys.Alt); //註冊Alt + 6為熱鍵
                            break;
                        case 7:
                            hotkey1 = new HotKey(this.Handle, Keys.D7, Keys.Alt); //註冊Alt + 7為熱鍵
                            break;
                        case 8:
                            hotkey1 = new HotKey(this.Handle, Keys.D8, Keys.Alt); //註冊Alt + 8為熱鍵
                            break;
                        case 9:
                            hotkey1 = new HotKey(this.Handle, Keys.D9, Keys.Alt); //註冊Alt + 9為熱鍵
                            break;
                    }
                }
                if (comboBox15.Text == "Ctrl")
                {
                    int intNum = Convert.ToInt32(comboBox16.Text);
                    switch (intNum)
                    {
                        case 0:
                            hotkey1 = new HotKey(this.Handle, Keys.D0, Keys.Control); //註冊Alt + 0為熱鍵
                            break;
                        case 1:
                            hotkey1 = new HotKey(this.Handle, Keys.D1, Keys.Control); //註冊Alt + 1為熱鍵
                            break;
                        case 2:
                            hotkey1 = new HotKey(this.Handle, Keys.D2, Keys.Control); //註冊Alt + 2為熱鍵
                            break;
                        case 3:
                            hotkey1 = new HotKey(this.Handle, Keys.D3, Keys.Control); //註冊Alt + 3為熱鍵
                            break;
                        case 4:
                            hotkey1 = new HotKey(this.Handle, Keys.D4, Keys.Control); //註冊Alt + 4為熱鍵
                            break;
                        case 5:
                            hotkey1 = new HotKey(this.Handle, Keys.D5, Keys.Control); //註冊Alt + 5為熱鍵
                            break;
                        case 6:
                            hotkey1 = new HotKey(this.Handle, Keys.D6, Keys.Control); //註冊Alt + 6為熱鍵
                            break;
                        case 7:
                            hotkey1 = new HotKey(this.Handle, Keys.D7, Keys.Control); //註冊Alt + 7為熱鍵
                            break;
                        case 8:
                            hotkey1 = new HotKey(this.Handle, Keys.D8, Keys.Control); //註冊Alt + 8為熱鍵
                            break;
                        case 9:
                            hotkey1 = new HotKey(this.Handle, Keys.D9, Keys.Control); //註冊Alt + 9為熱鍵
                            break;
                    }
                }
                if (comboBox15.Text == "Shift")
                {
                    int intNum = Convert.ToInt32(comboBox16.Text);
                    switch (intNum)
                    {
                        case 0:
                            hotkey1 = new HotKey(this.Handle, Keys.D0, Keys.Shift); //註冊Alt + 0為熱鍵
                            break;
                        case 1:
                            hotkey1 = new HotKey(this.Handle, Keys.D1, Keys.Shift); //註冊Alt + 1為熱鍵
                            break;
                        case 2:
                            hotkey1 = new HotKey(this.Handle, Keys.D2, Keys.Shift); //註冊Alt + 2為熱鍵
                            break;
                        case 3:
                            hotkey1 = new HotKey(this.Handle, Keys.D3, Keys.Shift); //註冊Alt + 3為熱鍵
                            break;
                        case 4:
                            hotkey1 = new HotKey(this.Handle, Keys.D4, Keys.Shift); //註冊Alt + 4為熱鍵
                            break;
                        case 5:
                            hotkey1 = new HotKey(this.Handle, Keys.D5, Keys.Shift); //註冊Alt + 5為熱鍵
                            break;
                        case 6:
                            hotkey1 = new HotKey(this.Handle, Keys.D6, Keys.Shift); //註冊Alt + 6為熱鍵
                            break;
                        case 7:
                            hotkey1 = new HotKey(this.Handle, Keys.D7, Keys.Shift); //註冊Alt + 7為熱鍵
                            break;
                        case 8:
                            hotkey1 = new HotKey(this.Handle, Keys.D8, Keys.Shift); //註冊Alt + 8為熱鍵
                            break;
                        case 9:
                            hotkey1 = new HotKey(this.Handle, Keys.D9, Keys.Shift); //註冊Alt + 9為熱鍵
                            break;
                    }
                }
                if(comboBox15.Text ==""||comboBox16.Text =="")
                {
                     hotkey1 = new HotKey(this.Handle, Keys.D, Keys.Control); //註冊CTRL + D為熱鍵
                }
            }
            else//沒設定 預設是CTRL+D
            {
                hotkey1 = new HotKey(this.Handle, Keys.D, Keys.Control); //註冊CTRL + D為熱鍵
            }

            hotkey1.OnHotkey += new HotKey.HotkeyEventHandler(hotkey1to4_OnHotkey); //獨立事件
        }
    }
}