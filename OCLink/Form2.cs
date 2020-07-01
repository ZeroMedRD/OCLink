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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.Text = "登入授權";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        public Form2(string mac)
        {
            InitializeComponent();
            textBox1.Text = mac;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0); //若認證不過關閉所有程式
        }
    }
}
