using System;
using System.IO;

namespace AndyB.Comms.Serial
{
    using Interop;
    using Properties;


    public partial class SerialPort
    {
        private Stream _stream;


        /// <summary>
        /// Gets the underlying stream for standard read/write functions.
        /// </summary>
        public Stream BaseStream
        {
            get
            {
                if (!IsOpen)
                    InternalResources.FileNotOpen();

                return _stream;
            }
        }


        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero based offset in the buffer at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="buffer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The specified port is not open.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameters are outside a valid
        /// region of <paramref name="buffer"/> being passed. Either <paramref name="offset"/> or
        /// <paramref name="count"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="offset"/> plus <paramref name="count"/> is greater than
        /// the length of the buffer.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <remarks>If <paramref name="buffer"/> length is 0, then the function returns immediately.</remarks>
        public void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);


        /// <summary>
        /// Reads a specified number of bytes from the serial port.
        /// </summary>
        /// <param name="buffer">The byte array that the data will be written to.</param>
        /// <param name="offset">The zero based offset in the buffer at which to begin copying bytes from the port.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="buffer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The specified port is not open.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameters are outside a valid
        /// region of <paramref name="buffer"/> being passed. Either <paramref name="offset"/> or
        /// <paramref name="count"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="offset"/> plus <paramref name="count"/> is greater than
        /// the length of the buffer.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <returns>The count of bytes read into the buffer.</returns>
        /// <remarks>If <paramref name="buffer"/> length is 0, then the function returns immediately.</remarks>
        public int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    }
}
