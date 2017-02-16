using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace BnS_Slider_Mod
{
	public class Form1 : Form
	{
		private string configFile = "config.xml";

		private Configuration config;

		private Memory memory;

		private Profile currentProfile;

		private Profile defaultProfile;

		private Profile memoryDefaultProfile;

		private IntPtr firstRecordAddress;

		private IContainer components;

		private MenuStrip menuStrip1;

		private ToolStripMenuItem fileToolStripMenuItem;

		private ToolStripMenuItem loadProfileToolStripMenuItem;

		private ToolStripMenuItem saveToolStripMenuItem;

		private ToolStripMenuItem exitToolStripMenuItem;

		private ToolStripMenuItem optionsToolStripMenuItem;

		private ToolStripMenuItem aboutToolStripMenuItem;

		private Panel panel1;

		private Panel panel2;

		private Label processLabel;

		private Button processRefreshButton;

		private ComboBox processSelectBox;

		private Panel panel3;

		private ComboBox selectBodyTypeBox;

		private Label label2;

		private PictureBox processIcon;

		private OpenFileDialog openFileMenu;

		private SaveFileDialog saveFileMenu;

		private Button resetButton;

		private TabControl tabControl;

		private ToolStripMenuItem settingsToolStripMenuItem;

		public Form1()
		{
			this.InitializeComponent();
			base.Shown += new EventHandler(this.FormShown);
		}

		private void AddTextBoxEventHandlers(TextBox box, Record record, Slider slider, Form1.UserValueChange function)
		{
			box.KeyDown += new KeyEventHandler((object send, KeyEventArgs ev) => {
				float single;
				if (ev.KeyCode == Keys.Return)
				{
					ev.Handled = true;
					ev.SuppressKeyPress = true;
					if (this.memory != null && float.TryParse(box.Text, out single))
					{
						try
						{
							function(record, single, slider);
						}
						catch (Exception exception) when (exception is AccessViolationException || exception is ArgumentException)
						{
							MessageBox.Show("Access Violation Error when trying to access the B&S process.\nMake sure B&S is running.");
						}
					}
				}
			});
			box.LostFocus += new EventHandler((object send, EventArgs ev) => {
				float single;
				if (box.Modified && this.memory != null && float.TryParse(box.Text, out single))
				{
					try
					{
						function(record, single, slider);
					}
					catch (Exception exception) when (exception is AccessViolationException || exception is ArgumentException)
					{
						MessageBox.Show("Access Violation Error when trying to access the B&S process.\nMake sure B&S is running.");
					}
				}
				box.Modified = false;
			});
			box.TextChanged += new EventHandler((object send, EventArgs ev) => box.Modified = true);
		}

		private void ChangeMaxDelegate(Record record, float value, Slider slider)
		{
			if (this.firstRecordAddress.Equals(IntPtr.Zero))
			{
				MessageBox.Show("Address is 0");
				return;
			}
			IntPtr zero = IntPtr.Zero;
			zero = (this.firstRecordAddress + record.Offset) + 4 * (slider.Id - 1 + 27);
			this.memory.WriteFloat(zero, value);
			slider.Max = new float?(value);
		}

		private void ChangeMinDelegate(Record record, float value, Slider slider)
		{
			if (this.firstRecordAddress.Equals(IntPtr.Zero))
			{
				MessageBox.Show("Address is 0");
				return;
			}
			IntPtr zero = IntPtr.Zero;
			zero = (this.firstRecordAddress + record.Offset) + 4 * (slider.Id - 1);
			this.memory.WriteFloat(zero, value);
			slider.Min = new float?(value);
		}

		private void ClearPanel()
		{
			foreach (TabPage tabPage in this.tabControl.TabPages)
			{
				for (int i = 0; i < tabPage.Controls.Count; i++)
				{
					tabPage.Controls[i].CausesValidation = false;
					tabPage.Controls[i].Dispose();
				}
				tabPage.Controls.Clear();
			}
		}

		private void CreateTabs()
		{
			foreach (SliderCategory sliderGroup in this.config.SliderGroups)
			{
				TabPage tabPage = new TabPage()
				{
					Text = sliderGroup.Description,
					AutoScroll = true
				};
				this.tabControl.TabPages.Add(tabPage);
			}
			this.tabControl.SelectedIndexChanged += new EventHandler((object sender, EventArgs e) => {
				if (this.tabControl.TabPages.Count <= 0)
				{
					return;
				}
				if (this.tabControl.SelectedTab.Controls.Count > 0)
				{
					this.tabControl.SelectedTab.Controls[0].Focus();
				}
			});
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.memory != null)
			{
				this.memory.Dispose();
			}
			this.memory = null;
			base.Close();
		}

		/*protected override void Finalize()
		{
			try
			{
				if (this.memory != null)
				{
					this.memory.Dispose();
				}
			}
			finally
			{
				base.Finalize();
			}
		}*/

		private void FormShown(object sender, EventArgs e)
		{
			this.config = new Configuration(this.configFile);
			Process process = this.LoadProcess();
			this.LoadMemory(process);
			this.SetProcessBox(process);
			if (this.memory != null)
			{
				this.ScanForRecordAddress();
			}
			this.InitializeProfiles(this.config.DefaultProfile);
			for (int i = 0; i < this.currentProfile.Records.Count; i++)
			{
				this.selectBodyTypeBox.Items.Add(this.currentProfile.Records[i]);
			}
			if (this.memory != null)
			{
				if (this.firstRecordAddress != IntPtr.Zero)
				{
					this.selectBodyTypeBox.Enabled = true;
				}
				else
				{
					MessageBox.Show("Found B&S process but got a bad address.  A patch may have changed memory addresses.\n\nTry selecting Full Scan in settings.");
				}
			}
			this.CreateTabs();
		}

		private SliderCategory GetGroup(Slider s)
		{
			SliderCategory sliderCategory;
			List<SliderCategory>.Enumerator enumerator = this.config.SliderGroups.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SliderCategory current = enumerator.Current;
					if (!current.Ids.Contains(s.Id))
					{
						continue;
					}
					sliderCategory = current;
					return sliderCategory;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return sliderCategory;
		}

		private TabPage GetTab(SliderCategory g)
		{
			TabPage tabPage;
			IEnumerator enumerator = this.tabControl.TabPages.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TabPage current = (TabPage)enumerator.Current;
					if (!current.Text.Equals(g.Description))
					{
						continue;
					}
					tabPage = current;
					return tabPage;
				}
				return null;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return tabPage;
		}

		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadProfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.resetButton = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.panel3 = new System.Windows.Forms.Panel();
            this.selectBodyTypeBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.processIcon = new System.Windows.Forms.PictureBox();
            this.processRefreshButton = new System.Windows.Forms.Button();
            this.processSelectBox = new System.Windows.Forms.ComboBox();
            this.processLabel = new System.Windows.Forms.Label();
            this.openFileMenu = new System.Windows.Forms.OpenFileDialog();
            this.saveFileMenu = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.processIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(18, 18);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(392, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadProfileToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadProfileToolStripMenuItem
            // 
            this.loadProfileToolStripMenuItem.Name = "loadProfileToolStripMenuItem";
            this.loadProfileToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.loadProfileToolStripMenuItem.Text = "Load";
            this.loadProfileToolStripMenuItem.Click += new System.EventHandler(this.loadProfileToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.resetButton);
            this.panel1.Controls.Add(this.tabControl);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(392, 555);
            this.panel1.TabIndex = 1;
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(157, 519);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(75, 23);
            this.resetButton.TabIndex = 2;
            this.resetButton.Text = "Reset All";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Location = new System.Drawing.Point(12, 88);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(368, 425);
            this.tabControl.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.selectBodyTypeBox);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Location = new System.Drawing.Point(12, 47);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(368, 37);
            this.panel3.TabIndex = 1;
            // 
            // selectBodyTypeBox
            // 
            this.selectBodyTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectBodyTypeBox.Enabled = false;
            this.selectBodyTypeBox.FormattingEnabled = true;
            this.selectBodyTypeBox.Location = new System.Drawing.Point(107, 7);
            this.selectBodyTypeBox.Name = "selectBodyTypeBox";
            this.selectBodyTypeBox.Size = new System.Drawing.Size(168, 21);
            this.selectBodyTypeBox.TabIndex = 1;
            this.selectBodyTypeBox.SelectedIndexChanged += new System.EventHandler(this.selectBodyTypeBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Select Race/Sex";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.processIcon);
            this.panel2.Controls.Add(this.processRefreshButton);
            this.panel2.Controls.Add(this.processSelectBox);
            this.panel2.Controls.Add(this.processLabel);
            this.panel2.Location = new System.Drawing.Point(12, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(368, 37);
            this.panel2.TabIndex = 0;
            // 
            // processIcon
            // 
            this.processIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.processIcon.Location = new System.Drawing.Point(107, 8);
            this.processIcon.Name = "processIcon";
            this.processIcon.Size = new System.Drawing.Size(20, 20);
            this.processIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.processIcon.TabIndex = 3;
            this.processIcon.TabStop = false;
            // 
            // processRefreshButton
            // 
            this.processRefreshButton.Location = new System.Drawing.Point(281, 5);
            this.processRefreshButton.Name = "processRefreshButton";
            this.processRefreshButton.Size = new System.Drawing.Size(75, 23);
            this.processRefreshButton.TabIndex = 2;
            this.processRefreshButton.Text = "Refresh Process";
            this.processRefreshButton.UseVisualStyleBackColor = true;
            this.processRefreshButton.Click += new System.EventHandler(this.processRefreshButton_Click);
            // 
            // processSelectBox
            // 
            this.processSelectBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processSelectBox.FormattingEnabled = true;
            this.processSelectBox.Location = new System.Drawing.Point(133, 7);
            this.processSelectBox.Name = "processSelectBox";
            this.processSelectBox.Size = new System.Drawing.Size(142, 21);
            this.processSelectBox.TabIndex = 1;
            // 
            // processLabel
            // 
            this.processLabel.AutoSize = true;
            this.processLabel.Location = new System.Drawing.Point(3, 11);
            this.processLabel.Name = "processLabel";
            this.processLabel.Size = new System.Drawing.Size(45, 13);
            this.processLabel.TabIndex = 0;
            this.processLabel.Text = "Process";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 579);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "B&S Slider Mod";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.processIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private void InitializeProfiles(string profileFile)
		{
			this.memoryDefaultProfile = new Profile(this.config.RecordList, this.config.SliderList);
			try
			{
				this.SyncMemoryWithProfile(this.memory, this.memoryDefaultProfile);
			}
			catch (Exception exception) when (exception is AccessViolationException || exception is ArgumentException)
			{
				MessageBox.Show("Error when trying to read or write from the B&S process.\n");
			}
			this.defaultProfile = new Profile(this.config.RecordList, this.config.SliderList);
			try
			{
				this.defaultProfile.Load(profileFile);
			}
			catch (Exception exception1)
			{
				MessageBox.Show("Error Loading Profile");
			}
			this.currentProfile = new Profile(this.defaultProfile);
			try
			{
				this.SyncMemoryWithProfile(this.memory, this.currentProfile);
			}
			catch (Exception exception2) when (exception2 is AccessViolationException || exception2 is ArgumentException)
			{
				MessageBox.Show("Error when trying to read or write from the B&S process.\n");
			}
		}

		private Memory LoadMemory(Process process)
		{
			if (process != null)
			{
				try
				{
					memory = new Memory(process);
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					MessageBox.Show(string.Concat("Could not connect to B&S process memory: \n", invalidOperationException.Message));
				}
			}
			return memory;
		}

		private Process LoadProcess()
		{
			Process[] processesByName = Process.GetProcessesByName(this.config.ProcessName);
			Process process = null;
			for (int i = 0; i < (int)processesByName.Length; i++)
			{
				if (!processesByName[i].MainWindowTitle.Equals(this.config.WindowTitle))
				{
					processesByName[i].Dispose();
				}
				else
				{
					process = processesByName[i];
				}
			}
			return process;
		}

		private void loadProfileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.openFileMenu.Title = "Select Profile file";
			this.openFileMenu.Filter = "XML|*.xml|All files|*.*";
			this.openFileMenu.InitialDirectory = Application.StartupPath;
			string fileName = "";
			if (this.openFileMenu.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				fileName = this.openFileMenu.FileName;
				int selectedIndex = this.selectBodyTypeBox.SelectedIndex;
				bool enabled = this.selectBodyTypeBox.Enabled;
				this.selectBodyTypeBox.Enabled = false;
				this.InitializeProfiles(fileName);
				this.selectBodyTypeBox.Items.Clear();
				for (int i = 0; i < this.currentProfile.Records.Count; i++)
				{
					this.selectBodyTypeBox.Items.Add(this.currentProfile.Records[i]);
				}
				this.selectBodyTypeBox.SelectedIndex = selectedIndex;
				this.selectBodyTypeBox.Enabled = enabled;
				if (enabled)
				{
					this.PopulatePanel();
				}
			}
		}

		private void PopulatePanel()
		{
			this.ClearPanel();
			if (this.selectBodyTypeBox.SelectedIndex == -1)
			{
				return;
			}
			Record selectedItem = (Record)this.selectBodyTypeBox.SelectedItem;
			Label label = null;
			for (int i = 0; i < selectedItem.Sliders.Count; i++)
			{
				SliderCategory group = this.GetGroup(selectedItem.Sliders[i]);
				TabPage tab = this.GetTab(group);
				int num = group.Ids.IndexOf(selectedItem.Sliders[i].Id);
				label = new Label()
				{
					Text = selectedItem.Sliders[i].Description,
					Location = new Point(tab.Left, 10 + (num + 1) * 30)
				};
				TextBox textBox = new TextBox()
				{
					Location = new Point(label.Right, label.Top - 3)
				};
				float? min = selectedItem.Sliders[i].Min;
				textBox.Text = min.ToString();
				TextBox str = new TextBox()
				{
					Location = new Point(textBox.Right, label.Top - 3)
				};
				min = selectedItem.Sliders[i].Max;
				str.Text = min.ToString();
				this.AddTextBoxEventHandlers(textBox, selectedItem, selectedItem.Sliders[i], new Form1.UserValueChange(this.ChangeMinDelegate));
				this.AddTextBoxEventHandlers(str, selectedItem, selectedItem.Sliders[i], new Form1.UserValueChange(this.ChangeMaxDelegate));
				tab.Controls.Add(label);
				tab.Controls.Add(textBox);
				tab.Controls.Add(str);
			}
			if (label != null)
			{
				foreach (TabPage tabPage in this.tabControl.TabPages)
				{
					Label point = new Label()
					{
						Text = "Min"
					};
					Label label1 = new Label()
					{
						Text = "Max"
					};
					point.Location = new Point(label.Right, 10);
					label1.Location = new Point(point.Right, point.Top);
					tabPage.Controls.Add(point);
					tabPage.Controls.Add(label1);
				}
			}
		}

		private void processRefreshButton_Click(object sender, EventArgs e)
		{
			this.selectBodyTypeBox.Enabled = false;
			this.ClearPanel();
			if (this.memory != null)
			{
				this.memory.Dispose();
			}
			this.memory = null;
			Process process = this.LoadProcess();
			this.LoadMemory(process);
			this.SetProcessBox(process);
			if (this.memory != null)
			{
				this.ScanForRecordAddress();
				this.memoryDefaultProfile = new Profile(this.config.RecordList, this.config.SliderList);
				try
				{
					this.SyncMemoryWithProfile(this.memory, this.memoryDefaultProfile);
				}
				catch (Exception exception) when (exception is AccessViolationException || exception is ArgumentException)
				{
					MessageBox.Show("Error when trying to read or write from the B&S process.\n");
				}
				try
				{
					this.SyncMemoryWithProfile(this.memory, this.currentProfile);
				}
				catch (Exception exception1) when (exception1 is AccessViolationException || exception1 is ArgumentException)
				{
					MessageBox.Show("Error when trying to read or write from the B&S process.\n");
				}
				if (this.firstRecordAddress == IntPtr.Zero)
				{
					MessageBox.Show("Found B&S process but got a bad address.  A patch may have changed memory addresses.\n\nTry selecting Full Scan in settings.");
					return;
				}
				this.selectBodyTypeBox.Enabled = true;
				this.PopulatePanel();
			}
		}

		private void resetButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.selectBodyTypeBox.SelectedIndex;
			bool enabled = this.selectBodyTypeBox.Enabled;
			this.selectBodyTypeBox.Enabled = false;
			this.currentProfile = new Profile(this.memoryDefaultProfile);
			try
			{
				this.SyncMemoryWithProfile(this.memory, this.currentProfile);
			}
			catch (Exception exception) when (exception is AccessViolationException || exception is ArgumentException)
			{
				MessageBox.Show("Error when trying to read or write from the B&S process.\n");
			}
			this.selectBodyTypeBox.Items.Clear();
			for (int i = 0; i < this.currentProfile.Records.Count; i++)
			{
				this.selectBodyTypeBox.Items.Add(this.currentProfile.Records[i]);
			}
			this.selectBodyTypeBox.SelectedIndex = selectedIndex;
			this.selectBodyTypeBox.Enabled = enabled;
			if (enabled)
			{
				this.PopulatePanel();
			}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.saveFileMenu.Title = "Save Profile file";
			this.saveFileMenu.Filter = "XML|*.xml|All files|*.*";
			this.saveFileMenu.InitialDirectory = Application.StartupPath;
			string fileName = "";
			if (this.saveFileMenu.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				fileName = this.saveFileMenu.FileName;
				this.currentProfile.Save(fileName);
			}
		}

		private IntPtr ScanForRecordAddress()
		{
			try
			{
				if (!this.config.ScanType.ToLower().Equals("offset"))
				{
					ProgressBarForm progressBarForm = new ProgressBarForm();
					progressBarForm.Show();
					Point location = base.Location;
					int x = location.X + base.Width / 2 - progressBarForm.Width / 2;
					location = base.Location;
					Point point = new Point(x, location.Y + 100);
					progressBarForm.Location = point;
					progressBarForm.Update();
					base.Update();
					if (this.config.BufferSize <= 0)
					{
						this.config.BufferSize = 1;
					}
					byte[] numArray = new byte[this.config.BufferSize * 1024];
					this.firstRecordAddress = MemoryScanner.ScanRange(this.memory, (IntPtr)0, (IntPtr)int.MaxValue, this.config.ByteArray, numArray);
					progressBarForm.Close();
				}
				else
				{
					this.firstRecordAddress = MemoryScanner.ScanModule(this.memory, this.config.Module, this.config.BaseAddress, this.config.ByteArray, this.config.MemoryRange);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
			return this.firstRecordAddress;
		}

		private void selectBodyTypeBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.selectBodyTypeBox.SelectedIndex == -1 || !this.selectBodyTypeBox.Enabled)
			{
				return;
			}
			this.PopulatePanel();
			base.ActiveControl = this.tabControl.SelectedTab;
		}

		private void SetProcessBox(Process process)
		{
			this.processSelectBox.Items.Clear();
			if (this.processIcon.Image != null)
			{
				this.processIcon.Image.Dispose();
			}
			this.processIcon.Image = null;
			if (process == null)
			{
				return;
			}
			this.processSelectBox.Items.Add(process.MainWindowTitle);
			Icon icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
			this.processSelectBox.SelectedIndex = 0;
			this.processIcon.Image = icon.ToBitmap();
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			(new SettingsForm(this.config)).Show();
		}

		private void SyncMemoryWithProfile(Memory memory, Profile profile)
		{
			float? min;
			if (this.firstRecordAddress.Equals(IntPtr.Zero))
			{
				return;
			}
			if (memory != null)
			{
				foreach (Record record in profile.Records)
				{
					for (int i = 0; i < record.Sliders.Count; i++)
					{
						IntPtr zero = IntPtr.Zero;
						zero = (this.firstRecordAddress + record.Offset) + 4 * (record.Sliders[i].Id - 1);
						if (!record.Sliders[i].Min.HasValue)
						{
							record.Sliders[i].Min = new float?(memory.ReadFloat(zero));
						}
						else
						{
							min = record.Sliders[i].Min;
							memory.WriteFloat(zero, min.Value);
						}
						zero = (this.firstRecordAddress + record.Offset) + 4 * (record.Sliders[i].Id - 1 + 27);
						if (!record.Sliders[i].Max.HasValue)
						{
							record.Sliders[i].Max = new float?(memory.ReadFloat(zero));
						}
						else
						{
							min = record.Sliders[i].Max;
							memory.WriteFloat(zero, min.Value);
						}
					}
				}
			}
		}

		private delegate void UserValueChange(Record record, float value, Slider slider);
	}
}