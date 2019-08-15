using LanguageExt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.IO
{
    public class Diff
    {
        /// <summary>
        /// git diff.exe process to show diff
        /// </summary>
        public Unit ShowDiff(string path1, string path2)
        {
            var info = new ProcessStartInfo(@".\diff.exe", $@"--unified --color {path1} {path2}")
            {
                UseShellExecute = false
            };
            var proc = Process.Start(info);
            proc.WaitForExit();
            return unit;
        }
    }
}
