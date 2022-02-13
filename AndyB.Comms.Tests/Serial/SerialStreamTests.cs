using System;
using System.IO;
using System.Text;
using Xunit;

namespace AndyB.Comms.Tests.Serial
{
    using AndyB.Comms.Serial;

    public class SerialStreamTests
    {
        //private static string portName = "COM14";

#if false
        [Fact]
        public void NullPortNameThrows ()
        {        
//            var target = new SerialStream2(portName, 9600, 8, Parity.None, StopBits.One, 0, 0, Handshake.None, false, false, (byte)'?');
//            target.Dispose();
        }

        [Theory]
        [InlineData("C")]
        [InlineData("COM")]
        [InlineData("TTY")]
        [InlineData("LPT")]
        [InlineData("readme.txt")]
        public void InvalidDeviceNameThrows(string name)
        {
            name = null;
            Assert.Throws<ArgumentException>(() =>
            {
                //new _SerialStream(name);
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-1000)]
        public void NonPositiveBufferSizeThrows(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                //new _SerialStream(portName, FileShare.None, size);
            });
        }

        [Theory]
        [InlineData(999)]
        public void InvalidShareArgumentThrows(uint share)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                //new _SerialStream(portName, (FileShare)share);
            });
        }

        [Fact]
        public void CanCreateStreamFromName()
        {
//            var target = new SerialStream2(portName, 9600, 8, Parity.None, StopBits.One, 1000, 1000, Handshake.None, false, false, (byte)'?');
            //Assert.NotNull(target);
        }

        [Fact]
        public void CanCreateStreamFromHandle()
        {
            var h = new Microsoft.Win32.SafeHandles.SafeFileHandle(IntPtr.Zero, true);

            var s = new FileStream(h, FileAccess.ReadWrite);

            // TODO:
        }

        [Fact]
        public void CanWriteSteam()
        {
            //var target = new SerialStream(portName);
            //var str = "THE QUICK FOX JUMPS OVER THE LAZY DOG";
            //var bytes = Encoding.ASCII.GetBytes(str);
            //target.Write(bytes, 0, bytes.Length);
        }
#endif
    }
}
