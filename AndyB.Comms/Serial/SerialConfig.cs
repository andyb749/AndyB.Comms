using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
    /// <summary>
    /// Class for the configuration of a serial port
    /// </summary>
     public class SerialConfig //: IPortConfig
    {
        /// <summary>
        /// Name of the serial port
        /// </summary>
        /// <remarks>This property is the name of the device as known by the operating system, e.g. 
        /// COM1</remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the baudrate
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// Gets/sets the number of databits
        /// </summary>
        public DataBits DataBits { get; set; }

        /// <summary>
        /// Gets/sets the parity bit
        /// </summary>
        public ParityBit ParityBit { get; set; }

        /// <summary>
        /// Gets/sets the stopbits
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// Gets/sets the DTR/DSR handshake
        /// </summary>
        public bool DtrDsrHandshake { get; set; }

        /// <summary>
        /// Gets/sets the RTS/CTS handshake
        /// </summary>
        public bool RtsCtsHandshake { get; set; }

        /// <summary>
        /// Gets/sets XON/XOFF handshake
        /// </summary>
        public bool XonXoffHandshake { get; set; }
    }
}
