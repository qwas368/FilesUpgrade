#nullable enable
using FilesUpgrade.Monad;
using FilesUpgrade.Service;
using FilesUpgrade.Validation;
using LanguageExt;
using System;
using System.Collections.Generic;
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

        private readonly MainValidation mainValidation;

        public MainController(
            UpgradeService upgradeService,
            MainValidation mainValidation
            )
        {
            this.upgradeService = upgradeService;
            this.mainValidation = mainValidation;
        }

        public virtual Subsystem<Unit> Upgrade(Seq<string> args) =>
            from paths in mainValidation.ValidateUpgradeParam(args)
            let t = (source: paths.Item1, target: paths.Item2) // Destructuring
            from _     in upgradeService.Upgrade(t.source, t.target)
            select unit;
    }
}
