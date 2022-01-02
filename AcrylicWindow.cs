using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace iPrintUtility
{
    /// <summary>
    /// Extended WPF Window with BlurBehind and compatibility with both MahApps and MaterialDesignInXamlToolkit
    /// </summary>
    public class AcrylicWindow : Window
    {
        /// <summary>
        /// This property can be used to find out if the current window is using the dark theme
        /// </summary>
        public static bool IsDarkTheme { get; private set; } = false;

        internal enum AccentState
        {
            ACCENT_DISABLED = 1,            // Disabled
            ACCENT_ENABLE_GRADIENT = 0,     // No idea, it's gray no matter what
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,  // Painted with accent colour
            ACCENT_ENABLE_BLURBEHIND = 3,       // Blurbehind effect
            ACCENT_INVALID_STATE = 4        // Invalid state
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;    // Query attributes
            public IntPtr Data;                                                     // Data buffer
            public int SizeOfData;                                              // Data size
        }

        internal enum WindowCompositionAttribute    // Same as NtUserGetWindowCompositionAttribute?
        {
            WCA_ACCENT_POLICY = 19
        }

        public AcrylicWindow() : base()
        {

        }

        /// <summary>
        /// Sets various DWM window attributes
        /// </summary>
        /// <param name="hwnd">The window to modify</param>
        /// <param name="data">Pointer to the structure with the attribute data</param>
        /// <returns>    Nonzero on success, zero otherwise. You can call GetLastError on failure.</returns>
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        /// <summary>
		/// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate. 
		/// </summary>
		public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (((App)App.Current).NoBlur)
                return;

            EnableBlurBehind();
        }

        /// <summary>
		/// Enables BlurBehind for our fancy window
		/// </summary>
		internal void EnableBlurBehind()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.AccentFlags = 2;
            accent.GradientColor = 0x00FFFFFF;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
