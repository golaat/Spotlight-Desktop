using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Spotlight_Desktop
{
    internal static class Program
    {
        private static string _currSpotlightPath;

        private static bool _RunOnce = false;

        const int SetWallpaper = 20;
        const int UpdateIniFile = 0x01;
        const int SendWinIniChange = 0x02;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);


        private static void ChangeWallpaper(string pathToImage)
        {
            SystemParametersInfo(SetWallpaper, 0, pathToImage, UpdateIniFile | SendWinIniChange);
        }


        private static void UpdateDesktop()
        {
            Console.WriteLine("The current Spotlight Lock Screen image is located at:\n" + _currSpotlightPath + "\n");
            ChangeWallpaper(_currSpotlightPath);
        }

        // Show output only if in a command prompt
        [DllImport("kernel32.dll")]
        private static extern void AttachConsole(int dwProcessId);

        private static void ParseArgs(List<string> args)
        {
            if (args == null || args.Count == 0)
                return;

            _RunOnce = args.Count(arg => arg.ToLower().EndsWith("runonce")) > 0;

        }

        private static void Main(string[] args)
        {
            AttachConsole(-1);
            ParseArgs(args.ToList());

            int count = 0;
            while (true)
            {
                // Run every minute
                string latestCurrentImage = FindImage.FindCurrentImage();
                if (_currSpotlightPath != latestCurrentImage)
                {
                    _currSpotlightPath = latestCurrentImage;
                    UpdateDesktop();
                }

                // Run twice a day
                if ((count % 60 * 12) == 0 && File.Exists("update.exe"))
                {
                    System.Diagnostics.Process.Start("update.exe");
                    count = 0;
                }

                if (_RunOnce)
                {
                    break;
                }

                // Check every minute
                Thread.Sleep(60 * 1000);
                count++;
            }

        }
    }
}
