using Fleck;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load +=Form1_Load;
        }
        private const int PORT = 5001;
        private const int INTERVAL_MS = 1000; // 定时抓取间隔，单位毫秒
        public static List<IWebSocketConnection> socketConnection;  //socket连接池

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

        }

        private static Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }
            return screenshot;
        }

        private static byte[] ConvertBitmapToBytes(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            var connectStr = "ws://"+text_ip.Text+":5001/";
            var socketserver = new WebSocketServer(connectStr);
            socketserver.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    if (socketConnection == null)
                        socketConnection = new List<IWebSocketConnection>();
                    socketConnection.Add(socket);
                };

                socket.OnClose = () =>
                {
                    socketConnection.Remove(socket);
                };

                socket.OnMessage = message =>
                {
                };
            });
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            Bitmap screenshot = CaptureScreen(); // 抓取屏幕截图
            byte[] imageData = ConvertBitmapToBytes(screenshot); // 将Bitmap转换为byte数组
            socketConnection[0].Send(imageData);
            Console.WriteLine("{0} bytes sent.", imageData.Length);
        }
    }
}