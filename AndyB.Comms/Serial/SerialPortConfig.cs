using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
    /// <summary>
    /// Object to encapsulate a serial port configuration.
    /// </summary>
    public class SerialPortConfig
    {
        /// <summary>
        /// Gets/set the port name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the baud rate.
        /// </summary>
        public BaudRate Baudrate { get; set; }

        /// <summary>
        /// Gets/sets the number of data bits.
        /// </summary>
        public DataBits DataBits { get; set; }

        /// <summary>
        /// Gets/sets the parity bit.
        /// </summary>
        public ParityBit Parity { get; set; }

        /// <summary>
        /// Gets/sets the stop bits.
        /// </summary>
        public StopBits StopBits { get; set; }
    }
}
