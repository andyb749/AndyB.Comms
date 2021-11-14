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
            //_port.TxTimeout = 10000;
            //_port.RxTimeout = 2000;
            //_port.RxIntervalTimeout = 2000;
            _port.Parity = Parity.Even;

            //_port.Connected += Port_Connected;
            _port.ErrorReceived += Port_ErrorReceived;
            //_port.DataReceived += Port_DataReceived;
            _port.PinChanged += Port_PinChanged;
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
            if (_port.IsOpen)
            {
                _port.Close();
                uxConnect.Text = "Connect";
                uxRefresh.Enabled = true;
                uxPorts.Enabled = true;
                uxConnected.Checked = false;
            }
            else
            {
                if (string.IsNullOrEmpty(_port.PortName))
                    return;

                _port.Open();
                uxConnect.Text = "Disconnect";
                uxRefresh.Enabled = false;
                uxPorts.Enabled = false;
                uxConnected.Checked = true;
            }
        }


        private void UxRefresh_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
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
#if false
            uxTxTimeout.Text = _port.TxTimeout.ToString();
            uxTxMultiplier.Text = _port.TxMultiplierTimeout.ToString();
            uxRxTimeout.Text = _port.RxTimeout.ToString();
            uxRxInterval.Text = _port.RxIntervalTimeout.ToString();
            uxRxMultiplier.Text = _port.RxMultiplyTimeout.ToString();
#endif
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

        private void Port_ErrorReceived(object sender, ErrorReceivedEventArgs e)
        {
            Log($"Error received: {e.EventType}");
        }

        private void Port_TransmitCompleted(object sender, EventArgs e)
        {
            Log("Transmit completed");
        }

        private void Port_PinChanged(object sender, PinChangedEventArgs e)
        {
            Log($"Modem pin changed: {e.EventType}");
            Toggle(uxDSR, e.PinState.HasFlag(ModemPinState.Dsr));
            Toggle(uxCTS, e.PinState.HasFlag(ModemPinState.Cts));
            Toggle(uxRLSD, e.PinState.HasFlag(ModemPinState.Rlsd));
            Toggle(uxRI, e.PinState.HasFlag(ModemPinState.Ring));
        }

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

        private void Port_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Log($"DataReceived:");
            var lines = Encoding.ASCII.GetString(e.ReceiveBuffer).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
                Log(line);
        }


        private void UxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
#if true
            if (!_port.IsOpen)
            {
                _port.PortName = _portNames.Values.ToArray()[uxPorts.SelectedIndex];
                uxPortName.Text = _port.PortName;
            }
#endif
        }

        private void UxRefreshModem_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                var state = _port.ModemPinState;
                uxDSR.Checked = state.HasFlag(ModemPinState.Dsr);
                uxCTS.Checked = state.HasFlag(ModemPinState.Cts);
                uxRLSD.Checked = state.HasFlag(ModemPinState.Rlsd);
                uxRI.Checked = state.HasFlag(ModemPinState.Ring);
            }
        }


        private bool _dtrState;
        private void UxDTR_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (_dtrState)
                    _port.ClrDtr();
                else
                    _port.SetDtr();
                _dtrState = !_dtrState;
            }
        }

        private bool _rtsState;
        private void UxRTS_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (_rtsState)
                    _port.ClrRts();
                else
                    _port.SetRts();
                _rtsState = !_rtsState;
            }
        }

        private bool _xonXoffState;
        private void UxXonXoff_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (_xonXoffState)
                    _port.SetXon();
                else
                    _port.SetXoff();
                _xonXoffState = !_xonXoffState;
            }
        }

        private bool _breakState;
        private void UxBreak_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (_breakState)
                    _port.ClrBrk();
                else
                    _port.SetBrk();
                _breakState = !_breakState;
            }
        }

        private void UxReset_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
#if false
                _port.Reset();
#endif
            }
        }

        private void UxClear_Click(object sender, EventArgs e)
        {
            uxLog.Items.Clear();
        }

        private void UxRefresh3_Click(object sender, EventArgs e)
        {
            uxBaud.Text = _port.BaudRate.ToString();
            uxData.Text = _port.DataBits.ToString();
            uxParity.Text = _port.Parity.ToString();
            uxStop.Text = _port.StopBits.ToString();
#if false
            uxTxFlowCts.Text = _port.Settings.TxFlowCTS.ToString();
            uxTxFlowDsr.Text = _port.Settings.TxFlowDSR.ToString();
#endif
            uxDtrControl.Text = _port.DtrControl.ToString();
            uxRtsControl.Text = _port.RtsControl.ToString();
#if false
            uxRxDsrSense.Text = _port.Settings.RxGateDSR.ToString();
            uxTxContinue.Text = _port.Settings.TxWhenRxXoff.ToString();
            uxTxFlowXoff.Text = _port.Settings.TxFlowX.ToString();
            uxRxFlowXoff.Text = _port.Settings.RxFlowX.ToString();
#endif
            if (_port.IsOpen)
            {
                var status = _port.GetStatus();
                uxCtsHold.Checked = status.CtsHold;
                uxDsrHold.Checked = status.DsrHold;
                uxRlsdHold.Checked = status.RlsdHold;
                uxXoffHold.Checked = status.XoffHold;
                uxXoffSent.Checked = status.XoffSent;
                uxEof.Checked = status.Eof;
                uxTxIm.Checked = status.TxIm;
                uxRxQueue.Text = status.InQueue.ToString();
                uxTxQueue.Text = status.OutQueue.ToString();
            }
        }

        private void UxTxTimeout_Leave(object sender, EventArgs e)
        {
#if false
            if (uint.TryParse(uxTxTimeout.Text, out uint txTimeout))
                _port.TxTimeout = txTimeout;
            RefreshTimeouts();
#endif
        }

        private void uxTxMultiplier_Leave(object sender, EventArgs e)
        {
#if false
            if (uint.TryParse(uxTxMultiplier.Text, out uint txTimeout))
                _port.TxMultiplierTimeout = txTimeout;
            RefreshTimeouts();
#endif
        }

        private void UxRxInterval_Leave(object sender, EventArgs e)
        {
#if false
            if (uint.TryParse(uxRxInterval.Text, out uint rxTimeout))
                _port.RxIntervalTimeout = rxTimeout;
            RefreshTimeouts();
#endif
        }

        private void UxRxTimeout_Leave(object sender, EventArgs e)
        {
#if false
            if (uint.TryParse(uxRxTimeout.Text, out uint rxTimeout))
                _port.RxTimeout = rxTimeout;
            RefreshTimeouts();
#endif
        }

        private void uxRxMultiplier_Leave(object sender, EventArgs e)
        {
#if false
            if (uint.TryParse(uxRxMultiplier.Text, out uint rxTimeout))
                _port.RxMultiplyTimeout = rxTimeout;
            RefreshTimeouts();
#endif
        }

        private const string txString = "The quick brown fox jumps over the lazy dog";
        private void UxSend_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                byte[] bytes;
                if (string.IsNullOrEmpty(uxTxBuffer.Text))
                    bytes = Encoding.ASCII.GetBytes(txString);
                else
                    bytes = Encoding.ASCII.GetBytes(uxTxBuffer.Text);
                var ar = _port.BeginWrite(bytes, 0, bytes.Length, iar=>
                {
                    _logger.Debug("User callback called");                    
                    _port.EndWrite(iar);
                }
                , null);

                //var ar = _port.BeginWrite(bytes, 0, bytes.Length, null, null);
                //ar.AsyncWaitHandle.WaitOne();
                //_port.EndWrite(ar);

                //var task = _port.WriteAsync(bytes, 0, bytes.Length);
                //task.Wait();

                var comp = ar.IsCompleted;
            }
        }

        private void UxDtrDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxDtrDisabled.Checked)
                    _port.DtrControl = PinStates.Disable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxDtrEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxDtrEnabled.Checked)
                    _port.DtrControl = PinStates.Enable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxDtrHandshake_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxDtrHandshake.Checked)
                    _port.DtrControl = PinStates.Handshake;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxRtsDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxRtsDisabled.Checked)
                    _port.RtsControl = PinStates.Disable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxRtsEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxRtsEnabled.Checked)
                    _port.RtsControl = PinStates.Enable;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxRtsHandshake_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxRtsHandshake.Checked)
                    _port.RtsControl = PinStates.Handshake;
                UxRefresh3_Click(sender, e);
            }
        }

        private void UxDsrEnable_CheckedChanged(object sender, EventArgs e)
        {
#if false
            _port.Settings.TxFlowDSR = uxDsrEnable.Checked;
            UxRefresh3_Click(sender, e);
#endif
        }

        private void UxCtsEnable_CheckedChanged(object sender, EventArgs e)
        {
#if false
            _port.Settings.TxFlowCTS = uxCtsEnable.Checked;
            UxRefresh3_Click(sender, e);
#endif
        }

        private void UxBaudrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.BaudRate = int.Parse(uxBaudrate.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.DataBits = int.Parse(uxDataBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxParityBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.Parity = Enum.Parse<Parity>(uxParityBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void uxStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.StopBits = Enum.Parse<StopBits>(uxStopBits.Text);
            UxRefresh3_Click(sender, e);
        }
    }
}
