using FilesUpgrade.Controller;
using FilesUpgrade.Monad;
using LanguageExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Test
{
    [TestClass]
    public class EntryTest
    {
        [TestMethod]
        public void FetchCommand_No_Command()
        {
            Entry entry = new Entry(null);
            var expr = entry.FetchCommand(new string[0]);
            
            expr().Error.Match(
                err => Assert.IsTrue(err.Message.StartsWith("Usage:") && err.Message.Contains("upgrade")),
                () => Assert.Fail());
        }

        [TestMethod]
        public void FetchCommand_Upgrade_Command()
        {
            Entry entry = new Entry(null);
            var expr = entry.FetchCommand(new string[1] { "upgrade" });

            Assert.AreEqual(expr().Value, "upgrade");
        }


        [TestMethod]
        public void Router_Unknown_Command()
        {
            Entry entry = new Entry(null);
            var expr = entry.Router("update???", Seq<string>());
            Assert.IsTrue(expr().Error.IsSome);
        }

        [TestMethod]
        public void Router_Upgrade_Command()
        {
            var mock = new Mock<MainController>(null);
            mock.Setup(m => m.Upgrade(Seq<string>())).Returns(Subsystem.Return(unit));

            Entry entry = new Entry(mock.Object);
            var expr = entry.Router("upgrade", Seq<string>());
            Assert.AreEqual(expr().Value, unit);
        }
    }
}
