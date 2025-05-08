using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClicklessMouse.WindowsInput;
using ClicklessMouse.Enums;

namespace ClicklessMouse.Interfaces
{
    /// <summary>
    /// The contract for a service that interprets the state of input devices.
    /// </summary>
    public interface IInputDeviceStateAdaptor
    {
        /// <summary>
        /// Determines whether the specified key is up or down.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        /// <returns>
        /// 	<c>true</c> if the key is down; otherwise, <c>false</c>.
        /// </returns>
        bool IsKeyDown(VirtualKeyCode keyCode);

        /// <summary>
        /// Determines whether the specified key is up or down.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        /// <returns>
        /// 	<c>true</c> if the key is up; otherwise, <c>false</c>.
        /// </returns>
        bool IsKeyUp(VirtualKeyCode keyCode);

        /// <summary>
        /// Determines whether the physical key is up or down at the time the function is called regardless of whether the application thread has read the keyboard event from the message pump.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        /// <returns>
        /// 	<c>true</c> if the key is down; otherwise, <c>false</c>.
        /// </returns>
        bool IsHardwareKeyDown(VirtualKeyCode keyCode);

        /// <summary>
        /// Determines whether the physical key is up or down at the time the function is called regardless of whether the application thread has read the keyboard event from the message pump.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        /// <returns>
        /// 	<c>true</c> if the key is up; otherwise, <c>false</c>.
        /// </returns>
        bool IsHardwareKeyUp(VirtualKeyCode keyCode);

        /// <summary>
        /// Determines whether the toggling key is toggled on (in-effect) or not.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        /// <returns>
        /// 	<c>true</c> if the toggling key is toggled on (in-effect); otherwise, <c>false</c>.
        /// </returns>
        bool IsTogglingKeyInEffect(VirtualKeyCode keyCode);
    }



    /// <summary>
    /// The contract for a service that dispatches <see cref="INPUT"/> messages to the appropriate destination.
    /// </summary>
    internal interface IInputMessageDispatcher
    {
        /// <summary>
        /// Dispatches the specified list of <see cref="INPUT"/> messages in their specified order.
        /// </summary>
        /// <param name="inputs">The list of <see cref="INPUT"/> messages to be dispatched.</param>
        /// <exception cref="ArgumentException">If the <paramref name="inputs"/> array is empty.</exception>
        /// <exception cref="ArgumentNullException">If the <paramref name="inputs"/> array is null.</exception>
        /// <exception cref="Exception">If the any of the commands in the <paramref name="inputs"/> array could not be sent successfully.</exception>
        void DispatchInput(INPUT[] inputs);
    }


    /// <summary>
    /// The contract for a service that simulates Keyboard and Mouse input and Hardware Input Device state detection for the Windows Platform.
    /// </summary>
    public interface IInputSimulator
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardSimulator"/> instance for simulating Keyboard input.
        /// </summary>
        /// <value>The <see cref="IKeyboardSimulator"/> instance.</value>
        IKeyboardSimulator Keyboard { get; }

        /// <summary>
        /// Gets the <see cref="IMouseSimulator"/> instance for simulating Mouse input.
        /// </summary>
        /// <value>The <see cref="IMouseSimulator"/> instance.</value>
        IMouseSimulator Mouse { get; }

        /// <summary>
        /// Gets the <see cref="IInputDeviceStateAdaptor"/> instance for determining the state of the various input devices.
        /// </summary>
        /// <value>The <see cref="IInputDeviceStateAdaptor"/> instance.</value>
        IInputDeviceStateAdaptor InputDeviceState { get; }
    }



    /// <summary>
    /// The service contract for a keyboard simulator for the Windows platform.
    /// </summary>
    public interface IKeyboardSimulator
    {
        /// <summary>
        /// Gets the <see cref="IMouseSimulator"/> instance for simulating Mouse input.
        /// </summary>
        /// <value>The <see cref="IMouseSimulator"/> instance.</value>
        IMouseSimulator Mouse { get; }

        /// <summary>
        /// Simulates the key down gesture for the specified key.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        IKeyboardSimulator KeyDown(VirtualKeyCode keyCode);

        /// <summary>
        /// Simulates the key press gesture for the specified key.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        IKeyboardSimulator KeyPress(VirtualKeyCode keyCode);

        /// <summary>
        /// Simulates a key press for each of the specified key codes in the order they are specified.
        /// </summary>
        /// <param name="keyCodes"></param>
        IKeyboardSimulator KeyPress(params VirtualKeyCode[] keyCodes);

        /// <summary>
        /// Simulates the key up gesture for the specified key.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        IKeyboardSimulator KeyUp(VirtualKeyCode keyCode);

        /// <summary>
        /// Simulates a modified keystroke where there are multiple modifiers and multiple keys like CTRL-ALT-K-C where CTRL and ALT are the modifierKeys and K and C are the keys.
        /// The flow is Modifiers KeyDown in order, Keys Press in order, Modifiers KeyUp in reverse order.
        /// </summary>
        /// <param name="modifierKeyCodes">The list of <see cref="VirtualKeyCode"/>s for the modifier keys.</param>
        /// <param name="keyCodes">The list of <see cref="VirtualKeyCode"/>s for the keys to simulate.</param>
        IKeyboardSimulator ModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, IEnumerable<VirtualKeyCode> keyCodes);

        /// <summary>
        /// Simulates a modified keystroke where there are multiple modifiers and one key like CTRL-ALT-C where CTRL and ALT are the modifierKeys and C is the key.
        /// The flow is Modifiers KeyDown in order, Key Press, Modifiers KeyUp in reverse order.
        /// </summary>
        /// <param name="modifierKeyCodes">The list of <see cref="VirtualKeyCode"/>s for the modifier keys.</param>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        IKeyboardSimulator ModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, VirtualKeyCode keyCode);

        /// <summary>
        /// Simulates a modified keystroke where there is one modifier and multiple keys like CTRL-K-C where CTRL is the modifierKey and K and C are the keys.
        /// The flow is Modifier KeyDown, Keys Press in order, Modifier KeyUp.
        /// </summary>
        /// <param name="modifierKey">The <see cref="VirtualKeyCode"/> for the modifier key.</param>
        /// <param name="keyCodes">The list of <see cref="VirtualKeyCode"/>s for the keys to simulate.</param>
        IKeyboardSimulator ModifiedKeyStroke(VirtualKeyCode modifierKey, IEnumerable<VirtualKeyCode> keyCodes);

        /// <summary>
        /// Simulates a simple modified keystroke like CTRL-C where CTRL is the modifierKey and C is the key.
        /// The flow is Modifier KeyDown, Key Press, Modifier KeyUp.
        /// </summary>
        /// <param name="modifierKeyCode">The <see cref="VirtualKeyCode"/> for the  modifier key.</param>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        IKeyboardSimulator ModifiedKeyStroke(VirtualKeyCode modifierKeyCode, VirtualKeyCode keyCode);

        /// <summary>
        /// Simulates uninterrupted text entry via the keyboard.
        /// </summary>
        /// <param name="text">The text to be simulated.</param>
        IKeyboardSimulator TextEntry(string text);

        /// <summary>
        /// Simulates a single character text entry via the keyboard.
        /// </summary>
        /// <param name="character">The unicode character to be simulated.</param>
        IKeyboardSimulator TextEntry(char character);

        /// <summary>
        /// Sleeps the executing thread to create a pause between simulated inputs.
        /// </summary>
        /// <param name="millsecondsTimeout">The number of milliseconds to wait.</param>
        IKeyboardSimulator Sleep(int millsecondsTimeout);

        /// <summary>
        /// Sleeps the executing thread to create a pause between simulated inputs.
        /// </summary>
        /// <param name="timeout">The time to wait.</param>
        IKeyboardSimulator Sleep(TimeSpan timeout);
    }


    /// <summary>
    /// The service contract for a mouse simulator for the Windows platform.
    /// </summary>
    public interface IMouseSimulator
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardSimulator"/> instance for simulating Keyboard input.
        /// </summary>
        /// <value>The <see cref="IKeyboardSimulator"/> instance.</value>
        IKeyboardSimulator Keyboard { get; }

        /// <summary>
        /// Simulates mouse movement by the specified distance measured as a delta from the current mouse location in pixels.
        /// </summary>
        /// <param name="pixelDeltaX">The distance in pixels to move the mouse horizontally.</param>
        /// <param name="pixelDeltaY">The distance in pixels to move the mouse vertically.</param>
        IMouseSimulator MoveMouseBy(int pixelDeltaX, int pixelDeltaY);

        /// <summary>
        /// Simulates mouse movement to the specified location on the primary display device.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the primary display device where 0 is the extreme left hand side of the display device and 65535 is the extreme right hand side of the display device.</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the primary display device where 0 is the top of the display device and 65535 is the bottom of the display device.</param>
        IMouseSimulator MoveMouseTo(double absoluteX, double absoluteY);

        /// <summary>
        /// Simulates mouse movement to the specified location on the Virtual Desktop which includes all active displays.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the virtual desktop where 0 is the left hand side of the virtual desktop and 65535 is the extreme right hand side of the virtual desktop.</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the virtual desktop where 0 is the top of the virtual desktop and 65535 is the bottom of the virtual desktop.</param>
        IMouseSimulator MoveMouseToPositionOnVirtualDesktop(double absoluteX, double absoluteY);

        /// <summary>
        /// Simulates a mouse left button down gesture.
        /// </summary>
        IMouseSimulator LeftButtonDown();

        /// <summary>
        /// Simulates a mouse left button up gesture.
        /// </summary>
        IMouseSimulator LeftButtonUp();

        /// <summary>
        /// Simulates a mouse left button click gesture.
        /// </summary>
        IMouseSimulator LeftButtonClick();

        /// <summary>
        /// Simulates a mouse left button double-click gesture.
        /// </summary>
        IMouseSimulator LeftButtonDoubleClick();

        /// <summary>
        /// Simulates a mouse right button down gesture.
        /// </summary>
        IMouseSimulator RightButtonDown();

        /// <summary>
        /// Simulates a mouse right button up gesture.
        /// </summary>
        IMouseSimulator RightButtonUp();

        /// <summary>
        /// Simulates a mouse right button click gesture.
        /// </summary>
        IMouseSimulator RightButtonClick();

        /// <summary>
        /// Simulates a mouse right button double-click gesture.
        /// </summary>
        IMouseSimulator RightButtonDoubleClick();

        /// <summary>
        /// Simulates a mouse X button down gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        IMouseSimulator XButtonDown(int buttonId);

        /// <summary>
        /// Simulates a mouse X button up gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        IMouseSimulator XButtonUp(int buttonId);

        /// <summary>
        /// Simulates a mouse X button click gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        IMouseSimulator XButtonClick(int buttonId);

        /// <summary>
        /// Simulates a mouse X button double-click gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        IMouseSimulator XButtonDoubleClick(int buttonId);

        /// <summary>
        /// Simulates mouse vertical wheel scroll gesture.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.</param>
        IMouseSimulator VerticalScroll(int scrollAmountInClicks);

        /// <summary>
        /// Simulates a mouse horizontal wheel scroll gesture. Supported by Windows Vista and later.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left.</param>
        IMouseSimulator HorizontalScroll(int scrollAmountInClicks);

        /// <summary>
        /// Sleeps the executing thread to create a pause between simulated inputs.
        /// </summary>
        /// <param name="millsecondsTimeout">The number of milliseconds to wait.</param>
        IMouseSimulator Sleep(int millsecondsTimeout);

        /// <summary>
        /// Sleeps the executing thread to create a pause between simulated inputs.
        /// </summary>
        /// <param name="timeout">The time to wait.</param>
        IMouseSimulator Sleep(TimeSpan timeout);
    }
}
