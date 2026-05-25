using System;
using System.Threading;
using WindowsInput.Native;
using Window = Avalonia.Controls.Window;

namespace ClicklessMouse
{
    public partial class MainWindow : Window
    {
        private Thread _thRkeymaster;

        private void key_press(VirtualKeyCode vkc, bool async, int downMs = 75)
        {
            if (async)
            {
                _thRkeymaster = new Thread(() => key_press(vkc, downMs));
                _thRkeymaster.Start();
            }
            else
                key_press(vkc, downMs);
        }

        private void key_press(VirtualKeyCode vkc, int downMs = 75)
        {
            _sim.Keyboard.KeyDown(vkc);
            Thread.Sleep(downMs);
            _sim.Keyboard.KeyUp(vkc);
        }

        private void key_down(VirtualKeyCode vkc)
        {
            #if _WINDOWS
            _sim.Keyboard.KeyDown(vkc);

#elif _LINUX
            Native.InputX11.KeyDown(vkc);
#endif
        }

        private void key_up(VirtualKeyCode vkc)
        {
#if _WINDOWS
_sim.Keyboard.KeyUp(vkc);

#elif _LINUX
                Native.InputX11.KeyUp(vkc);
#endif
        }

        private void release_buttons_and_keys()
        {
            if (_sim.InputDeviceState.IsKeyDown(VirtualKeyCode.LBUTTON))
            {
                left_up();
            }

            if (_sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
            {
                right_up();
            }

            foreach (VirtualKeyCode vkc in (VirtualKeyCode[])Enum.GetValues(typeof(VirtualKeyCode)))
            {
                if (_sim.InputDeviceState.IsKeyDown(vkc))
                    key_up(vkc);
            }
        }

        private void release_buttons()
        {
            if (_sim.InputDeviceState.IsKeyDown(VirtualKeyCode.LBUTTON))
            {
                left_up();
            }

            if (_sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
            {
                right_up();
            }
        }
    }
}