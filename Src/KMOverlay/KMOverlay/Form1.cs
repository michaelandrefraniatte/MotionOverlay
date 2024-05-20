using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using WebView2 = Microsoft.Web.WebView2.WinForms.WebView2;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KMOverlay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(Keys vKey);
        [DllImport("User32.dll")]
        public static extern bool GetCursorPos(out int x, out int y);
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        private bool closed = false;
        private int x, y, Width, Height, posx, posy, mousex, mousey;
        public static bool Desktop = false, English = false;
        public static double sensx = 1f, sensy = 1f;
        public WebView2 webView21 = new WebView2();
        private static DirectInput directInput = new DirectInput();
        private static bool mouseconnected, keyboardconnected;
        private static bool MouseWheelUp, MouseWheelDown;
        private const int GWL_STYLE = -16;
        private const uint WS_BORDER = 0x00800000;
        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_SYSMENU = 0x00080000;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const uint WS_OVERLAPPED = 0x00000000;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_TABSTOP = 0x00010000;
        private const uint WS_VISIBLE = 0x10000000;
        private static int[] wd = { 2, 2 };
        private static int[] wu = { 2, 2 };
        public static void valchanged(int n, bool val)
        {
            if (val)
            {
                if (wd[n] <= 1)
                {
                    wd[n] = wd[n] + 1;
                }
                wu[n] = 0;
            }
            else
            {
                if (wu[n] <= 1)
                {
                    wu[n] = wu[n] + 1;
                }
                wd[n] = 0;
            }
        }
        private async void Form1_Shown(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            Task.Run(() => StartWindowTitleRemover());
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disable-web-security --allow-file-access-from-files --allow-file-access", "en");
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView21.EnsureCoreWebView2Async(environment);
            webView21.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets", "assets", CoreWebView2HostResourceAccessKind.DenyCors);
            webView21.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView21.KeyDown += WebView21_KeyDown;
            webView21.Source = new Uri("https://appassets/index.html");
            webView21.Dock = DockStyle.Fill;
            webView21.DefaultBackgroundColor = Color.Transparent;
            this.Controls.Add(webView21);
            this.Location = new Point(Width - this.Size.Width, Height - this.Size.Height);
            using (System.IO.StreamReader file = new System.IO.StreamReader("params.txt"))
            {
                file.ReadLine();
                Desktop = bool.Parse(file.ReadLine());
                file.ReadLine();
                English = bool.Parse(file.ReadLine());
                file.ReadLine();
                sensx = Convert.ToSingle(file.ReadLine());
                file.ReadLine();
                sensy = Convert.ToSingle(file.ReadLine());
            }
            try
            {
                keyboardconnected = KeyboardInputHookConnect();
                mouseconnected = MouseInputHookConnect();
            }
            catch { }
        }
        private void StartWindowTitleRemover()
        {
            while (true)
            {
                valchanged(0, GetAsyncKeyState(Keys.PageDown));
                if (wu[0] == 1)
                {
                    int width = Screen.PrimaryScreen.Bounds.Width;
                    int height = Screen.PrimaryScreen.Bounds.Height;
                    IntPtr window = GetForegroundWindow();
                    SetWindowLong(window, GWL_STYLE, WS_SYSMENU);
                    SetWindowPos(window, -2, 0, 0, width, height, 0x0040);
                    DrawMenuBar(window);
                }
                valchanged(1, GetAsyncKeyState(Keys.PageUp));
                if (wu[1] == 1)
                {
                    IntPtr window = GetForegroundWindow();
                    SetWindowLong(window, GWL_STYLE, WS_CAPTION | WS_POPUP | WS_BORDER | WS_SYSMENU | WS_TABSTOP | WS_VISIBLE | WS_OVERLAPPED | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
                    DrawMenuBar(window);
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            OnKeyDown(keyData);
            return true;
        }
        private void WebView21_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void OnKeyDown(Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                const string message = "• Author: Michaël André Franiatte.\n\r\n\r• Contact: michael.franiatte@gmail.com.\n\r\n\r• Publisher: https://github.com/michaelandrefraniatte.\n\r\n\r• Copyrights: All rights reserved, no permissions granted.\n\r\n\r• License: Not open source, not free of charge to use.";
                const string caption = "About";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            taskEmulate();
        }
        public async void SetController(bool KeyboardKeyTab, bool KeyboardKeyNumberLock, bool KeyboardKeyLeftShift, bool KeyboardKeyLeftControl, bool KeyboardKeyLeftAlt, bool KeyboardKeyQ, bool KeyboardKeyW, bool KeyboardKeyE, bool KeyboardKeyR, bool KeyboardKeyT, bool KeyboardKeyA, bool KeyboardKeyS, bool KeyboardKeyD, bool KeyboardKeyF, bool KeyboardKeyG, bool KeyboardKeyZ, bool KeyboardKeyX, bool KeyboardKeyC, bool KeyboardKeyV, bool KeyboardKeyApostrophe, bool KeyboardKeyEscape, bool KeyboardKeyD1, bool KeyboardKeyD2, bool KeyboardKeyD3, bool KeyboardKeyD4, bool KeyboardKeyD5, bool KeyboardKeyD6, bool KeyboardKeyF1, bool KeyboardKeyF2, bool KeyboardKeyF3, bool KeyboardKeyF4, bool KeyboardKeyF5, bool MouseButtons0, bool MouseButtons1, bool MouseButtons2, bool MouseButtons3, bool MouseButtons4, bool MouseButtons5, bool MouseButtons6, bool MouseButtons7, double MouseAxisX, double MouseAxisY, bool MouseWheelUp, bool MouseWheelDown)
        {
            try
            {
                await execScriptHelper($"setController('{KeyboardKeyTab.ToString()}', '{KeyboardKeyNumberLock.ToString()}', '{KeyboardKeyLeftShift.ToString()}', '{KeyboardKeyLeftControl.ToString()}', '{KeyboardKeyLeftAlt.ToString()}', '{KeyboardKeyQ.ToString()}', '{KeyboardKeyW.ToString()}', '{KeyboardKeyE.ToString()}', '{KeyboardKeyR.ToString()}', '{KeyboardKeyT.ToString()}', '{KeyboardKeyA.ToString()}', '{KeyboardKeyS.ToString()}', '{KeyboardKeyD.ToString()}', '{KeyboardKeyF.ToString()}', '{KeyboardKeyG.ToString()}', '{KeyboardKeyZ.ToString()}', '{KeyboardKeyX.ToString()}', '{KeyboardKeyC.ToString()}', '{KeyboardKeyV.ToString()}', '{KeyboardKeyApostrophe.ToString()}', '{KeyboardKeyEscape.ToString()}', '{KeyboardKeyD1.ToString()}', '{KeyboardKeyD2.ToString()}', '{KeyboardKeyD3.ToString()}', '{KeyboardKeyD4.ToString()}', '{KeyboardKeyD5.ToString()}', '{KeyboardKeyD6.ToString()}', '{KeyboardKeyF1.ToString()}', '{KeyboardKeyF2.ToString()}', '{KeyboardKeyF3.ToString()}', '{KeyboardKeyF4.ToString()}', '{KeyboardKeyF5.ToString()}', '{MouseButtons0.ToString()}', '{MouseButtons1.ToString()}', '{MouseButtons2.ToString()}', '{MouseButtons3.ToString()}', '{MouseButtons4.ToString()}', '{MouseButtons5.ToString()}', '{MouseButtons6.ToString()}', '{MouseButtons7.ToString()}', '{MouseAxisX.ToString()}', '{MouseAxisY.ToString()}', '{MouseWheelUp.ToString()}', '{MouseWheelDown.ToString()}');");
            }
            catch { }
        }
        private async Task<String> execScriptHelper(String script)
        {
            var x = await webView21.ExecuteScriptAsync(script).ConfigureAwait(false);
            return x;
        }
        private async void taskEmulate()
        {
            try
            {
                if (mouseconnected)
                {
                    MouseInputProcess();
                    if (MouseAxisZ > 0)
                        MouseWheelUp = true;
                    else
                        MouseWheelUp = false;
                    if (MouseAxisZ < 0)
                        MouseWheelDown = true;
                    else
                        MouseWheelDown = false;
                }
                if (keyboardconnected)
                {
                    KeyboardInputProcess();
                }
                if (Desktop)
                {
                    GetCursorPos(out posx, out posy);
                    MouseAxisX = (posx - Screen.PrimaryScreen.Bounds.Width / 2) / 20;
                    MouseAxisY = (posy - Screen.PrimaryScreen.Bounds.Height / 2) / 20;
                }
                mousex = (int)((double)MouseAxisX * sensx);
                mousey = (int)((double)MouseAxisY * sensy);
                if (English)
                    SetController(KeyboardKeyTab, KeyboardKeyNumberLock, KeyboardKeyLeftShift, KeyboardKeyLeftControl, KeyboardKeyLeftAlt, KeyboardKeyQ, KeyboardKeyW, KeyboardKeyE, KeyboardKeyR, KeyboardKeyT, KeyboardKeyA, KeyboardKeyS, KeyboardKeyD, KeyboardKeyF, KeyboardKeyG, KeyboardKeyZ, KeyboardKeyX, KeyboardKeyC, KeyboardKeyV, KeyboardKeyApostrophe, KeyboardKeyEscape, KeyboardKeyD1, KeyboardKeyD2, KeyboardKeyD3, KeyboardKeyD4, KeyboardKeyD5, KeyboardKeyD6, KeyboardKeyF1, KeyboardKeyF2, KeyboardKeyF3, KeyboardKeyF4, KeyboardKeyF5, MouseButtons0, MouseButtons1, MouseButtons2, MouseButtons3, MouseButtons4, MouseButtons5, MouseButtons6, MouseButtons7, mousex, mousey, MouseWheelUp, MouseWheelDown);
                else
                    SetController(KeyboardKeyTab, KeyboardKeyNumberLock, KeyboardKeyLeftShift, KeyboardKeyLeftControl, KeyboardKeyLeftAlt, KeyboardKeyA, KeyboardKeyZ, KeyboardKeyE, KeyboardKeyR, KeyboardKeyT, KeyboardKeyQ, KeyboardKeyS, KeyboardKeyD, KeyboardKeyF, KeyboardKeyG, KeyboardKeyW, KeyboardKeyX, KeyboardKeyC, KeyboardKeyV, KeyboardKeyApostrophe, KeyboardKeyEscape, KeyboardKeyD1, KeyboardKeyD2, KeyboardKeyD3, KeyboardKeyD4, KeyboardKeyD5, KeyboardKeyD6, KeyboardKeyF1, KeyboardKeyF2, KeyboardKeyF3, KeyboardKeyF4, KeyboardKeyF5, MouseButtons0, MouseButtons1, MouseButtons2, MouseButtons3, MouseButtons4, MouseButtons5, MouseButtons6, MouseButtons7, mousex, mousey, MouseWheelUp, MouseWheelDown);
                MouseAxisX = 0;
                MouseAxisY = 0;
                MouseAxisZ = 0;
            }
            catch { }
        }
        private static Mouse[] mouse = new Mouse[] { null };
        private static Guid[] mouseGuid = new Guid[] { Guid.Empty };
        private static int mnum;
        public static bool MouseButtons0;
        public static bool MouseButtons1;
        public static bool MouseButtons2;
        public static bool MouseButtons3;
        public static bool MouseButtons4;
        public static bool MouseButtons5;
        public static bool MouseButtons6;
        public static bool MouseButtons7;
        public static int MouseAxisX;
        public static int MouseAxisY;
        public static int MouseAxisZ;
        public bool MouseInputHookConnect()
        {
            try
            {
                directInput = new DirectInput();
                mouseGuid = new Guid[] { Guid.Empty };
                mnum = 0;
                foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Mouse, DeviceEnumerationFlags.AllDevices))
                {
                    mouseGuid[mnum] = deviceInstance.InstanceGuid;
                    mnum++;
                    if (mnum >= 1)
                        break;
                }
            }
            catch { }
            if (mouseGuid[0] == Guid.Empty)
            {
                return false;
            }
            else
            {
                for (int inc = 0; inc < mnum; inc++)
                {
                    mouse[inc] = new Mouse(directInput);
                    mouse[inc].Properties.BufferSize = 128;
                    mouse[inc].Acquire();
                }
                return true;
            }
        }
        public void MouseInputProcess()
        {
            for (int inc = 0; inc < mnum; inc++)
            {
                mouse[inc].Poll();
                var datas = mouse[inc].GetBufferedData();
                foreach (var state in datas)
                {
                    if (inc == 0 & state.Offset == MouseOffset.X)
                        MouseAxisX = state.Value;
                    if (inc == 0 & state.Offset == MouseOffset.Y)
                        MouseAxisY = state.Value;
                    if (inc == 0 & state.Offset == MouseOffset.Z)
                        MouseAxisZ = state.Value;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons0 & state.Value == 128)
                        MouseButtons0 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons0 & state.Value == 0)
                        MouseButtons0 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons1 & state.Value == 128)
                        MouseButtons1 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons1 & state.Value == 0)
                        MouseButtons1 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons2 & state.Value == 128)
                        MouseButtons2 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons2 & state.Value == 0)
                        MouseButtons2 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons3 & state.Value == 128)
                        MouseButtons3 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons3 & state.Value == 0)
                        MouseButtons3 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons4 & state.Value == 128)
                        MouseButtons4 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons4 & state.Value == 0)
                        MouseButtons4 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons5 & state.Value == 128)
                        MouseButtons5 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons5 & state.Value == 0)
                        MouseButtons5 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons6 & state.Value == 128)
                        MouseButtons6 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons6 & state.Value == 0)
                        MouseButtons6 = false;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons7 & state.Value == 128)
                        MouseButtons7 = true;
                    if (inc == 0 & state.Offset == MouseOffset.Buttons7 & state.Value == 0)
                        MouseButtons7 = false;
                }
            }
        }
        private static Keyboard[] keyboard = new Keyboard[] { null };
        private static Guid[] keyboardGuid = new Guid[] { Guid.Empty };
        private static int knum = 0;
        public static bool KeyboardKeyEscape;
        public static bool KeyboardKeyD1;
        public static bool KeyboardKeyD2;
        public static bool KeyboardKeyD3;
        public static bool KeyboardKeyD4;
        public static bool KeyboardKeyD5;
        public static bool KeyboardKeyD6;
        public static bool KeyboardKeyD7;
        public static bool KeyboardKeyD8;
        public static bool KeyboardKeyD9;
        public static bool KeyboardKeyD0;
        public static bool KeyboardKeyMinus;
        public static bool KeyboardKeyEquals;
        public static bool KeyboardKeyBack;
        public static bool KeyboardKeyTab;
        public static bool KeyboardKeyQ;
        public static bool KeyboardKeyW;
        public static bool KeyboardKeyE;
        public static bool KeyboardKeyR;
        public static bool KeyboardKeyT;
        public static bool KeyboardKeyY;
        public static bool KeyboardKeyU;
        public static bool KeyboardKeyI;
        public static bool KeyboardKeyO;
        public static bool KeyboardKeyP;
        public static bool KeyboardKeyLeftBracket;
        public static bool KeyboardKeyRightBracket;
        public static bool KeyboardKeyReturn;
        public static bool KeyboardKeyLeftControl;
        public static bool KeyboardKeyA;
        public static bool KeyboardKeyS;
        public static bool KeyboardKeyD;
        public static bool KeyboardKeyF;
        public static bool KeyboardKeyG;
        public static bool KeyboardKeyH;
        public static bool KeyboardKeyJ;
        public static bool KeyboardKeyK;
        public static bool KeyboardKeyL;
        public static bool KeyboardKeySemicolon;
        public static bool KeyboardKeyApostrophe;
        public static bool KeyboardKeyGrave;
        public static bool KeyboardKeyLeftShift;
        public static bool KeyboardKeyBackslash;
        public static bool KeyboardKeyZ;
        public static bool KeyboardKeyX;
        public static bool KeyboardKeyC;
        public static bool KeyboardKeyV;
        public static bool KeyboardKeyB;
        public static bool KeyboardKeyN;
        public static bool KeyboardKeyM;
        public static bool KeyboardKeyComma;
        public static bool KeyboardKeyPeriod;
        public static bool KeyboardKeySlash;
        public static bool KeyboardKeyRightShift;
        public static bool KeyboardKeyMultiply;
        public static bool KeyboardKeyLeftAlt;
        public static bool KeyboardKeySpace;
        public static bool KeyboardKeyCapital;
        public static bool KeyboardKeyF1;
        public static bool KeyboardKeyF2;
        public static bool KeyboardKeyF3;
        public static bool KeyboardKeyF4;
        public static bool KeyboardKeyF5;
        public static bool KeyboardKeyF6;
        public static bool KeyboardKeyF7;
        public static bool KeyboardKeyF8;
        public static bool KeyboardKeyF9;
        public static bool KeyboardKeyF10;
        public static bool KeyboardKeyNumberLock;
        public static bool KeyboardKeyScrollLock;
        public static bool KeyboardKeyNumberPad7;
        public static bool KeyboardKeyNumberPad8;
        public static bool KeyboardKeyNumberPad9;
        public static bool KeyboardKeySubtract;
        public static bool KeyboardKeyNumberPad4;
        public static bool KeyboardKeyNumberPad5;
        public static bool KeyboardKeyNumberPad6;
        public static bool KeyboardKeyAdd;
        public static bool KeyboardKeyNumberPad1;
        public static bool KeyboardKeyNumberPad2;
        public static bool KeyboardKeyNumberPad3;
        public static bool KeyboardKeyNumberPad0;
        public static bool KeyboardKeyDecimal;
        public static bool KeyboardKeyOem102;
        public static bool KeyboardKeyF11;
        public static bool KeyboardKeyF12;
        public static bool KeyboardKeyF13;
        public static bool KeyboardKeyF14;
        public static bool KeyboardKeyF15;
        public static bool KeyboardKeyKana;
        public static bool KeyboardKeyAbntC1;
        public static bool KeyboardKeyConvert;
        public static bool KeyboardKeyNoConvert;
        public static bool KeyboardKeyYen;
        public static bool KeyboardKeyAbntC2;
        public static bool KeyboardKeyNumberPadEquals;
        public static bool KeyboardKeyPreviousTrack;
        public static bool KeyboardKeyAT;
        public static bool KeyboardKeyColon;
        public static bool KeyboardKeyUnderline;
        public static bool KeyboardKeyKanji;
        public static bool KeyboardKeyStop;
        public static bool KeyboardKeyAX;
        public static bool KeyboardKeyUnlabeled;
        public static bool KeyboardKeyNextTrack;
        public static bool KeyboardKeyNumberPadEnter;
        public static bool KeyboardKeyRightControl;
        public static bool KeyboardKeyMute;
        public static bool KeyboardKeyCalculator;
        public static bool KeyboardKeyPlayPause;
        public static bool KeyboardKeyMediaStop;
        public static bool KeyboardKeyVolumeDown;
        public static bool KeyboardKeyVolumeUp;
        public static bool KeyboardKeyWebHome;
        public static bool KeyboardKeyNumberPadComma;
        public static bool KeyboardKeyDivide;
        public static bool KeyboardKeyPrintScreen;
        public static bool KeyboardKeyRightAlt;
        public static bool KeyboardKeyPause;
        public static bool KeyboardKeyHome;
        public static bool KeyboardKeyUp;
        public static bool KeyboardKeyPageUp;
        public static bool KeyboardKeyLeft;
        public static bool KeyboardKeyRight;
        public static bool KeyboardKeyEnd;
        public static bool KeyboardKeyDown;
        public static bool KeyboardKeyPageDown;
        public static bool KeyboardKeyInsert;
        public static bool KeyboardKeyDelete;
        public static bool KeyboardKeyLeftWindowsKey;
        public static bool KeyboardKeyRightWindowsKey;
        public static bool KeyboardKeyApplications;
        public static bool KeyboardKeyPower;
        public static bool KeyboardKeySleep;
        public static bool KeyboardKeyWake;
        public static bool KeyboardKeyWebSearch;
        public static bool KeyboardKeyWebFavorites;
        public static bool KeyboardKeyWebRefresh;
        public static bool KeyboardKeyWebStop;
        public static bool KeyboardKeyWebForward;
        public static bool KeyboardKeyWebBack;
        public static bool KeyboardKeyMyComputer;
        public static bool KeyboardKeyMail;
        public static bool KeyboardKeyMediaSelect;
        public static bool KeyboardKeyUnknown;
        public bool KeyboardInputHookConnect()
        {
            try
            {
                directInput = new DirectInput();
                keyboardGuid = new Guid[] { Guid.Empty };
                foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Keyboard, DeviceEnumerationFlags.AllDevices))
                {
                    keyboardGuid[knum] = deviceInstance.InstanceGuid;
                    knum++;
                    if (knum >= 1)
                        break;
                }
            }
            catch { }
            if (keyboardGuid[0] == Guid.Empty)
            {
                return false;
            }
            else
            {
                for (int inc = 0; inc < knum; inc++)
                {
                    keyboard[inc] = new Keyboard(directInput);
                    keyboard[inc].Properties.BufferSize = 128;
                    keyboard[inc].Acquire();
                }
                return true;
            }
        }
        public void KeyboardInputProcess()
        {
            for (int inc = 0; inc < knum; inc++)
            {
                keyboard[inc].Poll();
                var datas = keyboard[inc].GetBufferedData();
                foreach (var state in datas)
                {
                    if (inc == 0 & state.IsPressed & state.Key == Key.Escape)
                        KeyboardKeyEscape = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Escape)
                        KeyboardKeyEscape = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D1)
                        KeyboardKeyD1 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D1)
                        KeyboardKeyD1 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D2)
                        KeyboardKeyD2 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D2)
                        KeyboardKeyD2 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D3)
                        KeyboardKeyD3 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D3)
                        KeyboardKeyD3 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D4)
                        KeyboardKeyD4 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D4)
                        KeyboardKeyD4 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D5)
                        KeyboardKeyD5 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D5)
                        KeyboardKeyD5 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D6)
                        KeyboardKeyD6 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D6)
                        KeyboardKeyD6 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D7)
                        KeyboardKeyD7 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D7)
                        KeyboardKeyD7 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D8)
                        KeyboardKeyD8 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D8)
                        KeyboardKeyD8 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D9)
                        KeyboardKeyD9 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D9)
                        KeyboardKeyD9 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D0)
                        KeyboardKeyD0 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D0)
                        KeyboardKeyD0 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Minus)
                        KeyboardKeyMinus = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Minus)
                        KeyboardKeyMinus = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Equals)
                        KeyboardKeyEquals = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Equals)
                        KeyboardKeyEquals = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Back)
                        KeyboardKeyBack = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Back)
                        KeyboardKeyBack = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Tab)
                        KeyboardKeyTab = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Tab)
                        KeyboardKeyTab = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Q)
                        KeyboardKeyQ = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Q)
                        KeyboardKeyQ = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.W)
                        KeyboardKeyW = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.W)
                        KeyboardKeyW = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.E)
                        KeyboardKeyE = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.E)
                        KeyboardKeyE = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.R)
                        KeyboardKeyR = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.R)
                        KeyboardKeyR = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.T)
                        KeyboardKeyT = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.T)
                        KeyboardKeyT = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Y)
                        KeyboardKeyY = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Y)
                        KeyboardKeyY = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.U)
                        KeyboardKeyU = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.U)
                        KeyboardKeyU = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.I)
                        KeyboardKeyI = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.I)
                        KeyboardKeyI = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.O)
                        KeyboardKeyO = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.O)
                        KeyboardKeyO = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.P)
                        KeyboardKeyP = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.P)
                        KeyboardKeyP = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.LeftBracket)
                        KeyboardKeyLeftBracket = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.LeftBracket)
                        KeyboardKeyLeftBracket = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.RightBracket)
                        KeyboardKeyRightBracket = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.RightBracket)
                        KeyboardKeyRightBracket = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Return)
                        KeyboardKeyReturn = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Return)
                        KeyboardKeyReturn = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.LeftControl)
                        KeyboardKeyLeftControl = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.LeftControl)
                        KeyboardKeyLeftControl = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.A)
                        KeyboardKeyA = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.A)
                        KeyboardKeyA = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.S)
                        KeyboardKeyS = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.S)
                        KeyboardKeyS = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.D)
                        KeyboardKeyD = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.D)
                        KeyboardKeyD = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F)
                        KeyboardKeyF = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F)
                        KeyboardKeyF = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.G)
                        KeyboardKeyG = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.G)
                        KeyboardKeyG = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.H)
                        KeyboardKeyH = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.H)
                        KeyboardKeyH = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.J)
                        KeyboardKeyJ = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.J)
                        KeyboardKeyJ = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.K)
                        KeyboardKeyK = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.K)
                        KeyboardKeyK = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.L)
                        KeyboardKeyL = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.L)
                        KeyboardKeyL = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Semicolon)
                        KeyboardKeySemicolon = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Semicolon)
                        KeyboardKeySemicolon = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Apostrophe)
                        KeyboardKeyApostrophe = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Apostrophe)
                        KeyboardKeyApostrophe = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Grave)
                        KeyboardKeyGrave = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Grave)
                        KeyboardKeyGrave = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.LeftShift)
                        KeyboardKeyLeftShift = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.LeftShift)
                        KeyboardKeyLeftShift = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Backslash)
                        KeyboardKeyBackslash = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Backslash)
                        KeyboardKeyBackslash = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Z)
                        KeyboardKeyZ = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Z)
                        KeyboardKeyZ = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.X)
                        KeyboardKeyX = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.X)
                        KeyboardKeyX = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.C)
                        KeyboardKeyC = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.C)
                        KeyboardKeyC = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.V)
                        KeyboardKeyV = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.V)
                        KeyboardKeyV = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.B)
                        KeyboardKeyB = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.B)
                        KeyboardKeyB = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.N)
                        KeyboardKeyN = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.N)
                        KeyboardKeyN = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.M)
                        KeyboardKeyM = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.M)
                        KeyboardKeyM = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Comma)
                        KeyboardKeyComma = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Comma)
                        KeyboardKeyComma = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Period)
                        KeyboardKeyPeriod = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Period)
                        KeyboardKeyPeriod = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Slash)
                        KeyboardKeySlash = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Slash)
                        KeyboardKeySlash = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.RightShift)
                        KeyboardKeyRightShift = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.RightShift)
                        KeyboardKeyRightShift = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Multiply)
                        KeyboardKeyMultiply = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Multiply)
                        KeyboardKeyMultiply = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.LeftAlt)
                        KeyboardKeyLeftAlt = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.LeftAlt)
                        KeyboardKeyLeftAlt = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Space)
                        KeyboardKeySpace = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Space)
                        KeyboardKeySpace = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Capital)
                        KeyboardKeyCapital = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Capital)
                        KeyboardKeyCapital = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F1)
                        KeyboardKeyF1 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F1)
                        KeyboardKeyF1 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F2)
                        KeyboardKeyF2 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F2)
                        KeyboardKeyF2 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F3)
                        KeyboardKeyF3 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F3)
                        KeyboardKeyF3 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F4)
                        KeyboardKeyF4 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F4)
                        KeyboardKeyF4 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F5)
                        KeyboardKeyF5 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F5)
                        KeyboardKeyF5 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F6)
                        KeyboardKeyF6 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F6)
                        KeyboardKeyF6 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F7)
                        KeyboardKeyF7 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F7)
                        KeyboardKeyF7 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F8)
                        KeyboardKeyF8 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F8)
                        KeyboardKeyF8 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F9)
                        KeyboardKeyF9 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F9)
                        KeyboardKeyF9 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F10)
                        KeyboardKeyF10 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F10)
                        KeyboardKeyF10 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberLock)
                        KeyboardKeyNumberLock = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberLock)
                        KeyboardKeyNumberLock = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.ScrollLock)
                        KeyboardKeyScrollLock = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.ScrollLock)
                        KeyboardKeyScrollLock = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad7)
                        KeyboardKeyNumberPad7 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad7)
                        KeyboardKeyNumberPad7 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad8)
                        KeyboardKeyNumberPad8 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad8)
                        KeyboardKeyNumberPad8 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad9)
                        KeyboardKeyNumberPad9 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad9)
                        KeyboardKeyNumberPad9 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Subtract)
                        KeyboardKeySubtract = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Subtract)
                        KeyboardKeySubtract = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad4)
                        KeyboardKeyNumberPad4 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad4)
                        KeyboardKeyNumberPad4 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad5)
                        KeyboardKeyNumberPad5 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad5)
                        KeyboardKeyNumberPad5 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad6)
                        KeyboardKeyNumberPad6 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad6)
                        KeyboardKeyNumberPad6 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Add)
                        KeyboardKeyAdd = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Add)
                        KeyboardKeyAdd = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad1)
                        KeyboardKeyNumberPad1 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad1)
                        KeyboardKeyNumberPad1 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad2)
                        KeyboardKeyNumberPad2 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad2)
                        KeyboardKeyNumberPad2 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad3)
                        KeyboardKeyNumberPad3 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad3)
                        KeyboardKeyNumberPad3 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPad0)
                        KeyboardKeyNumberPad0 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPad0)
                        KeyboardKeyNumberPad0 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Decimal)
                        KeyboardKeyDecimal = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Decimal)
                        KeyboardKeyDecimal = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Oem102)
                        KeyboardKeyOem102 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Oem102)
                        KeyboardKeyOem102 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F11)
                        KeyboardKeyF11 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F11)
                        KeyboardKeyF11 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F12)
                        KeyboardKeyF12 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F12)
                        KeyboardKeyF12 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F13)
                        KeyboardKeyF13 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F13)
                        KeyboardKeyF13 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F14)
                        KeyboardKeyF14 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F14)
                        KeyboardKeyF14 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.F15)
                        KeyboardKeyF15 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.F15)
                        KeyboardKeyF15 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Kana)
                        KeyboardKeyKana = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Kana)
                        KeyboardKeyKana = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.AbntC1)
                        KeyboardKeyAbntC1 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.AbntC1)
                        KeyboardKeyAbntC1 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Convert)
                        KeyboardKeyConvert = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Convert)
                        KeyboardKeyConvert = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NoConvert)
                        KeyboardKeyNoConvert = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NoConvert)
                        KeyboardKeyNoConvert = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Yen)
                        KeyboardKeyYen = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Yen)
                        KeyboardKeyYen = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.AbntC2)
                        KeyboardKeyAbntC2 = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.AbntC2)
                        KeyboardKeyAbntC2 = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPadEquals)
                        KeyboardKeyNumberPadEquals = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPadEquals)
                        KeyboardKeyNumberPadEquals = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.PreviousTrack)
                        KeyboardKeyPreviousTrack = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.PreviousTrack)
                        KeyboardKeyPreviousTrack = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.AT)
                        KeyboardKeyAT = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.AT)
                        KeyboardKeyAT = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Colon)
                        KeyboardKeyColon = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Colon)
                        KeyboardKeyColon = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Underline)
                        KeyboardKeyUnderline = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Underline)
                        KeyboardKeyUnderline = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Kanji)
                        KeyboardKeyKanji = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Kanji)
                        KeyboardKeyKanji = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Stop)
                        KeyboardKeyStop = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Stop)
                        KeyboardKeyStop = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.AX)
                        KeyboardKeyAX = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.AX)
                        KeyboardKeyAX = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Unlabeled)
                        KeyboardKeyUnlabeled = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Unlabeled)
                        KeyboardKeyUnlabeled = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NextTrack)
                        KeyboardKeyNextTrack = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NextTrack)
                        KeyboardKeyNextTrack = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPadEnter)
                        KeyboardKeyNumberPadEnter = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPadEnter)
                        KeyboardKeyNumberPadEnter = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.RightControl)
                        KeyboardKeyRightControl = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.RightControl)
                        KeyboardKeyRightControl = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Mute)
                        KeyboardKeyMute = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Mute)
                        KeyboardKeyMute = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Calculator)
                        KeyboardKeyCalculator = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Calculator)
                        KeyboardKeyCalculator = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.PlayPause)
                        KeyboardKeyPlayPause = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.PlayPause)
                        KeyboardKeyPlayPause = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.MediaStop)
                        KeyboardKeyMediaStop = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.MediaStop)
                        KeyboardKeyMediaStop = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.VolumeDown)
                        KeyboardKeyVolumeDown = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.VolumeDown)
                        KeyboardKeyVolumeDown = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.VolumeUp)
                        KeyboardKeyVolumeUp = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.VolumeUp)
                        KeyboardKeyVolumeUp = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebHome)
                        KeyboardKeyWebHome = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebHome)
                        KeyboardKeyWebHome = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.NumberPadComma)
                        KeyboardKeyNumberPadComma = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.NumberPadComma)
                        KeyboardKeyNumberPadComma = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Divide)
                        KeyboardKeyDivide = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Divide)
                        KeyboardKeyDivide = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.PrintScreen)
                        KeyboardKeyPrintScreen = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.PrintScreen)
                        KeyboardKeyPrintScreen = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.RightAlt)
                        KeyboardKeyRightAlt = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.RightAlt)
                        KeyboardKeyRightAlt = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Pause)
                        KeyboardKeyPause = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Pause)
                        KeyboardKeyPause = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Home)
                        KeyboardKeyHome = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Home)
                        KeyboardKeyHome = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Up)
                        KeyboardKeyUp = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Up)
                        KeyboardKeyUp = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.PageUp)
                        KeyboardKeyPageUp = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.PageUp)
                        KeyboardKeyPageUp = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Left)
                        KeyboardKeyLeft = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Left)
                        KeyboardKeyLeft = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Right)
                        KeyboardKeyRight = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Right)
                        KeyboardKeyRight = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.End)
                        KeyboardKeyEnd = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.End)
                        KeyboardKeyEnd = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Down)
                        KeyboardKeyDown = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Down)
                        KeyboardKeyDown = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.PageDown)
                        KeyboardKeyPageDown = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.PageDown)
                        KeyboardKeyPageDown = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Insert)
                        KeyboardKeyInsert = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Insert)
                        KeyboardKeyInsert = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Delete)
                        KeyboardKeyDelete = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Delete)
                        KeyboardKeyDelete = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.LeftWindowsKey)
                        KeyboardKeyLeftWindowsKey = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.LeftWindowsKey)
                        KeyboardKeyLeftWindowsKey = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.RightWindowsKey)
                        KeyboardKeyRightWindowsKey = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.RightWindowsKey)
                        KeyboardKeyRightWindowsKey = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Applications)
                        KeyboardKeyApplications = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Applications)
                        KeyboardKeyApplications = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Power)
                        KeyboardKeyPower = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Power)
                        KeyboardKeyPower = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Sleep)
                        KeyboardKeySleep = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Sleep)
                        KeyboardKeySleep = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Wake)
                        KeyboardKeyWake = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Wake)
                        KeyboardKeyWake = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebSearch)
                        KeyboardKeyWebSearch = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebSearch)
                        KeyboardKeyWebSearch = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebFavorites)
                        KeyboardKeyWebFavorites = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebFavorites)
                        KeyboardKeyWebFavorites = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebRefresh)
                        KeyboardKeyWebRefresh = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebRefresh)
                        KeyboardKeyWebRefresh = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebStop)
                        KeyboardKeyWebStop = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebStop)
                        KeyboardKeyWebStop = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebForward)
                        KeyboardKeyWebForward = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebForward)
                        KeyboardKeyWebForward = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.WebBack)
                        KeyboardKeyWebBack = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.WebBack)
                        KeyboardKeyWebBack = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.MyComputer)
                        KeyboardKeyMyComputer = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.MyComputer)
                        KeyboardKeyMyComputer = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Mail)
                        KeyboardKeyMail = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Mail)
                        KeyboardKeyMail = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.MediaSelect)
                        KeyboardKeyMediaSelect = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.MediaSelect)
                        KeyboardKeyMediaSelect = false;
                    if (inc == 0 & state.IsPressed & state.Key == Key.Unknown)
                        KeyboardKeyUnknown = true;
                    if (inc == 0 & state.IsReleased & state.Key == Key.Unknown)
                        KeyboardKeyUnknown = false;
                }
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
        }
    }
}