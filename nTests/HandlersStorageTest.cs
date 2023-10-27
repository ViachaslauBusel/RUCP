using NUnit.Framework;
using RUCP.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nTests
{
    internal class HandlersStorageTest
    {
    [Handler(2)]
    private static void TestMethod()
    {

    }
        [Test]
        public void Test1()
        {
          HandlersStorage<Action> storage= new HandlersStorage<Action>();
          storage.RegisterAllStaticHandlers();

          Assert.IsTrue(storage.GetHandler(2) != null);
          Assert.Pass();
        }
    }
}
