using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Xunit;


namespace AndyB.Comms.Tests.Serial
{
    using AndyB.Comms.Serial;


    public class Win32CommTests
    {
        private static string Port = "COM13";

#if false
        [Fact]
        public void CanOpenPort ()
        {
            
            var port = "\\\\.\\COM14";
            var safe = Win32Comm.CreateFile(port, Win32Comm.GENERIC_READ | Win32Comm.GENERIC_WRITE, 0, IntPtr.Zero,
                Win32Comm.OPEN_EXISTING, Win32Comm.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            var stream = new System.IO.FileStream(safe, System.IO.FileAccess.ReadWrite);
            Assert.NotNull(stream);

            stream.Write(new byte[] { 0x01, 0x02 });
#if false
            var target = new Win32Comm();
            var actual = target.Open(Port);
            Assert.True(actual);
#endif
        }
#endif
        [Theory]
        [InlineData(PinStates.Disable)]
        [InlineData(PinStates.Enable)]
        [InlineData(PinStates.Handshake)]
        public void CanSetDTR(PinStates state)
        {
#if false
            var target = new Win32Comm();
            target.Open(Port);
            var settings = new SerialSettings();
            target.ReadSettings(settings);
            settings.DtrControl = state;
            target.WriteSettings(settings);
            Assert.Equal(state, settings.DtrControl);
#endif
        }
    }
}
