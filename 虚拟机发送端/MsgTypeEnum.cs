using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 虚拟机发送端
{
    public class MsgTypeEnum
    {
        public enum msgTypeEnum
        {
            /// <summary>
            /// 键盘按下
            /// </summary>
            KeyDown = 0,
            /// <summary>
            /// 键盘抬起
            /// </summary>
            KeyUp = 1,
            /// <summary>
            /// 鼠标移动
            /// </summary>
            MouseMove = 2,
            /// <summary>
            /// 双击
            /// </summary>
            DoubleClick = 3,
            /// <summary>
            /// 右击
            /// </summary>
            RightButton = 4,
            /// <summary>
            /// 鼠标左键按下
            /// </summary>
            MouseDown = 5,
            /// <summary>
            /// 鼠标左键抬起
            /// </summary>
            MouseUp = 6,
            /// <summary>
            /// 鼠标滚轮事件
            /// </summary>
            MouseWheel = 7,
        }
    }
}
