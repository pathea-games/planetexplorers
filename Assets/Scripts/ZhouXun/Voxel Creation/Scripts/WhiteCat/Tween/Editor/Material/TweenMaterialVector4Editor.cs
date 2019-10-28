using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenMaterialVector))]
	class TweenMaterialVector4Editor : TweenMaterialPropertyEditor<TweenMaterialVector>
	{
		int mask;
		Vector4 from, to;


		protected override void OnDrawSubclass()
		{
			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawVector4Channels(ref mask, ref from, ref to);
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
