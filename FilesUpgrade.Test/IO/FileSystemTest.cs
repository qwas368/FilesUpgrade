using System;
using FilesUpgrade.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilesUpgrade.Monad;
using System.IO;

namespace FilesUpgrade.Test.IO
{
    [TestClass]
    public class FileSystemTest
    {
        [TestMethod]
        public void GetFileInfo_Normal_Case()
        {
            var expr = from fileinfo in new FileSystem().GetFileInfo(@"C:\")
                       select fileinfo;

            Assert.IsTrue(expr().IsSucceed);
            Assert.IsFalse(expr().Value.Exists);
        }

        [TestMethod]
        public void ExtractZipToCurrentDirectory_Normal_Case()
        {
            var expr = from path in new FileSystem().ExtractZipToCurrentDirectory(@"D:\repos\filesUpgrade\FilesUpgrade\_TestCase\EDconfig.zi")
                       select path;

            Assert.IsFalse(Directory.Exists(expr().Value));
        }
    }
}
