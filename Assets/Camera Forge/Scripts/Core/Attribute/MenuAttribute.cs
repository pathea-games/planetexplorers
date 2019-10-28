using UnityEngine;
using System;

namespace CameraForge
{
	public class MenuAttribute : Attribute
	{
		public MenuAttribute(string name, int order = 0)
		{
			Name = name;
			Order = order;
		}
		public string Name;
		public int Order;
	}
}