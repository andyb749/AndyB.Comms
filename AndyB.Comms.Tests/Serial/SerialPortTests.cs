using System;
using Xunit;

namespace AndyB.Comms.Tests
{
    using AndyB.Comms.Serial;

    public class SerialPortTests
    {
        private const string portname = "COM12";

#if false
        [Fact]
        public void CanCreateSerialPort1()
        {
            var target = new SerialPort();
            Assert.NotNull(target);
        }

        [Fact]
        public void CanCreateSerialPort2()
        {
            var target = new SerialPort(portname);
            Assert.NotNull(target);
        }

        [Fact]
        public void CanCreateSerialPort3()
        {
            var target = new SerialPort(portname, 9600);
            Assert.NotNull(target);
        }

        [Fact]
        public void CanCreateSerialPort4()
        {
            var target = new SerialPort(portname, 9600, Parity.None);
            Assert.NotNull(target);
        }

        [Fact]
        public void CanCreateSerialPort5()
        {
            var target = new SerialPort(portname, 9600, Parity.Even, 8);
            Assert.NotNull(target);
        }

        [Fact]
        public void CanCreateSerialPort6()
        {
            var target = new SerialPort(portname, 19200, Parity.Odd, 7, StopBits.One);
            Assert.NotNull(target);
        }

        [Fact]
        public void GetStreamOfClosedPortThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.BaseStream);
        }

        [Fact]
        public void CanGetStream()
        {
            var target = new SerialPort(portname);
            target.Open();
            var stream = target.BaseStream;
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<System.IO.Stream>(stream);
        }

        [Fact]
        public void EmptyPortNameThrowsOnOpen()
        {
            string port = null;
            Assert.Throws<ArgumentNullException>(() => new SerialPort(port));

            port = string.Empty;
            Assert.Throws<ArgumentException>(() => new SerialPort(port));
        }

        [Fact]
        public void InvalidNameThrows()
        {
            var port = "SER";
            var target = new SerialPort(port);
            Assert.Throws<ArgumentException>(() => target.Open());
        }

        [Fact]
        public void NameStartingWithSlashThrows()
        {
            var port = "\\\\";
            var target = new SerialPort();
            Assert.Throws<ArgumentException>(() => target.PortName = port);
        }

        [Fact]
        public void SettingNameWhenOpenThrows()
        {
            var target = new SerialPort(portname);
            target.Open();
            Assert.Throws<InvalidOperationException>(() => target.PortName = portname);
        }

        [Fact]
        public void InvalidReadBufferSizeThrows()
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.ReadBufferSize = 0);
        }

        [Fact]
        public void InvalidBaudrateThrows()
        {
            var target = new SerialPort();
            var baud = 0;
            Assert.Throws<ArgumentException>(() => target.BaudRate = baud);
        }

        [Fact]
        public void SetBreakWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.BreakState = true);
        }

        [Fact]
        public void GetBreakWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.BreakState);
        }

        [Fact]
        public void GetBytesToWriteWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.BytesToWrite);
        }

        [Fact]
        public void GetBytesToReadWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.BytesToRead);
        }

        [Fact]
        public void GetCDHoldingWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.CDHolding);
        }

        [Fact]
        public void GetCtsHoldingWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.CtsHolding);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void CanSetDataBits(int bits)
        {
            var target = new SerialPort();
            target.DataBits = bits;
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        [InlineData(9)]
        [InlineData(0)]
        public void InvalidDataBitsThrows(int bits)
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentNullException>(() => target.DataBits = bits);
        }

        [Fact]
        public void GetDsrHoldingWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.DsrHolding);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(6)]
        public void InvalidHandshakeThrows(int val)
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.Handshake = (Handshake)val);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(6)]
        public void InvalidParityThrows(int val)
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.Parity = (Parity)val);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void InvalidReadTimeoutThrows(int val)
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.ReadTimeout = val);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void InvalidReceivedBytesThresholdThrows(int val)
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.ReceivedBytesThreshold = val);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void InvalidWriteTimeoutThrows(int val)
        {
            var target = new SerialPort();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.WriteTimeout = val);
        }

        [Fact]
        public void DiscardInBufferWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.DiscardInBuffer());
        }

        [Fact]
        public void DiscardOutBufferWhenClosedThrows()
        {
            var target = new SerialPort();
            Assert.Throws<InvalidOperationException>(() => target.DiscardOutBuffer());
        }

        [Fact]
        public void CanGetPortNames()
        {
            var actual = SerialPort.GetPortNames();
            Assert.NotNull(actual);
            Assert.True(actual.Length > 0);
        }

        [Fact]
        public void OpenCreatesAStream()
        {
            var target = new SerialPort(portname);
            target.Open();
            var actual = target.BaseStream;
            Assert.NotNull(actual);
        }
#endif
    }
}
