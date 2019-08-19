#nullable enable
using FilesUpgrade.IO;
using FilesUpgrade.Monad;
using FilesUpgrade.Validation;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Service
{
    public class CopySettingService
    {
        private readonly FileSystem fs;

        private readonly MainValidation mainValidation;

        public CopySettingService(
            FileSystem fileSystem,
            MainValidation mainValidation)
        {
            this.fs = fileSystem;
            this.mainValidation = mainValidation;
        }

        public Subsystem<Unit> CopySetting(string target) =>
            from _1 in mainValidation.CheckDirectoryExist(new DirectoryInfo(target))
            let targetPath = @$"{target}\UpgradeSetting.json"
            from _2 in mainValidation.CheckFileNotExist(new FileInfo(targetPath))
            let _3 = fun<string, string>(fs.CopyEmbeddedFile)("FilesUpgrade.Resource.UpgradeSetting.json", targetPath)
            select unit;
    }
}
