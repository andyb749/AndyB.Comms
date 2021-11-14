using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms
{
    /// <summary>
    /// Represents the methods, properties and events that a device must implement
    /// </summary>
    public interface _IDevice
    {
        /// <summary>
        /// Connects the device
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise <c>false</c></returns>
        bool Connect();

        /// <summary>
        /// Disconnects the device
        /// </summary>
        void Disconnect();
    }
}
