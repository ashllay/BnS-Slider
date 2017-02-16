using System;
using System.Runtime.CompilerServices;

namespace BnS_Slider_Mod
{
	public class Slider
	{
		public string Description
		{
			get;
			set;
		}

		public int Id
		{
			get;
			set;
		}

		public float? Max
		{
			get;
			set;
		}

		public float? Min
		{
			get;
			set;
		}

		public Slider()
		{
			float? nullable = null;
			this.Min = nullable;
			nullable = null;
			this.Max = nullable;
		}

		public Slider(string desc, int id, float? min, float? max)
		{
			this.Description = desc;
			this.Id = id;
			this.Min = min;
			this.Max = max;
		}

		public Slider(string desc, float? min, float? max)
		{
			this.Description = desc;
			this.Min = min;
			this.Max = max;
		}

		public Slider(string desc, int id)
		{
			this.Description = desc;
			this.Id = id;
			float? nullable = null;
			this.Min = nullable;
			nullable = null;
			this.Max = nullable;
		}

		public Slider(Slider other)
		{
			this.Description = other.Description;
			this.Id = other.Id;
			this.Min = other.Min;
			this.Max = other.Max;
		}
	}
}