using NUnit.Framework;
using RUCP.BufferChannels;
using RUCP.Packets;
using RUCP.Tools;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP_Test
{
    public class Tools
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void NumberUtils_CompareShort()
        {
            int result = NumberUtils.UshortCompare(1, 0);
            Assert.IsTrue(result == 1);
            result = NumberUtils.UshortCompare(15_000, 0);
            Assert.IsTrue(result == 1);
            result = NumberUtils.UshortCompare(1, 50_000);
            Assert.IsTrue(result == 1);
            result = NumberUtils.UshortCompare(0, 65_000);
            Assert.IsTrue(result == 1);

        }
       /* [Test]
        public void Buffer_Test()
        {
            Buffer buffer = new Buffer(500);
            int expect = 0;
            for (int i = 0; i < 120_000; i++)
            {
                Packet packet = Packet.Create(null, Channel.Reliable);
                buffer.Insert(packet);
                Assert.IsTrue(expect == packet.ReadNumber());
                buffer.ConfirmAsk(expect);
                expect = (expect +1 ) % 65_000;
            }
        }*/
    }
}
