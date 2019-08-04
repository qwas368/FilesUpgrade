using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilesUpgrade.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FilesUpgrade.Model;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Model.Tests
{
    [TestClass()]
    public class NodeExtTests
    {
        [TestMethod()]
        public void Node_Enumerate_Test()
        {
            var node = new Node(new DirectoryInfo(@"D:\Faker"), 
                            Seq(new Node(new FileInfo(@"D:\Faker\123.txt")), 
                                new Node(new FileInfo(@"D:\Faker\456.txt"))));

            var enumNode = node.Enumerate().ToList();

            Assert.AreEqual(enumNode.Count(), 3);
        }
    }
}