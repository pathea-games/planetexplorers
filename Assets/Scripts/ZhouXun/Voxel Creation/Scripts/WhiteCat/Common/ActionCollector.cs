using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WhiteCat
{

	public class ActionCollector : MonoBehaviour
	{
		[SerializeField] UnityEvent _action = new UnityEvent();


		public event UnityAction action
		{
			add { _action.AddListener(value); }
			remove { _action.RemoveListener(value); }
		}


		public void InvokeActions()
		{
			_action.Invoke();
		}


#if UNITY_EDITOR

		[CustomEditor(typeof(ActionCollector))]
		class ActionCollectorInspector : Editor
		{
			SerializedProperty _actionProperty;


			void OnEnable()
			{
				_actionProperty = serializedObject.FindProperty("_action");
			}


			public override void OnInspectorGUI()
			{
				Rect rect = EditorGUILayout.GetControlRect(true, 18f);
				rect.width = EditorGUIUtility.labelWidth;

				if (GUI.Button(rect, "Invoke"))
				{
					(target as ActionCollector).InvokeActions();
				}

				serializedObject.Update();
				EditorGUILayout.PropertyField(_actionProperty);
				serializedObject.ApplyModifiedProperties();
            }
		}

#endif
	}

}