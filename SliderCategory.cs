using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BnS_Slider_Mod
{
	public class SliderCategory
	{
		public string Description
		{
			get;
			set;
		}

		public List<int> Ids
		{
			get;
			set;
		}

		public SliderCategory(string description)
		{
			Description = description;
			Ids = new List<int>();
		}
	}
}