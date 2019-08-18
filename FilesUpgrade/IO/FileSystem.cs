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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.IO
{
    public class FileSystem
    {
        public FileSystem()
        {

        }

        public virtual Subsystem<FileInfo> GetFileInfo(string path) => () =>
            Out<FileInfo>.FromValue(new FileInfo(path));

        public virtual Subsystem<DirectoryInfo> GetDirectoryInfo(string path) => () =>
            Out<DirectoryInfo>.FromValue(new DirectoryInfo(path));

        public Subsystem<string> ExtractZipToTmpDirectory(string path) => () =>
        {
            string extractPath = GetTmpPath() + Path.GetFileNameWithoutExtension(path);

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
        /// 複製整個資料夾
        /// </summary>
        public virtual void CopyDirectory(string source, string target)
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
                    if (IsFileFullyEqual(targetFile, file))
                    {
                        continue;
                    }
                    else if (File.Exists(targetFile))
                    {
                        try
                        {
                            File.Delete(targetFile);
                        }
                        catch
                        {
                            File.Move(targetFile, Path.Combine(targetFolder, "_" + Path.GetFileName(file)));
                        }
                    }
                    File.Move(file, targetFile);
                }
            }

            Directory.Delete(source, true);
        }

        /// <summary>
        /// 檢查兩個檔案是否完全一樣
        /// </summary>
        public virtual bool IsFileFullyEqual(string path1, string path2) =>
            File.Exists(path1) &&
            File.Exists(path2) &&
            new FileInfo(path1).Length == new FileInfo(path2).Length &&
            SHA256(path1) == SHA256(path2);

        /// <summary>
        /// Get File's Encoding
        /// </summary>
        /// <param name="filename">The path to the file
        public virtual Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8; // UTF-8 with BOM
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return new UTF8Encoding(false); // UTF-8 
        }

        private string SHA256(string filePath)
        {
            using SHA256 SHA256 = SHA256.Create();
            using FileStream fileStream = File.OpenRead(filePath);
            return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
        }

        public Unit DeleteSubFolder(string dir, string name)
        {
            foreach (DirectoryInfo subfolder in new DirectoryInfo(dir).GetDirectories(name, SearchOption.AllDirectories))
            {
                Directory.Delete(subfolder.FullName, true);
            }

            return unit;
        }

        /// <summary>
        /// Get Directory
        /// </summary>
        /// <returns>byte</returns>
        public long GetDirectorySize(string p)
        {
            string[] a = Directory.GetFiles(p, "*.*");

            long b = 0;
            foreach (string name in a)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            return b;
        }

        /// <summary>
        /// Create Directory
        /// </summary>
        /// <param name="force">刪除已存在的資料夾</param>
        /// <returns></returns>
        public string CreateDir(string path, bool force)
        {
            if (force && Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// 取得暫存路徑
        /// </summary>
        /// <returns></returns>
        public string GetTmpPath()
        {
            var tmp = Path.GetTempPath() + "FileSystem";

            if (!Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);

            return tmp + "\\";
        }
    }
}
