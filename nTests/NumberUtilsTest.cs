using NUnit.Framework;
using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nTests
{
    public class NumberUtilsTest
    {
        [Test]
        public void Test()
        {
            Assert.True(NumberUtils.UshortCompare(1, 0) == 1);
            Assert.True(NumberUtils.UshortCompare(1, 65_000) == 1);
            Assert.True(NumberUtils.UshortCompare(1, 65_535) == 1);
            Assert.True(NumberUtils.UshortCompare(10_000, 65_535) == 1);
            Assert.True(NumberUtils.UshortCompare(65_535, 35_000) == 1);

            Assert.True(NumberUtils.UshortCompare(65_535, 0) == -1);
        }
    }
}
