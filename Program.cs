using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WallP
{
    class Program
    {
        static void Main(string[] args)
        {
            string myName = typeof(Program).Namespace;
            string WPpath = "";
            string WPprev = "";
            string ImagePath = "";
            uint MonitorIndex = 999999999;
            string monitorID;
            int position = 99;
            string BackgroundColor = "";
            string MonitorUID = "All";
            uint MonitorCount = 1;
            bool ConMode = GetConsoleWindow() != IntPtr.Zero;
            bool getAvgColor = false;
            string AvgColor = "";
            string WallpaperKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";
            string SlideshowFolder = "";

            if (args.Length == 0)
            {

                if (ConMode)
                {
                    Console.WriteLine("Set wallpaper for one or more monitors");
                    Console.WriteLine("Full functionality requires Windows 8 or higher");
                    Console.WriteLine("Windows 7 limited to setting wallpaper for all monitors");
                    Console.WriteLine("Usage: WallP.exe [MonitorIndex] [ImageFilePath] [SlideshowFolder] [Position] [BackgroundColor] [/c]");
                    Console.WriteLine("Parameters can be specified in any order");
                    Console.WriteLine("MonitorIndex is a zero-based integer");
                    Console.WriteLine("ImageFilePath can be an absolute or relative path, or None to unset wallpaper");
                    Console.WriteLine("SlideshowFolder must be a complete path");
                    Console.WriteLine("If MonitorIndex is omitted, wallpaper will be set for all monitors");
                    Console.WriteLine("If ImageFilePath is omitted, MonitorIndex wallpaper path will be returned");
                    Console.WriteLine("Position can be one of: Center Tile Stretch Fit Fill Span");
                    Console.WriteLine("If Position is omitted, position is unchanged for Center Stretch Fit Fill");
                    Console.WriteLine("If Position is omitted, Span and Tile revert to Fill");
                    Console.WriteLine("BackgroundColor is specified as r,g,b. Example (Cool blue): 45,125,154");
                    Console.WriteLine("Include /c to calculate the current wallpaper's average color");
                }
                else
                {
                    CustomMessageBox.Show("\nSet wallpaper for one or more monitors" +
                      "\n\nFull functionality requires Windows 8 or higher" +
                      "\n\nWindows 7 limited to setting wallpaper for all monitors" +
                      "\n\nUsage: WallP.exe [MonitorIndex] [ImageFilePath] [Position] [BackgroundColor] [/c]" +
                      "\n\nParameters can be specified in any order" +
                      "\n\nMonitorIndex is a zero-based integer" +
                      "\n\nImageFilePath can be an absolute or relative path, or None to unset wallpaper" +
                      "\n\nSlideshowFolder must be a complete path" +
                      "\n\nIf MonitorIndex is omitted, wallpaper will be set for all monitors, or None to unset wallpaper" +
                      "\n\nIf ImageFilePath is omitted, MonitorIndex wallpaper path will be returned" +
                      "\n\nPosition can be one of: Center Tile Stretch Fit Fill Span" +
                      "\n\nIf Position is omitted, position is unchanged for Center Stretch Fit Fill" +
                      "\n\nIf Position is omitted, Span and Tile revert to Fill" +
                      "\n\nBackgroundColor is specified as r,g,b. Example (Cool blue): 45,125,154" +
                      "\n\nInclude /c to calculate the current wallpaper's average color");
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
                    if (args[i].ToLower() == "none") { ImagePath = ""; WPpath = "none"; }
                    if (args[i].ToLower() == "undo") { ImagePath = ""; WPpath = "undo"; }
                    if (System.IO.File.Exists(args[i])) { ImagePath = args[i]; WPpath = ImagePath; }
                    if (System.IO.Directory.Exists(args[i])) { SlideshowFolder = args[i]; }
                    try { MonitorIndex = Convert.ToUInt32(args[i]); } catch { }
                    if (args[i].Contains(",")) { BackgroundColor = args[i]; }
                    if (args[i].ToLower() == "/c") { getAvgColor = true; }
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

                if (ImagePath != "")
                {
                    ImagePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagePath));
                }

                if (Win7)
                {
                    string DesktopKey = @"HKEY_CURRENT_USER\Control Panel\Desktop";
                    WPprev = (string)Registry.GetValue(DesktopKey, "Wallpaper", "");

                    if (WPpath == "undo")
                    {
                        try
                        {
                            using (RegistryKey WallPKey = Software.CreateSubKey("WallP"))
                            {
                                using (RegistryKey All = WallPKey.CreateSubKey("All"))
                                {
                                    WPpath = (string)All.GetValue("WPprev", "");
                                }
                            }
                        }
                        catch { WPpath = WPprev; }
                        ImagePath = WPpath;
                    }

                    if (WPpath != "")
                    {
                        Registry.SetValue(DesktopKey, "Wallpaper", ImagePath, RegistryValueKind.String);
                        Registry.SetValue(DesktopKey, "WallpaperStyle", "10", RegistryValueKind.String);
                        Registry.SetValue(DesktopKey, "TileWallpaper", "0", RegistryValueKind.String);

                        SystemParametersInfo(0x0014, 0, ImagePath, 0x2);
                    }
                    WPpath = (string)Registry.GetValue(DesktopKey, "Wallpaper", "");
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

                    WPprev = handler.GetWallpaper(monitorID);

                    if (WPpath == "undo")
                    {
                        try
                        {
                            using (RegistryKey WallPKey = Software.CreateSubKey("WallP"))
                            {
                                using (RegistryKey MonitorUIDKey = WallPKey.CreateSubKey(MonitorUID))
                                {
                                    WPpath = (string)MonitorUIDKey.GetValue("WPprev", "");
                                }
                            }
                        }
                        catch { WPpath = WPprev;}
                        ImagePath = WPpath;
                    }

                    if (WPpath != "") handler.SetWallpaper(monitorID, ImagePath);

                    if (SlideshowFolder != "") SetWallpaperSlideshow(SlideshowFolder);

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
                        if (WPprev != WPpath) MonitorUIDKey.SetValue("WPprev", WPprev);
                        MonitorUIDKey.SetValue("WPpath", WPpath);
                        if (getAvgColor)
                        {
                            if (WPpath != "" && System.IO.File.Exists(WPpath)) AvgColor = GetAverageColor(WPpath); else AvgColor = GetCurrentBackgroundColorHex();
                            WallPKey.SetValue("AvgColor", AvgColor);
                            MonitorUIDKey.SetValue("AvgColor", AvgColor);
                        }
                    }
                    if (ImagePath != "") Registry.SetValue($@"{WallpaperKey}", "BackgroundType", 0, RegistryValueKind.DWord);
                }
                if (ImagePath == "" && SlideshowFolder == "")
                {
                    if (ConMode)
                    {
                        Console.WriteLine(MonitorUID);
                        Console.WriteLine(WPpath);
                        if (getAvgColor) Console.WriteLine(AvgColor);
                    }
                    else
                    {
                        string output = $"{MonitorUID}\n\n{WPpath}";
                        if (getAvgColor) output += $"\n\n{AvgColor}";
                        MessageBox.Show(output, myName, MessageBoxButtons.OK);
                    }
                }
            }

        }

        public static string GetCurrentBackgroundColorHex()
        {
            try
            {
                IDesktopWallpaper handler = (IDesktopWallpaper)new DesktopWallpaperClass();
                uint color = handler.GetBackgroundColor();

                // The color is in 0x00BBGGRR format (little-endian)
                int r = (int)(color & 0xFF);
                int g = (int)((color >> 8) & 0xFF);
                int b = (int)((color >> 16) & 0xFF);

                return $"#{r:X2}{g:X2}{b:X2}";
            }
            catch
            {
                // Fallback or error handling
                return "#000000";
            }
        }

        public static string GetAverageColor(string imagePath)
        {
            using (var original = new Bitmap(imagePath))
            {
                int maxDim = 100;
                int width = original.Width;
                int height = original.Height;

                // Calculate new dimensions while preserving aspect ratio
                if (width > maxDim || height > maxDim)
                {
                    double scale = Math.Min((double)maxDim / width, (double)maxDim / height);
                    width = (int)(width * scale);
                    height = (int)(height * scale);
                }

                using (var bmp = new Bitmap(width, height))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(original, 0, 0, width, height);

                    long r = 0, gSum = 0, b = 0;
                    int total = width * height;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Color pixel = bmp.GetPixel(x, y);
                            r += pixel.R;
                            gSum += pixel.G;
                            b += pixel.B;
                        }
                    }

                    int avgR = (int)(r / total);
                    int avgG = (int)(gSum / total);
                    int avgB = (int)(b / total);

                    return $"#{avgR:X2}{avgG:X2}{avgB:X2}";
                }
            }
        }

        public static void SetWallpaperSlideshow(string folderPath)
        {
            // Create IShellItem for the folder
            IShellItem shellItem;
            Guid shellItemGuid = typeof(IShellItem).GUID;
            SHCreateItemFromParsingName(folderPath, IntPtr.Zero, shellItemGuid, out shellItem);

            // Get the PIDL for the folder
            IntPtr pidl = IntPtr.Zero;
            SHGetIDListFromObject(shellItem, out pidl);

            // Create IShellItemArray from the PIDL
            IShellItemArray shellItemArray;
            SHCreateShellItemArrayFromIDLists(1, new IntPtr[] { pidl }, out shellItemArray);

            // Set the slideshow
            IDesktopWallpaper handler = (IDesktopWallpaper)new DesktopWallpaperClass();
            handler.SetSlideshow(Marshal.GetIUnknownForObject(shellItemArray));

            // Free PIDL memory
            if (pidl != IntPtr.Zero) CoTaskMemFree(pidl);
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object punk, out IntPtr ppidl);

        [ComImport]
        [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItemArray
        {
            // Only the methods needed for this operation are defined
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItem
        {
            // Only the methods needed for this operation are defined
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        static extern void SHCreateItemFromParsingName(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItem ppv);

        [DllImport("shell32.dll", PreserveSig = false)]
        static extern void SHCreateShellItemArrayFromIDLists(
            uint cidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] rgpidl,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsi);

        [DllImport("ole32.dll")]
        static extern void CoTaskMemFree(IntPtr ptr);

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
            void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string ImagePath);
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