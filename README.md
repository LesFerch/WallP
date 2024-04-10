# WallP

[![image](https://github.com/LesFerch/WinSetView/assets/79026235/0188480f-ca53-45d5-b9ff-daafff32869e)Download the zip file](https://github.com/LesFerch/WallP/releases/download/1.2.0/WallP.zip)

## Windows command line wallpaper changer for multiple monitors

Compatible with Windows 7, 8, 10, and 11 and multiple monitors.

## How to Download and Run

1. Download the zip file using the link above.
2. Extract **WallP.exe**. The zip file includes both a console version and windows version.
3. Open a Cmd prompt and run **WallP.exe** with no parameters to see the built-in help (or double-click the Windows version).
5. Use **WallP.exe** from the Cmd prompt, or from your own script, to set wallpaper and/or background color.

**Note**: Some antivirus software may falsely detect the download as a virus. This can happen any time you download a new executable and may require extra steps to whitelist the file. In testing, no issues were encountered using Windows Defender on Windows 10 and 11, but a false positive "virus detected" occurred on Windows Server 2022.

## Summary

**WallP** is a command line wallpaper tool for Windows. It can:
- Retrieve the current wallpaper for any given monitor (result is written to console and registry).
- Set the wallpaper for any given monitor or all monitors.
- Set wallpaper position (Center Tile Stretch Fit Fill Span)
- Set the background color for all monitors.

## How to Use

This is a command line tool for technical users. It is typically used from within a script (batch file, VBScript, PowerShell, etc.).

### Built in help

Running **WallP** with no parameters will display the built-in help:
```
Set wallpaper for one or more monitors
Full functionality requires Windows 8 or higher
Windows 7 limited to setting wallpaper for all monitors
Usage: WallP.exe [MonitorIndex] [ImageFilePath] [Position] [BackgroundColor]
Parameters can be specified in any order
MonitorIndex is a zero-based integer
ImageFilePath can be an absolute or relative path
If MonitorIndex is omitted, wallpaper will be set for all monitors
If ImageFilePath is omitted, MonitorIndex wallpaper path will be returned
Position can be one of: Center Tile Stretch Fit Fill Span
If Position is omitted, position is unchanged for Center Stretch Fit Fill
If Position is omitted, Span and Tile revert to Fill
BackgroundColor is specified as r,g,b. Example (Cool blue): 45,125,154
```


### Usage examples

**Example 1**:\
Get current wallpaper path for monitor 0:\
`WallP 0`\
**Note**: The UID for monitor 0 and the current wallpaper path will be written to the console and the registry key `HKCU\Software\WallP`

**Example 2:**\
Set monitor 0 to use img3.jpg:\
`WallP 0 "C:\Windows\Web\Wallpaper\Theme1\img3.jpg"`

**Example 3:**\
Set monitor 1 to use img2.jpg:\
`WallP 1 "C:\Windows\Web\Wallpaper\Theme1\img2.jpg"`

**Example 4:**\
Set all monitors to use wallpaper position "Fill".\
`WallP Fill`

**Example 5:**\
Set background color for all monitors to "Cool Blue"\
`WallP 45,125,154`
\
\
[![image](https://github.com/LesFerch/WinSetView/assets/79026235/63b7acbc-36ef-4578-b96a-d0b7ea0cba3a)](https://github.com/LesFerch/WallP)
