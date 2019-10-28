using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(TweenMaterialRange))]
	class TweenMaterialRangeEditor : TweenMaterialPropertyEditor<TweenMaterialRange>
	{
		protected SerializedProperty from;
		protected SerializedProperty to;

		float min, max;


		protected override void OnEnable()
		{
			base.OnEnable();

			from = serializedObject.FindProperty("from");
			to = serializedObject.FindProperty("to");
		}


		protected override void OnDrawSubclass()
		{
			serializedObject.Update();

			min = ShaderUtil.GetRangeLimits(shader, propertyIndex[menuIndex], 1);
			max = ShaderUtil.GetRangeLimits(shader, propertyIndex[menuIndex], 2);

			DrawSliderLayout(from.floatValue, min, max, value => from.floatValue = value, "From");
			DrawSliderLayout(to.floatValue, min, max, value => to.floatValue = value, "To");

			serializedObject.ApplyModifiedProperties();
		}
	}
}
