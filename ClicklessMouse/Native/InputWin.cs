using System.Drawing;
using System.Runtime.InteropServices;

namespace ClicklessMouse.Native
{

#if _WINDOWS
    public class InputWin
        {

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool GetCursorPos(out Point lpPoint);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern void SetCursorPos(int x, int y);

}
#endif
    
}
