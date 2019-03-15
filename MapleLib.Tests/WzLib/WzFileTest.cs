using System.IO;
using System.Text;
using MapleLib.WzLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapleLib.Tests.WzLib
{
    [TestClass]
    public class WzFileTest
    {
        [TestMethod]
        public void TestWzFile()
        {
            var file = new WzFile(1, WzMapleVersion.Classic);

            using (var ms = new MemoryStream())
            {
                file.ExportJson(ms);
                var contents = Encoding.ASCII.GetString(ms.ToArray());
                Assert.AreEqual(257, contents.Length);
            }
        }
    }
}