﻿namespace OCLink
{
    partial class Cutter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCutter = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCutter
            // 
            this.btnCutter.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCutter.Location = new System.Drawing.Point(1315, 748);
            this.btnCutter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCutter.Name = "btnCutter";
            this.btnCutter.Size = new System.Drawing.Size(91, 50);
            this.btnCutter.TabIndex = 0;
            this.btnCutter.Text = "截圖";
            this.btnCutter.UseVisualStyleBackColor = true;
            this.btnCutter.Click += new System.EventHandler(this.btnCutter_Click);
            // 
            // Cutter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 809);
            this.Controls.Add(this.btnCutter);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Cutter";
            this.Text = "Cutters";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Cutter_FormClosing);
            this.Load += new System.EventHandler(this.Cutter_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Cutter_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Cutter_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Cutter_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Cutter_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Cutter_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCutter;
    }
}