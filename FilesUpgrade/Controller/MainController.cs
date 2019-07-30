#nullable enable
using FilesUpgrade.Monad;
using FilesUpgrade.Service;
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
        public MainController(UpgradeService upgradeService)
        {
            this.upgradeService = upgradeService;
        }

        public virtual Subsystem<Unit> Upgrade(Seq<string> args) =>
            from paths in ValidateUpgradeParam(args)
            let t = (source: paths.Item1, target: paths.Item2) // Destructuring
            from _     in upgradeService.Upgrade(t.source, t.target)
            select unit;
    }
}
