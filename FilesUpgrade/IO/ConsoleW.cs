using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.IO
{
    public static class ConsoleW
    {
        public static Unit WriteLine(string value) =>
            fun(() => Console.WriteLine(value))();
    }
}
