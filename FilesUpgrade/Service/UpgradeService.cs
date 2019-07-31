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
using FilesUpgrade.Model;
using System.IO;

namespace FilesUpgrade.Service
{
    public class UpgradeService
    {
        private readonly FileSystem fileSystem;

        public UpgradeService(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Subsystem<Unit> Upgrade(string source, string target) =>
            from fileinfo        in fileSystem.GetFileInfo(source)
            from _1              in CheckFileExist(fileinfo)
            from _2              in IsZipFile(fileinfo)
            from _3              in Subsystem.WriteLine($"Check Upgrade file {fileinfo.Name}({fileinfo.Length / 1024}kb) is existed.")
            from upzipDictionary in fileSystem.ExtractZipToCurrentDirectory(source)
            from _4              in Subsystem.WriteLine($"Unzip to {upzipDictionary}")
            from targetDic       in CheckFolderExistOrCreate(target)
            from _5              in Subsystem.WriteLine($"Target Directory {targetDic.FullName} is existed.\nShow treeView List.")
            from targetNode      in WalkDirectoryTree(targetDic)
            let _6 = ConsoleW.PrintNode(targetNode, "", true)
            select unit;

        public Subsystem<Node> WalkDirectoryTree(DirectoryInfo root) => () =>
        {
            var files = root.GetFiles("*.*");
            var subDirs = root.GetDirectories("*.*");
            var filesNodes = files.Select(x => new Node(x)).ToSeq();
            var subDirNodes = subDirs
                .Select(x => WalkDirectoryTree(x)().Value)
                .ToSeq();

            return Out<Node>.FromValue(new Node(root, subDirNodes + filesNodes));
        };
    }
}
