using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCLink
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm lForm1 = (MainForm)this.Owner;//把Form2的父窗口指針賦給lForm1  
            lForm1.StrValueIP = textBox1.Text.Trim();//使用父窗口指針賦值 
            lForm1.StrValuedb = textBox2.Text.Trim();//使用父窗口指針賦值
            lForm1.StrValueid = textBox3.Text.Trim();//使用父窗口指針賦值
            lForm1.StrValuepw = textBox4.Text.Trim();//使用父窗口指針賦值
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
