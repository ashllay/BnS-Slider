using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BnS_Slider_Mod
{
	public class SettingsForm : Form
	{
		private CheckBox checkedScan;

		private IContainer components;
		private GroupBox groupBox1;
		private CheckBox scanFullCheckBox;
		private CheckBox scanOffsetCheckBox;
		private Button saveButton;
		private Button closeButton;
		private Label bufferSizeLabel;
		private TextBox bufferSizeTextBox;
		private Label kbLabel;
		private Label defaultProfileLabel;
		private TextBox defaultProfileTextBox;
		private OpenFileDialog openFileMenu;
		private Button defaultProfileButton;

		private Configuration Config
		{
			get;
			set;
		}

		public SettingsForm(Configuration cfg)
		{
			this.InitializeComponent();
			this.Config = cfg;
			if (this.Config != null)
			{
				this.SetCheckBoxes();
				this.bufferSizeTextBox.Text = this.Config.BufferSize.ToString();
				this.defaultProfileTextBox.Text = this.Config.DefaultProfile;
			}
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			base.Close();
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
			this.groupBox1 = new GroupBox();
			this.scanFullCheckBox = new CheckBox();
			this.scanOffsetCheckBox = new CheckBox();
			this.saveButton = new Button();
			this.closeButton = new Button();
			this.bufferSizeLabel = new Label();
			this.bufferSizeTextBox = new TextBox();
			this.kbLabel = new Label();
			this.defaultProfileLabel = new Label();
			this.defaultProfileTextBox = new TextBox();
			this.openFileMenu = new OpenFileDialog();
			this.defaultProfileButton = new Button();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			this.groupBox1.Controls.Add(this.scanFullCheckBox);
			this.groupBox1.Controls.Add(this.scanOffsetCheckBox);
			this.groupBox1.Location = new Point(13, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(314, 72);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Scan Method";
			this.scanFullCheckBox.AutoSize = true;
			this.scanFullCheckBox.Location = new Point(7, 46);
			this.scanFullCheckBox.Name = "scanFullCheckBox";
			this.scanFullCheckBox.Size = new System.Drawing.Size(78, 19);
			this.scanFullCheckBox.TabIndex = 1;
			this.scanFullCheckBox.Text = "Full Scan";
			this.scanFullCheckBox.UseVisualStyleBackColor = true;
			this.scanOffsetCheckBox.AutoSize = true;
			this.scanOffsetCheckBox.Location = new Point(7, 20);
			this.scanOffsetCheckBox.Name = "scanOffsetCheckBox";
			this.scanOffsetCheckBox.Size = new System.Drawing.Size(58, 19);
			this.scanOffsetCheckBox.TabIndex = 0;
			this.scanOffsetCheckBox.Text = "Offset";
			this.scanOffsetCheckBox.UseVisualStyleBackColor = true;
			this.saveButton.Location = new Point(171, 190);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(75, 23);
			this.saveButton.TabIndex = 1;
			this.saveButton.Text = "Ok";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new EventHandler(this.saveButton_Click);
			this.closeButton.Location = new Point(252, 190);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 2;
			this.closeButton.Text = "Cancel";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new EventHandler(this.closeButton_Click);
			this.bufferSizeLabel.AutoSize = true;
			this.bufferSizeLabel.Location = new Point(12, 94);
			this.bufferSizeLabel.Name = "bufferSizeLabel";
			this.bufferSizeLabel.Size = new System.Drawing.Size(69, 15);
			this.bufferSizeLabel.TabIndex = 3;
			this.bufferSizeLabel.Text = "Buffer Size:";
			this.bufferSizeTextBox.Location = new Point(87, 94);
			this.bufferSizeTextBox.Name = "bufferSizeTextBox";
			this.bufferSizeTextBox.Size = new System.Drawing.Size(87, 20);
			this.bufferSizeTextBox.TabIndex = 4;
			this.kbLabel.AutoSize = true;
			this.kbLabel.Location = new Point(180, 97);
			this.kbLabel.Name = "kbLabel";
			this.kbLabel.Size = new System.Drawing.Size(23, 15);
			this.kbLabel.TabIndex = 5;
			this.kbLabel.Text = "KB";
			this.defaultProfileLabel.AutoSize = true;
			this.defaultProfileLabel.Location = new Point(12, 127);
			this.defaultProfileLabel.Name = "defaultProfileLabel";
			this.defaultProfileLabel.Size = new System.Drawing.Size(87, 15);
			this.defaultProfileLabel.TabIndex = 6;
			this.defaultProfileLabel.Text = "Default Profile:";
			this.defaultProfileTextBox.Location = new Point(12, 145);
			this.defaultProfileTextBox.Name = "defaultProfileTextBox";
			this.defaultProfileTextBox.Size = new System.Drawing.Size(249, 20);
			this.defaultProfileTextBox.TabIndex = 7;
			this.defaultProfileButton.Location = new Point(267, 142);
			this.defaultProfileButton.Name = "defaultProfileButton";
			this.defaultProfileButton.Size = new System.Drawing.Size(59, 23);
			this.defaultProfileButton.TabIndex = 8;
			this.defaultProfileButton.Text = "Browse";
			this.defaultProfileButton.UseVisualStyleBackColor = true;
			this.defaultProfileButton.Click += new EventHandler(this.profileButton_Click);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(339, 227);
			base.Controls.Add(this.defaultProfileButton);
			base.Controls.Add(this.defaultProfileTextBox);
			base.Controls.Add(this.defaultProfileLabel);
			base.Controls.Add(this.kbLabel);
			base.Controls.Add(this.bufferSizeTextBox);
			base.Controls.Add(this.bufferSizeLabel);
			base.Controls.Add(this.closeButton);
			base.Controls.Add(this.saveButton);
			base.Controls.Add(this.groupBox1);
			base.Name = "SettingsForm";
			this.Text = "Settings";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void profileButton_Click(object sender, EventArgs e)
		{
			this.openFileMenu.Title = "Select Default Profile file";
			this.openFileMenu.Filter = "XML|*.xml|All files|*.*";
			this.openFileMenu.InitialDirectory = Application.StartupPath;
			string defaultProfile = this.Config.DefaultProfile;
			if (this.openFileMenu.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.defaultProfileTextBox.Text = this.openFileMenu.FileName;
			}
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			int num;
			if (this.checkedScan != null)
			{
				this.Config.ScanType = this.checkedScan.Text;
			}
			if (int.TryParse(this.bufferSizeTextBox.Text, out num) && num > 0)
			{
				this.Config.BufferSize = num;
			}
			this.Config.DefaultProfile = this.defaultProfileTextBox.Text;
			this.Config.Save(this.Config.Filename);
			base.Close();
		}

		private void SetCheckBoxes()
		{
			if (!Config.ScanType.ToLower().Equals("Full Scan".ToLower()))
			{
				scanOffsetCheckBox.Checked = true;
			}
			else
			{
				scanFullCheckBox.Checked = true;
			}
			this.scanOffsetCheckBox.CheckStateChanged += new EventHandler((object send, EventArgs ev) => {
				if (scanOffsetCheckBox.CheckState == CheckState.Checked)
				{
					checkedScan = scanOffsetCheckBox;
					scanFullCheckBox.CheckState = CheckState.Unchecked;
					scanOffsetCheckBox.AutoCheck = false;
					scanFullCheckBox.AutoCheck = true;
				}
			});
			this.scanFullCheckBox.CheckStateChanged += new EventHandler((object send, EventArgs ev) => {
				if (this.scanFullCheckBox.CheckState == CheckState.Checked)
				{
					this.checkedScan = this.scanFullCheckBox;
					this.scanOffsetCheckBox.CheckState = CheckState.Unchecked;
					this.scanOffsetCheckBox.AutoCheck = true;
					this.scanFullCheckBox.AutoCheck = false;
				}
			});
		}
	}
}