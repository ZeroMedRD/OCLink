using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Tesseract;

namespace OCLink
{
    public partial class MainForm : Form
    {
        public int nRed = 0, nGreen = 0, nBlue = 0, nResize = 6;

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

        public MainForm()
        {
            InitializeComponent();            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            uint ctrlHotKey = (uint)(KeyModifiers.Alt | KeyModifiers.Ctrl);
            //uint ctrlHotKey = (uint)(KeyModifiers.Ctrl);
            // 註冊熱鍵為Alt+Ctrl+A, "100"為唯一標識熱鍵
            HotKey.RegisterHotKey(Handle, 100, ctrlHotKey, Keys.A);

            // 設定 F1~F12 Key 的內容
            // 此內容皆在後台做維護,不再由App來做選擇更新,以達一鍵連結的統一性
            // 設定資料皆在 HotKey4OCLink Table 做取得
            // 全部診所皆在此依診所別在此 Table 取得一鍵連結相關資訊

            // 下列參數則一律儲存在使用者端的 C:\ZeroMed 目錄內,若檔案不在或參數變數不在則一律帶預設值
            tbResize.Text = "3";
            tbRed.Text = "50";
            tbGreen.Text = "50";
            tbBlue.Text = "50";
        }

        private void buttonS_Click(object sender, EventArgs e)
        {
            // 要重設座標故先把檔案刪除
            File.Delete(@"C:\Temp\TestFile.TXT");

            //if (!File.Exists(@"C:\Temp\TestFile.TXT"))
            //{
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

            Cutter cutter = new Cutter(btm); //傳送截圖
            
            if (!File.Exists(@"C:\Temp\TestFile.TXT"))
            {
                //cutter.FormBorderStyle = FormBorderStyle.None; //截圖全屏，無邊框
                cutter.BackgroundImage = btm; //新的窗體截圖做背景
                cutter.Show();
                cutter.TopMost = true;
            }

            //File.Delete(@"C:\Temp\BWImage.jpg");
            //File.Delete(@"C:\Temp\CaptureImage.jpg");
            //File.Delete(@"C:\Temp\Preprocess_HighRes.jpg");
            //File.Delete(@"C:\Temp\Preprocess_Resize.jpg");

            //cutter.FormBorderStyle = FormBorderStyle.None; //截圖全屏，無邊框            
            //cutter.BackgroundImage = btm;                    //新的窗體截圖做背景
            //cutter.Show();
            //}
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 100);
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

        // OCR 測試結果顯示
        private void btn_OCR_Click(object sender, EventArgs e)
        {
            string str = GetOCR();

            MessageBox.Show(str);
        }

        private string GetOCR()
        {
            // 依畫面再重新截圖
            int swidth = Screen.PrimaryScreen.Bounds.Width;
            int sheight = Screen.PrimaryScreen.Bounds.Height;
            Bitmap btm = new Bitmap(swidth, sheight); //空圖與螢幕同大小
            Graphics g = Graphics.FromImage(btm); //空圖的畫板
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(swidth, sheight)); //將螢幕內容複製到空圖

            Cutter cutter = new Cutter(btm); //傳送截圖

            if (!File.Exists(@"C:\Temp\TestFile.TXT"))
            {
                //cutter.FormBorderStyle = FormBorderStyle.None; //截圖全屏，無邊框
                cutter.BackgroundImage = btm; //新的窗體截圖做背景
                cutter.Show();
            }

            // 開始OCR
            //string s = ocr.Recognize(@"C:\Temp\CaptureImage.jpg", -1, startX, startY, width, height, AspriseOCR.RECOGNIZE_TYPE_ALL, AspriseOCR.OUTPUT_FORMAT_PLAINTEXT);
            TesseractEngine ocr;
            //ocr = new TesseractEngine("./tessdata", "chi_tra");//設定語言   中文
            ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);//設定語言   英文

            string str = String.Empty;

            //匯入圖片進行識別
            File.Delete(@"C:\Temp\BWImage.jpg");
            File.Delete(@"C:\Temp\Preprocess_HighRes.jpg");
            File.Delete(@"C:\Temp\Preprocess_Resize.jpg");

            //Bitmap bit = new Bitmap(Image.FromFile(@"C:\Temp\CaptureImage.jpg"));
            using (var fs = new System.IO.FileStream(@"C:\Temp\CaptureImage.jpg", System.IO.FileMode.Open))
            {
                var bmp = new Bitmap(fs);
                fs.Close();
                Bitmap bit = (Bitmap)bmp.Clone();

                if (File.Exists(@"C:\Temp\BWImage.jpg"))
                {
                    bit = new Bitmap(Image.FromFile(@"C:\Temp\BWImage.jpg"));
                }
                bit = PreprocesImage(bit, false);
                Page page = ocr.Process(bit);
                str = page.GetText();//識別後的內容

                page.Dispose();
                bit.Dispose();
                ocr.Dispose();

                LoadImageList();
            }

            return (str);
        }

        /// <summary>
        /// 圖片顏色區分，剩下白色和黑色
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap PreprocesImage(Bitmap image, bool lReverse)
        {
            Color cForeColor = Color.White;
            Color cBackColor = Color.Black;

            if (lReverse == true)
            {
                cForeColor = Color.Black;
                cBackColor = Color.White;
            }

            //You can change your new color here. Red,Green,LawnGreen any..
            Color actualColor;

            //make an empty bitmap the same size as scrBitmap
            nResize = (IsNumeric(tbResize.Text)) ? int.Parse(tbResize.Text.Trim()) : 1;
            image = ResizeImage(image, image.Width * nResize, image.Height * nResize);
            image.Save(@"C:\Temp\Preprocess_Resize.jpg");

            Bitmap newBitmap = new Bitmap(image.Width, image.Height);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    //get the pixel from the scrBitmap image
                    actualColor = image.GetPixel(i, j);
                    // > 150 because.. Images edges can be of low pixel colr. if we set all pixel color to new then there will be no smoothness left.

                    // 判斷 RGB 值
                    nRed = (IsNumeric(tbRed.Text)) ? int.Parse(tbRed.Text.Trim()) : 0;
                    nGreen = (IsNumeric(tbGreen.Text)) ? int.Parse(tbRed.Text.Trim()) : 0;
                    nBlue = (IsNumeric(tbBlue.Text)) ? int.Parse(tbRed.Text.Trim()) : 0;

                    if (actualColor.R > nRed && actualColor.G > nGreen && actualColor.B > nBlue)    //在這裡設定RGB
                        newBitmap.SetPixel(i, j, cForeColor);
                    else
                        newBitmap.SetPixel(i, j, cBackColor);
                }
            }

            newBitmap = ResizeImage(newBitmap, newBitmap.Width, newBitmap.Height);

            newBitmap.Save(@"C:\Temp\BWImage.jpg");

            return newBitmap;
        }
        
        /// <summary>
        /// 調整圖片大小和對比度
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution * 2);//2,3
            image.Save(@"C:\Temp\Preprocess_HighRes.jpg");

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceOver;
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

        public bool IsNumeric(String strNumber)
        {
            Regex NumberPattern = new Regex("[^0-9.-]");
            return !NumberPattern.IsMatch(strNumber);
        }

        private void LoadImageList()
        {
            FileStream photo = File.OpenRead(@"C:\Temp\CaptureImage.jpg");
            pictureBox1.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
            photo.Close();

            photo = File.OpenRead(@"C:\Temp\Preprocess_HighRes.jpg");
            pictureBox2.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
            photo.Close();

            photo = File.OpenRead(@"C:\Temp\Preprocess_Resize.jpg");
            pictureBox3.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
            photo.Close();

            photo = File.OpenRead(@"C:\Temp\BWImage.jpg");
            pictureBox4.Image = (System.Drawing.Bitmap)Image.FromStream(photo);
            photo.Close();
        }
    }
}
