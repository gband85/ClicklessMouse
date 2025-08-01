using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClicklessMouse
{
    public partial class WinInput
    {
        [DllImport("user32.dll", SetLastError = true)]

        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]

        public static extern void SetCursorPos(int x,int y);
    }
}
