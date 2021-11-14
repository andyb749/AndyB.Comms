using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using AndyB.Comms.Comm;

public class InfoForm : System.Windows.Forms.Form
{
	private System.Windows.Forms.TextBox textBox;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public InfoForm(QueueStatus s)
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		textBox.Text = s.ToString();
		textBox.Select(0, 0);
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
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
		this.textBox = new System.Windows.Forms.TextBox();
		this.SuspendLayout();
		// 
		// textBox
		// 
		this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
		this.textBox.Multiline = true;
		this.textBox.Name = "textBox";
		this.textBox.ReadOnly = true;
		this.textBox.Size = new System.Drawing.Size(272, 124);
		this.textBox.TabIndex = 0;
		this.textBox.Text = "";
		// 
		// InfoForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(272, 124);
		this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.textBox});
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.Name = "InfoForm";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "CommBase Queue Status";
		this.ResumeLayout(false);

	}
	#endregion
}
