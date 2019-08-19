#nullable enable
using FilesUpgrade.Monad;
using FilesUpgrade.Service;
using FilesUpgrade.Validation;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FilesUpgrade.Validation.MainValidation;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Controller
{
    public class MainController
    {
        private readonly UpgradeService upgradeService;

        private readonly CopySettingService copySettingService;

        private readonly MainValidation mainValidation;

        public MainController(
            UpgradeService upgradeService,
            CopySettingService copySettingService,
            MainValidation mainValidation
            )
        {
            this.upgradeService = upgradeService;
            this.copySettingService = copySettingService;
            this.mainValidation = mainValidation;
        }

        public virtual Subsystem<Unit> Upgrade(Seq<string> args) =>
            from paths in mainValidation.ValidateUpgradeParam(args)
            let t = (source: paths.Item1, target: paths.Item2) // Destructuring
            from _1    in mainValidation.CheckDirectoryExist(new DirectoryInfo(t.target))
            from _2    in mainValidation.CheckHasWhiteSpace(t.target, nameof(t.target))
            from _3    in upgradeService.Upgrade(t.source, t.target)
            select unit;

        public virtual Subsystem<Unit> CopySetting(Seq<string> args) =>
            from path in mainValidation.ValidateCopySettingParam(args)
            from _    in copySettingService.CopySetting(path)
            select unit;
    }
}
