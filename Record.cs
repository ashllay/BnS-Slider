using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BnS_Slider_Mod
{
	public class Record
	{
		public string Gender
		{
			get;
			set;
		}

		public int Offset
		{
			get;
			set;
		}

		public string Race
		{
			get;
			set;
		}

		public List<Slider> Sliders
		{
			get;
			set;
		}

		public Record()
		{
			this.Sliders = new List<Slider>();
		}

		public Record(string race, string gender, int offset)
		{
			this.Race = race;
			this.Gender = gender;
			this.Offset = offset;
			this.Sliders = new List<Slider>();
		}

		public Record(Record other)
		{
			this.Race = other.Race;
			this.Gender = other.Gender;
			this.Offset = other.Offset;
			this.Sliders = new List<Slider>();
			foreach (Slider slider in other.Sliders)
			{
				this.Sliders.Add(new Slider(slider));
			}
		}

		public Slider GetSliderById(int id)
		{
			for (int i = 0; i < this.Sliders.Count; i++)
			{
				if (this.Sliders[i].Id == id)
				{
					return this.Sliders[i];
				}
			}
			return null;
		}

		public override string ToString()
		{
			return string.Concat(this.Race, " ", this.Gender);
		}
	}
}