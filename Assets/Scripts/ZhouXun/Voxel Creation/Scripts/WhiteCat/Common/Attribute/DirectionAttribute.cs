using System;
using UnityEngine;

namespace WhiteCat
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class DirectionAttribute : PropertyAttribute
	{
		public readonly float length;
		public bool initialized = false;
		public Vector3 eulerAngles;


		public DirectionAttribute(float length = 1.0f)
		{
			this.length = length;
		}
	}
}