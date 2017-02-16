using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace BnS_Slider_Mod
{
	public class Profile
	{
		public string FileName
		{
			get;
			set;
		}

		public List<Record> Records
		{
			get;
			set;
		}

		public Profile()
		{
			this.Records = new List<Record>();
		}

		public Profile(string filename)
		{
			this.FileName = filename;
			this.Records = new List<Record>();
		}

		public Profile(Profile other)
		{
			this.Records = new List<Record>();
			foreach (Record record in other.Records)
			{
				this.Records.Add(new Record(record));
			}
		}

		public Profile(List<Record> records, List<Slider> sliders)
		{
			this.Records = new List<Record>();
			foreach (Record record in records)
			{
				Record record1 = new Record(record.Race, record.Gender, record.Offset);
				foreach (Slider slider in sliders)
				{
					record1.Sliders.Add(new Slider(slider.Description, slider.Id, slider.Min, slider.Max));
				}
				this.Records.Add(record1);
			}
		}

		public void CopyValues(Profile other)
		{
			foreach (Record record in other.Records)
			{
				foreach (Record record1 in this.Records)
				{
					if (!record1.ToString().Equals(record.ToString()))
					{
						continue;
					}
					foreach (Slider slider in record.Sliders)
					{
						foreach (Slider slider1 in record1.Sliders)
						{
							if (slider1.Id != slider.Id)
							{
								continue;
							}
							slider1.Min = (slider.Min.HasValue ? slider.Min : slider1.Min);
							slider1.Max = (slider.Max.HasValue ? slider.Max : slider1.Max);
						}
					}
				}
			}
		}

		private Record GetRecord(string race, string gender)
		{
			return this.GetRecord(string.Concat(race, " ", gender));
		}

		private Record GetRecord(string description)
		{
			for (int i = 0; i < this.Records.Count; i++)
			{
				if (this.Records[i].ToString().Equals(description))
				{
					return this.Records[i];
				}
			}
			return null;
		}

		public void Load()
		{
			this.Load(this.FileName);
		}

		public void Load(string filename)
		{
			if (filename == null || filename.Equals(""))
			{
				return;
			}
			this.FileName = filename;
			foreach (XElement xElement in XElement.Load(filename).Elements("Record"))
			{
				this.LoadRecord(xElement);
			}
		}

		private Record LoadRecord(XElement xRecordElem)
		{
			float single;
			string value = xRecordElem.Element("Race").Value;
			string str = xRecordElem.Element("Gender").Value;
			Record record = this.GetRecord(value, str);
			if (record == null)
			{
				return null;
			}
			foreach (XElement xElement in xRecordElem.Elements("Value"))
			{
				try
				{
					string value1 = xElement.Attribute("id").Value;
					Slider sliderById = record.GetSliderById(int.Parse(value1));
					if (sliderById != null)
					{
						string str1 = (string)xElement.Element("Min");
						string str2 = (string)xElement.Element("Max");
						sliderById.Min = (float.TryParse(str1, out single) ? new float?(single) : sliderById.Min);
						sliderById.Max = (float.TryParse(str2, out single) ? new float?(single) : sliderById.Max);
					}
				}
				catch (Exception exception)
				{
				}
			}
			return record;
		}

		public void Save(string filename)
		{
			XElement xElement = new XElement("Profile");
			foreach (Record record in this.Records)
			{
				XElement xElement1 = new XElement("Record");
				XElement xElement2 = new XElement("Race")
				{
					Value = record.Race
				};
				xElement1.Add(xElement2);
				XElement xElement3 = new XElement("Gender")
				{
					Value = record.Gender
				};
				xElement1.Add(xElement3);
				foreach (Slider slider in record.Sliders)
				{
					XElement xElement4 = new XElement("Value");
					xElement4.SetAttributeValue("id", slider.Id);
					if (slider.Min.HasValue)
					{
						xElement4.Add(new XElement("Min", (object)slider.Min));
					}
					if (slider.Max.HasValue)
					{
						xElement4.Add(new XElement("Max", (object)slider.Max));
					}
					xElement1.Add(xElement4);
				}
				xElement.Add(xElement1);
			}
			xElement.Save(filename);
		}

		public void Save()
		{
			this.Save(this.FileName);
		}
	}
}