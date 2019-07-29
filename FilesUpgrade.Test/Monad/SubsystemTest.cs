using FilesUpgrade.Monad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FilesUpgrade.Test
{
    [TestClass]
    public class SubsystemTest
    {
        [TestMethod]
        public void NormalCase()
        {
            var expr = from a in Subsystem.Return<string>("Hello")
                       from b in Subsystem.Return<string>("World")
                       select a + b;

            var r = expr.Execute();

            Assert.AreEqual(r.IsSucceed, true);
            Assert.AreEqual(r.Value, "HelloWorld");
        }
    }
}
