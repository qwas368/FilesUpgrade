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
using FilesUpgrade.Model;
using System.IO;
using FilesUpgrade.Model.UpgradeSetting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static LanguageExt.Prelude;
using static FilesUpgrade.Validation.MainValidation;

namespace FilesUpgrade.Service
{
    public class UpgradeService
    {
        private readonly FileSystem fs;

        private readonly Diff diff;

        private readonly MainValidation mainValidation;

        public UpgradeService(
            FileSystem fileSystem, 
            Diff diff,
            MainValidation mainValidation)
        {
            this.fs = fileSystem;
            this.diff = diff;
            this.mainValidation = mainValidation;
        }

        public Subsystem<Unit> Upgrade(string source, string target) =>
            from info       in mainValidation.ValidateSource(source)
            from upzipPath  in CopyToTempPath(info)
            from _1         in Subsystem.WriteLine($"Unzip to {upzipPath}")
            from targetDic  in mainValidation.CheckFolderExistOrCreate(target)
            from _2         in Subsystem.WriteLine($"Target Directory {targetDic.FullName} is existed.")
            from targetNode in WalkDirectoryTree(targetDic)
            from cfg        in FetchConfig(Path.Combine(target, @"UpgradeSetting.json"))
            let upzipDic = new DirectoryInfo(upzipPath)
            from _3         in DeleteIgnoreList(upzipPath, cfg.IgnoreList)
            from sourceNode in WalkDirectoryTree(upzipDic)
            from renameNode in FullRename(upzipPath, cfg)
            from _4         in ReplaceFileContent(renameNode, cfg.ReplaceList)
            from planedNode in ShowUpgradePlan(renameNode, upzipPath, target)
            from _5         in SelectUpgradePlanDiff(renameNode, upzipPath, target)
            from _6         in ShowUpgradePlan(renameNode, upzipPath, target)
            from _7         in CheckYorN("Continue to upgrade? (y/N)")
            from _8         in CopyDirectory(upzipPath, target)
            select unit;

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
                    var expr = from context in fs.ReadAllText(info.FullName)
                               let cfg = JsonConvert.DeserializeObject<Config>(context)
                               select cfg;

                    return expr();
                }
            };

            return from info in fs.GetFileInfo(configPath)
                   from cfg  in ParseConfig(info)
                   select cfg;
        }

        public Subsystem<Node> FullRename(string dir, Config config) => () =>
        {
            if (config.ReplaceList.Count() == 0)
                return WalkDirectoryTree(new DirectoryInfo(dir))();
            else
            {
                fs.RenameAll(dir, config.ReplaceList, false);
                return WalkDirectoryTree(new DirectoryInfo(dir))();
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

        public Subsystem<Unit> SelectUpgradePlanDiff(Node sourceNode, string sourceDir, string targetDir) => () =>
        {
            var nodeList = (from node in sourceNode.Enumerate().Tail()
                            let colorNode = MarkKUpgradePlanColor(node, sourceDir, targetDir)
                            select colorNode)
                            .ToList();

            var index = 0;

            Console.WriteLine("Press (Top/Donw/Enter/End) to select file and diff...");
            while (true)
            {
                ConsoleKeyInfo ckey = Console.ReadKey();
                Console.Clear();

                switch (ckey.Key)
                {
                    case ConsoleKey.DownArrow:
                        index = index + 1 < nodeList.Count() ? index + 1 : 0;
                        break;
                    case ConsoleKey.RightArrow:
                        index = index + 5 < nodeList.Count() ? index + 5 : index + 5 - nodeList.Count();
                        break;
                    case ConsoleKey.UpArrow:
                        index = index - 1 >= 0 ? index - 1 : nodeList.Count() - 1;
                        break;
                    case ConsoleKey.LeftArrow:
                        index = index - 5 >= 0 ? index - 5 : nodeList.Count() + index - 5;
                        break;
                    case ConsoleKey.Enter:
                        Diff(nodeList[index], sourceDir, targetDir);
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case ConsoleKey.Escape:
                        return Out<Unit>.FromValue(unit);
                }
                ConsoleW.PrintNode(sourceNode, "", true, nodeList[index]);
            }
        };

        private Node MarkKUpgradePlanColor(Node node, string sourceDir, string targetDir)
        {
            if (node.Info.IsRight)
                return node;
            var fileInfo = node.Info.IfRight(() => default);

            var oldPath = fileInfo.FullName;
            var newPath = fileInfo.FullName.Replace(sourceDir, targetDir);
            var newFileInfo = new FileInfo(newPath);

            if (newFileInfo.Exists && fs.IsFileFullyEqual(oldPath, newPath))
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

        private Unit Diff(Node node, string sourceDir, string targetDir)
        {
            if (node.Info.IsRight)
                return unit;

            var fileInfo = node.Info.IfRight(() => default);
            var oldPath = fileInfo.FullName;
            var newPath = fileInfo.FullName.Replace(sourceDir, targetDir);
            diff.ShowDiff(oldPath, newPath);
            return unit;
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

        private Subsystem<Unit> CopyDirectory(string source, string target) => () =>
        {
            fs.MoveDirectory(source, target);
            return Out<Unit>.FromValue(unit);
        };

        public Subsystem<Unit> ReplaceFileContent(Node nodeTree, List<Replace> totelReplaces) => () =>
        {
            totelReplaces = totelReplaces.Where(x => x.Type == Enum.Type.FileContent).ToList();
            foreach (var node in nodeTree.Enumerate())
            {
                if (node.Info.IsLeft)
                {
                    var fileInfo = node.Info.IfRight(() => default);

                    var replaces = totelReplaces.Where(x => x.FileMatch == null || Regex.IsMatch(fileInfo.Name, x.FileMatch)).ToList();

                    if (fileInfo.Extension == ".xml" || 
                        fileInfo.Extension == ".json" || 
                        fileInfo.Extension == ".txt" || 
                        fileInfo.Extension == ".config")
                    {
                        string text = File.ReadAllText(fileInfo.FullName);
                        text = replaces.Fold(text, (s, repalce) => Regex.Replace(s, repalce.Pattern, repalce.Replacement));

                        File.WriteAllText(fileInfo.FullName, text, fs.GetEncoding(fileInfo.FullName));
                    }
                }
            }

            return Out<Unit>.FromValue(unit);
        };

        public Subsystem<Unit> DeleteIgnoreList(string dir, List<string> names) => () =>
        {
            foreach (var name in names)
                fs.DeleteSubFolder(dir, name);

            return Out<Unit>.FromValue(unit);
        };

        /// <summary>
        /// 取得暫存路徑
        /// </summary>
        public Subsystem<string> CopyToTempPath(Either<FileInfo, DirectoryInfo> info) => 
            info.Match(
                rhs => CopyDirToTemp(rhs.FullName),
                lhs => fs.ExtractZipToTmpDirectory(lhs.FullName)
            );
        
        public Subsystem<string> CopyDirToTemp(string source) => () =>
        {
            var tmp = fs.CreateDir(fs.GetTmpPath() + Path.GetFileName(source), true);
            fs.CopyDirectory(source, tmp);
            return Out<string>.FromValue(tmp);
        };
    }
}
