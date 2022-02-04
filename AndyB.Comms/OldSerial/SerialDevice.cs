using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace AndyB.Comms.Serial
{
#if false
    /// <summary>
    /// A representation of a serial port
    /// </summary>
    public class SerialDevice
    {
        private SerialStream2 _stream = null;


        /// <summary>
        /// Gets the status of the port whether its opened.
        /// </summary>
        /// <value><c>true</c> if the <see cref="SerialDevice"/> is open; otherwise <c>false</c>.</value>
        /// <exception cref="InvalidOperationException">The port is already open.</exception>
        public bool IsOpen { get => throw new NotImplementedException(); }


        /// <summary>
        /// Open the <see cref="SerialDevice"/> for use.
        /// </summary>
        public void Open()
        {

            throw new NotImplementedException();    
        }

        /// <summary>
        /// Gets the stream for this <see cref="SerialDevice"/>.
        /// </summary>
        /// <returns>The <see cref="Stream"/> for this <see cref="SerialDevice"/>.</returns>
        /// <exception cref="InvalidOperationException">The port is closed.</exception>
        public Stream GetStream()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port is already open");

            //_stream = new SerialStream2();
            throw new NotImplementedException();
        }
    }
#endif
}
