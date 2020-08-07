using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCLink
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        public string hisid;
        public Form1()
        {
            InitializeComponent();
            this.Text = "使用者登入";
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
        }
        private string string1;
        public string String1
        {
            set
            {
                string1 = value;
            }
        }
        public void SetValue()
        {
            hisid = string1;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string sql = "server=23.97.65.134,1933;database=his" + hisid + ";user=sa;password=I@ntif@t;";
            string strcon = "select top 1 * from employee where emp_account = '" + textBox1.Text + "'";
            SqlConnection conn = new SqlConnection(sql);
            conn.Open();
            SqlCommand cmd = new SqlCommand(strcon, conn);
            SqlDataReader clinic = cmd.ExecuteReader(); 
            while (clinic.Read())
            {
                if (textBox2.Text == clinic["pwd"].ToString())
                {
                    MainForm mF = (MainForm)this.Owner;//把Form2的父視窗指標賦給lForm1
                    mF.StrValue = textBox1.Text;//使用父視窗指標賦值
                    MessageBox.Show("登入成功");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("帳號或密碼錯誤 請重新輸入");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string test = @"C:\ZMTemp\System.ini";
            StringBuilder sb = new StringBuilder(500);
            uint res1 = GetPrivateProfileString("AppName", "登入帳號", "", sb, (uint)sb.Capacity, test);
            MainForm mF = (MainForm)this.Owner;//把Form2的父視窗指標賦給lForm1
            mF.StrValue = sb.ToString();//使用父視窗指標賦值
            this.Close();
        }
    }
}
