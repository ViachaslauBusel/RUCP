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
        [Test]
        public void Test_RelativeSequenceNumber()
        {

        //    Console.WriteLine($"test_1:{1*2*3*4*5} test_2:{1*2*3*5*4}");
            //int max = 32768;
            //int halfMax = max / 2;
            //Console.WriteLine($"halfMax:{halfMax} test:{max - halfMax}");
            //Console.WriteLine($"ushort.MaxValue:{ushort.MaxValue} short.MaxValue{short.MaxValue} halfUshort.MaxValue:{ushort.MaxValue / 2} test:{ushort.MaxValue - short.MaxValue}");
            //Console.WriteLine($"NumberUtils.RelativeSequenceNumber(1, 0):{NumberUtils.RelativeSequenceNumber(1, 0)}");
            //Console.WriteLine($"NumberUtils.RelativeSequenceNumber(1, 33_000):{NumberUtils.RelativeSequenceNumber(1, 33_000)}");
            //Console.WriteLine($"NumberUtils.RelativeSequenceNumber(1, 16000):{NumberUtils.RelativeSequenceNumber(1, 16_000)}");
            //Console.WriteLine($"NumberUtils.RelativeSequenceNumber(65_535, 0):{NumberUtils.RelativeSequenceNumber(65_535, 0)}");
          //  Console.WriteLine($"NumberUtils.RelativeSequenceNumber(0, 65_535):{NumberUtils.RelativeSequenceNumber(0, ushort.MaxValue)}");
            // Console.WriteLine($"test:{64_535 + Buffer}")
            Assert.True(NumberUtils.RelativeSequenceNumber(1, 0) > 0);
            Assert.True(NumberUtils.RelativeSequenceNumber(1, 65_000) > 0);
            Assert.True(NumberUtils.RelativeSequenceNumber(1, 64_535) > 0);
            Assert.True(NumberUtils.RelativeSequenceNumber(10_000, 64_535) > 0);
            Assert.True(NumberUtils.RelativeSequenceNumber(64_535, 35_000) > 0);

            Assert.True(NumberUtils.RelativeSequenceNumber(64_535, 0) < 0);
        }
    }
}
