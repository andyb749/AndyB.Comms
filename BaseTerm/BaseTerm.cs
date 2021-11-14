using System;
using System.Windows.Forms;
using System.IO;
using AndyB.Comms.Comm;

namespace BaseTerm
{
	public class BaseTerm : CommBase
	{
		public static TermForm frm;
		public static BaseTerm term;
		public static CommBaseTermSettings settings;
		public static string settingsFileName = "";

		private int lineCount = 0;

		public class CommBaseTermSettings : CommBaseSettings
		{
			public bool showAsHex = false;
			public bool breakLineOnChar = false;
			public ASCII lineBreakChar = 0;
			public int charsInLine = 0;

			public static new CommBaseTermSettings LoadFromXML(Stream s)
			{
				return (CommBaseTermSettings)LoadFromXML(s, typeof(CommBaseTermSettings));
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			if (args.Length == 1)
			{
				FileInfo f = new FileInfo(args[0]);
				if (f.Exists)
				{
					settingsFileName = f.Name;
					settings = CommBaseTermSettings.LoadFromXML(f.OpenRead());
					if (settings == null)
					{
						MessageBox.Show("Bad settings file", "CommBase Terminal", MessageBoxButtons.OK);
						return 0;
					}
				}
			}
			else
			{
				settings = new CommBaseTermSettings();
			}
			term = new BaseTerm();
			frm = new TermForm();
			Application.Run(frm);
			return 0;
		}

		public bool Immediate = false;

		public void SendChar(byte c)
		{
			try
			{
				if (Immediate)
					SendImmediate(c);
				else
					Send(c);
			}
			catch (CommPortException e)
			{
				frm.ShowException(e);
			}
		}

		public bool SendCtrl(string s)
		{
			ASCII a = 0;
			try
			{
				a = (ASCII)ASCII.Parse(a.GetType(), s, true);
			}
			catch
			{
				return false;
			}
			SendChar((byte)a);
			return true;
		}

		public void setOPTicks(CheckBox chk)
		{
			switch (int.Parse(chk.Tag.ToString()))
			{
				case 0:
					chk.Enabled = base.RTSavailable;
					chk.Checked = base.RTS;
					break;
				case 1:
					chk.Enabled = base.DTRavailable;
					chk.Checked = base.DTR;
					break;
				case 2:
					chk.Enabled = true;
					chk.Checked = base.Break;
					break;
			}
		}

		public void OPClick(CheckBox chk)
		{
			try
			{
				switch (int.Parse(chk.Tag.ToString()))
				{
					case 0: base.RTS = chk.Checked; break;
					case 1: base.DTR = chk.Checked; break;
					case 2: base.Break = chk.Checked; break;
				}
			}
			catch (CommPortException e)
			{
				frm.ShowException(e);
			}
		}

		public void ShowInfo()
		{
			QueueStatus q;
			try
			{
				q = GetQueueStatus();
			}
			catch (CommPortException e)
			{
				frm.ShowException(e);
				return;
			}
			InfoForm f = new InfoForm(q);
			f.ShowDialog();
		}

		public void Settings()
		{
			SettingsForm f = new SettingsForm(settings);
			f.ShowDialog();
		}

		protected override CommBaseSettings CommSettings()
		{
			return settings;
		}

		protected override void OnRxChar(byte c)
		{
			string s; bool nl = false;
			ASCII v = (ASCII)c;
			if (settings.charsInLine > 0)
			{
				nl = (++lineCount >= settings.charsInLine);
			}
			if (settings.breakLineOnChar) if (v == settings.lineBreakChar) nl = true;
			if (nl) lineCount = 0;
			if (settings.showAsHex)
			{
				s = c.ToString("X2");
				if (!nl) s += " ";
			}
			else
			{
				if ((c < 0x20) || (c > 0x7E))
				{
					s = "<" + v.ToString() + ">";
				}
				else
				{
					s = new string((char)c, 1);
				}
			}
			frm.ShowChar(s, nl);
		}

		protected override void OnBreak()
		{
			frm.ShowMsg(">>>> BREAK");
		}

		protected override bool AfterOpen()
		{
			frm.OnOpen();
			ModemStatus m = GetModemStatus();
			frm.SetIndics(m.cts, m.dsr, m.rlsd, m.ring);
			return true;
		}

		protected override void BeforeClose(bool e)
		{
			if ((settings.autoReopen) && (e))
			{
				frm.OnOpen();
			}
			else
			{
				frm.OnClose();
				frm.ShowMsg(">>>> OFFLINE");
			}
		}

		protected override void OnStatusChange(ModemStatus c, ModemStatus v)
		{
			frm.SetIndics(v.cts, v.dsr, v.rlsd, v.ring);
		}
	}
}