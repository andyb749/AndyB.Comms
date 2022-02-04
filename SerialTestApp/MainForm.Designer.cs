
namespace SerialTestApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.uxLog = new Krypton.Toolkit.KryptonListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.uxConnected = new SerialTestApp.Controls.LedControl();
            this.uxPortName = new Krypton.Toolkit.KryptonTextBox();
            this.uxPorts = new Krypton.Toolkit.KryptonComboBox();
            this.uxRefresh = new Krypton.Toolkit.KryptonButton();
            this.uxConnect = new Krypton.Toolkit.KryptonButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.uxRI = new SerialTestApp.Controls.LedControl();
            this.uxRLSD = new SerialTestApp.Controls.LedControl();
            this.uxCTS = new SerialTestApp.Controls.LedControl();
            this.uxDSR = new SerialTestApp.Controls.LedControl();
            this.uxRefreshModem = new Krypton.Toolkit.KryptonButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.uxReset = new Krypton.Toolkit.KryptonButton();
            this.uxBreak = new Krypton.Toolkit.KryptonButton();
            this.uxXonXoff = new Krypton.Toolkit.KryptonButton();
            this.uxRTS = new Krypton.Toolkit.KryptonButton();
            this.uxDTR = new Krypton.Toolkit.KryptonButton();
            this.uxClear = new Krypton.Toolkit.KryptonButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label8 = new Krypton.Toolkit.KryptonLabel();
            this.uxErrorChars = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel7 = new Krypton.Toolkit.KryptonLabel();
            this.uxPackedValue = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel6 = new Krypton.Toolkit.KryptonLabel();
            this.uxEventChar = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel5 = new Krypton.Toolkit.KryptonLabel();
            this.uxEofChar = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel4 = new Krypton.Toolkit.KryptonLabel();
            this.uxErrorChar = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel3 = new Krypton.Toolkit.KryptonLabel();
            this.uxXoffChar = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel2 = new Krypton.Toolkit.KryptonLabel();
            this.uxXonChar = new Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
            this.uxRxFlowXoff = new Krypton.Toolkit.KryptonTextBox();
            this.uxRtsControl = new Krypton.Toolkit.KryptonTextBox();
            this.uxTxFlowXoff = new Krypton.Toolkit.KryptonTextBox();
            this.uxTxContinue = new Krypton.Toolkit.KryptonTextBox();
            this.uxRxDsrSense = new Krypton.Toolkit.KryptonTextBox();
            this.uxDtrControl = new Krypton.Toolkit.KryptonTextBox();
            this.uxTxFlowDsr = new Krypton.Toolkit.KryptonTextBox();
            this.uxTxFlowCts = new Krypton.Toolkit.KryptonTextBox();
            this.uxStop = new Krypton.Toolkit.KryptonTextBox();
            this.uxParity = new Krypton.Toolkit.KryptonTextBox();
            this.uxData = new Krypton.Toolkit.KryptonTextBox();
            this.label12 = new Krypton.Toolkit.KryptonLabel();
            this.label11 = new Krypton.Toolkit.KryptonLabel();
            this.label10 = new Krypton.Toolkit.KryptonLabel();
            this.label9 = new Krypton.Toolkit.KryptonLabel();
            this.label7 = new Krypton.Toolkit.KryptonLabel();
            this.label6 = new Krypton.Toolkit.KryptonLabel();
            this.label5 = new Krypton.Toolkit.KryptonLabel();
            this.label4 = new Krypton.Toolkit.KryptonLabel();
            this.label3 = new Krypton.Toolkit.KryptonLabel();
            this.label2 = new Krypton.Toolkit.KryptonLabel();
            this.label1 = new Krypton.Toolkit.KryptonLabel();
            this.uxBaud = new Krypton.Toolkit.KryptonTextBox();
            this.uxRefresh3 = new Krypton.Toolkit.KryptonButton();
            this.uxToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.uxTxXOff = new System.Windows.Forms.CheckBox();
            this.uxTxXon = new System.Windows.Forms.CheckBox();
            this.uxRxXon = new System.Windows.Forms.CheckBox();
            this.uxParityReplace = new System.Windows.Forms.CheckBox();
            this.uxDiscardNull = new System.Windows.Forms.CheckBox();
            this.uxAbortOnError = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label14 = new Krypton.Toolkit.KryptonLabel();
            this.label13 = new Krypton.Toolkit.KryptonLabel();
            this.uxBreakDet = new SerialTestApp.Controls.LedControl();
            this.uxFraming = new SerialTestApp.Controls.LedControl();
            this.uxParityErr = new SerialTestApp.Controls.LedControl();
            this.uxOverrun = new SerialTestApp.Controls.LedControl();
            this.uxOverflow = new SerialTestApp.Controls.LedControl();
            this.uxTxQueue = new Krypton.Toolkit.KryptonTextBox();
            this.uxRxQueue = new Krypton.Toolkit.KryptonTextBox();
            this.uxTxIm = new SerialTestApp.Controls.LedControl();
            this.uxEof = new SerialTestApp.Controls.LedControl();
            this.uxXoffSent = new SerialTestApp.Controls.LedControl();
            this.uxXoffHold = new SerialTestApp.Controls.LedControl();
            this.uxRlsdHold = new SerialTestApp.Controls.LedControl();
            this.uxDsrHold = new SerialTestApp.Controls.LedControl();
            this.uxCtsHold = new SerialTestApp.Controls.LedControl();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.uxTxMultiplier = new Krypton.Toolkit.KryptonTextBox();
            this.uxRxMultiplier = new Krypton.Toolkit.KryptonTextBox();
            this.label19 = new Krypton.Toolkit.KryptonLabel();
            this.label18 = new Krypton.Toolkit.KryptonLabel();
            this.label17 = new Krypton.Toolkit.KryptonLabel();
            this.uxTxTimeout = new Krypton.Toolkit.KryptonTextBox();
            this.label16 = new Krypton.Toolkit.KryptonLabel();
            this.uxRxInterval = new Krypton.Toolkit.KryptonTextBox();
            this.label15 = new Krypton.Toolkit.KryptonLabel();
            this.uxRxTimeout = new Krypton.Toolkit.KryptonTextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.uxDtrHandshake = new Krypton.Toolkit.KryptonRadioButton();
            this.uxDtrEnabled = new Krypton.Toolkit.KryptonRadioButton();
            this.uxDtrDisabled = new Krypton.Toolkit.KryptonRadioButton();
            this.uxTxBuffer = new Krypton.Toolkit.KryptonTextBox();
            this.uxSend = new Krypton.Toolkit.KryptonButton();
            this.uxBaudrate = new Krypton.Toolkit.KryptonComboBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.uxRtsHandshake = new Krypton.Toolkit.KryptonRadioButton();
            this.uxRtsEnabled = new Krypton.Toolkit.KryptonRadioButton();
            this.uxRtsDisabled = new Krypton.Toolkit.KryptonRadioButton();
            this.uxDsrEnable = new System.Windows.Forms.CheckBox();
            this.uxCtsEnable = new System.Windows.Forms.CheckBox();
            this.uxDataBits = new Krypton.Toolkit.KryptonComboBox();
            this.uxParityBits = new Krypton.Toolkit.KryptonComboBox();
            this.uxStopBits = new Krypton.Toolkit.KryptonComboBox();
            this.uxDsrSense = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxPorts)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxBaudrate)).BeginInit();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxDataBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uxParityBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uxStopBits)).BeginInit();
            this.SuspendLayout();
            // 
            // uxLog
            // 
            this.uxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uxLog.FormattingEnabled = true;
            this.uxLog.Location = new System.Drawing.Point(0, 628);
            this.uxLog.Name = "uxLog";
            this.uxLog.Size = new System.Drawing.Size(915, 124);
            this.uxLog.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.uxConnected);
            this.groupBox1.Controls.Add(this.uxPortName);
            this.groupBox1.Controls.Add(this.uxPorts);
            this.groupBox1.Controls.Add(this.uxRefresh);
            this.groupBox1.Controls.Add(this.uxConnect);
            this.groupBox1.Location = new System.Drawing.Point(0, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 112);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // uxConnected
            // 
            this.uxConnected.Enabled = false;
            this.uxConnected.ForeColor = System.Drawing.SystemColors.ControlText;
            this.uxConnected.Location = new System.Drawing.Point(7, 80);
            this.uxConnected.Name = "uxConnected";
            this.uxConnected.OffColour = System.Drawing.Color.Black;
            this.uxConnected.OnColour = System.Drawing.Color.Red;
            this.uxConnected.Size = new System.Drawing.Size(82, 20);
            this.uxConnected.TabIndex = 4;
            this.uxConnected.Values.Text = "Connected";
            // 
            // uxPortName
            // 
            this.uxPortName.Location = new System.Drawing.Point(7, 51);
            this.uxPortName.Name = "uxPortName";
            this.uxPortName.ReadOnly = true;
            this.uxPortName.Size = new System.Drawing.Size(83, 23);
            this.uxPortName.TabIndex = 3;
            // 
            // uxPorts
            // 
            this.uxPorts.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.uxPorts.DropDownWidth = 84;
            this.uxPorts.FormattingEnabled = true;
            this.uxPorts.IntegralHeight = false;
            this.uxPorts.Location = new System.Drawing.Point(6, 22);
            this.uxPorts.Name = "uxPorts";
            this.uxPorts.Size = new System.Drawing.Size(84, 21);
            this.uxPorts.TabIndex = 2;
            this.uxPorts.SelectedIndexChanged += new System.EventHandler(this.UxPorts_SelectedIndexChanged);
            // 
            // uxRefresh
            // 
            this.uxRefresh.Location = new System.Drawing.Point(96, 51);
            this.uxRefresh.Name = "uxRefresh";
            this.uxRefresh.Size = new System.Drawing.Size(75, 23);
            this.uxRefresh.TabIndex = 1;
            this.uxRefresh.Values.Text = "Refresh";
            this.uxRefresh.Click += new System.EventHandler(this.UxRefresh_Click);
            // 
            // uxConnect
            // 
            this.uxConnect.Location = new System.Drawing.Point(96, 22);
            this.uxConnect.Name = "uxConnect";
            this.uxConnect.Size = new System.Drawing.Size(75, 23);
            this.uxConnect.TabIndex = 0;
            this.uxConnect.Values.Text = "Connect";
            this.uxConnect.Click += new System.EventHandler(this.UxConnect_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.uxRI);
            this.groupBox2.Controls.Add(this.uxRLSD);
            this.groupBox2.Controls.Add(this.uxCTS);
            this.groupBox2.Controls.Add(this.uxDSR);
            this.groupBox2.Controls.Add(this.uxRefreshModem);
            this.groupBox2.Location = new System.Drawing.Point(0, 118);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(106, 171);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Modem Signals";
            // 
            // uxRI
            // 
            this.uxRI.AutoCheck = false;
            this.uxRI.Location = new System.Drawing.Point(7, 80);
            this.uxRI.Name = "uxRI";
            this.uxRI.OffColour = System.Drawing.Color.Empty;
            this.uxRI.OnColour = System.Drawing.Color.Empty;
            this.uxRI.Size = new System.Drawing.Size(34, 20);
            this.uxRI.TabIndex = 4;
            this.uxRI.Values.Text = "RI";
            // 
            // uxRLSD
            // 
            this.uxRLSD.AutoCheck = false;
            this.uxRLSD.Location = new System.Drawing.Point(7, 61);
            this.uxRLSD.Name = "uxRLSD";
            this.uxRLSD.OffColour = System.Drawing.Color.Empty;
            this.uxRLSD.OnColour = System.Drawing.Color.Empty;
            this.uxRLSD.Size = new System.Drawing.Size(52, 20);
            this.uxRLSD.TabIndex = 3;
            this.uxRLSD.Values.Text = "RLSD";
            // 
            // uxCTS
            // 
            this.uxCTS.AutoCheck = false;
            this.uxCTS.Location = new System.Drawing.Point(7, 42);
            this.uxCTS.Name = "uxCTS";
            this.uxCTS.OffColour = System.Drawing.Color.Empty;
            this.uxCTS.OnColour = System.Drawing.Color.Empty;
            this.uxCTS.Size = new System.Drawing.Size(44, 20);
            this.uxCTS.TabIndex = 2;
            this.uxCTS.Values.Text = "CTS";
            // 
            // uxDSR
            // 
            this.uxDSR.AutoCheck = false;
            this.uxDSR.Location = new System.Drawing.Point(7, 23);
            this.uxDSR.Name = "uxDSR";
            this.uxDSR.OffColour = System.Drawing.Color.Empty;
            this.uxDSR.OnColour = System.Drawing.Color.Empty;
            this.uxDSR.Size = new System.Drawing.Size(46, 20);
            this.uxDSR.TabIndex = 1;
            this.uxDSR.Values.Text = "DSR";
            // 
            // uxRefreshModem
            // 
            this.uxRefreshModem.Location = new System.Drawing.Point(6, 105);
            this.uxRefreshModem.Name = "uxRefreshModem";
            this.uxRefreshModem.Size = new System.Drawing.Size(75, 23);
            this.uxRefreshModem.TabIndex = 0;
            this.uxRefreshModem.Values.Text = "Refresh";
            this.uxRefreshModem.Click += new System.EventHandler(this.UxRefreshModem_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.uxReset);
            this.groupBox3.Controls.Add(this.uxBreak);
            this.groupBox3.Controls.Add(this.uxXonXoff);
            this.groupBox3.Controls.Add(this.uxRTS);
            this.groupBox3.Controls.Add(this.uxDTR);
            this.groupBox3.Location = new System.Drawing.Point(112, 118);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(94, 171);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Handshakes";
            // 
            // uxReset
            // 
            this.uxReset.Location = new System.Drawing.Point(7, 135);
            this.uxReset.Name = "uxReset";
            this.uxReset.Size = new System.Drawing.Size(75, 23);
            this.uxReset.TabIndex = 4;
            this.uxReset.Values.Text = "RESET";
            this.uxReset.Click += new System.EventHandler(this.UxReset_Click);
            // 
            // uxBreak
            // 
            this.uxBreak.Location = new System.Drawing.Point(7, 106);
            this.uxBreak.Name = "uxBreak";
            this.uxBreak.Size = new System.Drawing.Size(75, 23);
            this.uxBreak.TabIndex = 3;
            this.uxBreak.Values.Text = "BREAK";
            this.uxBreak.Click += new System.EventHandler(this.UxBreak_Click);
            // 
            // uxXonXoff
            // 
            this.uxXonXoff.Location = new System.Drawing.Point(7, 77);
            this.uxXonXoff.Name = "uxXonXoff";
            this.uxXonXoff.Size = new System.Drawing.Size(75, 23);
            this.uxXonXoff.TabIndex = 2;
            this.uxXonXoff.Values.Text = "XONXOFF";
            this.uxXonXoff.Click += new System.EventHandler(this.UxXonXoff_Click);
            // 
            // uxRTS
            // 
            this.uxRTS.Location = new System.Drawing.Point(7, 47);
            this.uxRTS.Name = "uxRTS";
            this.uxRTS.Size = new System.Drawing.Size(75, 23);
            this.uxRTS.TabIndex = 1;
            this.uxRTS.Values.Text = "RTS";
            this.uxRTS.Click += new System.EventHandler(this.UxRTS_Click);
            // 
            // uxDTR
            // 
            this.uxDTR.Location = new System.Drawing.Point(7, 18);
            this.uxDTR.Name = "uxDTR";
            this.uxDTR.Size = new System.Drawing.Size(75, 23);
            this.uxDTR.TabIndex = 0;
            this.uxDTR.Values.Text = "DTS";
            this.uxDTR.Click += new System.EventHandler(this.UxDTR_Click);
            // 
            // uxClear
            // 
            this.uxClear.Location = new System.Drawing.Point(6, 467);
            this.uxClear.Name = "uxClear";
            this.uxClear.Size = new System.Drawing.Size(75, 23);
            this.uxClear.TabIndex = 4;
            this.uxClear.Values.Text = "Clear";
            this.uxClear.Click += new System.EventHandler(this.UxClear_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.uxAbortOnError);
            this.groupBox4.Controls.Add(this.uxDiscardNull);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.uxErrorChars);
            this.groupBox4.Controls.Add(this.kryptonLabel7);
            this.groupBox4.Controls.Add(this.uxPackedValue);
            this.groupBox4.Controls.Add(this.kryptonLabel6);
            this.groupBox4.Controls.Add(this.uxEventChar);
            this.groupBox4.Controls.Add(this.kryptonLabel5);
            this.groupBox4.Controls.Add(this.uxEofChar);
            this.groupBox4.Controls.Add(this.kryptonLabel4);
            this.groupBox4.Controls.Add(this.uxErrorChar);
            this.groupBox4.Controls.Add(this.kryptonLabel3);
            this.groupBox4.Controls.Add(this.uxXoffChar);
            this.groupBox4.Controls.Add(this.kryptonLabel2);
            this.groupBox4.Controls.Add(this.uxXonChar);
            this.groupBox4.Controls.Add(this.kryptonLabel1);
            this.groupBox4.Controls.Add(this.uxRxFlowXoff);
            this.groupBox4.Controls.Add(this.uxRtsControl);
            this.groupBox4.Controls.Add(this.uxTxFlowXoff);
            this.groupBox4.Controls.Add(this.uxTxContinue);
            this.groupBox4.Controls.Add(this.uxRxDsrSense);
            this.groupBox4.Controls.Add(this.uxDtrControl);
            this.groupBox4.Controls.Add(this.uxTxFlowDsr);
            this.groupBox4.Controls.Add(this.uxTxFlowCts);
            this.groupBox4.Controls.Add(this.uxStop);
            this.groupBox4.Controls.Add(this.uxParity);
            this.groupBox4.Controls.Add(this.uxData);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.uxBaud);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupBox4.Location = new System.Drawing.Point(212, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(185, 563);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Settings";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(9, 510);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 20);
            this.label8.TabIndex = 9;
            this.label8.Values.Text = "RTS Control";
            // 
            // uxErrorChars
            // 
            this.uxErrorChars.Location = new System.Drawing.Point(94, 453);
            this.uxErrorChars.Name = "uxErrorChars";
            this.uxErrorChars.ReadOnly = true;
            this.uxErrorChars.Size = new System.Drawing.Size(72, 23);
            this.uxErrorChars.TabIndex = 40;
            this.uxToolTip.SetToolTip(this.uxErrorChars, "RX XON/XOFF Enabled");
            // 
            // kryptonLabel7
            // 
            this.kryptonLabel7.Location = new System.Drawing.Point(9, 453);
            this.kryptonLabel7.Name = "kryptonLabel7";
            this.kryptonLabel7.Size = new System.Drawing.Size(76, 20);
            this.kryptonLabel7.TabIndex = 39;
            this.kryptonLabel7.Values.Text = "Error Chars?";
            // 
            // uxPackedValue
            // 
            this.uxPackedValue.Location = new System.Drawing.Point(94, 247);
            this.uxPackedValue.Name = "uxPackedValue";
            this.uxPackedValue.ReadOnly = true;
            this.uxPackedValue.Size = new System.Drawing.Size(72, 23);
            this.uxPackedValue.TabIndex = 38;
            this.uxToolTip.SetToolTip(this.uxPackedValue, "If true, transmission is halted unless CTS is asserted");
            // 
            // kryptonLabel6
            // 
            this.kryptonLabel6.Location = new System.Drawing.Point(9, 250);
            this.kryptonLabel6.Name = "kryptonLabel6";
            this.kryptonLabel6.Size = new System.Drawing.Size(88, 20);
            this.kryptonLabel6.TabIndex = 37;
            this.kryptonLabel6.Values.Text = "Packed Values";
            // 
            // uxEventChar
            // 
            this.uxEventChar.Location = new System.Drawing.Point(94, 221);
            this.uxEventChar.Name = "uxEventChar";
            this.uxEventChar.ReadOnly = true;
            this.uxEventChar.Size = new System.Drawing.Size(72, 23);
            this.uxEventChar.TabIndex = 36;
            this.uxToolTip.SetToolTip(this.uxEventChar, "Number of stop bits");
            // 
            // kryptonLabel5
            // 
            this.kryptonLabel5.Location = new System.Drawing.Point(9, 224);
            this.kryptonLabel5.Name = "kryptonLabel5";
            this.kryptonLabel5.Size = new System.Drawing.Size(69, 20);
            this.kryptonLabel5.TabIndex = 35;
            this.kryptonLabel5.Values.Text = "Event Char";
            // 
            // uxEofChar
            // 
            this.uxEofChar.Location = new System.Drawing.Point(94, 194);
            this.uxEofChar.Name = "uxEofChar";
            this.uxEofChar.ReadOnly = true;
            this.uxEofChar.Size = new System.Drawing.Size(72, 23);
            this.uxEofChar.TabIndex = 34;
            this.uxToolTip.SetToolTip(this.uxEofChar, "Number of stop bits");
            // 
            // kryptonLabel4
            // 
            this.kryptonLabel4.Location = new System.Drawing.Point(9, 197);
            this.kryptonLabel4.Name = "kryptonLabel4";
            this.kryptonLabel4.Size = new System.Drawing.Size(61, 20);
            this.kryptonLabel4.TabIndex = 33;
            this.kryptonLabel4.Values.Text = "EOF Char";
            // 
            // uxErrorChar
            // 
            this.uxErrorChar.Location = new System.Drawing.Point(94, 168);
            this.uxErrorChar.Name = "uxErrorChar";
            this.uxErrorChar.ReadOnly = true;
            this.uxErrorChar.Size = new System.Drawing.Size(72, 23);
            this.uxErrorChar.TabIndex = 32;
            this.uxToolTip.SetToolTip(this.uxErrorChar, "Number of stop bits");
            // 
            // kryptonLabel3
            // 
            this.kryptonLabel3.Location = new System.Drawing.Point(9, 171);
            this.kryptonLabel3.Name = "kryptonLabel3";
            this.kryptonLabel3.Size = new System.Drawing.Size(65, 20);
            this.kryptonLabel3.TabIndex = 31;
            this.kryptonLabel3.Values.Text = "Error Char";
            // 
            // uxXoffChar
            // 
            this.uxXoffChar.Location = new System.Drawing.Point(94, 142);
            this.uxXoffChar.Name = "uxXoffChar";
            this.uxXoffChar.ReadOnly = true;
            this.uxXoffChar.Size = new System.Drawing.Size(72, 23);
            this.uxXoffChar.TabIndex = 30;
            this.uxToolTip.SetToolTip(this.uxXoffChar, "Number of stop bits");
            // 
            // kryptonLabel2
            // 
            this.kryptonLabel2.Location = new System.Drawing.Point(9, 145);
            this.kryptonLabel2.Name = "kryptonLabel2";
            this.kryptonLabel2.Size = new System.Drawing.Size(68, 20);
            this.kryptonLabel2.TabIndex = 29;
            this.kryptonLabel2.Values.Text = "XOFF Char";
            // 
            // uxXonChar
            // 
            this.uxXonChar.Location = new System.Drawing.Point(94, 116);
            this.uxXonChar.Name = "uxXonChar";
            this.uxXonChar.ReadOnly = true;
            this.uxXonChar.Size = new System.Drawing.Size(72, 23);
            this.uxXonChar.TabIndex = 28;
            this.uxToolTip.SetToolTip(this.uxXonChar, "Number of stop bits");
            // 
            // kryptonLabel1
            // 
            this.kryptonLabel1.Location = new System.Drawing.Point(9, 119);
            this.kryptonLabel1.Name = "kryptonLabel1";
            this.kryptonLabel1.Size = new System.Drawing.Size(65, 20);
            this.kryptonLabel1.TabIndex = 27;
            this.kryptonLabel1.Values.Text = "XON Char";
            // 
            // uxRxFlowXoff
            // 
            this.uxRxFlowXoff.Location = new System.Drawing.Point(94, 427);
            this.uxRxFlowXoff.Name = "uxRxFlowXoff";
            this.uxRxFlowXoff.ReadOnly = true;
            this.uxRxFlowXoff.Size = new System.Drawing.Size(72, 23);
            this.uxRxFlowXoff.TabIndex = 26;
            this.uxToolTip.SetToolTip(this.uxRxFlowXoff, "RX XON/XOFF Enabled");
            // 
            // uxRtsControl
            // 
            this.uxRtsControl.Location = new System.Drawing.Point(94, 507);
            this.uxRtsControl.Name = "uxRtsControl";
            this.uxRtsControl.ReadOnly = true;
            this.uxRtsControl.Size = new System.Drawing.Size(72, 23);
            this.uxRtsControl.TabIndex = 22;
            this.uxToolTip.SetToolTip(this.uxRtsControl, "RTS Control mode");
            // 
            // uxTxFlowXoff
            // 
            this.uxTxFlowXoff.Location = new System.Drawing.Point(94, 403);
            this.uxTxFlowXoff.Name = "uxTxFlowXoff";
            this.uxTxFlowXoff.ReadOnly = true;
            this.uxTxFlowXoff.Size = new System.Drawing.Size(72, 23);
            this.uxTxFlowXoff.TabIndex = 25;
            this.uxToolTip.SetToolTip(this.uxTxFlowXoff, "TX XON/XOFF Enabled");
            // 
            // uxTxContinue
            // 
            this.uxTxContinue.Location = new System.Drawing.Point(94, 377);
            this.uxTxContinue.Name = "uxTxContinue";
            this.uxTxContinue.ReadOnly = true;
            this.uxTxContinue.Size = new System.Drawing.Size(72, 23);
            this.uxTxContinue.TabIndex = 24;
            this.uxToolTip.SetToolTip(this.uxTxContinue, "When true, when XOFF, TX starts after any char");
            // 
            // uxRxDsrSense
            // 
            this.uxRxDsrSense.Location = new System.Drawing.Point(94, 352);
            this.uxRxDsrSense.Name = "uxRxDsrSense";
            this.uxRxDsrSense.ReadOnly = true;
            this.uxRxDsrSense.Size = new System.Drawing.Size(72, 23);
            this.uxRxDsrSense.TabIndex = 23;
            this.uxToolTip.SetToolTip(this.uxRxDsrSense, "If true, characters are ignored unless DSR is asserted");
            // 
            // uxDtrControl
            // 
            this.uxDtrControl.Location = new System.Drawing.Point(94, 325);
            this.uxDtrControl.Name = "uxDtrControl";
            this.uxDtrControl.ReadOnly = true;
            this.uxDtrControl.Size = new System.Drawing.Size(72, 23);
            this.uxDtrControl.TabIndex = 21;
            this.uxToolTip.SetToolTip(this.uxDtrControl, "DTR control mode");
            // 
            // uxTxFlowDsr
            // 
            this.uxTxFlowDsr.Location = new System.Drawing.Point(94, 299);
            this.uxTxFlowDsr.Name = "uxTxFlowDsr";
            this.uxTxFlowDsr.ReadOnly = true;
            this.uxTxFlowDsr.Size = new System.Drawing.Size(72, 23);
            this.uxTxFlowDsr.TabIndex = 20;
            this.uxToolTip.SetToolTip(this.uxTxFlowDsr, "If true, transmission is halted unless DSR is asserted");
            // 
            // uxTxFlowCts
            // 
            this.uxTxFlowCts.Location = new System.Drawing.Point(94, 273);
            this.uxTxFlowCts.Name = "uxTxFlowCts";
            this.uxTxFlowCts.ReadOnly = true;
            this.uxTxFlowCts.Size = new System.Drawing.Size(72, 23);
            this.uxTxFlowCts.TabIndex = 19;
            this.uxToolTip.SetToolTip(this.uxTxFlowCts, "If true, transmission is halted unless CTS is asserted");
            // 
            // uxStop
            // 
            this.uxStop.Location = new System.Drawing.Point(94, 90);
            this.uxStop.Name = "uxStop";
            this.uxStop.ReadOnly = true;
            this.uxStop.Size = new System.Drawing.Size(72, 23);
            this.uxStop.TabIndex = 18;
            this.uxToolTip.SetToolTip(this.uxStop, "Number of stop bits");
            // 
            // uxParity
            // 
            this.uxParity.Location = new System.Drawing.Point(94, 66);
            this.uxParity.Name = "uxParity";
            this.uxParity.ReadOnly = true;
            this.uxParity.Size = new System.Drawing.Size(72, 23);
            this.uxParity.TabIndex = 17;
            this.uxToolTip.SetToolTip(this.uxParity, "Parity");
            // 
            // uxData
            // 
            this.uxData.Location = new System.Drawing.Point(94, 42);
            this.uxData.Name = "uxData";
            this.uxData.ReadOnly = true;
            this.uxData.Size = new System.Drawing.Size(72, 23);
            this.uxData.TabIndex = 16;
            this.uxToolTip.SetToolTip(this.uxData, "Number of databits");
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(9, 427);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 20);
            this.label12.TabIndex = 13;
            this.label12.Values.Text = "RX Flow Xoff";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(9, 406);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(79, 20);
            this.label11.TabIndex = 12;
            this.label11.Values.Text = "TX Flow Xoff";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(9, 380);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 20);
            this.label10.TabIndex = 11;
            this.label10.Values.Text = "TX Continue";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(9, 355);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(86, 20);
            this.label9.TabIndex = 10;
            this.label9.Values.Text = "RX DSR Sense";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(9, 328);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 20);
            this.label7.TabIndex = 8;
            this.label7.Values.Text = "DTR Control";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(9, 302);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 20);
            this.label6.TabIndex = 7;
            this.label6.Values.Text = "TX Flow DSR";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(9, 276);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 20);
            this.label5.TabIndex = 6;
            this.label5.Values.Text = "TX Flow CTS";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(9, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 20);
            this.label4.TabIndex = 5;
            this.label4.Values.Text = "Stop Bits";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(9, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 20);
            this.label3.TabIndex = 4;
            this.label3.Values.Text = "Parity";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(9, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 20);
            this.label2.TabIndex = 3;
            this.label2.Values.Text = "Data Bits";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 20);
            this.label1.TabIndex = 2;
            this.label1.Values.Text = "Baud";
            // 
            // uxBaud
            // 
            this.uxBaud.Location = new System.Drawing.Point(94, 18);
            this.uxBaud.Name = "uxBaud";
            this.uxBaud.ReadOnly = true;
            this.uxBaud.Size = new System.Drawing.Size(72, 23);
            this.uxBaud.TabIndex = 1;
            this.uxToolTip.SetToolTip(this.uxBaud, "Baudrate in use");
            // 
            // uxRefresh3
            // 
            this.uxRefresh3.Location = new System.Drawing.Point(212, 599);
            this.uxRefresh3.Name = "uxRefresh3";
            this.uxRefresh3.Size = new System.Drawing.Size(75, 23);
            this.uxRefresh3.TabIndex = 0;
            this.uxRefresh3.Values.Text = "Refresh";
            this.uxRefresh3.Click += new System.EventHandler(this.UxRefresh3_Click);
            // 
            // uxTxXOff
            // 
            this.uxTxXOff.AutoSize = true;
            this.uxTxXOff.Location = new System.Drawing.Point(756, 180);
            this.uxTxXOff.Name = "uxTxXOff";
            this.uxTxXOff.Size = new System.Drawing.Size(66, 19);
            this.uxTxXOff.TabIndex = 18;
            this.uxTxXOff.Text = "TX XOff";
            this.uxToolTip.SetToolTip(this.uxTxXOff, "TX Continue on XOff");
            this.uxTxXOff.UseVisualStyleBackColor = true;
            this.uxTxXOff.CheckedChanged += new System.EventHandler(this.uxTxXOff_CheckedChanged);
            // 
            // uxTxXon
            // 
            this.uxTxXon.AutoSize = true;
            this.uxTxXon.Location = new System.Drawing.Point(756, 199);
            this.uxTxXon.Name = "uxTxXon";
            this.uxTxXon.Size = new System.Drawing.Size(94, 19);
            this.uxTxXon.TabIndex = 19;
            this.uxTxXon.Text = "TX XOn/XOff";
            this.uxToolTip.SetToolTip(this.uxTxXon, "Use XON/XOFF on transmission");
            this.uxTxXon.UseVisualStyleBackColor = true;
            this.uxTxXon.CheckedChanged += new System.EventHandler(this.uxTxXon_CheckedChanged);
            // 
            // uxRxXon
            // 
            this.uxRxXon.AutoSize = true;
            this.uxRxXon.Location = new System.Drawing.Point(756, 219);
            this.uxRxXon.Name = "uxRxXon";
            this.uxRxXon.Size = new System.Drawing.Size(95, 19);
            this.uxRxXon.TabIndex = 20;
            this.uxRxXon.Text = "RX XOn/XOff";
            this.uxToolTip.SetToolTip(this.uxRxXon, "RX Continue on XOff");
            this.uxRxXon.UseVisualStyleBackColor = true;
            this.uxRxXon.CheckedChanged += new System.EventHandler(this.uxRxXon_CheckedChanged);
            // 
            // uxParityReplace
            // 
            this.uxParityReplace.AutoSize = true;
            this.uxParityReplace.Location = new System.Drawing.Point(756, 240);
            this.uxParityReplace.Name = "uxParityReplace";
            this.uxParityReplace.Size = new System.Drawing.Size(100, 19);
            this.uxParityReplace.TabIndex = 21;
            this.uxParityReplace.Text = "Parity Replace";
            this.uxToolTip.SetToolTip(this.uxParityReplace, " Parity errors are replaced with the error character");
            this.uxParityReplace.UseVisualStyleBackColor = true;
            this.uxParityReplace.CheckedChanged += new System.EventHandler(this.uxParityReplace_CheckedChanged);
            // 
            // uxDiscardNull
            // 
            this.uxDiscardNull.AutoSize = true;
            this.uxDiscardNull.Location = new System.Drawing.Point(71, 479);
            this.uxDiscardNull.Name = "uxDiscardNull";
            this.uxDiscardNull.Size = new System.Drawing.Size(93, 17);
            this.uxDiscardNull.TabIndex = 22;
            this.uxDiscardNull.Text = "Discard Nulls";
            this.uxToolTip.SetToolTip(this.uxDiscardNull, "Discard Null characters");
            this.uxDiscardNull.UseVisualStyleBackColor = true;
            this.uxDiscardNull.CheckedChanged += new System.EventHandler(this.uxDiscardNull_CheckedChanged);
            // 
            // uxAbortOnError
            // 
            this.uxAbortOnError.AutoSize = true;
            this.uxAbortOnError.Location = new System.Drawing.Point(61, 536);
            this.uxAbortOnError.Name = "uxAbortOnError";
            this.uxAbortOnError.Size = new System.Drawing.Size(102, 17);
            this.uxAbortOnError.TabIndex = 23;
            this.uxAbortOnError.Text = "Abort On Error";
            this.uxToolTip.SetToolTip(this.uxAbortOnError, "Abort on errors");
            this.uxAbortOnError.UseVisualStyleBackColor = true;
            this.uxAbortOnError.CheckedChanged += new System.EventHandler(this.uxAbortOnError_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.uxBreakDet);
            this.groupBox5.Controls.Add(this.uxFraming);
            this.groupBox5.Controls.Add(this.uxParityErr);
            this.groupBox5.Controls.Add(this.uxOverrun);
            this.groupBox5.Controls.Add(this.uxOverflow);
            this.groupBox5.Controls.Add(this.uxTxQueue);
            this.groupBox5.Controls.Add(this.uxRxQueue);
            this.groupBox5.Controls.Add(this.uxTxIm);
            this.groupBox5.Controls.Add(this.uxEof);
            this.groupBox5.Controls.Add(this.uxXoffSent);
            this.groupBox5.Controls.Add(this.uxXoffHold);
            this.groupBox5.Controls.Add(this.uxRlsdHold);
            this.groupBox5.Controls.Add(this.uxDsrHold);
            this.groupBox5.Controls.Add(this.uxCtsHold);
            this.groupBox5.Location = new System.Drawing.Point(403, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(347, 193);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Status";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(160, 158);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(64, 20);
            this.label14.TabIndex = 15;
            this.label14.Values.Text = "TX Queue";
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(160, 136);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 20);
            this.label13.TabIndex = 14;
            this.label13.Values.Text = "RX Queue";
            // 
            // uxBreakDet
            // 
            this.uxBreakDet.AutoCheck = false;
            this.uxBreakDet.Location = new System.Drawing.Point(160, 113);
            this.uxBreakDet.Name = "uxBreakDet";
            this.uxBreakDet.OffColour = System.Drawing.Color.Empty;
            this.uxBreakDet.OnColour = System.Drawing.Color.Empty;
            this.uxBreakDet.Size = new System.Drawing.Size(54, 20);
            this.uxBreakDet.TabIndex = 13;
            this.uxBreakDet.Values.Text = "Break";
            // 
            // uxFraming
            // 
            this.uxFraming.AutoCheck = false;
            this.uxFraming.Location = new System.Drawing.Point(160, 91);
            this.uxFraming.Name = "uxFraming";
            this.uxFraming.OffColour = System.Drawing.Color.Empty;
            this.uxFraming.OnColour = System.Drawing.Color.Empty;
            this.uxFraming.Size = new System.Drawing.Size(98, 20);
            this.uxFraming.TabIndex = 12;
            this.uxFraming.Values.Text = "Framing Error";
            // 
            // uxParityErr
            // 
            this.uxParityErr.AutoCheck = false;
            this.uxParityErr.Location = new System.Drawing.Point(160, 69);
            this.uxParityErr.Name = "uxParityErr";
            this.uxParityErr.OffColour = System.Drawing.Color.Empty;
            this.uxParityErr.OnColour = System.Drawing.Color.Empty;
            this.uxParityErr.Size = new System.Drawing.Size(84, 20);
            this.uxParityErr.TabIndex = 11;
            this.uxParityErr.Values.Text = "Parity Error";
            // 
            // uxOverrun
            // 
            this.uxOverrun.AutoCheck = false;
            this.uxOverrun.Location = new System.Drawing.Point(160, 47);
            this.uxOverrun.Name = "uxOverrun";
            this.uxOverrun.OffColour = System.Drawing.Color.Empty;
            this.uxOverrun.OnColour = System.Drawing.Color.Empty;
            this.uxOverrun.Size = new System.Drawing.Size(68, 20);
            this.uxOverrun.TabIndex = 10;
            this.uxOverrun.Values.Text = "Overrun";
            // 
            // uxOverflow
            // 
            this.uxOverflow.AutoCheck = false;
            this.uxOverflow.Location = new System.Drawing.Point(160, 23);
            this.uxOverflow.Name = "uxOverflow";
            this.uxOverflow.OffColour = System.Drawing.Color.Empty;
            this.uxOverflow.OnColour = System.Drawing.Color.Empty;
            this.uxOverflow.Size = new System.Drawing.Size(73, 20);
            this.uxOverflow.TabIndex = 9;
            this.uxOverflow.Values.Text = "Overflow";
            // 
            // uxTxQueue
            // 
            this.uxTxQueue.Location = new System.Drawing.Point(225, 155);
            this.uxTxQueue.Name = "uxTxQueue";
            this.uxTxQueue.ReadOnly = true;
            this.uxTxQueue.Size = new System.Drawing.Size(75, 23);
            this.uxTxQueue.TabIndex = 8;
            // 
            // uxRxQueue
            // 
            this.uxRxQueue.Location = new System.Drawing.Point(225, 133);
            this.uxRxQueue.Name = "uxRxQueue";
            this.uxRxQueue.ReadOnly = true;
            this.uxRxQueue.Size = new System.Drawing.Size(75, 23);
            this.uxRxQueue.TabIndex = 7;
            // 
            // uxTxIm
            // 
            this.uxTxIm.AutoCheck = false;
            this.uxTxIm.Location = new System.Drawing.Point(7, 157);
            this.uxTxIm.Name = "uxTxIm";
            this.uxTxIm.OffColour = System.Drawing.Color.Empty;
            this.uxTxIm.OnColour = System.Drawing.Color.Empty;
            this.uxTxIm.Size = new System.Drawing.Size(68, 20);
            this.uxTxIm.TabIndex = 6;
            this.uxTxIm.Values.Text = "TX Imm.";
            // 
            // uxEof
            // 
            this.uxEof.AutoCheck = false;
            this.uxEof.Location = new System.Drawing.Point(7, 135);
            this.uxEof.Name = "uxEof";
            this.uxEof.OffColour = System.Drawing.Color.Empty;
            this.uxEof.OnColour = System.Drawing.Color.Empty;
            this.uxEof.Size = new System.Drawing.Size(45, 20);
            this.uxEof.TabIndex = 5;
            this.uxEof.Values.Text = "EOF";
            // 
            // uxXoffSent
            // 
            this.uxXoffSent.AutoCheck = false;
            this.uxXoffSent.Location = new System.Drawing.Point(7, 113);
            this.uxXoffSent.Name = "uxXoffSent";
            this.uxXoffSent.OffColour = System.Drawing.Color.Empty;
            this.uxXoffSent.OnColour = System.Drawing.Color.Empty;
            this.uxXoffSent.Size = new System.Drawing.Size(80, 20);
            this.uxXoffSent.TabIndex = 4;
            this.uxXoffSent.Values.Text = "XOFF Sent";
            // 
            // uxXoffHold
            // 
            this.uxXoffHold.AutoCheck = false;
            this.uxXoffHold.Location = new System.Drawing.Point(7, 91);
            this.uxXoffHold.Name = "uxXoffHold";
            this.uxXoffHold.OffColour = System.Drawing.Color.Empty;
            this.uxXoffHold.OnColour = System.Drawing.Color.Empty;
            this.uxXoffHold.Size = new System.Drawing.Size(82, 20);
            this.uxXoffHold.TabIndex = 3;
            this.uxXoffHold.Values.Text = "XOFF Hold";
            // 
            // uxRlsdHold
            // 
            this.uxRlsdHold.AutoCheck = false;
            this.uxRlsdHold.Location = new System.Drawing.Point(7, 69);
            this.uxRlsdHold.Name = "uxRlsdHold";
            this.uxRlsdHold.OffColour = System.Drawing.Color.Empty;
            this.uxRlsdHold.OnColour = System.Drawing.Color.Empty;
            this.uxRlsdHold.Size = new System.Drawing.Size(82, 20);
            this.uxRlsdHold.TabIndex = 2;
            this.uxRlsdHold.Values.Text = "RLSD Hold";
            // 
            // uxDsrHold
            // 
            this.uxDsrHold.AutoCheck = false;
            this.uxDsrHold.Location = new System.Drawing.Point(7, 47);
            this.uxDsrHold.Name = "uxDsrHold";
            this.uxDsrHold.OffColour = System.Drawing.Color.Empty;
            this.uxDsrHold.OnColour = System.Drawing.Color.Empty;
            this.uxDsrHold.Size = new System.Drawing.Size(76, 20);
            this.uxDsrHold.TabIndex = 1;
            this.uxDsrHold.Values.Text = "DSR Hold";
            // 
            // uxCtsHold
            // 
            this.uxCtsHold.AutoCheck = false;
            this.uxCtsHold.Location = new System.Drawing.Point(7, 25);
            this.uxCtsHold.Name = "uxCtsHold";
            this.uxCtsHold.OffColour = System.Drawing.Color.Empty;
            this.uxCtsHold.OnColour = System.Drawing.Color.Empty;
            this.uxCtsHold.Size = new System.Drawing.Size(74, 20);
            this.uxCtsHold.TabIndex = 0;
            this.uxCtsHold.Values.Text = "CTS Hold";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.uxTxMultiplier);
            this.groupBox6.Controls.Add(this.uxRxMultiplier);
            this.groupBox6.Controls.Add(this.label19);
            this.groupBox6.Controls.Add(this.label18);
            this.groupBox6.Controls.Add(this.label17);
            this.groupBox6.Controls.Add(this.uxTxTimeout);
            this.groupBox6.Controls.Add(this.label16);
            this.groupBox6.Controls.Add(this.uxRxInterval);
            this.groupBox6.Controls.Add(this.label15);
            this.groupBox6.Controls.Add(this.uxRxTimeout);
            this.groupBox6.Location = new System.Drawing.Point(403, 198);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(347, 127);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Timeouts";
            // 
            // uxTxMultiplier
            // 
            this.uxTxMultiplier.Location = new System.Drawing.Point(261, 49);
            this.uxTxMultiplier.Name = "uxTxMultiplier";
            this.uxTxMultiplier.Size = new System.Drawing.Size(74, 23);
            this.uxTxMultiplier.TabIndex = 9;
            this.uxTxMultiplier.Leave += new System.EventHandler(this.uxTxMultiplier_Leave);
            // 
            // uxRxMultiplier
            // 
            this.uxRxMultiplier.Location = new System.Drawing.Point(94, 78);
            this.uxRxMultiplier.Name = "uxRxMultiplier";
            this.uxRxMultiplier.Size = new System.Drawing.Size(74, 23);
            this.uxRxMultiplier.TabIndex = 8;
            this.uxRxMultiplier.Leave += new System.EventHandler(this.uxRxMultiplier_Leave);
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(7, 81);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(81, 20);
            this.label19.TabIndex = 7;
            this.label19.Values.Text = "RX Multiplier";
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(174, 52);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(80, 20);
            this.label18.TabIndex = 6;
            this.label18.Values.Text = "TX Multiplier";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(174, 22);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(73, 20);
            this.label17.TabIndex = 5;
            this.label17.Values.Text = "TX Timeout";
            // 
            // uxTxTimeout
            // 
            this.uxTxTimeout.Location = new System.Drawing.Point(261, 19);
            this.uxTxTimeout.Name = "uxTxTimeout";
            this.uxTxTimeout.Size = new System.Drawing.Size(74, 23);
            this.uxTxTimeout.TabIndex = 4;
            this.uxTxTimeout.Leave += new System.EventHandler(this.UxTxTimeout_Leave);
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(7, 51);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(69, 20);
            this.label16.TabIndex = 3;
            this.label16.Values.Text = "RX Interval";
            // 
            // uxRxInterval
            // 
            this.uxRxInterval.Location = new System.Drawing.Point(94, 48);
            this.uxRxInterval.Name = "uxRxInterval";
            this.uxRxInterval.Size = new System.Drawing.Size(74, 23);
            this.uxRxInterval.TabIndex = 2;
            this.uxRxInterval.Leave += new System.EventHandler(this.UxRxInterval_Leave);
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(7, 23);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(74, 20);
            this.label15.TabIndex = 1;
            this.label15.Values.Text = "RX Timeout";
            // 
            // uxRxTimeout
            // 
            this.uxRxTimeout.Location = new System.Drawing.Point(94, 19);
            this.uxRxTimeout.Name = "uxRxTimeout";
            this.uxRxTimeout.Size = new System.Drawing.Size(74, 23);
            this.uxRxTimeout.TabIndex = 0;
            this.uxRxTimeout.Leave += new System.EventHandler(this.UxRxTimeout_Leave);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.uxDtrHandshake);
            this.groupBox7.Controls.Add(this.uxDtrEnabled);
            this.groupBox7.Controls.Add(this.uxDtrDisabled);
            this.groupBox7.Location = new System.Drawing.Point(403, 450);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(99, 89);
            this.groupBox7.TabIndex = 8;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "DTR Control";
            // 
            // uxDtrHandshake
            // 
            this.uxDtrHandshake.Location = new System.Drawing.Point(5, 62);
            this.uxDtrHandshake.Name = "uxDtrHandshake";
            this.uxDtrHandshake.Size = new System.Drawing.Size(83, 20);
            this.uxDtrHandshake.TabIndex = 2;
            this.uxDtrHandshake.Values.Text = "Handshake";
            this.uxDtrHandshake.CheckedChanged += new System.EventHandler(this.UxDtrHandshake_CheckedChanged);
            // 
            // uxDtrEnabled
            // 
            this.uxDtrEnabled.Location = new System.Drawing.Point(6, 39);
            this.uxDtrEnabled.Name = "uxDtrEnabled";
            this.uxDtrEnabled.Size = new System.Drawing.Size(66, 20);
            this.uxDtrEnabled.TabIndex = 1;
            this.uxDtrEnabled.Values.Text = "Enabled";
            this.uxDtrEnabled.CheckedChanged += new System.EventHandler(this.UxDtrEnabled_CheckedChanged);
            // 
            // uxDtrDisabled
            // 
            this.uxDtrDisabled.Location = new System.Drawing.Point(6, 15);
            this.uxDtrDisabled.Name = "uxDtrDisabled";
            this.uxDtrDisabled.Size = new System.Drawing.Size(70, 20);
            this.uxDtrDisabled.TabIndex = 0;
            this.uxDtrDisabled.Values.Text = "Disabled";
            this.uxDtrDisabled.CheckedChanged += new System.EventHandler(this.UxDtrDisabled_CheckedChanged);
            // 
            // uxTxBuffer
            // 
            this.uxTxBuffer.Location = new System.Drawing.Point(303, 599);
            this.uxTxBuffer.Name = "uxTxBuffer";
            this.uxTxBuffer.Size = new System.Drawing.Size(410, 23);
            this.uxTxBuffer.TabIndex = 9;
            // 
            // uxSend
            // 
            this.uxSend.Location = new System.Drawing.Point(719, 599);
            this.uxSend.Name = "uxSend";
            this.uxSend.Size = new System.Drawing.Size(75, 23);
            this.uxSend.TabIndex = 10;
            this.uxSend.Values.Text = "Send";
            this.uxSend.Click += new System.EventHandler(this.UxSend_Click);
            // 
            // uxBaudrate
            // 
            this.uxBaudrate.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.uxBaudrate.DropDownWidth = 75;
            this.uxBaudrate.IntegralHeight = false;
            this.uxBaudrate.Items.AddRange(new object[] {
            "128000",
            "115200",
            "57600",
            "38400",
            "19200",
            "14400",
            "9600",
            "7200",
            "4800",
            "1800",
            "1200",
            "600",
            "300",
            "110",
            "75"});
            this.uxBaudrate.Location = new System.Drawing.Point(306, 572);
            this.uxBaudrate.Name = "uxBaudrate";
            this.uxBaudrate.Size = new System.Drawing.Size(75, 21);
            this.uxBaudrate.TabIndex = 11;
            this.uxBaudrate.Text = "9600";
            this.uxBaudrate.SelectedIndexChanged += new System.EventHandler(this.UxBaudrate_SelectedIndexChanged);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.uxRtsHandshake);
            this.groupBox8.Controls.Add(this.uxRtsEnabled);
            this.groupBox8.Controls.Add(this.uxRtsDisabled);
            this.groupBox8.Location = new System.Drawing.Point(403, 355);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(99, 89);
            this.groupBox8.TabIndex = 9;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "RTS Control";
            // 
            // uxRtsHandshake
            // 
            this.uxRtsHandshake.Location = new System.Drawing.Point(5, 62);
            this.uxRtsHandshake.Name = "uxRtsHandshake";
            this.uxRtsHandshake.Size = new System.Drawing.Size(83, 20);
            this.uxRtsHandshake.TabIndex = 2;
            this.uxRtsHandshake.Values.Text = "Handshake";
            this.uxRtsHandshake.CheckedChanged += new System.EventHandler(this.UxRtsHandshake_CheckedChanged);
            // 
            // uxRtsEnabled
            // 
            this.uxRtsEnabled.Location = new System.Drawing.Point(6, 39);
            this.uxRtsEnabled.Name = "uxRtsEnabled";
            this.uxRtsEnabled.Size = new System.Drawing.Size(66, 20);
            this.uxRtsEnabled.TabIndex = 1;
            this.uxRtsEnabled.Values.Text = "Enabled";
            this.uxRtsEnabled.CheckedChanged += new System.EventHandler(this.UxRtsEnabled_CheckedChanged);
            // 
            // uxRtsDisabled
            // 
            this.uxRtsDisabled.Location = new System.Drawing.Point(6, 16);
            this.uxRtsDisabled.Name = "uxRtsDisabled";
            this.uxRtsDisabled.Size = new System.Drawing.Size(70, 20);
            this.uxRtsDisabled.TabIndex = 0;
            this.uxRtsDisabled.Values.Text = "Disabled";
            this.uxRtsDisabled.CheckedChanged += new System.EventHandler(this.UxRtsDisabled_CheckedChanged);
            // 
            // uxDsrEnable
            // 
            this.uxDsrEnable.AutoSize = true;
            this.uxDsrEnable.Location = new System.Drawing.Point(756, 37);
            this.uxDsrEnable.Name = "uxDsrEnable";
            this.uxDsrEnable.Size = new System.Drawing.Size(109, 19);
            this.uxDsrEnable.TabIndex = 12;
            this.uxDsrEnable.Text = "DSR Handshake";
            this.uxDsrEnable.UseVisualStyleBackColor = true;
            this.uxDsrEnable.CheckedChanged += new System.EventHandler(this.UxDsrEnable_CheckedChanged);
            // 
            // uxCtsEnable
            // 
            this.uxCtsEnable.AutoSize = true;
            this.uxCtsEnable.Location = new System.Drawing.Point(756, 12);
            this.uxCtsEnable.Name = "uxCtsEnable";
            this.uxCtsEnable.Size = new System.Drawing.Size(108, 19);
            this.uxCtsEnable.TabIndex = 13;
            this.uxCtsEnable.Text = "CTS Handshake";
            this.uxCtsEnable.UseVisualStyleBackColor = true;
            this.uxCtsEnable.CheckedChanged += new System.EventHandler(this.UxCtsEnable_CheckedChanged);
            // 
            // uxDataBits
            // 
            this.uxDataBits.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.uxDataBits.DropDownWidth = 75;
            this.uxDataBits.IntegralHeight = false;
            this.uxDataBits.Items.AddRange(new object[] {
            "8",
            "7",
            "6",
            "5"});
            this.uxDataBits.Location = new System.Drawing.Point(387, 572);
            this.uxDataBits.Name = "uxDataBits";
            this.uxDataBits.Size = new System.Drawing.Size(75, 21);
            this.uxDataBits.TabIndex = 14;
            this.uxDataBits.Text = "8";
            this.uxDataBits.SelectedIndexChanged += new System.EventHandler(this.uxDataBits_SelectedIndexChanged);
            // 
            // uxParityBits
            // 
            this.uxParityBits.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.uxParityBits.DropDownWidth = 75;
            this.uxParityBits.IntegralHeight = false;
            this.uxParityBits.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even",
            "Mark",
            "Space"});
            this.uxParityBits.Location = new System.Drawing.Point(468, 572);
            this.uxParityBits.Name = "uxParityBits";
            this.uxParityBits.Size = new System.Drawing.Size(75, 21);
            this.uxParityBits.TabIndex = 15;
            this.uxParityBits.Text = "None";
            this.uxParityBits.SelectedIndexChanged += new System.EventHandler(this.uxParityBits_SelectedIndexChanged);
            // 
            // uxStopBits
            // 
            this.uxStopBits.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.uxStopBits.DropDownWidth = 75;
            this.uxStopBits.IntegralHeight = false;
            this.uxStopBits.Items.AddRange(new object[] {
            "One",
            "Two",
            "OnePointFive"});
            this.uxStopBits.Location = new System.Drawing.Point(549, 570);
            this.uxStopBits.Name = "uxStopBits";
            this.uxStopBits.Size = new System.Drawing.Size(75, 21);
            this.uxStopBits.TabIndex = 16;
            this.uxStopBits.Text = "One";
            this.uxStopBits.SelectedIndexChanged += new System.EventHandler(this.uxStopBits_SelectedIndexChanged);
            // 
            // uxDsrSense
            // 
            this.uxDsrSense.AutoSize = true;
            this.uxDsrSense.Location = new System.Drawing.Point(756, 160);
            this.uxDsrSense.Name = "uxDsrSense";
            this.uxDsrSense.Size = new System.Drawing.Size(103, 19);
            this.uxDsrSense.TabIndex = 17;
            this.uxDsrSense.Text = "DSR Sensitivity";
            this.uxDsrSense.UseVisualStyleBackColor = true;
            this.uxDsrSense.CheckedChanged += new System.EventHandler(this.uxDsrSense_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 752);
            this.Controls.Add(this.uxParityReplace);
            this.Controls.Add(this.uxRxXon);
            this.Controls.Add(this.uxTxXon);
            this.Controls.Add(this.uxTxXOff);
            this.Controls.Add(this.uxDsrSense);
            this.Controls.Add(this.uxStopBits);
            this.Controls.Add(this.uxParityBits);
            this.Controls.Add(this.uxDataBits);
            this.Controls.Add(this.uxCtsEnable);
            this.Controls.Add(this.uxDsrEnable);
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.uxBaudrate);
            this.Controls.Add(this.uxSend);
            this.Controls.Add(this.uxTxBuffer);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.uxClear);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.uxLog);
            this.Controls.Add(this.uxRefresh3);
            this.Name = "MainForm";
            this.Text = "Serial Test App";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxPorts)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxBaudrate)).EndInit();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxDataBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uxParityBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uxStopBits)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Krypton.Toolkit.KryptonListBox uxLog;
        private System.Windows.Forms.GroupBox groupBox1;
        private Krypton.Toolkit.KryptonComboBox uxPorts;
        private Krypton.Toolkit.KryptonButton uxRefresh;
        private Krypton.Toolkit.KryptonButton uxConnect;
        private Krypton.Toolkit.KryptonTextBox uxPortName;
        private Controls.LedControl uxConnected;
        private System.Windows.Forms.GroupBox groupBox2;
        private Controls.LedControl uxRI;
        private Controls.LedControl uxRLSD;
        private Controls.LedControl uxCTS;
        private Controls.LedControl uxDSR;
        private Krypton.Toolkit.KryptonButton uxRefreshModem;
        private System.Windows.Forms.GroupBox groupBox3;
        private Krypton.Toolkit.KryptonButton uxRTS;
        private Krypton.Toolkit.KryptonButton uxDTR;
        private Krypton.Toolkit.KryptonButton uxReset;
        private Krypton.Toolkit.KryptonButton uxBreak;
        private Krypton.Toolkit.KryptonButton uxXonXoff;
        private Krypton.Toolkit.KryptonButton uxClear;
        private System.Windows.Forms.GroupBox groupBox4;
        private Krypton.Toolkit.KryptonTextBox uxBaud;
        private Krypton.Toolkit.KryptonButton uxRefresh3;
        private Krypton.Toolkit.KryptonLabel label5;
        private Krypton.Toolkit.KryptonLabel label4;
        private Krypton.Toolkit.KryptonLabel label3;
        private Krypton.Toolkit.KryptonLabel label2;
        private Krypton.Toolkit.KryptonLabel label1;
        private Krypton.Toolkit.KryptonLabel label12;
        private Krypton.Toolkit.KryptonLabel label11;
        private Krypton.Toolkit.KryptonLabel label10;
        private Krypton.Toolkit.KryptonLabel label9;
        private Krypton.Toolkit.KryptonLabel label8;
        private Krypton.Toolkit.KryptonLabel label7;
        private Krypton.Toolkit.KryptonLabel label6;
        private Krypton.Toolkit.KryptonTextBox uxRxFlowXoff;
        private Krypton.Toolkit.KryptonTextBox uxTxFlowXoff;
        private Krypton.Toolkit.KryptonTextBox uxTxContinue;
        private Krypton.Toolkit.KryptonTextBox uxRxDsrSense;
        private Krypton.Toolkit.KryptonTextBox uxRtsControl;
        private Krypton.Toolkit.KryptonTextBox uxDtrControl;
        private Krypton.Toolkit.KryptonTextBox uxTxFlowDsr;
        private Krypton.Toolkit.KryptonTextBox uxTxFlowCts;
        private Krypton.Toolkit.KryptonTextBox uxStop;
        private Krypton.Toolkit.KryptonTextBox uxParity;
        private Krypton.Toolkit.KryptonTextBox uxData;
        private System.Windows.Forms.ToolTip uxToolTip;
        private System.Windows.Forms.GroupBox groupBox5;
        private Controls.LedControl uxTxIm;
        private Controls.LedControl uxEof;
        private Controls.LedControl uxXoffSent;
        private Controls.LedControl uxXoffHold;
        private Controls.LedControl uxRlsdHold;
        private Controls.LedControl uxDsrHold;
        private Controls.LedControl uxCtsHold;
        private Krypton.Toolkit.KryptonLabel label14;
        private Krypton.Toolkit.KryptonLabel label13;
        private Controls.LedControl uxBreakDet;
        private Controls.LedControl uxFraming;
        private Controls.LedControl uxParityErr;
        private Controls.LedControl uxOverrun;
        private Controls.LedControl uxOverflow;
        private Krypton.Toolkit.KryptonTextBox uxTxQueue;
        private Krypton.Toolkit.KryptonTextBox uxRxQueue;
        private System.Windows.Forms.GroupBox groupBox6;
        private Krypton.Toolkit.KryptonLabel label17;
        private Krypton.Toolkit.KryptonTextBox uxTxTimeout;
        private Krypton.Toolkit.KryptonLabel label16;
        private Krypton.Toolkit.KryptonTextBox uxRxInterval;
        private Krypton.Toolkit.KryptonLabel label15;
        private Krypton.Toolkit.KryptonTextBox uxRxTimeout;
        private System.Windows.Forms.GroupBox groupBox7;
        private Krypton.Toolkit.KryptonRadioButton uxDtrHandshake;
        private Krypton.Toolkit.KryptonRadioButton uxDtrEnabled;
        private Krypton.Toolkit.KryptonRadioButton uxDtrDisabled;
        private Krypton.Toolkit.KryptonTextBox uxTxBuffer;
        private Krypton.Toolkit.KryptonButton uxSend;
        private Krypton.Toolkit.KryptonComboBox uxBaudrate;
        private System.Windows.Forms.GroupBox groupBox8;
        private Krypton.Toolkit.KryptonRadioButton uxRtsHandshake;
        private Krypton.Toolkit.KryptonRadioButton uxRtsEnabled;
        private Krypton.Toolkit.KryptonRadioButton uxRtsDisabled;
        private System.Windows.Forms.CheckBox uxDsrEnable;
        private System.Windows.Forms.CheckBox uxCtsEnable;
        private Krypton.Toolkit.KryptonTextBox uxTxMultiplier;
        private Krypton.Toolkit.KryptonTextBox uxRxMultiplier;
        private Krypton.Toolkit.KryptonLabel label19;
        private Krypton.Toolkit.KryptonLabel label18;
        private Krypton.Toolkit.KryptonComboBox uxDataBits;
        private Krypton.Toolkit.KryptonComboBox uxParityBits;
        private Krypton.Toolkit.KryptonComboBox uxStopBits;
        private System.Windows.Forms.CheckBox uxDsrSense;
        private System.Windows.Forms.CheckBox uxTxXOff;
        private System.Windows.Forms.CheckBox uxTxXon;
        private System.Windows.Forms.CheckBox uxRxXon;
        private System.Windows.Forms.CheckBox uxParityReplace;
        private System.Windows.Forms.CheckBox uxDiscardNull;
        private System.Windows.Forms.CheckBox uxAbortOnError;
        private Krypton.Toolkit.KryptonTextBox uxEventChar;
        private Krypton.Toolkit.KryptonLabel kryptonLabel5;
        private Krypton.Toolkit.KryptonTextBox uxEofChar;
        private Krypton.Toolkit.KryptonLabel kryptonLabel4;
        private Krypton.Toolkit.KryptonTextBox uxErrorChar;
        private Krypton.Toolkit.KryptonLabel kryptonLabel3;
        private Krypton.Toolkit.KryptonTextBox uxXoffChar;
        private Krypton.Toolkit.KryptonLabel kryptonLabel2;
        private Krypton.Toolkit.KryptonTextBox uxXonChar;
        private Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private Krypton.Toolkit.KryptonTextBox uxPackedValue;
        private Krypton.Toolkit.KryptonLabel kryptonLabel6;
        private Krypton.Toolkit.KryptonTextBox uxErrorChars;
        private Krypton.Toolkit.KryptonLabel kryptonLabel7;
    }
}

