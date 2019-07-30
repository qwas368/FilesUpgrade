using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static FilesUpgrade.Validation.MainValidation;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Test.Validation
{
    [TestClass]
    public class MainValidationTest
    {
        [TestMethod]
        public void ValidateUpgradeParam_One_Parameter()
        {
            var expr = ValidateUpgradeParam(Seq1("param1"));
            var r = expr();

            Assert.AreEqual(r.Value, ("param1", Directory.GetCurrentDirectory()));
        }

        [TestMethod]
        public void ValidateUpgradeParam_No_Parameter()
        {
            var expr = ValidateUpgradeParam(Seq<string>());
            var r = expr();

            Assert.IsFalse(r.IsFailed);
        }

        [TestMethod]
        public void CheckFileExist_File_Exists()
        {
            var expr = CheckFileExist(new FileInfo(@"C:\Windows\notepad.exe"));
            var r = expr();

            Assert.IsTrue(r.Value);
        }


        [TestMethod]
        public void IsZipFile_Is_Zip_File()
        {
            var expr = IsZipFile(new FileInfo(@"C:\Windows.zip"));
            var r = expr();

            Assert.IsTrue(r.Value);
        }
    }
}
