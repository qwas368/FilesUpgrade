using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilesUpgrade.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FilesUpgrade.Service.Tests
{
    [TestClass]
    public class UpgradeServiceTests
    {
        [TestMethod]
        public void WalkDirectoryTreeTest()
        {
            var service = new UpgradeService(null);
            var expr = service.WalkDirectoryTree(new DirectoryInfo(Directory.GetCurrentDirectory()));
            var r = expr().Value;

            r.Info.Match(
                left => Assert.AreEqual(left.FullName, Directory.GetCurrentDirectory()), 
                right => Assert.Fail());
        }
    }
}