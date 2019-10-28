using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	abstract class TweenMaterialPropertyEditor<T> : TweenEditor<T> where T : TweenMaterialProperty
	{
		GUIContent refMaterialLabel = new GUIContent("Material");

		SerializedProperty refMaterial, renderer, materialIndex, useSharedMaterial;

		protected Material material;
		protected Shader shader;
		protected string[] propertyDescription;
		protected string[] propertyName;
		protected int[] propertyIndex;
		protected int menuIndex;


		protected override void OnEnable()
		{
			base.OnEnable();

			refMaterial = serializedObject.FindProperty("_refMaterial");
			renderer = serializedObject.FindProperty("renderer");
			materialIndex = serializedObject.FindProperty("materialIndex");
			useSharedMaterial = serializedObject.FindProperty("useSharedMaterial");
		}


		protected abstract void OnDrawSubclass();


		public override sealed void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.PropertyField(refMaterial, refMaterialLabel);

			if (refMaterial.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(renderer);
				EditorGUILayout.PropertyField(useSharedMaterial);

				if (target.renderer)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(materialIndex);
					if (EditorGUI.EndChangeCheck())
					{
						materialIndex.intValue = Mathf.Clamp(materialIndex.intValue, 0, target.renderer.sharedMaterials.Length - 1);
					}
				}
			}

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();

			material = target.material;
			if (material)
			{
				// (重新)建立属性表
				if(shader != material.shader)
				{
					shader = material.shader;
					int count = ShaderUtil.GetPropertyCount(shader);

					List<int> indexList = new List<int>(count);
					for (int i = 0; i < count; i++)
					{
						if(ShaderUtil.GetPropertyType(shader, i) == target.propertyType)
						{
							indexList.Add(i);
						}
					}

					propertyIndex = indexList.ToArray();
					propertyName = new string[propertyIndex.Length];
					propertyDescription = new string[propertyIndex.Length];
					for (int i = 0; i < propertyIndex.Length; i++)
					{
						propertyName[i] = ShaderUtil.GetPropertyName(shader, propertyIndex[i]);
						propertyDescription[i] = ShaderUtil.GetPropertyDescription(shader, propertyIndex[i]);
					}

					if (propertyName.Length > 0)
					{
						menuIndex = System.Array.IndexOf(propertyName, target.propertyName);
						if (menuIndex < 0)
						{
							Undo.RecordObject(target, undoString);
							target.propertyName = propertyName[menuIndex = 0];
							EditorUtility.SetDirty(target);
						}
					}
				}

				// 绘制属性和子类参数
				if (propertyName.Length > 0)
				{
					DrawIntPopupLayout(menuIndex, propertyDescription, value => target.propertyName = propertyName[menuIndex = value], "Property");
					OnDrawSubclass();
					return;
				}
			}

			EditorGUILayout.LabelField("* No material or available property.");
		}
	}
}
