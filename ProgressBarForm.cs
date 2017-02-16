using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BnS_Slider_Mod
{
	public class ProgressBarForm : Form
	{
		private IContainer components;

		private Label label1;

		public ProgressBarForm()
		{
			this.InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.label1 = new Label();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(163, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Scanning Process Memory...";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(195, 49);
			base.Controls.Add(this.label1);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "ProgressBarForm";
			this.Text = "Scanning...";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}