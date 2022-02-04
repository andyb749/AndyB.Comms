using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;
using AndyB.Comms.Serial;


namespace SerialTestApp
{
    public partial class MainForm : KryptonForm
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SerialPort _port;
        private IDictionary<string, string> _portNames;


        public MainForm()
        {
            InitializeComponent();
            _port = new SerialPort();

            //_port.Connected += Port_Connected;
            //_port.ErrorReceived += Port_ErrorReceived;
            //_port.DataReceived += Port_DataReceived;
            //_port.PinChanged += Port_PinChanged;
            //_port.TransmitCompleted += Port_TransmitCompleted;
        }

        private readonly int[] _baudRates = new int[] { 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115000 };
        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshPorts();
            RefreshTimeouts();
            foreach (var baud in _baudRates)
                uxBaudrate.Items.Add(baud);
            uxBaudrate.SelectedValue = _port.BaudRate;
        }

        private void UxConnect_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                _port.Disconnect();
                uxConnect.Text = "Connect";
                uxRefresh.Enabled = true;
                uxPorts.Enabled = true;
                uxConnected.Checked = false;
            }
            else
            {
                if (string.IsNullOrEmpty(_port.PortName))
                    return;

                _port.Connect();
                uxConnect.Text = "Disconnect";
                uxRefresh.Enabled = false;
                uxPorts.Enabled = false;
                uxConnected.Checked = true;
            }
        }


        private void UxRefresh_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
                return;
            RefreshPorts();
        }


        private void RefreshPorts()
        {
            _portNames = SerialPort.GetPortNames();
            uxPorts.Items.Clear();
            foreach (var pair in _portNames)
            {
                uxPorts.Items.Add($"{pair.Key} - {pair.Value}");
                Log($"Found {pair.Key} - {pair.Value}");
            }
        }

        private void RefreshTimeouts()
        {
            uxTxTimeout.Text = _port.TxTimeout.ToString();
            //uxTxMultiplier.Text = _port.TxMultiplierTimeout.ToString();
            uxRxTimeout.Text = _port.RxTimeout.ToString();
            //uxRxInterval.Text = _port.RxIntervalTimeout.ToString();
            //uxRxMultiplier.Text = _port.RxMultiplyTimeout.ToString();
        }


        private void Log(string msg)
        {
            // Check if we need to invoke if caller is on different thread
            if (uxLog.InvokeRequired)
            {
                Action safe = delegate { Log(msg); };
                uxLog.Invoke(safe);
            }
            else
            {
                uxLog.Items.Add(msg);
                uxLog.TopIndex = uxLog.Items.Count - 1;
            }
        }

#if false
        private void Port_Connected(object sender, ConnectedEventArgs e)
        {
            if (e.IsConnected)
                Log($"Serial port connected");
            else
                Log($"Serial port disconnected");
        }
#endif

#if false
        private void Port_ErrorReceived(object sender, ErrorReceivedEventArgs e)
        {
            Log($"Error received: {e.EventType}");
        }
#endif
#if false
        private void Port_TransmitCompleted(object sender, EventArgs e)
        {
            Log("Transmit completed");
        }
#endif
#if false
        private void Port_PinChanged(object sender, PinChangedEventArgs e)
        {
            Log($"Modem pin changed: {e.EventType}");
            Toggle(uxDSR, e.PinState.HasFlag(ModemPinState.Dsr));
            Toggle(uxCTS, e.PinState.HasFlag(ModemPinState.Cts));
            Toggle(uxRLSD, e.PinState.HasFlag(ModemPinState.Rlsd));
            Toggle(uxRI, e.PinState.HasFlag(ModemPinState.Ring));
        }
#endif
        private void Toggle(Controls.LedControl sender, bool state)
        {
            if (sender.InvokeRequired)
            {
                Action safe = delegate { Toggle(sender, state); };
                sender.Invoke(safe);
            }
            else
                sender.Checked = state;
        }

#if false
        private void Port_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Log($"DataReceived:");
            var lines = Encoding.ASCII.GetString(e.ReceiveBuffer).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
                Log(line);
        }
#endif

        private void UxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_port.IsConnected)
            {
                _port.PortName = _portNames.Values.ToArray()[uxPorts.SelectedIndex];
                uxPortName.Text = _port.PortName;
            }
        }

        private void UxRefreshModem_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                var state = _port.ModemStatus;
                // FIXME: check these names & namespace and duplication in events!
                uxDSR.Checked = state.HasFlag(AndyB.Comms.Serial.Interop.ModemStat.Dsr);
                uxCTS.Checked = state.HasFlag(AndyB.Comms.Serial.Interop.ModemStat.Cts);
                uxRLSD.Checked = state.HasFlag(AndyB.Comms.Serial.Interop.ModemStat.RLSD);
                uxRI.Checked = state.HasFlag(AndyB.Comms.Serial.Interop.ModemStat.Ring);
            }
        }


        #region Escape Functions

        private bool _dtrState;
        private void UxDTR_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
                _port.Dtr = _dtrState = !_dtrState;
        }

        private bool _rtsState;
        private void UxRTS_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
                _port.Rts = _rtsState = !_rtsState;
        }

        private bool _xonXoffState;
        private void UxXonXoff_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
                _port.XonXoff = _xonXoffState = !_xonXoffState;
        }

        private bool _breakState;
        private void UxBreak_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
                _port.Break = _breakState = !_breakState;
        }

        private bool _resetState;
        private void UxReset_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
                _port.Reset = _resetState = !_resetState;
        }

        #endregion


        private void UxClear_Click(object sender, EventArgs e)
        {
            uxLog.Items.Clear();
        }

        private void UxRefresh3_Click(object sender, EventArgs e)
        {
            // DCB
            uxBaud.Text = _port.BaudRate.ToString();
            uxData.Text = _port.DataBits.ToString();
            uxParity.Text = _port.Parity.ToString();
            uxStop.Text = _port.StopBits.ToString();
            if (_port.IsConnected)
            {
                // DCB
                uxTxFlowCts.Text = _port.TxFlowCts.ToString();
                uxTxFlowDsr.Text = _port.TxFlowDsr.ToString();
                uxDtrControl.Text = _port.DtrControl.ToString();
                uxRtsControl.Text = _port.RtsControl.ToString();
                uxRxDsrSense.Text = _port.RxDsrSensitivity.ToString();
                uxTxContinue.Text = _port.TxContinue.ToString();
                uxTxFlowXoff.Text = _port.TxFlowXoff.ToString();
                uxRxFlowXoff.Text = _port.RxFlowXoff.ToString();
                uxXonChar.Text = $"{_port.XonCharacter:X02}";
                uxXoffChar.Text = $"{_port.XoffCharacter:X02}";
                uxErrorChar.Text = $"{_port.ErrorChar:X02}";
                uxEofChar.Text = $"{_port.EofChar:X02}";
                uxEventChar.Text = $"{_port.EventChar:X02}";
                uxPackedValue.Text = $"{_port.PackedValues:X08}";

                //uxErrorChars.Text = ;
                //uxDiscardNull.Checked;
                //uxAbortOnError.Checked;
                //var status = _port.GetStatus();
                //uxCtsHold.Checked = status.CtsHold;
                //uxDsrHold.Checked = status.DsrHold;
                //uxRlsdHold.Checked = status.RlsdHold;
                //uxXoffHold.Checked = status.XoffHold;
                //uxXoffSent.Checked = status.XoffSent;
                //uxEof.Checked = status.Eof;
                //uxTxIm.Checked = status.TxIm;
                //uxRxQueue.Text = status.InQueue.ToString();
                //uxTxQueue.Text = status.OutQueue.ToString();
            }
        }

        private void UxTxTimeout_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxTxTimeout.Text, out uint txTimeout))
                _port.TxTimeout = txTimeout;
            RefreshTimeouts();
        }

        private void uxTxMultiplier_Leave(object sender, EventArgs e)
        {
            //if (uint.TryParse(uxTxMultiplier.Text, out uint txTimeout))
            //    _port.TxMultiplierTimeout = txTimeout;
            RefreshTimeouts();
        }

        private void UxRxInterval_Leave(object sender, EventArgs e)
        {
            //if (uint.TryParse(uxRxInterval.Text, out uint rxTimeout))
            //    _port.RxIntervalTimeout = rxTimeout;
            RefreshTimeouts();
        }

        private void UxRxTimeout_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxRxTimeout.Text, out uint rxTimeout))
                _port.RxTimeout = rxTimeout;
            RefreshTimeouts();
        }

        private void uxRxMultiplier_Leave(object sender, EventArgs e)
        {
            //if (uint.TryParse(uxRxMultiplier.Text, out uint rxTimeout))
            //    _port.RxMultiplyTimeout = rxTimeout;
            RefreshTimeouts();
        }

        private const string txString = "The quick brown fox jumps over the lazy dog";
        private void UxSend_Click(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                byte[] bytes;
                if (string.IsNullOrEmpty(uxTxBuffer.Text))
                    bytes = Encoding.ASCII.GetBytes(txString);
                else
                    bytes = Encoding.ASCII.GetBytes(uxTxBuffer.Text);
                //                var ar = _port.BeginWrite(bytes, 0, bytes.Length, iar=>
                //                {
                //                    _logger.Debug("User callback called");                    
                //                    _port.EndWrite(iar);
                //                }
                //                , null);

                //var ar = _port.BeginWrite(bytes, 0, bytes.Length, null, null);
                //ar.AsyncWaitHandle.WaitOne();
                //_port.EndWrite(ar);

                //var task = _port.WriteAsync(bytes, 0, bytes.Length);
                //task.Wait();
                _port.Write(bytes, 0, bytes.Length);
//                var comp = ar.IsCompleted;
            }
        }

        #region DCB Controls

        private void UxDtrDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                if (uxDtrDisabled.Checked)
                    _port.DtrControl = PinStates.Disable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxDtrEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                if (uxDtrEnabled.Checked)
                    _port.DtrControl = PinStates.Enable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxDtrHandshake_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                if (uxDtrHandshake.Checked)
                    _port.DtrControl = PinStates.Handshake;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxRtsDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                if (uxRtsDisabled.Checked)
                    _port.RtsControl = PinStates.Disable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxRtsEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                if (uxRtsEnabled.Checked)
                    _port.RtsControl = PinStates.Enable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxRtsHandshake_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsConnected)
            {
                if (uxRtsHandshake.Checked)
                    _port.RtsControl = PinStates.Handshake;
                UxRefresh3_Click(sender, e);
            }
        }

        #endregion


        private void UxDsrEnable_CheckedChanged(object sender, EventArgs e)
        {
            //_port.TxFlowDsr = uxDsrEnable.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxCtsEnable_CheckedChanged(object sender, EventArgs e)
        {
            //_port.TxFlowCts = uxCtsEnable.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void uxDsrSense_CheckedChanged(object sender, EventArgs e)
        {
            //_port.RxDsrSensitivity = uxDsrSense.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxBaudrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.BaudRate = Enum.Parse<BaudRate>(uxBaudrate.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.DataBits = Enum.Parse<DataBits>(uxDataBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxParityBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.Parity = Enum.Parse<ParityBit>(uxParityBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.StopBits = Enum.Parse<StopBits>(uxStopBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxTxXOff_CheckedChanged(object sender, EventArgs e)
        {
            //_port.TxContinue = uxTxXon.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void uxTxXon_CheckedChanged(object sender, EventArgs e)
        {
            //_port.TxFlowXoff = uxTxXon.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void uxRxXon_CheckedChanged(object sender, EventArgs e)
        {
            //_port.RxFlowXoff = uxRxXon.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void uxParityReplace_CheckedChanged(object sender, EventArgs e)
        {
            //_port.ParityReplace = uxParityReplace.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void uxDiscardNull_CheckedChanged(object sender, EventArgs e)
        {
            //_port.DiscardNull = uxDiscardNull.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void uxAbortOnError_CheckedChanged(object sender, EventArgs e)
        {
            //_port.AbortOnError = uxAbortOnError.Checked;
            UxRefresh3_Click(sender, e);
        }

    }
}
