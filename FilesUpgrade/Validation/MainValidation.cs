#nullable enable
using FilesUpgrade.IO;
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
    public class MainValidation
    {
        private readonly FileSystem fs;

        public MainValidation(FileSystem fs)
        {
            this.fs = fs;
        }

        /// <returns>(source, target)</returns>
        public Subsystem<(string, string)> ValidateUpgradeParam(Seq<string> args) => () =>
        {
            var paths = args.Count() switch
            {
                var c when c >= 2 => (args[0], args[1]),
                var c when c == 1 => (args[0], Directory.GetCurrentDirectory()),
                _ => ("", "")
            };

            return Out<(string, string)>.FromValue(paths);
        };

        public Subsystem<bool> CheckFileExist(FileInfo fileInfo) => () =>
            fileInfo.Exists 
                ? Out<bool>.FromValue(true) 
                : Out<bool>.FromError(($"File {fileInfo.FullName} is not existed"));

        public Subsystem<DirectoryInfo> CheckFolderExistOrCreate(string path) => () =>
            !Directory.Exists(path)
                ? Out<DirectoryInfo>.FromValue(Directory.CreateDirectory(path))
                : Out<DirectoryInfo>.FromValue(new DirectoryInfo(path));

        public Subsystem<bool> IsZipFile(FileInfo fileInfo) => () =>
            Path.GetExtension(fileInfo.Name) == ".zip" 
                ? Out<bool>.FromValue(true)
                : Out<bool>.FromError($"File {fileInfo.FullName} is not a zip file.");

        public Subsystem<Either<FileInfo, DirectoryInfo>> ValidateSource(string source) =>
            Directory.Exists(source)
                ? ValidateAsDir(source).Bind(d => Subsystem.Return<Either<FileInfo, DirectoryInfo>>(d))
                : ValidateAsFile(source).Bind(f => Subsystem.Return<Either<FileInfo, DirectoryInfo>>(f));

        private Subsystem<FileInfo> ValidateAsFile(string source) =>
            from fileinfo in fs.GetFileInfo(source)
            from _1 in CheckFileExist(fileinfo)
            from _2 in IsZipFile(fileinfo)
            from _3 in Subsystem.WriteLine($"Check Upgrade file {fileinfo.Name}({fileinfo.Length / 1024}kb) is existed.")
            select fileinfo;

        private Subsystem<DirectoryInfo> ValidateAsDir(string source) =>
            from dirInfo in fs.GetDirectoryInfo(source)
            let size = fs.GetDirectorySize(source)
            from _ in Subsystem.WriteLine($"Check Upgrade file {dirInfo.Name}({size / 1024}kb) is existed.")
            select dirInfo;
    }
}
