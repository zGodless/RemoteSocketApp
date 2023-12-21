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
using 虚拟机发送端;
using System.Runtime.InteropServices;
using log4net;
using log4net.Config;
using System.Web.UI.WebControls;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // 导入 user32.dll 库中的函数
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        //设置鼠标位置
        [DllImport("user32.dll")] //DllImpor针对非托管的。非托管指的是不利用.net 生成的DLL
                                  //声明一个外部实现方法SetCursorPos()
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
        public int XBUTTON1 = 0x0001;//设置是否按下或释放第一个 X 按钮。
        public int XBUTTON2 = 0x0002;//设置是否按下或释放第二个 X 按钮。

        public const int INPUT_MOUSE = 0;
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //模拟鼠标滚轮滚动操作，必须配合dwData参数
        const int MOUSEEVENTF_WHEEL = 0x0800;


        // 定义一个静态的日志记录器
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load; ;
            FormClosing +=Form1_FormClosing;
        }
        static Timer timer;
        private const int PORT = 5001;
        private const int INTERVAL_MS = 20; // 定时抓取间隔，单位毫秒
        public static List<IWebSocketConnection> socketConnection = new List<IWebSocketConnection>();  //socket连接池
        public static WebSocketServer WebSocketServer = null;
        public static string connectStr = "";
        public bool isHoldMouse = false;//是否按住左键，用于判断拖拽
        public Point oldPositon = new Point(0, 0);//原始坐标，用于拖拽

        FormPoint formpoint = new FormPoint();
        private void Form1_Load(object sender, EventArgs e)
        {
            // 加载log4net配置文件
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            // 记录日志
            log.Info("程序启动");

            if (WebSocketServer == null && !string.IsNullOrEmpty(text_ip.Text))
            {
                connectStr = "ws://"+text_ip.Text+":"+PORT+"/";
                DoConnect(connectStr);
            }

            // 创建一个 Timer 对象
            timer = new Timer();

            // 设置定时器的间隔 毫秒
            timer.Interval = INTERVAL_MS;

            // 设置定时器的 Tick 事件处理程序
            timer.Tick += TimerTick;

            // 启动定时器
            timer.Start();
        }

        private void Form1_showMsg(object sender, EventArgs e, string content)
        {

        }

        private static void TimerTick(object sender, EventArgs e)
        {
            if(WebSocketServer != null)
            {
                Bitmap screenshot = CaptureScreen(); // 抓取屏幕截图
                byte[] imageData = ConvertBitmapToBytes(screenshot); // 将Bitmap转换为byte数组
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
                            log.Info($"接收消息：{message}");
                        }
                        switch(receiveModel.type)
                        {
                            case MsgTypeEnum.msgTypeEnum.MouseDown:
                                {
                                    // 要点击的坐标
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    log.Info($"执行【左键下下下下下下下下下下下下下下】，({x},{y})");
                                    isHoldMouse = true;
                                    oldPositon = new Point(x, y);//记录点击坐标
                                    //ClickMouse(x, y);
                                    MouseLeftDown(x, y);
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.MouseUp:
                                {
                                    // 要点击的坐标
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    log.Info($"执行【左键点击Up上】，odl:({oldPositon.X},{oldPositon.Y}), ({x},{y})");
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
                                    // 获取当前鼠标坐标
                                    Point cursorPosition = Cursor.Position;
                                    // 鼠标的坐标
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
                                        //log.Info($"执行【SendInput】，结果:({uSent})");

                                    }
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.RightButton:
                                {
                                    // 要点击的坐标
                                    int x = (int)receiveModel.x;
                                    int y = (int)receiveModel.y;
                                    log.Info($"执行【右键点击】，({x},{y})");
                                    ClickMouseRight(x, y);
                                    break;
                                }
                            case MsgTypeEnum.msgTypeEnum.KeyUp:
                                {
                                    break;
                                    log.Info($"执行【键盘抬起】，keyCode:({receiveModel.key})");
                                    if (receiveModel.key < 49 || receiveModel.key > 90)
                                        break;
                                    // 将键盘代码转换为实际键盘文本
                                    string keyText = Enum.GetName(typeof(Keys), receiveModel.key);
                                    // 向计算机发送按键事件
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

            //鼠标定位
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
            log.Info($"执行【SendInput, DragA2B】，结果:({uSent})");
        }

        public void MouseLeftDown(int x, int y)
        {
            //// 将坐标点转换为无符号整型
            //uint xPos = Convert.ToUInt32(x);
            //uint yPos = Convert.ToUInt32(y);

            ////鼠标定位
            //SetCursorPos(x, y);

            //// 发送鼠标左键点击事件
            //mouse_event(MOUSEEVENTF_LEFTDOWN, xPos, yPos, 0, UIntPtr.Zero);

            //鼠标定位
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
            log.Info($"执行【SendInput, MouseLeftDown】，结果:({uSent})");
        }
        public void MouseLeftUp(int x, int y)
        {
            //鼠标定位
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
            log.Info($"执行【SendInput, MouseLeftUp】，结果:({uSent})");
        }

        static void ClickMouse(int x, int y)
        {
            // 将坐标点转换为无符号整型
            uint xPos = Convert.ToUInt32(x);
            uint yPos = Convert.ToUInt32(y);

            //鼠标定位
            SetCursorPos(x, y);

            // 发送鼠标左键点击事件
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, xPos, yPos, 0, UIntPtr.Zero);
        }

        static void ClickMouseRight(int x, int y)
        {
            // 将坐标点转换为无符号整型
            uint xPos = Convert.ToUInt32(x);
            uint yPos = Convert.ToUInt32(y);

            //鼠标定位
            SetCursorPos(x, y);

            // 发送鼠标左键释放事件
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, xPos, yPos, 0, UIntPtr.Zero);
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            Bitmap screenshot = CaptureScreen(); // 抓取屏幕截图
            byte[] imageData = ConvertBitmapToBytes(screenshot); // 将Bitmap转换为byte数组
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

            //// 检查文件是否存在
            //if (File.Exists(filePath))
            //{
            //    if (MessageBox.Show("是否清除日志", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //    {
            //        // 清空日志
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