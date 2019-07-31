#nullable enable
using FilesUpgrade.Model;
using FilesUpgrade.Monad;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
    }
}
