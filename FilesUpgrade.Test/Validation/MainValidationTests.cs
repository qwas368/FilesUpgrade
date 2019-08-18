using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilesUpgrade.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FilesUpgrade.IO;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Validation.Tests
{
    [TestClass()]
    public class MainValidationTests
    {
        private FileSystem fs;

        private MainValidation mainValidation;

        [TestInitialize]
        public void TestInitialize()
        {
            fs = new FileSystem();
            mainValidation = new MainValidation(fs);
        }

        [TestMethod]
        public void ValidateUpgradeParam_One_Parameter()
        {
            var expr = mainValidation.ValidateUpgradeParam(Seq1("param1"));
            var r = expr();

            Assert.AreEqual(r.Value, ("param1", Directory.GetCurrentDirectory()));
        }

        [TestMethod]
        public void ValidateUpgradeParam_No_Parameter()
        {
            var expr = mainValidation.ValidateUpgradeParam(Seq<string>());
            var r = expr();

            Assert.IsFalse(r.IsFailed);
        }

        [TestMethod]
        public void CheckFileExist_File_Exists()
        {
            var expr = mainValidation.CheckFileExist(new FileInfo(@"C:\Windows\notepad.exe"));
            var r = expr();

            Assert.IsTrue(r.Value);
        }


        [TestMethod]
        public void IsZipFile_Is_Zip_File()
        {
            var expr = mainValidation.IsZipFile(new FileInfo(@"C:\Windows.zip"));
            var r = expr();

            Assert.IsTrue(r.Value);
        }

        [TestMethod()]
        public void CheckFolderExistOrCreate_New_Folder()
        {
            string path = $@"{fs.GetTmpPath()}\\{new Random().Next(0, 2000)}";
            var expr = mainValidation.CheckFolderExistOrCreate(path);
            expr();

            Assert.IsTrue(Directory.Exists(path));
            Directory.Delete(path, true);
        }

        [TestMethod()]
        public void CheckFolderExistOrCreate_New_Folder_Recursive()
        {
            string path = $@"{fs.GetTmpPath()}\\{new Random().Next(0, 2000)}\{new Random().Next(0, 2000)}";
            var expr = mainValidation.CheckFolderExistOrCreate(path);
            expr();

            Assert.IsTrue(Directory.Exists(path));
            Directory.Delete(Directory.GetParent(path).FullName, true);
        }

        [TestMethod()]
        public void ValidateSourceTest_Directory_Case()
        {
            var tmp = fs.GetTmpPath();
            var result = mainValidation.ValidateSource(tmp)();
            Assert.IsTrue(result.IsSucceed);
            Assert.AreEqual(result.Value.IfLeft(() => default).FullName,
                fs.GetTmpPath());
        }
    }
}