#nullable enable
using FilesUpgrade.Model;
using FilesUpgrade.Monad;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Validation
{
    public static class MainValidation
    {
        /// <returns>(source, target)</returns>
        public static Subsystem<(string, string)> ValidateUpgradeParam(Seq<string> args) => () =>
        {
            var paths = args.Count() switch
            {
                var c when c >= 2 => (args[0], args[1]),
                var c when c == 1 => (args[0], Directory.GetCurrentDirectory()),
                _ => ("", "")
            };

            return Out<(string, string)>.FromValue(paths);
        };

        public static Subsystem<bool> CheckFileExist(FileInfo fileInfo) => () =>
            fileInfo.Exists 
                ? Out<bool>.FromValue(true) 
                : Out<bool>.FromError(($"File {fileInfo.FullName} is not existed"));

        public static Subsystem<DirectoryInfo> CheckFolderExistOrCreate(string path) => () =>
            !Directory.Exists(path)
                ? Out<DirectoryInfo>.FromValue(Directory.CreateDirectory(path))
                : Out<DirectoryInfo>.FromValue(new DirectoryInfo(path));

        public static Subsystem<bool> IsZipFile(FileInfo fileInfo) => () =>
            Path.GetExtension(fileInfo.Name) == ".zip" 
                ? Out<bool>.FromValue(true)
                : Out<bool>.FromError($"File {fileInfo.FullName} is not a zip file.");
    }
}
