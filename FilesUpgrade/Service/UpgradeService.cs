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
using FilesUpgrade.Model.UpgradeSetting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
            from fileinfo   in TryGetFileInfo(source)
            from upzipPath  in fileSystem.ExtractZipToCurrentDirectory(source)
            from _1         in Subsystem.WriteLine($"Unzip to {upzipPath}")
            from targetDic  in CheckFolderExistOrCreate(target)
            from _2         in Subsystem.WriteLine($"Target Directory {targetDic.FullName} is existed.")
            from targetNode in WalkDirectoryTree(targetDic)
            from cfg        in FetchConfig(Path.Combine(target, @"UpgradeSetting.json"))
            let upzipDic = new DirectoryInfo(upzipPath)
            from sourceNode in WalkDirectoryTree(upzipDic)
            from renameNode in FullRename(upzipPath, cfg)
            from _3         in ReplaceFileContent(renameNode, cfg.ReplaceList)
            from planedNode in ShowUpgradePlan(renameNode, upzipPath, target)
            from _4         in CheckYorN("Continue to upgrade? (y/N)")
            from needBackup in GetYorN("Need to backup? (y/N)")
            from _5         in CopyDirectory(upzipPath, target, needBackup)
            select unit;

        private Subsystem<FileInfo> TryGetFileInfo(string source) =>
            from fileinfo in fileSystem.GetFileInfo(source)
            from _1 in CheckFileExist(fileinfo)
            from _2 in IsZipFile(fileinfo)
            from _3 in Subsystem.WriteLine($"Check Upgrade file {fileinfo.Name}({fileinfo.Length / 1024}kb) is existed.")
            select fileinfo;

        private Subsystem<Node> TryGetNode(DirectoryInfo info) => () =>
        {
            ConsoleW.Write(info.FullName + "\n", ConsoleColor.Yellow);

            var expr = from targetNode in WalkDirectoryTree(info)
                       let _3 = ConsoleW.PrintNode(targetNode, "", true)
                       select targetNode;

            return expr();
        };

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

        public Subsystem<Config> FetchConfig(string configPath)
        {
            Subsystem<Config> ParseConfig (FileInfo info) => () => {
                if (!info.Exists)
                {
                    ConsoleW.Write("Dangerous ", ConsoleColor.Red);
                    ConsoleW.WriteLine($"Config {info.FullName} is not existed.");
                    return Out<Config>.FromValue(new Config());
                }
                else
                {
                    var expr = from context in fileSystem.ReadAllText(info.FullName)
                               let cfg = JsonConvert.DeserializeObject<Config>(context)
                               select cfg;

                    return expr();
                }
            };

            return from info in fileSystem.GetFileInfo(configPath)
                   from cfg  in ParseConfig(info)
                   select cfg;
        }

        public Subsystem<Node> FullRename(string dir, Config config) => () =>
        {
            if (config.ReplaceList.Count() == 0)
                return WalkDirectoryTree(new DirectoryInfo(dir))();
            else
            {
                var renameDir = fileSystem.RenameAll(dir, config.ReplaceList);
                return WalkDirectoryTree(new DirectoryInfo(renameDir))();
            }
        };

        public Subsystem<Node> ShowUpgradePlan(Node sourceNode, string sourceDir, string targetDir) => () =>
        {
            foreach(var node in sourceNode.Enumerate().Tail())
            {
                MarkKUpgradePlanColor(node, sourceDir, targetDir);
            }

            ConsoleW.WriteLine(@"Please Check your upgrade plan", ConsoleColor.Black, ConsoleColor.White);
            ConsoleW.PrintNode(sourceNode, "", true);

            return Out<Node>.FromValue(sourceNode);
        };

        private Node MarkKUpgradePlanColor(Node node, string sourceDir, string targetDir)
        {
            if (node.Info.IsRight)
                return node;
            var fileInfo = node.Info.IfRight(() => default);

            var oldPath = fileInfo.FullName;
            var newPath = fileInfo.FullName.Replace(sourceDir, targetDir);
            var newFileInfo = new FileInfo(newPath);

            if (newFileInfo.Exists && fileSystem.IsFileFullyEqual(oldPath, newPath))
            {
                node.Color = ConsoleColor.Gray;
            }
            else if (newFileInfo.Exists)
            {
                var oldVersionString = FileVersionInfo.GetVersionInfo(fileInfo.FullName).FileVersion;
                var newVersionString = FileVersionInfo.GetVersionInfo(newFileInfo.FullName).FileVersion;
                if (oldVersionString != null && newVersionString != null)
                {
                    var oldVersion = new Version(oldVersionString);
                    var newVersion = new Version(newVersionString);
                    if (newVersion > oldVersion)
                        node.Color = ConsoleColor.Red;
                    else
                        node.Color = ConsoleColor.DarkGreen;
                }
                else
                {
                    node.Color = ConsoleColor.Yellow;
                }
            }
            else
            {
                node.Color = ConsoleColor.Green;
            }

            return node;
        }

        private Subsystem<Unit> CheckYorN(string message) => () =>
        {
            ConsoleW.Write(message);
            var keyin = Console.ReadLine();
            if (keyin.StartsWith("Y") || keyin.StartsWith("y"))
                return Out<Unit>.FromValue(unit);
            else
                return Out<Unit>.FromError("Stop upgrade");
        };

        private Subsystem<bool> GetYorN(string message) => () =>
        {
            ConsoleW.Write(message);
            var keyin = Console.ReadLine();
            if (keyin.StartsWith("Y") || keyin.StartsWith("y"))
                return Out<bool>.FromValue(true);
            else
                return Out<bool>.FromValue(false);
        };

        private Subsystem<Unit> CopyDirectory(string source, string target, bool needBackup) => () =>
        {
            fileSystem.CopyDirectory(source, target, needBackup);
            return Out<Unit>.FromValue(unit);
        };

        public Subsystem<Unit> ReplaceFileContent(Node nodeTree, List<Replace> replaces) => () =>
        {
            replaces = replaces.Where(x => x.Type == Enum.Type.FileContent).ToList();
            foreach (var node in nodeTree.Enumerate())
            {
                if (node.Info.IsLeft)
                {
                    var fileInfo = node.Info.IfRight(() => default);
                    if (fileInfo.Extension == ".xml" || fileInfo.Extension == ".json" || fileInfo.Extension == ".txt" || fileInfo.Extension == ".config")
                    {
                        string text = File.ReadAllText(fileInfo.FullName);
                        text = replaces.Fold(text, (s, repalce) => Regex.Replace(s, repalce.Pattern, repalce.Replacement));

                        File.WriteAllText(fileInfo.FullName, text, fileSystem.GetEncoding(fileInfo.FullName));
                    }
                }
            }

            return Out<Unit>.FromValue(unit);
        };
    }
}
