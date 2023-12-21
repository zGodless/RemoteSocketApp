using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace 虚拟机发送端
{
    public partial class FormPoint : Form
    {
        //public delegate void showPoint(Point point);
        public FormPoint()
        {
            InitializeComponent();
            Load +=FormPoint_Load;
        }

        private void FormPoint_Load(object sender, EventArgs e)
        {
        }

        public void showPoint(string message)
        {
            if (labelPoint.InvokeRequired)
            {
                labelPoint.Invoke(new Action<string>(showPoint), message);
            }
            else
            {
                labelPoint.Text = message;
            }
        }
    }
}
