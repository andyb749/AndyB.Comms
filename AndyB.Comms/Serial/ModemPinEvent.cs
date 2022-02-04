using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Specifies the type of change that occurred on the <see cref="SerialPort"/> object.
	/// </summary>
	/// <remarks><para>This enumeration is used with the <see cref="SerialPort.ModemEvent(Action{ModemPinState})"/> event.</para>
	/// <para>A serial port pin changes state when it is asserted or unasserted.</para></remarks>
	public enum ModemPinEvent
	{
		/// <summary>
		/// The Clear To Send (CTS) pin changed state. This signal is used to indicate whether
		/// data can be sent over the serial port.
		/// </summary>
		CtsChanged,

		/// <summary>
		/// The Data Set Ready (DSR) pin changed state. This signal is used to indicate whether
		/// the device on the serial port is ready to operate.
		/// </summary>
		DsrChanged,

		/// <summary>
		/// Received Line Signal Detect (RLSD) or Carrier Detect (CD) pin changed state. This pin
		/// is used to indicate whether a modem is connected to a working phone line and a data carrier
		/// signal is detected.
		/// </summary>
		RlsdChanged,

		/// <summary>
		/// A ring indicator pin changed state.
		/// </summary>
		RingChanged,

		/// <summary>
		/// A Break was detected on input.
		/// </summary>
		Break
	}

	/// <summary>
	/// Provides data for the <see cref="SerialPort.ModemEvent(Action{ModemPinState})"/> event.
	/// </summary>
	public class ModemPinState
	{
		/// <summary>
		/// Initialises a new <see cref="ModemPinEvent"/> object with the
		/// <see cref="ModemPinEvent"/> event type.
		/// </summary>
		/// <param name="eventType">The event type that caused the exception.</param>
		/// <param name="dsr">Status of DSR pin.</param>
		/// <param name="ring">Status of RI pin.</param>
		/// <param name="rlsd">Status of RLSD pin.</param>
		/// <param name="cts">Status of RTS pin.</param>
		public ModemPinState(ModemPinEvent eventType, bool cts, bool dsr, bool rlsd, bool ring)
		{
			EventType = eventType;
			Cts = cts;
			Dsr = dsr;
			Rlsd = rlsd;
			Ring = ring;
		}


		/// <summary>
		/// Gets the event type.
		/// </summary>
		public ModemPinEvent EventType { get; private set; }


		/// <summary>
		/// Gets the state of CTS
		/// </summary>
		/// <remarks>This property contains the state of the CTS pin when the event occurred.</remarks>
		/// <value><c>true</c> if the pin is asserted; otherwise <c>false</c>.</value>
		public bool Cts { get; private set; }


		/// <summary>
		/// Gets the state of DSR
		/// </summary>
		/// <remarks>This property contains the state of the DSR pin when the event occurred.</remarks>
		/// <value><c>true</c> if the pin is asserted; otherwise <c>false</c>.</value>
		public bool Dsr { get; private set; }


		/// <summary>
		/// Gets the state of RLSD
		/// </summary>
		/// <remarks>This property contains the state of the RLSD pin when the event occurred.</remarks>
		/// <value><c>true</c> if the pin is asserted; otherwise <c>false</c>.</value>
		public bool Rlsd { get; private set; }


		/// <summary>
		/// Gets the state of RI
		/// </summary>
		/// <remarks>This property contains the state of the RI pin when the event occurred.</remarks>
		/// <value><c>true</c> if the pin is asserted; otherwise <c>false</c>.</value>
		public bool Ring { get; private set; }

	}

}
