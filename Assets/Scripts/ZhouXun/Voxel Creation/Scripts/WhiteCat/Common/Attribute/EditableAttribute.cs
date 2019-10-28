using System;
using UnityEngine;

namespace WhiteCat
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class EditableAttribute : PropertyAttribute
	{
		public readonly bool editMode;
		public readonly bool playMode;

		public EditableAttribute(bool editMode, bool playMode)
		{
			this.editMode = editMode;
			this.playMode = playMode;
		}
	}
}