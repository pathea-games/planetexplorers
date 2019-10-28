using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenMaterialTextureOffset))]
	class TweenMaterialTextureOffsetEditor : TweenMaterialPropertyEditor<TweenMaterialTextureOffset>
	{
		int mask;
		Vector2 from, to;


		protected override void OnDrawSubclass()
		{
			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawVector2Channels(ref mask, ref from, ref to);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, undoString);
				target.mask = mask;
				target.from = from;
				target.to = to;
				EditorUtility.SetDirty(target);
			}
		}
	}


	[CustomEditor(typeof(TweenMaterialTextureScale))]
	class TweenMaterialTextureScaleEditor : TweenMaterialPropertyEditor<TweenMaterialTextureScale>
	{
		int mask;
		Vector2 from, to;


		protected override void OnDrawSubclass()
		{
			mask = target.mask;
			from = target.from;
			to = target.to;

			EditorGUI.BeginChangeCheck();
			DrawVector2Channels(ref mask, ref from, ref to);
			if (EditorGUI.EndChangeCheck())
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
