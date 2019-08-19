using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilesUpgrade.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FilesUpgrade.Monad;
using FilesUpgrade.Model.UpgradeSetting;
using System.IO.Compression;

namespace FilesUpgrade.IO.Tests
{
    [TestClass()]
    public class FileSystemTests
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
        public void ExtractZipToCurrentDirectory_Fail_Case()
        {
            var expr = from path in new FileSystem().ExtractZipToTmpDirectory(@"D:\repos\filesUpgrade\FilesUpgrade\_TestCase\EDconfig.zi")
                       select path;

            Assert.IsFalse(Directory.Exists(expr().Value));
        }

        [TestMethod]
        public void ExtractZipToCurrentDirectory_Normal_Case()
        {
            var fs = new FileSystem();
            var tmp = fs.GetTmpPath();
            var testDir = $"{tmp}test_dir";
            var testZip = $"{tmp}result.zip";
            if (Directory.Exists(testDir))
                Directory.Delete(testDir);
            Directory.CreateDirectory(testDir);
            if (File.Exists(testZip))
                File.Delete(testZip);
            ZipFile.CreateFromDirectory(testDir, testZip);

            var expr = from path in fs.ExtractZipToTmpDirectory(testZip)
                       select path;

            Assert.IsTrue(Directory.Exists(expr().Value));
        }

        [TestMethod()]
        public void RenameAll_Normal_Test()
        {
            var fileSystem = new FileSystem();
            var replaces = new List<Replace>() {
                new Replace()
                {
                    Pattern = @"BTSK.{4}",
                    Replacement = @"BTSK7702",
                    Type = Enum.Type.Directory
                },
                new Replace()
                {
                    Pattern = @"BTSK.{4}",
                    Replacement = @"BTSK7702",
                    Type = Enum.Type.File
                }
            };

            string testFolder = @"D:/TestForUnitTest";
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);
            Directory.CreateDirectory(@"D:/TestForUnitTest");
            Directory.CreateDirectory(@"D:/TestForUnitTest/BTSK7701");
            Directory.CreateDirectory(@"D:/TestForUnitTest/BTSK7701/BTSK7701_1");
            File.CreateText(@"D:/TestForUnitTest/BTSK7701.xml").Dispose();
            File.CreateText(@"D:/TestForUnitTest/BTSK7701/BTSK7701_1/BTSK7701_1.xml").Dispose();

            fileSystem.RenameAll(@"D:/TestForUnitTest", replaces, false);

            Assert.IsTrue(Directory.Exists(@"D:/TestForUnitTest/BTSK7702"));
            Assert.IsTrue(Directory.Exists(@"D:/TestForUnitTest/BTSK7702/BTSK7702_1"));
            Assert.IsTrue(File.Exists(@"D:/TestForUnitTest/BTSK7702.xml"));
            Assert.IsTrue(File.Exists(@"D:/TestForUnitTest/BTSK7702/BTSK7702_1/BTSK7702_1.xml"));
            Directory.Delete(testFolder, true);
        }

        [TestMethod()]
        public void IsFileFullyEqualTest_Not_Equal_Test()
        {
            if (File.Exists(@"test1.txt"))
                File.Delete(@"test1.txt");
            if (File.Exists(@"test2.txt"))
                File.Delete(@"test2.txt");
            var stream1 = File.CreateText(@"test1.txt");
            var stream2 = File.CreateText(@"test2.txt");
            stream1.WriteLine("12");
            stream2.WriteLine("34");
            stream1.Dispose();
            stream2.Dispose();

            Assert.IsFalse(new FileSystem().IsFileFullyEqual(@"test1.txt", @"test2.txt"));
        }

        [TestMethod()]
        public void CreateDirTest_Normal_Case()
        {
            var fs = new FileSystem();
            var targetDir = fs.GetTmpPath() + "dir1";
            var dummyFile = fs.GetTmpPath() + "dir1\\dummy.txt";
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            Directory.CreateDirectory(targetDir);
            File.CreateText(dummyFile).Close();

            var path = fs.CreateDir(targetDir, true);

            Assert.IsTrue(Directory.Exists(path));
            Assert.AreEqual(path, targetDir);
            Assert.AreEqual(new DirectoryInfo(path).GetFiles().Length, 0);
        }
    }
}