using UnityEngine;
using UnityEditor;
using System.Reflection;
using WhiteCat.ReflectionExtension;
using WhiteCat.Internal;

namespace WhiteCat
{
	struct PathEditorSettings
	{
		public const float dotCapSize = 0.05f;
		public static Color normalColor = new Color(0.5f, 1, 0, 0.7f);
		public static Color dotCapColor = new Color(0.2f, 0.7f, 1);
		public static Color selectedDotCapColor = new Color(0, 1, 0.75f);
		public static Color lineColor = new Color(0.5f, 0.5f, 0.5f, 1);
		public static Color darkBackgroundColor = new Color(0, 0, 0, 0.1f);
		public static string[] headerLabels = { "Nodes", "Splines" };


		static Texture[] _menuIcon;
		public static Texture menuIcon
		{
			get
			{
				if (_menuIcon == null)
				{
					_menuIcon = new Texture[2]
					{
						EditorKit.LoadAssetAtRelativePath<Texture>("Path/Editor/Skin/Light/menuIcon.png"),
						EditorKit.LoadAssetAtRelativePath<Texture>("Path/Editor/Skin/Dark/menuIcon.png")
					};
				}
				return _menuIcon[EditorGUIUtility.isProSkin ? 1 : 0];
			}
		}
	}


	abstract class PathEditor<Path, Node, Spline> : Editor<Path> where Path : WhiteCat.Path where Node : PathNode where Spline : PathSpline
	{
		protected ReorderableList<Node> nodesList;
		protected ReorderableList<Spline> splineList;

		SerializedProperty isCircular;
		SerializedProperty lenghtError;

		FieldInfo selectedSplineField;
		int indexToInsert = -1;
		int indexToRemove = -1;

		protected bool freeDrag = false;
		protected bool useTools = false;
		protected bool showNormals = false;

		bool foldout = true;
		int headerIndex = 0;

		protected abstract float nodeElementHeight { get; }
		protected abstract float splineElementHeight { get; }
		protected abstract void DrawNodeElement(Rect rect, int index);
		protected abstract void DrawSplineElement(Rect rect, int index);


		void OnEnable()
		{
			selectedSplineField = target.GetFieldInfo("_selectedSplineIndex");

			isCircular = serializedObject.FindProperty("_isCircular");
			lenghtError = serializedObject.FindProperty("_lengthError");

			nodesList = new ReorderableList<Node>(target.GetFieldValue("_nodes"), false, true, false, false);
			splineList = new ReorderableList<Spline>(target.GetFieldValue("_splines"), false, true, false, false);

			nodesList.headerHeight = 3;
			nodesList.footerHeight = 1;
			nodesList.elementHeight = nodeElementHeight + 19;
			nodesList.drawHeaderCallback = null;
			nodesList.drawElementCallback = DrawNodeElement;
			nodesList.onSelectCallback = list =>
				{
					SceneView.lastActiveSceneView.LookAt(target.GetNodePosition(list.index));
				};

			splineList.headerHeight = 3;
			splineList.footerHeight = 1;
			splineList.elementHeight = splineElementHeight + 19;
			splineList.drawHeaderCallback = null;
			splineList.drawElementCallback = DrawSplineElement;
			splineList.onSelectCallback = list =>
				{
					selectedSplineField.SetValue(target, list.index);
					SceneView.lastActiveSceneView.LookAt(target.GetSplinePoint(list.index, 0.5f));
				};
		}


		void OnDisable()
		{
			Tools.hidden = false;
			selectedSplineField.SetValue(target, -1);
		}


		void PopupMenu(int nodeIndex, Rect rect)
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Insert Before"), false, () => { indexToInsert = nodeIndex; Repaint(); } );
			menu.AddItem(new GUIContent("Insert After"), false, () => { indexToInsert = nodeIndex + 1; Repaint(); } );

			menu.AddSeparator(null);
			menu.AddItem(new GUIContent("Insert Before First"), false, () => { indexToInsert = 0; Repaint(); });
			menu.AddItem(new GUIContent("Insert After Last"), false, () => { indexToInsert = target.nodesCount; Repaint(); });

			menu.AddSeparator(null);
			if (target.isNodeRemovable)
			{
				menu.AddItem(new GUIContent("Remove"), false, () => { indexToRemove = nodeIndex; Repaint(); });
			}
			else menu.AddDisabledItem(new GUIContent("Remove"));

			menu.DropDown(rect);
		}


		public override void OnInspectorGUI()
		{
			target.CheckAndResetAllSplines();

			EditorGUILayout.GetControlRect(false, 2);
			Rect rect = EditorGUILayout.GetControlRect(false, 18);
			rect.width = (rect.width - 8) * 0.3f;

			freeDrag = GUI.Toggle(rect, freeDrag, "Free Drag", "Button");

			EditorGUI.BeginChangeCheck();
			rect.x += rect.width + 4;
			useTools = GUI.Toggle(rect, useTools, "Use Tools", "Button");
			if (EditorGUI.EndChangeCheck()) Tools.hidden = useTools;

			rect.x += rect.width + 4;
			rect.width *= 1.3333f;
			showNormals = GUI.Toggle(rect, showNormals, "Show Normals", "Button");

			EditorGUILayout.Space();
			serializedObject.Update();
			EditorGUILayout.PropertyField(isCircular);
			EditorGUILayout.PropertyField(lenghtError);
			serializedObject.ApplyModifiedProperties();

			rect = EditorGUILayout.GetControlRect();
			GUI.Label(rect, "Path Length");

			rect.xMin += EditorGUIUtility.labelWidth;
			rect.width = (rect.width - 4) * 0.44f;
			EditorGUI.BeginDisabledGroup(true);
			if (target.isLengthValid) EditorGUI.FloatField(rect, GUIContent.none, target.pathTotalLength);
			else EditorGUI.TextField(rect, GUIContent.none, "");
			EditorGUI.EndDisabledGroup();

			rect.x += rect.width + 4;
			rect.width *= 1.2727f;
			if (target.isLengthValid)
			{
				if (GUI.Button(rect, "Clear"))
				{
					Undo.RecordObject(target, undoString);
					target.ClearAllSamples();
					EditorUtility.SetDirty(target);
				}
			}
			else
			{
				if (GUI.Button(rect, "Calculate"))
				{
					Undo.RecordObject(target, undoString);
					target.CalculatePathLength(target.splinesCount - 1);
					EditorUtility.SetDirty(target);
				}
				
			}

			EditorGUILayout.Space();
			rect = EditorGUILayout.GetControlRect();
			float buttonWidth = rect.width - EditorGUIUtility.labelWidth;
			rect.width = EditorGUIUtility.labelWidth;

			EditorGUI.BeginChangeCheck();
			foldout = EditorGUI.Foldout(rect, foldout, "Details");
			if (EditorGUI.EndChangeCheck())
			{
				selectedSplineField.SetValue(target, foldout && headerIndex == 1 ? splineList.index : -1);
			}

			if (foldout)
			{
				rect.x = rect.xMax + 1;
				rect.width = buttonWidth - 2;

				EditorGUI.BeginChangeCheck();
				headerIndex = GUI.Toolbar(rect, headerIndex, PathEditorSettings.headerLabels);
				if(EditorGUI.EndChangeCheck())
				{
					selectedSplineField.SetValue(target, headerIndex == 0 ? -1 : splineList.index);
				}

				EditorGUILayout.GetControlRect(false, 0);
				if (headerIndex == 0) nodesList.DoLayoutList();
				else splineList.DoLayoutList();

				// 执行移除
				if (indexToRemove >= 0)
				{
					Undo.RecordObject(target, undoString);
					target.RemoveNode(indexToRemove);
					EditorUtility.SetDirty(target);
					nodesList.index = indexToRemove == 0 ? 0 : indexToRemove - 1;
					indexToRemove = -1;
				}

				// 执行插入
				if (indexToInsert >= 0)
				{
					Undo.RecordObject(target, undoString);
					target.InsertNode(indexToInsert);
					EditorUtility.SetDirty(target);
					nodesList.index = indexToInsert;
					indexToInsert = -1;
				}
			}
		}


		void DrawNodeElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (!isActive && index % 2 == 1)
			{
				rect.xMin -= 4;
				rect.xMax += 4;
				EditorGUI.DrawRect(rect, PathEditorSettings.darkBackgroundColor);
				rect.xMin += 4;
				rect.xMax -= 4;
			}
			GUI.Label(rect, "Node " + index, EditorStyles.boldLabel);

			rect.yMin += 19;
			DrawNodeElement(rect, index);

			rect.y -= 19;
			rect.x = rect.xMax - 19;
			rect.width = rect.height = 19;
			if (GUI.Button(rect, PathEditorSettings.menuIcon, GUIStyle.none))
			{
				PopupMenu(index, rect);
			}
		}


		void DrawSplineElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (!isActive && index % 2 == 1)
			{
				rect.xMin -= 4;
				rect.xMax += 4;
				EditorGUI.DrawRect(rect, PathEditorSettings.darkBackgroundColor);
				rect.xMin += 4;
				rect.xMax -= 4;
			}
			EditorGUI.BeginDisabledGroup(index >= target.splinesCount);
				
			GUI.Label(rect, "Spline " + index, EditorStyles.boldLabel);
			rect.yMin += 19;
			DrawSplineElement(rect, index);
				
			EditorGUI.EndDisabledGroup();
		}


		protected virtual void OnSceneGUI()
		{
			if(nodesList.index >= 0 && nodesList.index < target.nodesCount)
			{
				Event evt = Event.current;

				if (evt.type == EventType.keyDown)
				{
					if (evt.character == 'F' || evt.character == 'f')
					{
						evt.Use();
						SceneView.lastActiveSceneView.LookAt(target.GetNodePosition(nodesList.index));
					}
				}
				else if(evt.type == EventType.mouseDown)
				{
					if (evt.button == 1)
					{
						evt.Use();
					}
				}
				else if(evt.type == EventType.MouseUp)
				{
					if (evt.button == 1)
					{
						evt.Use();
						PopupMenu(nodesList.index, new Rect(evt.mousePosition.x, evt.mousePosition.y, 0, 0));
					}
				}
			}
		}
	}
}