using System;
using System.Collections;
using UnityEditor;

namespace WhiteCat
{
	// ReorderableList: http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/

	public class ReorderableList<T> : UnityEditorInternal.ReorderableList
	{
		public ReorderableList(object list, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
			: base(list as IList, typeof(T), draggable, displayHeader, displayAddButton, displayRemoveButton)
		{
		}
	}
}
