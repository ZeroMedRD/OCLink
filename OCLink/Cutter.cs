using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Runtime.InteropServices;

namespace OCLink
{
    public partial class Cutter : Form
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        Bitmap screenBtmp = null; //電腦螢幕的截圖
        bool btz1;
        public Cutter(Bitmap btm,bool btz)
        {
            InitializeComponent();
            btz1 = btz;
            screenBtmp = btm;
            //Rectangle catchRec;//存放擷取範圍

            try
            {
                // Create an instance of StreamReader to read from a file.'

                // The using statement also closes the StreamReader.
                if(btz1 == true)
                {
                    using (StreamReader sr = new StreamReader(@"C:\ZMTemp\TestFile.TXT"))     //小寫TXT
                    {
                        String line;
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        line = sr.ReadLine();
                        var matches = Regex.Match(line,
                               @"\D*(\d+)\D*(\d+)\D*(\d+)\D*(\d+)");

                        catchRec = new Rectangle(int.Parse(matches.Groups[1].Value),
                             int.Parse(matches.Groups[2].Value),
                             int.Parse(matches.Groups[3].Value),
                             int.Parse(matches.Groups[4].Value));


                        CatchStart = false;
                        catchFinished = true;
                        //sr.Close();

                        CaptureHandle(false);
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(@"C:\ZMTemp\TestFile_office.TXT"))     //小寫TXT
                    {
                        String line;
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        line = sr.ReadLine();
                        var matches = Regex.Match(line,
                               @"\D*(\d+)\D*(\d+)\D*(\d+)\D*(\d+)");

                        catchRec = new Rectangle(int.Parse(matches.Groups[1].Value),
                             int.Parse(matches.Groups[2].Value),
                             int.Parse(matches.Groups[3].Value),
                             int.Parse(matches.Groups[4].Value));


                        CatchStart = false;
                        catchFinished = true;
                        //sr.Close();

                        CaptureHandle(false);
                    }
                }
               
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                //MessageBox.Show("參數檔不存在或檔案損壞，請重新設定座標 !" + e.ToString());
                //Console.WriteLine(e.Message);
            }
        }

        private void Cutter_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        bool CatchStart = false; //自由截圖開始
        Point downPoint; //初始點

        //滑鼠左鍵按下-開始自由截圖
        private void Cutter_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!CatchStart)
                {
                    CatchStart = true;
                    downPoint = new Point(e.X, e.Y); //初始點
                }
            }
        }

        Rectangle catchRec;//存放擷取範圍

        //滑鼠移動-繪製自由截圖路徑
        private void Cutter_MouseMove(object sender, MouseEventArgs e)
        {
            //路徑繪製，核心
            if (CatchStart)
            {
                //
                //二次緩衝
                //不是直接在控制元件的背景畫板上進行繪製滑鼠移動路徑，那樣會造成繪製很多路徑，因為前面繪製的路徑還在
                //而是在記憶體中每移動一次滑鼠就建立一張和螢幕截圖一樣的新BImtap,在這個Bitmap中繪製滑鼠移動路徑
                //然後在窗體背景畫板上，繪製這個新的Bitmap,這樣就不會造成繪製很多路徑，因為每次都繪製了全新的Bitmao
                //但是這樣做的話，因為滑鼠移動的次數是大量的，所以在記憶體中會建立大量的Bitmap會造成記憶體消耗嚴重，所以每次移動繪製完後，
                //需要釋放Dispose() 畫板，畫筆，Bitmap資源。
                //
                Bitmap copyBtmp = (Bitmap)screenBtmp.Clone(); //建立新的,在其上繪製路徑
                //左上角
                Point firstP = new Point(downPoint.X, downPoint.Y);
                //新建畫板，畫筆
                Graphics g = Graphics.FromImage(copyBtmp);
                Pen p = new Pen(Color.Red, 1);
                //計算路徑範圍
                int width = Math.Abs(e.X - downPoint.X);
                int height = Math.Abs(e.Y - downPoint.Y);
                if (e.X < downPoint.X)
                {
                    firstP.X = e.X;
                }
                if (e.Y < downPoint.Y)
                {
                    firstP.Y = e.Y;
                }
                //繪製路徑
                catchRec = new Rectangle(firstP, new Size(width, height));
                //將路徑繪製在新的BItmap上，之後要釋放
                g.DrawRectangle(p, catchRec);
                g.Dispose();
                p.Dispose();

                //窗體背景畫板
                Graphics gf = this.CreateGraphics();
                //將新圖繪製在窗體的畫板上   --   自由截圖-路徑繪製處，其實還是一張和螢幕同樣大小的圖片，只不過上面有紅色的選擇路徑
                gf.DrawImage(copyBtmp, new Point(0, 0));
                gf.Dispose();
                //釋放記憶體Bimtap
                copyBtmp.Dispose();
            }
        }

        bool catchFinished = false; //自由截圖結束標誌

        //滑鼠左鍵彈起-結束自由截圖
        private void Cutter_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (CatchStart)
                {
                    CatchStart = false;
                    catchFinished = true;
                }
            }
        }

        //滑鼠左鍵雙擊，儲存自由擷取的圖片
        private void Cutter_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && catchFinished)
            {
                CaptureHandle(true);
            }
        }

        private void CaptureHandle(bool bEvent)
        {
            //建立使用者擷取的範圍大小的空圖
            Bitmap catchBtmp = new Bitmap(catchRec.Width, catchRec.Height);
            Graphics g = Graphics.FromImage(catchBtmp);
            //在原始的螢幕截圖ScreenBitmap上 擷取 使用者選擇範圍大小的區域   繪製到上面的空圖
            //繪製完後，這個空圖就是我們想要的擷取的圖片
            //引數1  原圖
            //引數2  在空圖上繪製的範圍區域
            //引數3  原圖的擷取範圍
            //引數4  度量單位
            g.DrawImage(screenBtmp, new Rectangle(0, 0, catchRec.Width, catchRec.Height), catchRec, GraphicsUnit.Pixel);

            //將自由擷取的圖片儲存到剪下板中
            Clipboard.Clear();
            Clipboard.SetImage(catchBtmp);

            //MessageBox.Show(catchRec.ToString());

            if (Clipboard.ContainsImage())
            {
                bool Return;
                if(btz1 == true)
                {
                    if (!Directory.Exists(@"C:\ZMTemp")) { Directory.CreateDirectory(@"C:\ZMTemp"); }
                    Clipboard.GetImage().Save(@"C:\ZMTemp\CaptureImage.jpg");
                    //Clipboard.SetText($"![image](/posts/2018/{Path.GetFileName(fileName)})");
                    string test = @"C:\ZMTemp\System.ini";
                    if (bEvent)
                    {
                        // 將選取的矩形座標存入參數檔內
                        using (StreamWriter sw = new StreamWriter(@"C:\ZMTemp\TestFile.TXT"))
                        {
                            // Add some text to the file.
                            sw.Write(catchRec.ToString());
                            Return = WritePrivateProfileString("AppName", "REG", catchRec.ToString(), test);
                            //sw.Write(catchRec.X.ToString() + Environment.NewLine);
                            //sw.Write(catchRec.Y.ToString() + Environment.NewLine);
                            //sw.Write(catchRec.Width.ToString() + Environment.NewLine);
                            //sw.Write(catchRec.Height.ToString());
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(@"C:\ZMTemp")) { Directory.CreateDirectory(@"C:\ZMTemp"); }
                    Clipboard.GetImage().Save(@"C:\ZMTemp\CaptureImage_office.jpg");
                    //Clipboard.SetText($"![image](/posts/2018/{Path.GetFileName(fileName)})");
                    string test = @"C:\ZMTemp\System.ini";

                    if (bEvent)
                    {
                        // 將選取的矩形座標存入參數檔內
                        using (StreamWriter sw = new StreamWriter(@"C:\ZMTemp\TestFile_office.TXT"))
                        {
                            // Add some text to the file.
                            sw.Write(catchRec.ToString());
                            Return = WritePrivateProfileString("AppName", "OPD", catchRec.ToString(), test);
                            //sw.Write(catchRec.X.ToString() + Environment.NewLine);
                            //sw.Write(catchRec.Y.ToString() + Environment.NewLine);
                            //sw.Write(catchRec.Width.ToString() + Environment.NewLine);
                            //sw.Write(catchRec.Height.ToString());
                        }
                    }
                }
               
            }

            g.Dispose();
            catchFinished = false;
            this.BackgroundImage = screenBtmp;
            catchBtmp.Dispose();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cutter_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.Focus();
        }

        private void btnCutter_Click(object sender, EventArgs e)
        {
            if (catchFinished)
            {
                CaptureHandle(true);
            }
        }
    }
}
