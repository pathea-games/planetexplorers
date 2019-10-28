using System;
using UnityEngine;

namespace WhiteCat
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class MaxAttribute : PropertyAttribute
	{
		public readonly float max;

		public MaxAttribute(float max)
		{
			this.max = max;
		}
	}
}