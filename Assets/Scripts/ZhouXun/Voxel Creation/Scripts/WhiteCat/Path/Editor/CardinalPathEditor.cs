using UnityEngine;
using UnityEditor;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(CardinalPath))]
	class CardinalPathEditor : PathEditor<CardinalPath, CardinalNode, CardinalSpline>
	{
		protected override float nodeElementHeight { get { return 40; } }
		protected override float splineElementHeight{ get { return 58; } }


		protected override void DrawNodeElement(Rect rect, int index)
		{
			rect.height = 16;
			GUI.Label(rect, "Position");

			float labelWidth = rect.width * 0.25f;
			rect.xMin += labelWidth;
			DrawVector3Field(rect, target.GetNodeLocalPosition(index), value => target.SetNodeLocalPosition(index, value));

			rect.y += 18;
			rect.x -= labelWidth;
			GUI.Label(rect, "Rotation");

			Vector3 angles = target.GetNodeLocalRotation(index).eulerAngles;

			float editorLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 13;

			EditorGUI.BeginDisabledGroup(true);
			rect.width = (rect.width - 4) / 3;
			rect.x += labelWidth;
			EditorGUI.FloatField(rect, "X", angles.x);
			rect.x += rect.width + 2;
			EditorGUI.FloatField(rect, "Y", angles.y);
			EditorGUI.EndDisabledGroup();

			rect.x += rect.width + 2;
			DrawFloatField(rect, angles.z, value => target.SetNodeLocalRotation(index, Quaternion.Euler(angles.x, angles.y, value)), "Z");

			EditorGUIUtility.labelWidth = editorLabelWidth;
		}


		protected override void DrawSplineElement(Rect rect, int index)
		{
			rect.height = 16;
			DrawFloatField(rect, target.GetSplineTension(index), value => target.SetSplineTension(index, value), "Tension");

			EditorGUI.BeginDisabledGroup(true);

			if(target.IsSplineSamplesInvalid(index))
			{
				rect.y += 18;
				EditorGUI.TextField(rect, "Samples Count", "");
				rect.y += 18;
				EditorGUI.TextField(rect, "Length", "");
			}
			else
			{
				rect.y += 18;
				EditorGUI.IntField(rect, "Samples Count", target.GetSplineSamplesCount(index));
				rect.y += 18;
				float splineLength = target.GetSplineTotalLength(index);
				float t = target.GetPathLength(index) / target.pathTotalLength;
				EditorGUI.TextField(rect, "Length", splineLength.ToString() + " (" + t.ToString() + ")");
			}

			EditorGUI.EndDisabledGroup();
		}


		int nodesCount;
		float handleSize;
		Vector3 position;
		Vector3 normal;
		Quaternion rotation = Quaternion.identity;


		protected override void OnSceneGUI()
		{
			base.OnSceneGUI();

			nodesCount = target.nodesCount;

			if (!target.isCircular)
			{
				Handles.color = PathEditorSettings.lineColor;
				Handles.DrawLine(target.GetNodePosition(0), target.GetNodePosition(1));
				Handles.DrawLine(target.GetNodePosition(nodesCount - 2), target.GetNodePosition(nodesCount - 1));
			}

			for (int i = 0; i < nodesCount; i++)
			{
				handleSize = HandleUtility.GetHandleSize(position = target.GetNodePosition(i));

				if (showNormals)
				{
					Handles.color = PathEditorSettings.normalColor;
					normal.y = handleSize;
					Handles.DrawLine(position, position + target.GetNodeRotation(i) * normal);
				}

				Handles.color = nodesList.index == i ? PathEditorSettings.selectedDotCapColor : PathEditorSettings.dotCapColor;
				EditorKit.BeginHandleEventCheck();

				SmartDraw(
					() => Handles.FreeMoveHandle(position, rotation, handleSize * PathEditorSettings.dotCapSize, Vector3.zero, Handles.DotCap),
					value => { if (freeDrag) target.SetNodePosition(i, value); });

				if (EditorKit.EndHandleEventCheck() == HandleEvent.release)
				{
					nodesList.index = i;
					Repaint();
				}
			}

			if (useTools && nodesList.index >= 0 && nodesList.index < nodesCount)
			{
				if (Tools.current == Tool.Move)
				{
					SmartDraw(
						() => Handles.PositionHandle(
							target.GetNodePosition(nodesList.index),
							Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : target.transform.rotation),
						value => target.SetNodePosition(nodesList.index, value));
				}
				else if (Tools.current == Tool.Rotate)
				{
					Handles.color = PathEditorSettings.dotCapColor;

					SmartDraw(
						() => Handles.Disc(
							rotation = target.GetNodeRotation(nodesList.index),
							position = target.GetNodePosition(nodesList.index),
							rotation * Vector3.forward,
							HandleUtility.GetHandleSize(position),
							false,
							0),
						value => target.SetNodeRotation(nodesList.index, value));
				}
			}
		}

	}
}