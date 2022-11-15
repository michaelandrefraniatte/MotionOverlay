using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
namespace KMOverlay
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
        public static bool closed = false, Desktop = false, English = false;
        public static double sensx = 1f, sensy = 1f;
        public static int x, y, Width, Height;
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
            string readtext = DecryptFiles("km-overlay.html.encrypted", "tybtrybrtyertu50727885");
            string filepath = "file:///" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"file:\", "").Replace(Process.GetCurrentProcess().ProcessName + ".exe", "").Replace(@"\", "/").Replace(@"//", "");
            this.webView1.LoadHtml(readtext.Replace("kbtabs.png", filepath + "kbtabs.png").Replace("kbnumlocks.png", filepath + "kbnumlocks.png").Replace("kbshifts.png", filepath + "kbshifts.png").Replace("kbcontrols.png", filepath + "kbcontrols.png").Replace("kbalts.png", filepath + "kbalts.png").Replace("kbqs.png", filepath + "kbqs.png").Replace("kbws.png", filepath + "kbws.png").Replace("kbes.png", filepath + "kbes.png").Replace("kbrs.png", filepath + "kbrs.png").Replace("kbts.png", filepath + "kbts.png").Replace("kbas.png", filepath + "kbas.png").Replace("kbss.png", filepath + "kbss.png").Replace("kbds.png", filepath + "kbds.png").Replace("kbfs.png", filepath + "kbfs.png").Replace("kbgs.png", filepath + "kbgs.png").Replace("kbzs.png", filepath + "kbzs.png").Replace("kbxs.png", filepath + "kbxs.png").Replace("kbcs.png", filepath + "kbcs.png").Replace("kbvs.png", filepath + "kbvs.png").Replace("kbapostrophes.png", filepath + "kbapostrophes.png").Replace("kbescapes.png", filepath + "kbescapes.png").Replace("kbd1s.png", filepath + "kbd1s.png").Replace("kbd2s.png", filepath + "kbd2s.png").Replace("kbd3s.png", filepath + "kbd3s.png").Replace("kbd4s.png", filepath + "kbd4s.png").Replace("kbd5s.png", filepath + "kbd5s.png").Replace("kbd6s.png", filepath + "kbd6s.png").Replace("kbf1s.png", filepath + "kbf1s.png").Replace("kbf2s.png", filepath + "kbf2s.png").Replace("kbf3s.png", filepath + "kbf3s.png").Replace("kbf4s.png", filepath + "kbf4s.png").Replace("kbf5s.png", filepath + "kbf5s.png").Replace("msb0s.png", filepath + "msb0s.png").Replace("msb1s.png", filepath + "msb1s.png").Replace("msb2s.png", filepath + "msb2s.png").Replace("msb3s.png", filepath + "msb3s.png").Replace("msb4s.png", filepath + "msb4s.png").Replace("mswus.png", filepath + "mswus.png").Replace("mswds.png", filepath + "mswds.png").Replace("kb.png", filepath + "kb.png").Replace("ms.png", filepath + "ms.png"));
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
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Desktop)
                Desktop = false;
            else if (!Desktop)
                Desktop = true;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (English)
                English = false;
            else if (!English)
                English = true;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            sensx = Convert.ToSingle(textBox1.Text);
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            sensy = Convert.ToSingle(textBox2.Text);
        }
        public void SetController(bool KeyboardKeyTab,bool KeyboardKeyNumberLock,bool KeyboardKeyLeftShift,bool KeyboardKeyLeftControl,bool KeyboardKeyLeftAlt,bool KeyboardKeyQ,bool KeyboardKeyW,bool KeyboardKeyE,bool KeyboardKeyR,bool KeyboardKeyT,bool KeyboardKeyA,bool KeyboardKeyS,bool KeyboardKeyD,bool KeyboardKeyF,bool KeyboardKeyG,bool KeyboardKeyZ,bool KeyboardKeyX,bool KeyboardKeyC,bool KeyboardKeyV,bool KeyboardKeyApostrophe,bool KeyboardKeyEscape,bool KeyboardKeyD1,bool KeyboardKeyD2,bool KeyboardKeyD3,bool KeyboardKeyD4,bool KeyboardKeyD5,bool KeyboardKeyD6,bool KeyboardKeyF1,bool KeyboardKeyF2,bool KeyboardKeyF3,bool KeyboardKeyF4,bool KeyboardKeyF5,bool MouseButtons0,bool MouseButtons1,bool MouseButtons2,bool MouseButtons3,bool MouseButtons4,bool MouseButtons5,bool MouseButtons6,bool MouseButtons7, double MouseAxisX, double MouseAxisY, bool MouseWheelUp, bool MouseWheelDown)
        {
            try
            {
                webView1.GetDOMWindow().InvokeFunction("setController", new object[] { KeyboardKeyTab.ToString(), KeyboardKeyNumberLock.ToString(), KeyboardKeyLeftShift.ToString(), KeyboardKeyLeftControl.ToString(), KeyboardKeyLeftAlt.ToString(), KeyboardKeyQ.ToString(), KeyboardKeyW.ToString(), KeyboardKeyE.ToString(), KeyboardKeyR.ToString(), KeyboardKeyT.ToString(), KeyboardKeyA.ToString(), KeyboardKeyS.ToString(), KeyboardKeyD.ToString(), KeyboardKeyF.ToString(), KeyboardKeyG.ToString(), KeyboardKeyZ.ToString(), KeyboardKeyX.ToString(), KeyboardKeyC.ToString(), KeyboardKeyV.ToString(), KeyboardKeyApostrophe.ToString(), KeyboardKeyEscape.ToString(), KeyboardKeyD1.ToString(), KeyboardKeyD2.ToString(), KeyboardKeyD3.ToString(), KeyboardKeyD4.ToString(), KeyboardKeyD5.ToString(), KeyboardKeyD6.ToString(), KeyboardKeyF1.ToString(), KeyboardKeyF2.ToString(), KeyboardKeyF3.ToString(), KeyboardKeyF4.ToString(), KeyboardKeyF5.ToString(), MouseButtons0.ToString(), MouseButtons1.ToString(), MouseButtons2.ToString(), MouseButtons3.ToString(), MouseButtons4.ToString(), MouseButtons5.ToString(), MouseButtons6.ToString(), MouseButtons7.ToString(), MouseAxisX.ToString(), MouseAxisY.ToString(), MouseWheelUp.ToString(), MouseWheelDown.ToString() });
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