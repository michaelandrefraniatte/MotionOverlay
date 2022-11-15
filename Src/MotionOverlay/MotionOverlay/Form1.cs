using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
namespace MotionOverlay
{
    public partial class Form1 : Form
    {
        private int x, y, Width, Height;
        private Point drawOrigin, drawOriginLC, drawOriginRC, drawOriginMC, drawOriginR;
        private Math3D.Math3D.Cube cube, cubeLC, cubeRC, cubeMC,  cubeR;
        private Bitmap bmp, bmpLC, bmpRC, bmpMC, bmpR, result;
        private float X = 0f, Y = 0f, Z = 0f;
        public static int MouseDesktopHookX, MouseDesktopHookY, MouseHookX, MouseHookY, MouseHookWheelM, MouseHookWheelK;
        public static bool MouseHookLeftButton, MouseHookRightButton, MouseHookDoubleClick, MouseHookMiddleButton, getstate;
        public static IntPtr Param;
        MouseHook mouseHook = new MouseHook();
        private void Form1_Load(object sender, EventArgs e)
        {
            Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            mouseHook.Hook += new MouseHook.MouseHookCallback(mouseHook_Hook);
            mouseHook.Install();
        }
        public Form1()
        {
            InitializeComponent();
            drawOrigin = new Point(picture1.Width / 2, picture1.Height / 2);
            cube = new Math3D.Math3D.Cube(50, 70, 0);
            drawOriginLC = new Point(picture1.Width / 2 - 25, picture1.Height / 2 - 40);
            cubeLC = new Math3D.Math3D.Cube(10, 15, 0);
            drawOriginRC = new Point(picture1.Width / 2 + 25, picture1.Height / 2 - 40);
            cubeRC = new Math3D.Math3D.Cube(10, 15, 0);
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            if (this.FormBorderStyle == FormBorderStyle.FixedToolWindow) 
                return;
            if (x != 0 && y != 0)
            {
                this.Left = x;
                this.Top = y;
            }
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.BackColor = Color.White;
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            var borderHeight = this.Bounds.Height - this.ClientRectangle.Height;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
        }
        private void mouseHook_Hook(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            MouseHookX = mouseStruct.pt.x;
            MouseHookY = mouseStruct.pt.y;
            if (MouseHook.MouseMessages.WM_LBUTTONDOWN == (MouseHook.MouseMessages)Param)
                MouseHookLeftButton = true;
            if (MouseHook.MouseMessages.WM_LBUTTONUP == (MouseHook.MouseMessages)Param)
                MouseHookLeftButton = false;
            if (MouseHook.MouseMessages.WM_RBUTTONDOWN == (MouseHook.MouseMessages)Param)
                MouseHookRightButton = true;
            if (MouseHook.MouseMessages.WM_RBUTTONUP == (MouseHook.MouseMessages)Param)
                MouseHookRightButton = false;
            if (MouseHook.MouseMessages.WM_MBUTTONDOWN == (MouseHook.MouseMessages)Param)
                MouseHookMiddleButton = true;
            if (MouseHook.MouseMessages.WM_MBUTTONUP == (MouseHook.MouseMessages)Param)
                MouseHookMiddleButton = false;
            if (MouseHook.MouseMessages.WM_LBUTTONDBLCLK == (MouseHook.MouseMessages)Param)
                MouseHookDoubleClick = true;
            else
                MouseHookDoubleClick = false;
            if (MouseHook.MouseMessages.WM_MOUSEWHEEL == (MouseHook.MouseMessages)Param)
            {
                MouseHookWheelM = (int)mouseStruct.mouseData;
                MouseHookWheelK = (int)mouseStruct.mouseData;
            }
            else
            {
                MouseHookWheelM = 0;
                MouseHookWheelK = 0;
            }
            cube.InitializeCube();
            cube.RotateX = (float)(Y * 180.0f / Math.PI);
            cube.RotateY = (float)(Z * 180.0f / Math.PI);
            cube.RotateZ = (float)(X * 180.0f / Math.PI); 
            bmp = (Bitmap)cube.DrawCube(drawOrigin);
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(128, 128, 128));
                }
            }
            cubeLC.InitializeCube();
            cubeLC.RotateX = (float)(Y * 180.0f / Math.PI);
            cubeLC.RotateY = (float)(Z * 180.0f / Math.PI);
            cubeLC.RotateZ = (float)(X * 180.0f / Math.PI);
            bmpLC = (Bitmap)cubeLC.DrawCube(drawOriginLC);
            for (int x = 0; x < bmpLC.Width; x++)
            {
                for (int y = 0; y < bmpLC.Height; y++)
                {
                    bmpLC.GetPixel(x, y);
                    if (MouseHookLeftButton)
                        bmpLC.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    else
                        bmpLC.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
            cubeRC.InitializeCube();
            cubeRC.RotateX = (float)(Y * 180.0f / Math.PI);
            cubeRC.RotateY = (float)(Z * 180.0f / Math.PI);
            cubeRC.RotateZ = (float)(X * 180.0f / Math.PI);
            bmpRC = (Bitmap)cubeRC.DrawCube(drawOriginRC);
            for (int x = 0; x < bmpRC.Width; x++)
            {
                for (int y = 0; y < bmpRC.Height; y++)
                {
                    bmpRC.GetPixel(x, y);
                    if (MouseHookRightButton)
                        bmpRC.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    else
                        bmpRC.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
            result = new Bitmap(((Bitmap)cube.DrawCube(drawOrigin)).Width, ((Bitmap)cube.DrawCube(drawOrigin)).Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, Point.Empty);
                g.DrawImage(bmpLC, Point.Empty);
                g.DrawImage(bmpRC, new Point(picture1.Width / 2 + 16, picture1.Height / 2 - 44));
            }
            picture1.Image = result;
            this.Left = MouseHookX / 10;
            this.Top = MouseHookY / 10 + Height - 200;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            MouseHookWheelM = 0;
            MouseHookWheelK = 0;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mouseHook.Hook -= new MouseHook.MouseHookCallback(mouseHook_Hook);
            mouseHook.Uninstall();
        }
    }
    class MouseHook
    {
        private delegate IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
        private MouseHookHandler hookHandler;
        public delegate void MouseHookCallback(MSLLHOOKSTRUCT mouseStruct);
        public event MouseHookCallback LeftButtonDown;
        public event MouseHookCallback LeftButtonUp;
        public event MouseHookCallback RightButtonDown;
        public event MouseHookCallback RightButtonUp;
        public event MouseHookCallback MouseMove;
        public event MouseHookCallback MouseWheel;
        public event MouseHookCallback DoubleClick;
        public event MouseHookCallback MiddleButtonDown;
        public event MouseHookCallback MiddleButtonUp;
        public event MouseHookCallback Hook;
        private IntPtr hookID = IntPtr.Zero;
        public void Install()
        {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }
        public void Uninstall()
        {
            if (hookID == IntPtr.Zero)
                return;
            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
        }
        ~MouseHook()
        {
            Uninstall();
        }
        private IntPtr SetHook(MouseHookHandler proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(module.ModuleName), 0);
        }
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Form1.Param = wParam;
                if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                    if (LeftButtonDown != null)
                        LeftButtonDown((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
                    if (LeftButtonUp != null)
                        LeftButtonUp((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
                    if (RightButtonDown != null)
                        RightButtonDown((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam)
                    if (RightButtonUp != null)
                        RightButtonUp((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
                    if (MouseMove != null)
                        MouseMove((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_MOUSEWHEEL == (MouseMessages)wParam)
                    if (MouseWheel != null)
                        MouseWheel((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_LBUTTONDBLCLK == (MouseMessages)wParam)
                    if (DoubleClick != null)
                        DoubleClick((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_MBUTTONDOWN == (MouseMessages)wParam)
                    if (MiddleButtonDown != null)
                        MiddleButtonDown((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                if (MouseMessages.WM_MBUTTONUP == (MouseMessages)wParam)
                    if (MiddleButtonUp != null)
                        MiddleButtonUp((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                Hook((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
        private const int WH_MOUSE_LL = 14;
        public enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookHandler lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
