using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace WallP
{
    class Program
    {
        static void Main(string[] args)
        {
            string myName = typeof(Program).Namespace;
            string WPpath = "";
            string ImagePath = "";
            uint MonitorIndex = 999999999;
            string monitorID;
            int position = 99;
            string BackgroundColor = "";
            string MonitorUID = "All";
            uint MonitorCount = 1;
            bool ConMode = GetConsoleWindow() != IntPtr.Zero;

            if (args.Length == 0)
            {

                if (ConMode)
                {
                    Console.WriteLine("Set wallpaper for one or more monitors");
                    Console.WriteLine("Full functionality requires Windows 8 or higher");
                    Console.WriteLine("Windows 7 limited to setting wallpaper for all monitors");
                    Console.WriteLine("Usage: WallP.exe [MonitorIndex] [ImageFilePath] [Position] [BackgroundColor]");
                    Console.WriteLine("Parameters can be specified in any order");
                    Console.WriteLine("MonitorIndex is a zero-based integer");
                    Console.WriteLine("ImageFilePath can be an absolute or relative path");
                    Console.WriteLine("If MonitorIndex is omitted, wallpaper will be set for all monitors");
                    Console.WriteLine("If ImageFilePath is omitted, MonitorIndex wallpaper path will be returned");
                    Console.WriteLine("Position can be one of: Center Tile Stretch Fit Fill Span");
                    Console.WriteLine("If Position is omitted, position is unchanged for Center Stretch Fit Fill");
                    Console.WriteLine("If Position is omitted, Span and Tile revert to Fill");
                    Console.WriteLine("BackgroundColor is specified as r,g,b. Example (Cool blue): 45,125,154");
                }
                else
                {
                    CustomMessageBox.Show("\nSet wallpaper for one or more monitors" +
                      "\n\nFull functionality requires Windows 8 or higher" +
                      "\n\nWindows 7 limited to setting wallpaper for all monitors" +
                      "\n\nUsage: WallP.exe [MonitorIndex] [ImageFilePath] [Position] [BackgroundColor]" +
                      "\n\nParameters can be specified in any order" +
                      "\n\nMonitorIndex is a zero-based integer" +
                      "\n\nImageFilePath can be an absolute or relative path" +
                      "\n\nIf MonitorIndex is omitted, wallpaper will be set for all monitors" +
                      "\n\nIf ImageFilePath is omitted, MonitorIndex wallpaper path will be returned" +
                      "\n\nPosition can be one of: Center Tile Stretch Fit Fill Span" +
                      "\n\nIf Position is omitted, position is unchanged for Center Stretch Fit Fill" +
                      "\n\nIf Position is omitted, Span and Tile revert to Fill" +
                      "\n\nBackgroundColor is specified as r,g,b. Example (Cool blue): 45,125,154");
                }

            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "center") { position = 0; }
                    if (args[i].ToLower() == "tile") { position = 1; }
                    if (args[i].ToLower() == "stretch") { position = 2; }
                    if (args[i].ToLower() == "fit") { position = 3; }
                    if (args[i].ToLower() == "fill") { position = 4; }
                    if (args[i].ToLower() == "span") { position = 5; }
                    if (System.IO.File.Exists(args[i])) { ImagePath = args[i]; WPpath = ImagePath; }
                    try { MonitorIndex = Convert.ToUInt32(args[i]); }
                    catch { }
                    if (args[i].Contains(",")) { BackgroundColor = args[i]; }
                }

                string NTVer = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "6.0");
                if (Version.Parse(NTVer) < new Version("6.1"))
                {
                    string Msg = "Windows 7 or greater is required";
                    if (ConMode)
                    {
                        Console.WriteLine(Msg);
                    }
                    else
                    {
                        MessageBox.Show(Msg, myName, MessageBoxButtons.OK);
                    }
                    return;
                }
                bool Win7 = NTVer == "6.1";
                RegistryKey Software = Registry.CurrentUser.CreateSubKey("Software");

                if (WPpath != "")
                {
                    WPpath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WPpath));
                }

                if (Win7)
                {
                    if (WPpath != "")
                    {
                        string DesktopKey = @"HKEY_CURRENT_USER\Control Panel\Desktop";
                        Registry.SetValue(DesktopKey, "Wallpaper", WPpath, RegistryValueKind.String);
                        Registry.SetValue(DesktopKey, "WallpaperStyle", "10", RegistryValueKind.String);
                        Registry.SetValue(DesktopKey, "TileWallpaper", "0", RegistryValueKind.String);

                        SystemParametersInfo(0x0014, 0, WPpath, 0x2);
                    }
                    WPpath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", "");
                }
                else
                {
                    IDesktopWallpaper handler = (IDesktopWallpaper)new DesktopWallpaperClass();
                    MonitorCount = handler.GetMonitorDevicePathCount();
                    if ((MonitorIndex >= MonitorCount) && (MonitorIndex != 999999999)) { return; }
                    try { monitorID = handler.GetMonitorDevicePathAt(MonitorIndex); }
                    catch { monitorID = ""; }
                    try { MonitorUID = monitorID.Substring(monitorID.IndexOf("UID"), 11); }
                    catch { MonitorUID = "All"; }
                    if (WPpath != "") { handler.SetWallpaper(monitorID, WPpath); }
                    WPpath = handler.GetWallpaper(monitorID);
                    if (position != 99) { handler.SetPosition(position); }
                    if (BackgroundColor != "") { handler.SetBackgroundColor(IntColor(BackgroundColor)); }
                }
                using (RegistryKey WallPKey = Software.CreateSubKey("WallP"))
                {
                    WallPKey.SetValue("MonitorCount", MonitorCount);
                    WallPKey.SetValue("Monitor", MonitorUID);
                    WallPKey.SetValue("WPpath", WPpath);
                    using (RegistryKey MonitorUIDKey = WallPKey.CreateSubKey(MonitorUID))
                    {
                        MonitorUIDKey.SetValue("WPpath", WPpath);
                    }
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", "BackgroundType", 0, RegistryValueKind.DWord);
                }
                if (ImagePath == "")
                {
                    if (ConMode)
                    {
                        Console.WriteLine(MonitorUID);
                        Console.WriteLine(WPpath);
                    }
                    else
                    {
                        MessageBox.Show($"{MonitorUID}\n\n{WPpath}", myName, MessageBoxButtons.OK);
                    }
                }
            }

        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        private static uint IntColor(string rgb)
        {
            uint r = 0;
            uint g = 0;
            uint b = 0;
            string[] RGB = rgb.Split(',');
            try { r = Convert.ToUInt32(RGB[0]); }
            catch { }
            try { g = Convert.ToUInt32(RGB[1]); }
            catch { }
            try { b = Convert.ToUInt32(RGB[2]); }
            catch { }
            Color c = Color.FromArgb(0, (byte)r, (byte)g, (byte)b);
            return (uint)((c.R << 0) | (c.G << 8) | (c.B << 16));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [ComImport, Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDesktopWallpaper
        {
            void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string WPpath);
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetMonitorDevicePathAt(uint monitorIndex);
            [return: MarshalAs(UnmanagedType.U4)]
            uint GetMonitorDevicePathCount();
            [return: MarshalAs(UnmanagedType.Struct)]
            Rect GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID);
            void SetBackgroundColor([MarshalAs(UnmanagedType.U4)] uint color);
            [return: MarshalAs(UnmanagedType.U4)]
            uint GetBackgroundColor();
            void SetPosition([MarshalAs(UnmanagedType.I4)] int position);
            [return: MarshalAs(UnmanagedType.I4)]
            string GetPosition();
            void SetSlideshow(IntPtr items);
            IntPtr GetSlideshow();
            bool Enable();
        }
        [ComImport, Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
        public class DesktopWallpaperClass
        {
        }
    }

    public class CustomMessageBox : Form
    {
        private Panel scrollPanel;
        private Label messageLabel;

        public CustomMessageBox(string message)
        {
            InitializeComponents();

            // Set the message text
            messageLabel.Text = message;

            // Set the form properties
            this.Text = "WallP";
            this.Size = new Size(650, 465);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowIcon = false;
            this.Font = new Font("Consolas", 10);
        }

        private void InitializeComponents()
        {
            scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill; // The panel fills the entire form
            scrollPanel.AutoScroll = true;

            messageLabel = new Label();
            messageLabel.AutoSize = true;
            messageLabel.MaximumSize = new Size(this.Width - 40, 0); // Set max width for text wrapping
            messageLabel.Location = new Point(0, 0);

            scrollPanel.Controls.Add(messageLabel);
            this.Controls.Add(scrollPanel);

            // Adjust the layout when the form is resized
            this.Resize += (sender, e) =>
            {
                messageLabel.MaximumSize = new Size(this.Width - 40, 0); // Adjust max width for wrapping
            };
        }

        public static void Show(string message)
        {
            using (CustomMessageBox customMessageBox = new CustomMessageBox(message))
            {
                customMessageBox.ShowDialog();
            }
        }
    }


}