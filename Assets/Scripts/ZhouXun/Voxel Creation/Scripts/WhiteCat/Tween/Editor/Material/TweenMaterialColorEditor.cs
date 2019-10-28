using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenMaterialColor))]
	class TweenMaterialColorEditor : TweenMaterialPropertyEditor<TweenMaterialColor>
	{
		int mask;
		Color from, to;


		protected override void OnDrawSubclass()
		{
			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawColorChannels(ref mask, ref from, ref to);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, undoString);
				target.mask = mask;
				target.from = from;
				target.to = to;
				EditorUtility.SetDirty(target);
			}
		}
	}
}
