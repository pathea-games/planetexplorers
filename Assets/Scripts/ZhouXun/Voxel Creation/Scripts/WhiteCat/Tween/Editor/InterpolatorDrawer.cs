using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.ReflectionExtension;

namespace WhiteCat
{
	[CustomPropertyDrawer(typeof(InterpolatorAttribute), true)]
	public class InterpolatorDrawer : AttributePropertyDrawer<InterpolatorAttribute>
	{
		static Texture[] _addButton;
		static Texture addButton
		{
			get
			{
				if (_addButton == null)
				{
					_addButton = new Texture[2]
					{
						EditorKit.LoadAssetAtRelativePath<Texture>("Tween/Editor/Skin/Light/add.png"),
						EditorKit.LoadAssetAtRelativePath<Texture>("Tween/Editor/Skin/Dark/add.png")
					};
				}
				return _addButton[EditorGUIUtility.isProSkin ? 1 : 0];
			}
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}


		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			TweenBase target = property.serializedObject.targetObject as TweenBase;
			Object interpolator = target.interpolator;

			if (!interpolator) rect.xMax -= rect.height + 4;

			EditorGUI.BeginChangeCheck();
			interpolator = EditorGUI.ObjectField(rect, "interpolator", interpolator, typeof(TweenInterpolator), true);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Set Interpolator");
				target.interpolator = interpolator as TweenInterpolator;
				EditorUtility.SetDirty(target);
			}

			if (!interpolator)
			{
				rect.xMin = rect.xMax + 4;
				rect.width = rect.height;

				if (GUI.Button(rect, addButton, GUIStyle.none))
				{
					Undo.AddComponent<TweenInterpolator>(target.gameObject);
				}
			}
		}
	}
}
