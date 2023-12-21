using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.text_ip = new System.Windows.Forms.TextBox();
            this.button_connect = new System.Windows.Forms.Button();
            this.btn_send = new System.Windows.Forms.Button();
            this.text_info = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // text_ip
            // 
            this.text_ip.Location = new System.Drawing.Point(111, 56);
            this.text_ip.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.text_ip.Name = "text_ip";
            this.text_ip.Size = new System.Drawing.Size(294, 21);
            this.text_ip.TabIndex = 0;
            this.text_ip.Text = "192.168.92.128";
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(239, 102);
            this.button_connect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(64, 29);
            this.button_connect.TabIndex = 1;
            this.button_connect.Text = "连接";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button_connect_Click_1);
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(339, 104);
            this.btn_send.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(64, 27);
            this.btn_send.TabIndex = 2;
            this.btn_send.Text = "发送";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.btn_send_Click);
            // 
            // text_info
            // 
            this.text_info.Location = new System.Drawing.Point(111, 186);
            this.text_info.Multiline = true;
            this.text_info.Name = "text_info";
            this.text_info.Size = new System.Drawing.Size(294, 98);
            this.text_info.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.AllowDrop = true;
            this.button1.Location = new System.Drawing.Point(458, 56);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 318);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.text_info);
            this.Controls.Add(this.btn_send);
            this.Controls.Add(this.button_connect);
            this.Controls.Add(this.text_ip);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox text_ip;
        private Button button_connect;
        private Button btn_send;
        private TextBox text_info;
        private Button button1;
    }
}