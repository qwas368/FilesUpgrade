#nullable enable
using FilesUpgrade.Model;
using FilesUpgrade.Model.UpgradeSetting;
using FilesUpgrade.Monad;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FilesUpgrade.IO
{
    public class FileSystem
    {
        public FileSystem()
        {

        }

        public virtual Subsystem<FileInfo> GetFileInfo(string path) => () =>
            Out<FileInfo>.FromValue(new FileInfo(path));

        public Subsystem<string> ExtractZipToCurrentDirectory(string path) => () =>
        {
            string extractPath = Path.Combine(Path.GetDirectoryName(path), "tmp");

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, recursive: true);
            ZipFile.ExtractToDirectory(path, extractPath);

            return Out<string>.FromValue(extractPath);
        };

        public virtual Subsystem<string> ReadAllText(string path) => () =>
            Out<string>.FromValue(File.ReadAllText(path));

        /// <summary>
        /// 更名所有資料夾和檔案
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="replaces"></param>
        /// <returns>rename dir</returns>
        public virtual string RenameAll(string dir, List<Replace> replaces)
        {
            var dirNames = Directory.EnumerateDirectories(dir);
            var fileNames = Directory.EnumerateFiles(dir);

            // rename subDir
            foreach (string path in dirNames)
            {
                RenameAll(path, replaces);
            }

            // rename files
            foreach (string path in fileNames)
            {
                var fileName = Path.GetFileName(path);

                var replace = replaces
                    .Where(x => x.Type == Enum.Type.File)
                    .FirstOrDefault(x => Regex.IsMatch(fileName, x.Pattern));

                if (replace != null)
                {
                    var newPath = Path.Combine(dir, Regex.Replace(fileName, replace.Pattern, replace.Replacement));
                    File.Move(path, newPath);
                }
            }

            // rename current dir
            var info = new DirectoryInfo(dir);
            var replace2 = replaces
                .Where(x => x.Type == Enum.Type.Directory)
                .FirstOrDefault(x => Regex.IsMatch(info.Name, x.Pattern));
            if (replace2 != null)
            {
                var newPath = Path.Combine(Path.GetDirectoryName(info.FullName), Regex.Replace(info.Name, replace2.Pattern, replace2.Replacement));
                Directory.Move(dir, newPath);
                return newPath;
            }
            else
            {
                return dir;
            }
        }

        /// <summary>
        /// 複製過去
        /// </summary>
        public virtual void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile) && new FileInfo(targetFile).Length == new FileInfo(file).Length)
                    {
                        continue;
                    }
                    else if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                    File.Move(file, targetFile);
                }
            }

            Directory.Delete(source, true);
        }
    }
}
