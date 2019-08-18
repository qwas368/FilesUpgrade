using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilesUpgrade.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Moq;
using FilesUpgrade.IO;
using FilesUpgrade.Model;
using Telerik.JustMock;

namespace FilesUpgrade.Service.Tests
{
    [TestClass]
    public class UpgradeServiceTests
    {
        [TestMethod]
        public void WalkDirectoryTreeTest()
        {
            var service = new UpgradeService(null, null, null);
            var expr = service.WalkDirectoryTree(new DirectoryInfo(Directory.GetCurrentDirectory()));
            var r = expr().Value;

            r.Info.Match(
                left => Assert.AreEqual(left.FullName, Directory.GetCurrentDirectory()),
                right => Assert.Fail());
        }

        [TestMethod()]
        public void FetchConfigTest_Success()
        {
            var fakeJson = @"C:\mock.json";

            // mock filInfo
            var fileInfo = Telerik.JustMock.Mock.Create<FileInfo>(Constructor.Mocked);
            Telerik.JustMock.Mock.Arrange(() => fileInfo.Exists).Returns(true);
            Telerik.JustMock.Mock.Arrange(() => fileInfo.FullName).Returns(fakeJson);

            // mock FileSystem
            var mock = new Mock<FileSystem>();
            mock.Setup(m => m.GetFileInfo(fakeJson)).Returns(() => Out<FileInfo>.FromValue(fileInfo));
            mock.Setup(m => m.ReadAllText(fakeJson)).Returns(() => Out<string>.FromValue("{\"replaceList\":[{\"pattern\":\"EDconfig\",\"replacement\":\"EDconfig.1\",\"type\":0}],\"ignoreList\":[\"CVS\"]}"));

            var service = new UpgradeService(mock.Object, null, null);
            var expr = service.FetchConfig(fakeJson);
            var value = expr().Value;

            Assert.AreEqual(value.IgnoreList[0], "CVS");
            Assert.AreEqual(value.ReplaceList[0].Replacement, "EDconfig.1");
        }
    }
}