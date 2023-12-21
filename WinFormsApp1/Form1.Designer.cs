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
            text_ip=new TextBox();
            button_connect=new Button();
            btn_send=new Button();
            SuspendLayout();
            // 
            // text_ip
            // 
            text_ip.Location=new Point(130, 79);
            text_ip.Name="text_ip";
            text_ip.Size=new Size(342, 23);
            text_ip.TabIndex=0;
            // 
            // button_connect
            // 
            button_connect.Location=new Point(279, 145);
            button_connect.Name="button_connect";
            button_connect.Size=new Size(75, 23);
            button_connect.TabIndex=1;
            button_connect.Text="连接";
            button_connect.UseVisualStyleBackColor=true;
            button_connect.Click+=button_connect_Click;
            // 
            // btn_send
            // 
            btn_send.Location=new Point(395, 147);
            btn_send.Name="btn_send";
            btn_send.Size=new Size(75, 23);
            btn_send.TabIndex=2;
            btn_send.Text="发送";
            btn_send.UseVisualStyleBackColor=true;
            btn_send.Click+=btn_send_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions=new SizeF(7F, 17F);
            AutoScaleMode=AutoScaleMode.Font;
            ClientSize=new Size(800, 450);
            Controls.Add(btn_send);
            Controls.Add(button_connect);
            Controls.Add(text_ip);
            Name="Form1";
            Text="Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox text_ip;
        private Button button_connect;
        private Button btn_send;
    }
}