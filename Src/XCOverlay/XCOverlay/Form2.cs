using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
namespace XCOverlay
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static bool closed = false, Motion = false;
        public static int x, y, Width, Height;
        public static bool Controller_A, Controller_B, Controller_X, Controller_Y, Controller_LB, Controller_RB, Controller_LT, Controller_RT, Controller_MAP, Controller_MENU, Controller_LSTICK, Controller_RSTICK, Controller_DU, Controller_DD, Controller_DL, Controller_DR, Controller_XBOX;
        public static double Controller_LX, Controller_LY, Controller_RX, Controller_RY, dzrsx = 0f, dzrsy = 0f, dzlsx = 0f, dzlsy = 0f;
        private void Form2_Shown(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            EO.WebBrowser.Runtime.DefaultEngineOptions.SetDefaultBrowserOptions(options);
            EO.WebEngine.Engine.Default.Options.AllowProprietaryMediaFormats();
            EO.WebEngine.Engine.Default.Options.SetDefaultBrowserOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Create(this.Handle);
            this.webView1.Engine.Options.AllowProprietaryMediaFormats();
            this.webView1.SetOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Engine.Options.DisableGPU = false;
            this.webView1.Engine.Options.DisableSpellChecker = true;
            this.webView1.Engine.Options.CustomUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            string readtext = DecryptFiles("xc-overlay.html.encrypted", "tybtrybrtyertu50727885");
            string filepath = "file:///" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"file:\", "").Replace(Process.GetCurrentProcess().ProcessName + ".exe", "").Replace(@"\", "/").Replace(@"//", "");
            this.webView1.LoadHtml(readtext.Replace("xc-tr.png", filepath + "xc-tr.png").Replace("sr-tr.png", filepath + "sr-tr.png").Replace("sl-tr.png", filepath + "sl-tr.png").Replace("srs-tr.png", filepath + "srs-tr.png").Replace("sls-tr.png", filepath + "sls-tr.png").Replace("bs-tr.png", filepath + "bs-tr.png").Replace("as-tr.png", filepath + "as-tr.png").Replace("xs-tr.png", filepath + "xs-tr.png").Replace("ys-tr.png", filepath + "ys-tr.png").Replace("backs-tr.png", filepath + "backs-tr.png").Replace("selects-tr.png", filepath + "selects-tr.png").Replace("downs-tr.png", filepath + "downs-tr.png").Replace("lefts-tr.png", filepath + "lefts-tr.png").Replace("rights-tr.png", filepath + "rights-tr.png").Replace("ups-tr.png", filepath + "ups-tr.png").Replace("lss-tr.png", filepath + "lss-tr.png").Replace("rss-tr.png", filepath + "rss-tr.png").Replace("lts-tr.png", filepath + "lts-tr.png").Replace("rts-tr.png", filepath + "rts-tr.png"));
        }
        public static string DecryptFiles(string inputFile, string password)
        {
            using (var input = File.OpenRead(inputFile))
            {
                byte[] salt = new byte[8];
                input.Read(salt, 0, salt.Length);
                using (var decryptedStream = new MemoryStream())
                using (var pbkdf = new Rfc2898DeriveBytes(password, salt))
                using (var aes = new RijndaelManaged())
                using (var decryptor = aes.CreateDecryptor(pbkdf.GetBytes(aes.KeySize / 8), pbkdf.GetBytes(aes.BlockSize / 8)))
                using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    string contents;
                    int data;
                    while ((data = cs.ReadByte()) != -1)
                        decryptedStream.WriteByte((byte)data);
                    decryptedStream.Position = 0;
                    using (StreamReader sr = new StreamReader(decryptedStream))
                        contents = sr.ReadToEnd();
                    decryptedStream.Flush();
                    return contents;
                }
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dzrsx = Convert.ToSingle(textBox1.Text);
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            dzrsy = Convert.ToSingle(textBox2.Text);
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            dzlsx = Convert.ToSingle(textBox3.Text);
        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            dzlsy = Convert.ToSingle(textBox4.Text);
        }
        private static double Scale(double value, double min, double max, double minScale, double maxScale)
        {
            double scaled = minScale + (double)(value - min) / (max - min) * (maxScale - minScale);
            return scaled;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Motion)
                Motion = false;
            else if (!Motion)
                Motion = true;
        }
        public void SetController(bool Controller1ButtonAPressed, bool Controller1ButtonBPressed, bool Controller1ButtonXPressed, bool Controller1ButtonYPressed, bool Controller1ButtonStartPressed, bool Controller1ButtonBackPressed, bool Controller1ButtonDownPressed, bool Controller1ButtonUpPressed, bool Controller1ButtonLeftPressed, bool Controller1ButtonRightPressed, bool Controller1ButtonShoulderLeftPressed, bool Controller1ButtonShoulderRightPressed, bool Controller1ThumbpadLeftPressed, bool Controller1ThumbpadRightPressed, double Controller1TriggerLeftPosition, double Controller1TriggerRightPosition, double Controller1ThumbLeftX, double Controller1ThumbLeftY, double Controller1ThumbRightX, double Controller1ThumbRightY)
        {
            try
            {
                if (Controller1ThumbRightX >= 0f)
                    Controller_RX = Scale(Controller1ThumbRightX, dzrsx / 100f * 32767f, 32767f, 0f, 32767f);
                if (Controller1ThumbRightX <= 0f)
                    Controller_RX = Scale(Controller1ThumbRightX, -32767f, -dzrsx / 100f * 32767f, -32767f, 0f);
                if (Controller1ThumbRightY >= 0f)
                    Controller_RY = Scale(Controller1ThumbRightY, dzrsy / 100f * 32767f, 32767f, 0f, 32767f);
                if (Controller1ThumbRightY <= 0f)
                    Controller_RY = Scale(Controller1ThumbRightY, -32767f, -dzrsy / 100f * 32767f, -32767f, 0f);
                if (Controller1ThumbLeftX >= 0f)
                    Controller_LX = Scale(Controller1ThumbLeftX, dzlsx / 100f * 32767f, 32767f, 0f, 32767f);
                if (Controller1ThumbLeftX <= 0f)
                    Controller_LX = Scale(Controller1ThumbLeftX, -32767f, -dzlsx / 100f * 32767f, -32767f, 0f);
                if (Controller1ThumbLeftY >= 0f)
                    Controller_LY = Scale(Controller1ThumbLeftY, dzlsy / 100f * 32767f, 32767f, 0f, 32767f);
                if (Controller1ThumbLeftY <= 0f)
                    Controller_LY = Scale(Controller1ThumbLeftY, -32767f, -dzlsy / 100f * 32767f, -32767f, 0f);
                Controller_A = Controller1ButtonAPressed;
                Controller_B = Controller1ButtonBPressed;
                Controller_X = Controller1ButtonXPressed;
                Controller_Y = Controller1ButtonYPressed;
                Controller_LB = Controller1ButtonShoulderLeftPressed;
                Controller_RB = Controller1ButtonShoulderRightPressed;
                Controller_LT = Controller1TriggerLeftPosition >= 250f ? true : false;
                Controller_RT = Controller1TriggerRightPosition >= 250f ? true : false;
                Controller_MAP = Controller1ButtonBackPressed;
                Controller_MENU = Controller1ButtonStartPressed;
                Controller_LSTICK = Controller1ThumbpadLeftPressed;
                Controller_RSTICK = Controller1ThumbpadRightPressed;
                Controller_DU = Controller1ButtonUpPressed;
                Controller_DD = Controller1ButtonDownPressed;
                Controller_DL = Controller1ButtonLeftPressed;
                Controller_DR = Controller1ButtonRightPressed;
                Controller_XBOX = false;
                webView1.GetDOMWindow().InvokeFunction("setController", new object[] { Controller_A.ToString(), Controller_B.ToString(), Controller_X.ToString(), Controller_Y.ToString(), Controller_MAP.ToString(), Controller_MENU.ToString(), Controller_DD.ToString(), Controller_DU.ToString(), Controller_DL.ToString(), Controller_DR.ToString(), Controller_LB.ToString(), Controller_RB.ToString(), Controller_LSTICK.ToString(), Controller_RSTICK.ToString(), Controller_LT.ToString(), Controller_RT.ToString(), Controller_XBOX.ToString(), Controller_LX.ToString(), Controller_LY.ToString(), Controller_RX.ToString(), Controller_RY.ToString(), Motion.ToString() });
            }
            catch { }
        }
        public Image PrintWindow()
        {
            return webView1.Capture(new Rectangle(0, 0, webView1.GetPageSize().Width, webView1.GetPageSize().Height));
        }
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
