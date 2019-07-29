#nullable enable
using FilesUpgrade.Monad;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesUpgrade.Validation
{
    public static class MainValidation
    {
        public static Subsystem<string> ValidateUpgradeParam(Seq<string> args) =>
            Subsystem.Return(args.Count() > 0 ? args[0] : "");

        public static Subsystem<bool> CheckFileExist(FileInfo fileInfo) =>
            fileInfo.Exists 
                ? Subsystem.Return(true) 
                : Subsystem.Fail<bool>($"File {fileInfo.FullName} is not existed");

        public static Subsystem<bool> IsZipFile(FileInfo fileInfo) =>
            Path.GetExtension(fileInfo.Name) == ".zip" 
                ? Subsystem.Return(true) 
                : Subsystem.Fail<bool>($"File {fileInfo.FullName} is not a zip file.");
    }
}
