using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace BnS_Slider_Mod
{
	public class Configuration
	{
		public List<SliderCategory> SliderGroups;

		public int BaseAddress
		{
			get;
			set;
		}

		public int BufferSize { get; set; } = 512;

		public byte[] ByteArray
		{
			get;
			set;
		}

		public string DefaultProfile
		{
			get;
			set;
		}

		public string Filename
		{
			get;
			set;
		}

		public int MemoryRange
		{
			get;
			set;
		}

		public string Module
		{
			get;
			set;
		}

		public List<int> Offsets
		{
			get;
			set;
		}

		public string ProcessName
		{
			get;
			set;
		}

		public List<Record> RecordList
		{
			get;
			set;
		}

		public string ScanType
		{
			get;
			set;
		}

		public List<Slider> SliderList
		{
			get;
			set;
		}

		public string WindowTitle
		{
			get;
			set;
		}

		public Configuration()
		{
		}

		public Configuration(string configFileName)
		{
			this.Filename = configFileName;
			this.Load();
		}

		private static int HexStringToInt(string str)
		{
			if (str.Equals(""))
			{
				return 0;
			}
			if (str.StartsWith("0x"))
			{
				str = str.Substring(2);
			}
			return Convert.ToInt32(str, 16);
		}

		public void Load()
		{
			this.Load(this.Filename);
		}

		public void Load(string filename)
		{
			int num;
			this.Offsets = new List<int>();
			XElement xElement = XElement.Load(filename);
			this.LoadProcessData(xElement);
			this.LoadSliders(xElement);
			this.LoadRecords(xElement);
			this.DefaultProfile = xElement.Element("DefaultProfile").Value;
			this.LoadByteArray(xElement);
			this.MemoryRange = Configuration.HexStringToInt(xElement.Element("MemoryRange").Value);
			this.LoadSliderGroups(xElement);
			this.ScanType = xElement.Element("DefaultScan").Value;
			this.BufferSize = (int.TryParse(xElement.Element("BufferSize").Value, out num) ? num : this.BufferSize);
		}

		private void LoadByteArray(XElement xelem)
		{
			string value = xelem.Element("ByteArray").Value;
			byte[] num = new byte[value.Length / 2];
			for (int i = 0; i < (int)num.Length; i++)
			{
				num[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
			}
			this.ByteArray = num;
		}

		private void LoadProcessData(XElement xelem)
		{
			this.ProcessName = xelem.Element("ProcessName").Value;
			this.Module = xelem.Element("Module").Value;
			this.BaseAddress = Configuration.HexStringToInt(xelem.Element("BaseAddress").Value);
			this.WindowTitle = xelem.Element("WindowTitle").Value;
			try
			{
				foreach (XElement xElement in xelem.Element("OffsetList").Elements("Offset"))
				{
					this.Offsets.Add(Configuration.HexStringToInt(xElement.Value));
				}
			}
			catch (NullReferenceException nullReferenceException)
			{
			}
		}

		private void LoadRecords(XElement xelem)
		{
			this.RecordList = new List<Record>();
			foreach (XElement xElement in xelem.Element("Records").Elements("Record"))
			{
				string value = xElement.Element("Race").Value;
				string str = xElement.Element("Gender").Value;
				int num = Configuration.HexStringToInt(xElement.Element("Offset").Value);
				this.RecordList.Add(new Record(value, str, num));
			}
		}

		private void LoadSliderGroups(XElement xelem)
		{
			this.SliderGroups = new List<SliderCategory>();
			foreach (XElement xElement in xelem.Element("Groups").Elements("Group"))
			{
				SliderCategory sliderCategory = new SliderCategory(xElement.Attribute("description").Value)
				{
					Ids = new List<int>()
				};
				foreach (XElement xElement1 in xElement.Elements("Value"))
				{
					sliderCategory.Ids.Add(int.Parse(xElement1.Attribute("id").Value));
				}
				this.SliderGroups.Add(sliderCategory);
			}
		}

		private void LoadSliders(XElement xelem)
		{
			this.SliderList = new List<Slider>();
			foreach (XElement xElement in xelem.Element("Values").Elements("Value"))
			{
				int num = Convert.ToInt32(xElement.Attribute("id").Value);
				string value = xElement.Attribute("description").Value;
				this.SliderList.Add(new Slider(value, num));
			}
		}

		public bool Save(string filename)
		{
			XElement scanType = XElement.Load(filename);
			scanType.Element("DefaultScan").Value = this.ScanType;
			scanType.Element("BufferSize").Value = this.BufferSize.ToString();
			scanType.Element("DefaultProfile").Value = this.DefaultProfile;
			scanType.Save(this.Filename);
			return true;
		}
	}
}