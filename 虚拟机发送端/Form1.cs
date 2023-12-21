using Fleck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using ��������Ͷ�;
using System.Runtime.InteropServices;
using log4net;
using log4net.Config;
using System.Web.UI.WebControls;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // ���� user32.dll ���еĺ���
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        //�������λ��
        [DllImport("user32.dll")] //DllImpor��Է��йܵġ����й�ָ���ǲ�����.net ���ɵ�DLL
                                  //����һ���ⲿʵ�ַ���SetCursorPos()
        public static extern bool SetCursorPos(int X, int Y);


        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public MOUSEKEYBDHARDWAREINPUT mkhi;
        }

        public uint INPUT_TYPE_MOUSE = 0;
        public uint INPUT_TYPE_KEYBOARD = 1;
        public uint INPUT_TYPE_HARDWARE = 2;

        [StructLayout(LayoutKind.Explicit)]
        struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }


        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
        public int XBUTTON1 = 0x0001;//�����Ƿ��»��ͷŵ�һ�� X ��ť��
        public int XBUTTON2 = 0x0002;//�����Ƿ��»��ͷŵڶ��� X ��ť��

        public const int INPUT_MOUSE = 0;
        //�ƶ���� 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //ģ������������ 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //ģ��������̧�� 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //ģ������Ҽ����� 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //ģ������Ҽ�̧�� 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //ģ������м����� 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //ģ������м�̧�� 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //��ʾ�Ƿ���þ������� 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //ģ�������ֹ����������������dwData����
        const int MOUSEEVENTF_WHEEL = 0x0800;


        // ����һ����̬����־��¼��
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load; ;
            FormClosing +=Form1_FormClosing;
        }
        static Timer timer;
        private const int PORT = 5001;
        private const int INTERVAL_MS = 20; // ��ʱץȡ�������λ����
        public static List<IWebSocketConnection> socketConnection = new List<IWebSocketConnection>();  //socket���ӳ�
        public static WebSocketServer WebSocketServer = null;
        public static string connectStr = "";
        public bool isHoldMouse = false;//�Ƿ�ס����������ж���ק
        public Point oldPositon = new Point(0, 0);//ԭʼ���꣬������ק

        FormPoint formpoint = new FormPoint();
        private void Form1_Load(object sender, EventArgs e)
        {
            // ����log4net�����ļ�
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            // ��¼��־
            log.Info("��������");

            if (WebSocketServer == null && !string.IsNullOrEmpty(text_ip.Text))
            {
                connectStr = "ws://"+text_ip.Text+":"+PORT+"/";
                DoConnect(connectStr);
            }

            // ����һ�� Timer ����
            timer = new Timer();

            // ���ö�ʱ���ļ�� ����
            timer.Interval = INTERVAL_MS;

            // ���ö�ʱ���� Tick �¼��������
            timer.Tick += TimerTick;

            // ������ʱ��
            timer.Start();
        }

        private void Form1_showMsg(object sender, EventArgs e, string content)
        {

        }

        private static void TimerTick(object sender, EventArgs e)
        {
            if(WebSocketServer != null)
            {
                Bitmap screenshot = CaptureScreen(); // ץȡ��Ļ��ͼ
                byte[] imageData = ConvertBitmapToBytes(screenshot); // ��Bitmapת��Ϊbyte����
                if(socketConnection.Count > 0)
                {
                    socketConnection.ForEach(m => m.Send(imageData));
                }
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

        private void DoConnect(string cStr)
        {
            WebSocketServer = new WebSocketServer(cStr);
            WebSocketServer.Start(socket =>
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
                    var receiveModel = JsonConvert.DeserializeObject<ReceiveMsgModel>(message);


                    if (receiveModel != null)
                    {
                        if(receiveModel.type != MsgTypeEnum.msgTypeEnum.MouseMove)
                        {
                            log.Info($"������Ϣ��{message}");
                        }
                        switch(receiveModel.type)
                        {
                            case MsgTypeEnum.msgTypeEnum.MouseDown:
                                {
                                    // Ҫ���������
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    log.Info($"ִ�С�������������������������������¡���({x},{y})");
                                    isHoldMouse = true;
                                    oldPositon = new Point(x, y);//��¼�������
                                    //ClickMouse(x, y);
                                    MouseLeftDown(x, y);
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.MouseUp:
                                {
                                    // Ҫ���������
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    log.Info($"ִ�С�������Up�ϡ���odl:({oldPositon.X},{oldPositon.Y}), ({x},{y})");
                                    if (x != oldPositon.X || y != oldPositon.Y)
                                    {
                                        isHoldMouse = false;
                                        DragA2B(oldPositon, new Point(x, y));
                                    }
                                    else
                                    {
                                        MouseLeftUp(x, y);
                                    }
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.MouseMove:
                                {
                                    // ��ȡ��ǰ�������
                                    Point cursorPosition = Cursor.Position;
                                    // ��������
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    if (!isHoldMouse)
                                    {
                                        SetCursorPos(x, y);
                                    }
                                    else
                                    {
                                        //INPUT[] inputs = new INPUT[3];

                                        //inputs[0].type = INPUT_TYPE_MOUSE;
                                        //inputs[0].mkhi.mi = new MOUSEINPUT()
                                        //{
                                        //    dx = cursorPosition.X,
                                        //    dy = cursorPosition.Y,
                                        //    dwExtraInfo = IntPtr.Zero,
                                        //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN
                                        //};

                                        //inputs[1].type = INPUT_TYPE_MOUSE;
                                        //inputs[1].mkhi.mi = new MOUSEINPUT()
                                        //{
                                        //    dx = x,
                                        //    dy = y,
                                        //    dwExtraInfo = IntPtr.Zero,
                                        //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE
                                        //};

                                        //inputs[2].type = INPUT_TYPE_MOUSE;
                                        //inputs[2].mkhi.mi = new MOUSEINPUT()
                                        //{
                                        //    dx = x,
                                        //    dy = y,
                                        //    dwExtraInfo = IntPtr.Zero,
                                        //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP
                                        //};


                                        //INPUT[] inputs = new INPUT[1];
                                        //inputs[0].type = INPUT_TYPE_MOUSE;
                                        //inputs[0].mkhi.mi = new MOUSEINPUT()
                                        //{
                                        //    dx = x,
                                        //    dy = y,
                                        //    dwExtraInfo = IntPtr.Zero,
                                        //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE
                                        //};
                                        //var uSent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
                                        //log.Info($"ִ�С�SendInput�������:({uSent})");

                                    }
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.RightButton:
                                {
                                    // Ҫ���������
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    log.Info($"ִ�С��Ҽ��������({x},{y})");
                                    ClickMouseRight(x, y);
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.KeyUp:
                                {
                                    break;
                                    log.Info($"ִ�С�����̧�𡿣�keyCode:({receiveModel.key})");
                                    if (receiveModel.key < 49 || receiveModel.key > 90)
                                        break;
                                    // �����̴���ת��Ϊʵ�ʼ����ı�
                                    string keyText = Enum.GetName(typeof(Keys), receiveModel.key);
                                    // ���������Ͱ����¼�
                                    SendKeys.SendWait(keyText);
                                    break;
                                }
                            default: break;
                        }
                    }
                    this.Invoke(new Action(() =>
                    {
                        text_info.Text = (message + text_info.Text).Substring(0, Math.Min((message + text_info.Text).Length, 300));
                    }));
                    //text_info.Text = (message + text_info.Text).Substring(0, 300);
                };
            });
        }

        public void DragA2B(Point a, Point b)
        {
            INPUT[] inputs = new INPUT[4];

            //inputs[0].type = INPUT_TYPE_MOUSE;
            //inputs[0].mkhi.mi = new MOUSEINPUT()
            //{
            //    dx = a.X,
            //    dy = a.Y,
            //    dwExtraInfo = IntPtr.Zero,
            //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP
            //};

            //inputs[1].type = INPUT_TYPE_MOUSE;
            //inputs[1].mkhi.mi = new MOUSEINPUT()
            //{
            //    dx = a.X,
            //    dy = a.Y,
            //    dwExtraInfo = IntPtr.Zero,
            //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN
            //};

            //inputs[2].type = INPUT_TYPE_MOUSE;
            //inputs[2].mkhi.mi = new MOUSEINPUT()
            //{
            //    dx = b.X,
            //    dy = b.Y,
            //    dwExtraInfo = IntPtr.Zero,
            //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE
            //};

            //inputs[3].type = INPUT_TYPE_MOUSE;
            //inputs[3].mkhi.mi = new MOUSEINPUT()
            //{
            //    dx = b.X,
            //    dy = b.Y,
            //    dwExtraInfo = IntPtr.Zero,
            //    dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP
            //};

            //��궨λ
            SetCursorPos(b.X, b.Y);

            inputs[0].type = INPUT_TYPE_MOUSE;
            inputs[0].mkhi.mi = new MOUSEINPUT()
            {
                dx = b.X,
                dy = b.Y,
                dwExtraInfo = IntPtr.Zero,
                dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP
            };

            var uSent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            log.Info($"ִ�С�SendInput, DragA2B�������:({uSent})");
        }

        public void MouseLeftDown(int x, int y)
        {
            //// �������ת��Ϊ�޷�������
            //uint xPos = Convert.ToUInt32(x);
            //uint yPos = Convert.ToUInt32(y);

            ////��궨λ
            //SetCursorPos(x, y);

            //// ��������������¼�
            //mouse_event(MOUSEEVENTF_LEFTDOWN, xPos, yPos, 0, UIntPtr.Zero);

            //��궨λ
            SetCursorPos(x, y);
            INPUT[] inputs = new INPUT[1];

            inputs[0].type = INPUT_TYPE_MOUSE;
            inputs[0].mkhi.mi = new MOUSEINPUT()
            {
                dx = x,
                dy = y,
                dwExtraInfo = IntPtr.Zero,
                dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN
            };

            var uSent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            log.Info($"ִ�С�SendInput, MouseLeftDown�������:({uSent})");
        }
        public void MouseLeftUp(int x, int y)
        {
            //��궨λ
            SetCursorPos(x, y);
            INPUT[] inputs = new INPUT[1];

            inputs[0].type = INPUT_TYPE_MOUSE;
            inputs[0].mkhi.mi = new MOUSEINPUT()
            {
                dx = x,
                dy = y,
                dwExtraInfo = IntPtr.Zero,
                dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP
            };

            var uSent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            log.Info($"ִ�С�SendInput, MouseLeftUp�������:({uSent})");
        }

        static void ClickMouse(int x, int y)
        {
            // �������ת��Ϊ�޷�������
            uint xPos = Convert.ToUInt32(x);
            uint yPos = Convert.ToUInt32(y);

            //��궨λ
            SetCursorPos(x, y);

            // ��������������¼�
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, xPos, yPos, 0, UIntPtr.Zero);
        }

        static void ClickMouseRight(int x, int y)
        {
            // �������ת��Ϊ�޷�������
            uint xPos = Convert.ToUInt32(x);
            uint yPos = Convert.ToUInt32(y);

            //��궨λ
            SetCursorPos(x, y);

            // �����������ͷ��¼�
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, xPos, yPos, 0, UIntPtr.Zero);
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            Bitmap screenshot = CaptureScreen(); // ץȡ��Ļ��ͼ
            byte[] imageData = ConvertBitmapToBytes(screenshot); // ��Bitmapת��Ϊbyte����
            if (socketConnection.Count > 0)
            {
                socketConnection.ForEach(m => m.Send(imageData));
            }
        }

        private void button_connect_Click_1(object sender, EventArgs e)
        {
            connectStr = "ws://"+text_ip.Text+":"+PORT+"/";
            DoConnect(connectStr);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //string filePath = Path.Combine(Application.StartupPath, "logs", "log.txt");

            //// ����ļ��Ƿ����
            //if (File.Exists(filePath))
            //{
            //    if (MessageBox.Show("�Ƿ������־", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //    {
            //        // �����־
            //        log.Logger.Repository.ResetConfiguration();
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}
            //else
            //{
            //    return;
            //}
        }

        public class ReceiveMsgModel
        {
            public MsgTypeEnum.msgTypeEnum type { get; set; }
            public int key { get; set; }
            public decimal x { get; set; }
            public decimal y { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cursorPosition = Cursor.Position;
            INPUT[] inputs = new INPUT[3];
        }
    }
}