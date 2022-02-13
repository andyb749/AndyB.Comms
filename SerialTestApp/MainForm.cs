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
            _port.TxEmpty += (o, e)=> Log($"Transmitter empty");
            _port.DataReceived += (o, e) => Log($"Data received {e.EventType}");
            _port.ErrorReceived += (o, e) =>
            {
                Log($"Error received {e.EventType}");
            };
            _port.PinChanged += (o, e) =>
            {
                Log($"Pin changed {e.EventType}, Modem status {e.ModemStatus}");
                uxDSR.Checked = e.ModemStatus.HasFlag(ModemStatus.Dsr);
                uxCTS.Checked = e.ModemStatus.HasFlag(ModemStatus.Cts);
                uxRLSD.Checked = e.ModemStatus.HasFlag(ModemStatus.Rlsd);
                uxRI.Checked = e.ModemStatus.HasFlag(ModemStatus.Ring);
            };
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshPorts();
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
            uxTxTimeout.Text = _port.TxTimeout.ToString();
            uxTxMultiplier.Text = _port.TxMultiplierTimeout.ToString();
            uxRxTimeout.Text = _port.RxTimeout.ToString();
            uxRxInterval.Text = _port.RxIntervalTimeout.ToString();
            uxRxMultiplier.Text = _port.RxMultiplierTimeout.ToString();
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
#endif

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
            if (!_port.IsOpen)
            {
                _port.PortName = _portNames.Values.ToArray()[uxPorts.SelectedIndex];
                uxPortName.Text = _port.PortName;
            }
        }


        #region Escape Functions

        private bool _dtrState;
        private void UxDTR_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
                _port.Dtr = _dtrState = !_dtrState;
        }

        private bool _rtsState;
        private void UxRTS_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
                _port.Rts = _rtsState = !_rtsState;
        }

        private bool _xonXoffState;
        private void UxXonXoff_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
                _port.XonXoff = _xonXoffState = !_xonXoffState;
        }

        private bool _breakState;
        private void UxBreak_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
                _port.Break = _breakState = !_breakState;
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
            if (_port.IsOpen)
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
                var status = _port.PortStatus;
                uxCtsHold.Checked = status.HasFlag(CommStatus.CtsHold);
                uxDsrHold.Checked = status.HasFlag(CommStatus.DsrHold);
                uxRlsdHold.Checked = status.HasFlag(CommStatus.RlsdHold);
                uxXoffHold.Checked = status.HasFlag(CommStatus.XoffHold);
                uxXoffSent.Checked = status.HasFlag(CommStatus.XoffSent);
                uxEof.Checked = status.HasFlag(CommStatus.Eof);
                uxTxIm.Checked = status.HasFlag(CommStatus.TxIm);
                uxRxQueue.Text = _port.RxQueueCount.ToString();
                uxTxQueue.Text = _port.TxQueueCount.ToString();

                RefreshTimeouts();
            }
        }


        #region Timeouts

        private void UxTxTimeout_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxTxTimeout.Text, out uint txTimeout))
                _port.TxTimeout = txTimeout;
            RefreshTimeouts();
        }


        private void UxTxMultiplier_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxTxMultiplier.Text, out uint txTimeout))
                _port.TxMultiplierTimeout = txTimeout;
            RefreshTimeouts();
        }

        private void UxRxInterval_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxRxInterval.Text, out uint rxTimeout))
                _port.RxIntervalTimeout = rxTimeout;
            RefreshTimeouts();
        }

        private void UxRxTimeout_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxRxTimeout.Text, out uint rxTimeout))
                _port.RxTimeout = rxTimeout;
            RefreshTimeouts();
        }

        private void UxRxMultiplier_Leave(object sender, EventArgs e)
        {
            if (uint.TryParse(uxRxMultiplier.Text, out uint rxTimeout))
                _port.RxMultiplierTimeout = rxTimeout;
            RefreshTimeouts();
        }

        #endregion


        private const string txString = "Quick brown fox jumps over the lazy dog";
        private async void UxSend_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                byte[] bytes;
                if (string.IsNullOrEmpty(uxTxBuffer.Text))
                    bytes = Encoding.ASCII.GetBytes(txString);
                else
                    bytes = Encoding.ASCII.GetBytes(uxTxBuffer.Text);
                try
                {
                    //var ar = _port.BaseStream.BeginWrite(bytes, 0, bytes.Length, iar =>
                    //{
                    //    _logger.Debug("User callback called");
                    //    _port.BaseStream.EndWrite(iar);
                    //}, null);

                    //var ar = _port.BaseStream.BeginWrite(bytes, 0, bytes.Length, null, null);
                    //ar.AsyncWaitHandle.WaitOne();
                    //_port.BaseStream.EndWrite(ar);

                    //_port.BaseStream.Write(bytes, 0, bytes.Length);

                    await _port.BaseStream.WriteAsync(bytes, 0, bytes.Length);
                }
                catch (TimeoutException)
                {
                    Log("Timedout in write");
                }
                catch (Exception ex)
                {
                    Log($"Exception {ex} in write");
                }

                //AsyncUtil.RunSync(() => _port.WriteAsync(bytes, 0, bytes.Length));
                //var task = _port.WriteAsync(bytes, 0, bytes.Length);
                //task.Wait();
                //var comp = ar.IsCompleted;
            }
        }


        private void uxRead_Click(object sender, EventArgs e)
        {
            var buffer = new byte[256];
            try
            {
                var count = _port.Read(buffer, 0, buffer.Length);
                Log($"{count} bytes read from port");
                Log(Encoding.ASCII.GetString(buffer, 0, count));
            }
            catch (TimeoutException)
            {
                Log($"Timedout in read");
            }
            catch (Exception ex)
            {
                Log($"Exception {ex} in read");
            }
        }


        #region DCB Controls

        #region DTR Handshake Radio Buttons

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

        private void uxDtrToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxDtrToggle.Checked)
                    _port.DtrControl = PinStates.Toggle;
                UxRefresh3_Click(sender, e);
            }
        }

        #endregion

        #region RTS Radio Buttons

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

        private void uxRtsToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                if (uxRtsToggle.Checked)
                    _port.RtsControl = PinStates.Toggle;
                UxRefresh3_Click(sender, e);
            }
        }

        #endregion

        private void UxDsrEnable_CheckedChanged(object sender, EventArgs e)
        {
            _port.TxFlowDsr = uxDsrEnable.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxCtsEnable_CheckedChanged(object sender, EventArgs e)
        {
            _port.TxFlowCts = uxCtsEnable.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxDsrSense_CheckedChanged(object sender, EventArgs e)
        {
            _port.RxDsrSensitivity = uxDsrSense.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxBaudrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.BaudRate = Enum.Parse<BaudRate>(uxBaudrate.Text);
            UxRefresh3_Click(sender, e);
        }

        private void UxDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.DataBits = Enum.Parse<DataBits>(uxDataBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void UxParityBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.Parity = Enum.Parse<ParityBit>(uxParityBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void UxStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            _port.StopBits = Enum.Parse<StopBits>(uxStopBits.Text);
            UxRefresh3_Click(sender, e);
        }

        private void UxTxXOff_CheckedChanged(object sender, EventArgs e)
        {
            _port.TxContinue = uxTxXon.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxTxXon_CheckedChanged(object sender, EventArgs e)
        {
            _port.TxFlowXoff = uxTxXon.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxRxXon_CheckedChanged(object sender, EventArgs e)
        {
            _port.RxFlowXoff = uxRxXon.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxParityReplace_CheckedChanged(object sender, EventArgs e)
        {
            // TODO:
            //_port.ParityReplace = uxParityReplace.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxDiscardNull_CheckedChanged(object sender, EventArgs e)
        {
            // TODO:
            //_port.DiscardNull = uxDiscardNull.Checked;
            UxRefresh3_Click(sender, e);
        }

        private void UxAbortOnError_CheckedChanged(object sender, EventArgs e)
        {
            // TODO:
            //_port.AbortOnError = uxAbortOnError.Checked;
            UxRefresh3_Click(sender, e);
        }


        #endregion

        private void uxDtrRts_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                var checkBox = sender as KryptonCheckBox;
                _port.DtrDsrHandshake = checkBox.Checked;
            }
        }

        private void uxRtsCts_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                var checkBox = sender as KryptonCheckBox;
                _port.RtsCtsHandshake = checkBox.Checked;
            }
        }

        private void uxDtrEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                var checkBox = sender as KryptonCheckBox;
                _port.Dtr = checkBox.Checked;
            }
        }

        private void uxRtsEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                var checkBox = sender as KryptonCheckBox;
                _port.Rts = checkBox.Checked;
            }

        }

        private void uxPurge_Click(object sender, EventArgs e)
        {
            if (_port.IsOpen)
            {
                _port.RxPurge();
                _port.TxPurge();
            }
        }
    }
}
