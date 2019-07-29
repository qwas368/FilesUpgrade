#nullable enable
using FilesUpgrade.IO;
using FilesUpgrade.Monad;
using FilesUpgrade.Validation;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using static FilesUpgrade.Validation.MainValidation;

namespace FilesUpgrade.Service
{
    public class UpgradeService
    {
        private readonly FileSystem fileSystem;

        public UpgradeService(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Subsystem<Unit> Upgrade(string path) =>
            from fileinfo        in fileSystem.GetFileInfo(path)
            from _1              in CheckFileExist(fileinfo)
            from _2              in IsZipFile(fileinfo)
            let _3  = fun(() => Console.WriteLine($"Check Upgrade file {fileinfo.Name}({fileinfo.Length/1024}kb) is existed."))()
            from upzipDictionary in fileSystem.ExtractZipToCurrentDirectory(path)
            let _5  = fun(() => Console.WriteLine($"Unzip to {upzipDictionary}"))()
            select unit;
    }
}
