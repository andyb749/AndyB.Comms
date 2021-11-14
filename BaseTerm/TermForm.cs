using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using AndyB.Comms.Comm;

namespace BaseTerm
{
	/// <summary>
	/// Summary description for Form.
	/// </summary>
	public class TermForm : System.Windows.Forms.Form
	{
		private bool inEscape = false;
		private string esc;
		private string mark;

		private System.Windows.Forms.TextBox textBoxS;
		private System.Windows.Forms.Button buttonO;
		private System.Windows.Forms.Button buttonStatus;
		private System.Windows.Forms.Button buttonSet;
		private System.Windows.Forms.RichTextBox textBoxR;
		private System.Windows.Forms.Button buttonClr;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButtonRLSD;
		private System.Windows.Forms.RadioButton radioButtonDSR;
		private System.Windows.Forms.RadioButton radioButtonCTS;
		private System.Windows.Forms.CheckBox checkBoxBK;
		private System.Windows.Forms.CheckBox checkBoxDTR;
		private System.Windows.Forms.CheckBox checkBoxRTS;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.RadioButton radioButtonRng;
		private System.Windows.Forms.Button buttonImm;
		private System.ComponentModel.IContainer components;

		public TermForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				BaseTerm.term.Close();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.textBoxS = new System.Windows.Forms.TextBox();
			this.buttonO = new System.Windows.Forms.Button();
			this.buttonStatus = new System.Windows.Forms.Button();
			this.buttonSet = new System.Windows.Forms.Button();
			this.textBoxR = new System.Windows.Forms.RichTextBox();
			this.buttonClr = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioButtonRng = new System.Windows.Forms.RadioButton();
			this.checkBoxBK = new System.Windows.Forms.CheckBox();
			this.checkBoxDTR = new System.Windows.Forms.CheckBox();
			this.checkBoxRTS = new System.Windows.Forms.CheckBox();
			this.radioButtonRLSD = new System.Windows.Forms.RadioButton();
			this.radioButtonDSR = new System.Windows.Forms.RadioButton();
			this.radioButtonCTS = new System.Windows.Forms.RadioButton();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.buttonImm = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxS
			// 
			this.textBoxS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxS.BackColor = System.Drawing.Color.White;
			this.textBoxS.CausesValidation = false;
			this.textBoxS.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxS.Location = new System.Drawing.Point(8, 8);
			this.textBoxS.Name = "textBoxS";
			this.textBoxS.ReadOnly = true;
			this.textBoxS.Size = new System.Drawing.Size(328, 22);
			this.textBoxS.TabIndex = 0;
			this.textBoxS.TabStop = false;
			this.textBoxS.Text = "";
			this.toolTip.SetToolTip(this.textBoxS, "Type character to send, or type \'<\' then ASCII control name or number 0..255, the" +
				"n \'>\' to send. Type \'<\' twice for that character");
			// 
			// buttonO
			// 
			this.buttonO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonO.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonO.Location = new System.Drawing.Point(8, 224);
			this.buttonO.Name = "buttonO";
			this.buttonO.TabIndex = 3;
			this.buttonO.TabStop = false;
			this.buttonO.Tag = "0";
			this.buttonO.Text = "Offline";
			this.toolTip.SetToolTip(this.buttonO, "Press to change between Queued and Immediate transmission.");
			this.buttonO.Click += new System.EventHandler(this.buttonO_Click);
			// 
			// buttonStatus
			// 
			this.buttonStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonStatus.Enabled = false;
			this.buttonStatus.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonStatus.Location = new System.Drawing.Point(344, 224);
			this.buttonStatus.Name = "buttonStatus";
			this.buttonStatus.TabIndex = 11;
			this.buttonStatus.TabStop = false;
			this.buttonStatus.Text = "Status";
			this.buttonStatus.Click += new System.EventHandler(this.buttonStatus_Click);
			// 
			// buttonSet
			// 
			this.buttonSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonSet.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonSet.Location = new System.Drawing.Point(8, 192);
			this.buttonSet.Name = "buttonSet";
			this.buttonSet.TabIndex = 12;
			this.buttonSet.TabStop = false;
			this.buttonSet.Text = "Settings";
			this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
			// 
			// textBoxR
			// 
			this.textBoxR.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxR.BackColor = System.Drawing.Color.LightGray;
			this.textBoxR.DetectUrls = false;
			this.textBoxR.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxR.HideSelection = false;
			this.textBoxR.Location = new System.Drawing.Point(8, 38);
			this.textBoxR.Name = "textBoxR";
			this.textBoxR.ReadOnly = true;
			this.textBoxR.ShowSelectionMargin = true;
			this.textBoxR.Size = new System.Drawing.Size(410, 144);
			this.textBoxR.TabIndex = 13;
			this.textBoxR.Text = "";
			// 
			// buttonClr
			// 
			this.buttonClr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonClr.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonClr.Location = new System.Drawing.Point(344, 192);
			this.buttonClr.Name = "buttonClr";
			this.buttonClr.TabIndex = 14;
			this.buttonClr.TabStop = false;
			this.buttonClr.Text = "Clear";
			this.buttonClr.Click += new System.EventHandler(this.buttonClr_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.radioButtonRng);
			this.groupBox1.Controls.Add(this.checkBoxBK);
			this.groupBox1.Controls.Add(this.checkBoxDTR);
			this.groupBox1.Controls.Add(this.checkBoxRTS);
			this.groupBox1.Controls.Add(this.radioButtonRLSD);
			this.groupBox1.Controls.Add(this.radioButtonDSR);
			this.groupBox1.Controls.Add(this.radioButtonCTS);
			this.groupBox1.Location = new System.Drawing.Point(90, 187);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(248, 61);
			this.groupBox1.TabIndex = 15;
			this.groupBox1.TabStop = false;
			// 
			// radioButtonRng
			// 
			this.radioButtonRng.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.radioButtonRng.AutoCheck = false;
			this.radioButtonRng.Enabled = false;
			this.radioButtonRng.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButtonRng.Location = new System.Drawing.Point(184, 13);
			this.radioButtonRng.Name = "radioButtonRng";
			this.radioButtonRng.Size = new System.Drawing.Size(56, 19);
			this.radioButtonRng.TabIndex = 15;
			this.radioButtonRng.Text = "Ring";
			// 
			// checkBoxBK
			// 
			this.checkBoxBK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxBK.Enabled = false;
			this.checkBoxBK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxBK.Location = new System.Drawing.Point(160, 37);
			this.checkBoxBK.Name = "checkBoxBK";
			this.checkBoxBK.Size = new System.Drawing.Size(56, 16);
			this.checkBoxBK.TabIndex = 14;
			this.checkBoxBK.Tag = "2";
			this.checkBoxBK.Text = "Break";
			this.checkBoxBK.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// checkBoxDTR
			// 
			this.checkBoxDTR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxDTR.Enabled = false;
			this.checkBoxDTR.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxDTR.Location = new System.Drawing.Point(96, 37);
			this.checkBoxDTR.Name = "checkBoxDTR";
			this.checkBoxDTR.Size = new System.Drawing.Size(56, 16);
			this.checkBoxDTR.TabIndex = 13;
			this.checkBoxDTR.Tag = "1";
			this.checkBoxDTR.Text = "DTR";
			this.checkBoxDTR.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// checkBoxRTS
			// 
			this.checkBoxRTS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxRTS.Enabled = false;
			this.checkBoxRTS.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxRTS.Location = new System.Drawing.Point(32, 37);
			this.checkBoxRTS.Name = "checkBoxRTS";
			this.checkBoxRTS.Size = new System.Drawing.Size(56, 16);
			this.checkBoxRTS.TabIndex = 12;
			this.checkBoxRTS.Tag = "0";
			this.checkBoxRTS.Text = "RTS";
			this.checkBoxRTS.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// radioButtonRLSD
			// 
			this.radioButtonRLSD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.radioButtonRLSD.AutoCheck = false;
			this.radioButtonRLSD.Enabled = false;
			this.radioButtonRLSD.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButtonRLSD.Location = new System.Drawing.Point(123, 13);
			this.radioButtonRLSD.Name = "radioButtonRLSD";
			this.radioButtonRLSD.Size = new System.Drawing.Size(56, 19);
			this.radioButtonRLSD.TabIndex = 10;
			this.radioButtonRLSD.Text = "RLSD";
			// 
			// radioButtonDSR
			// 
			this.radioButtonDSR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.radioButtonDSR.AutoCheck = false;
			this.radioButtonDSR.Enabled = false;
			this.radioButtonDSR.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButtonDSR.Location = new System.Drawing.Point(67, 13);
			this.radioButtonDSR.Name = "radioButtonDSR";
			this.radioButtonDSR.Size = new System.Drawing.Size(56, 19);
			this.radioButtonDSR.TabIndex = 9;
			this.radioButtonDSR.Text = "DSR";
			// 
			// radioButtonCTS
			// 
			this.radioButtonCTS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.radioButtonCTS.AutoCheck = false;
			this.radioButtonCTS.Enabled = false;
			this.radioButtonCTS.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButtonCTS.Location = new System.Drawing.Point(11, 13);
			this.radioButtonCTS.Name = "radioButtonCTS";
			this.radioButtonCTS.Size = new System.Drawing.Size(56, 19);
			this.radioButtonCTS.TabIndex = 8;
			this.radioButtonCTS.Text = "CTS";
			// 
			// buttonImm
			// 
			this.buttonImm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonImm.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonImm.Location = new System.Drawing.Point(344, 8);
			this.buttonImm.Name = "buttonImm";
			this.buttonImm.TabIndex = 16;
			this.buttonImm.Tag = "0";
			this.buttonImm.Text = "Queued";
			this.buttonImm.Click += new System.EventHandler(this.buttonImm_Click);
			// 
			// TermForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(426, 254);
			this.Controls.Add(this.buttonImm);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonClr);
			this.Controls.Add(this.textBoxR);
			this.Controls.Add(this.buttonSet);
			this.Controls.Add(this.buttonStatus);
			this.Controls.Add(this.buttonO);
			this.Controls.Add(this.textBoxS);
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(434, 180);
			this.Name = "TermForm";
			this.Text = "CommBase Terminal";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_KeyDown);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form_KeyPress);
			this.Load += new System.EventHandler(this.BaseTermForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void Form_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			byte c;
			c = (byte)e.KeyChar;
			if ((c == 13) && (!inEscape))
			{
				textBoxS.AppendText("<CR>");
				BaseTerm.term.SendCtrl("CR");
				return;
			}
			if ((c < 0x20) || (c > 0x7E)) return;
			if (inEscape)
			{
				if (c == (byte)'<')
				{
					BaseTerm.term.SendChar(c);
					textBoxS.Text = mark;
					inEscape = false;
					textBoxS.BackColor = Color.White;
				}
				else
				{
					if (c == (byte)'>')
					{
						if (!BaseTerm.term.SendCtrl(esc))
						{
							textBoxS.Text = mark;
							c = 0;
						}
						inEscape = false;
						textBoxS.BackColor = Color.White;
					}
					else
					{
						esc += ((char)c).ToString();
					}
				}
			}
			else
			{
				if (c == (byte)'<')
				{
					inEscape = true;
					esc = "";
					mark = textBoxS.Text;
					textBoxS.BackColor = Color.Yellow;
				}
				else
				{
					BaseTerm.term.SendChar(c);
				}
			}
			if (c >= (byte)' ') textBoxS.AppendText(e.KeyChar.ToString());
			e.Handled = true;
		}

		private void buttonO_Click(object sender, System.EventArgs e)
		{
			if (buttonO.Tag.ToString() == "1")
			{
				BaseTerm.term.Close();
				buttonO.Tag = "0";
				buttonO.Text = "Offline";
			}
			else
			{
				try
				{
					if (BaseTerm.term.Open())
					{
						ShowMsg(">>>> ONLINE");
						buttonO.Tag = "1";
						buttonO.Text = "Online";
					}
					else
					{
						ShowMsg(">>>> PORT UNAVAILABLE");
					}
				}
				catch (CommPortException ex)
				{
					ShowException(ex);
				}
			}
			textBoxS.Focus();
		}

		public void ShowChar(string s, bool nl)
		{
			if (textBoxR.InvokeRequired)
            {
				Action safe = delegate { ShowChar(s, nl); };
				textBoxR.Invoke(safe);
            }
            else
            {
				textBoxR.AppendText(s);
				if (nl) textBoxR.AppendText("\r\n");
			}
		}

		public void ShowMsg(string s)
		{
			if (textBoxR.InvokeRequired)
            {
				Action safe = delegate { ShowMsg(s); };
				textBoxR.Invoke(safe);
            }
            else
            {
				Color c = textBoxR.SelectionColor;
				textBoxR.SelectionColor = Color.Green;
				textBoxR.AppendText("\r\n" + s + "\r\n");
				textBoxR.SelectionColor = c;
			}
		}

		public void ShowException(CommPortException e)
		{
			Color c = textBoxR.SelectionColor;
			textBoxR.SelectionColor = Color.Red;
			textBoxR.AppendText("\r\n>>>> EXCEPTION\r\n");
			textBoxR.SelectionColor = Color.Red;
			textBoxR.AppendText(e.Message);
			if (e.InnerException != null)
			{
				textBoxR.AppendText("\r\n");
				textBoxR.SelectionColor = Color.Red;
				textBoxR.AppendText(e.InnerException.Message);
			}
			textBoxR.SelectionColor = Color.Red;
			textBoxR.AppendText("\r\n>>>> END EXCEPTION\r\n");
			textBoxR.SelectionColor = c;
		}

		public void SetIndics(bool CTS, bool DSR, bool RLSD, bool Rng)
		{
			if (radioButtonCTS.InvokeRequired)
			{
				Action safe = delegate { SetIndics(CTS, DSR, RLSD, Rng); };
				radioButtonCTS.Invoke(safe);
			}
			else
			{
				radioButtonCTS.Checked = CTS;
				radioButtonDSR.Checked = DSR;
				radioButtonRLSD.Checked = RLSD;
				radioButtonRng.Checked = Rng;
			}
		}

		private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Back)
			{
				textBoxS.Text = "";
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Delete)
			{
				textBoxS.Text = "";
				textBoxR.Text = "";
				e.Handled = true;
			}
		}

		private void checkBox_CheckedChanged(object sender, System.EventArgs e)
		{
			BaseTerm.term.OPClick((CheckBox)sender);
		}

		public void OnClose()
		{
			textBoxR.BackColor = Color.LightGray;

			buttonStatus.Enabled = false;
			buttonSet.Enabled = true;

			radioButtonCTS.Checked = false;
			radioButtonDSR.Checked = false;
			radioButtonRLSD.Checked = false;
			radioButtonRng.Checked = false;
			checkBoxRTS.Checked = false;
			checkBoxDTR.Checked = false;
			checkBoxBK.Checked = false;

			radioButtonCTS.Enabled = false;
			radioButtonDSR.Enabled = false;
			radioButtonRLSD.Enabled = false;
			radioButtonRng.Enabled = false;
			checkBoxRTS.Enabled = false;
			checkBoxDTR.Enabled = false;
			checkBoxBK.Enabled = false;
			buttonO.Tag = "0";
			buttonO.Text = "Offline";
		}

		public void OnOpen()
		{
			buttonO.Tag = "1";
			buttonO.Text = "Online";
			radioButtonCTS.Enabled = true;
			radioButtonDSR.Enabled = true;
			radioButtonRLSD.Enabled = true;
			radioButtonRng.Enabled = true;
			BaseTerm.term.setOPTicks(checkBoxDTR);
			BaseTerm.term.setOPTicks(checkBoxRTS);
			BaseTerm.term.setOPTicks(checkBoxBK);
			buttonStatus.Enabled = true;
			buttonSet.Enabled = false;
			textBoxR.BackColor = Color.White;
		}

		private void buttonStatus_Click(object sender, System.EventArgs e)
		{
			BaseTerm.term.ShowInfo();
			textBoxS.Focus();
		}

		private void buttonSet_Click(object sender, System.EventArgs e)
		{
			BaseTerm.term.Settings();
			textBoxS.Focus();
		}

		private void buttonClr_Click(object sender, System.EventArgs e)
		{
			textBoxS.Text = "";
			textBoxR.Text = "";
			inEscape = false;
			esc = "";
			mark = "";
			textBoxS.BackColor = Color.White;
			textBoxS.Focus();
		}

		private void BaseTermForm_Load(object sender, System.EventArgs e)
		{
			if (BaseTerm.settingsFileName == "")
				this.Text = "CommBase Terminal";
			else
				this.Text = "CommBase Terminal [" + BaseTerm.settingsFileName + "]";
			textBoxS.Focus();
		}

		private void buttonImm_Click(object sender, System.EventArgs e)
		{
			if (buttonImm.Tag.ToString() == "0")
			{
				buttonImm.Tag = "1";
				buttonImm.Text = "Immediate";
				BaseTerm.term.Immediate = true;
			}
			else
			{
				buttonImm.Tag = "0";
				buttonImm.Text = "Queued";
				BaseTerm.term.Immediate = false;
			}
			textBoxS.Focus();
		}
	}
}