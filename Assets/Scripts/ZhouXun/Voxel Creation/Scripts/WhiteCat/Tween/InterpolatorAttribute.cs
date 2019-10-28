using System;
using System.Reflection;
using UnityEngine;

namespace WhiteCat.Internal
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class InterpolatorAttribute : PropertyAttribute
	{
	}
}