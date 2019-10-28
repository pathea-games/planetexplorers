using System;
using UnityEngine;

namespace WhiteCat
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public sealed class LineTextAttribute : PropertyAttribute
	{
		public readonly string text;

		public LineTextAttribute(string text)
		{
			this.text = text;
		}
	}
}