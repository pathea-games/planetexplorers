using UnityEngine;
using UnityEditor;
using WhiteCat.Internal;

namespace WhiteCat
{
	[CustomEditor(typeof(BezierPath))]
	class BezierPathEditor : PathEditor<BezierPath, BezierNode, BezierSpline>
	{
		protected override float nodeElementHeight { get { return 58; } }
		protected override float splineElementHeight { get { return 40; } }


		protected override void DrawNodeElement(Rect rect, int index)
		{
			rect.height = 16;
			float labelWidth = rect.width * 0.25f;
			GUI.Label(rect, "Position");
			rect.xMin += labelWidth;
			DrawVector3Field(rect, target.GetNodeLocalPosition(index),
				value => target.SetNodeLocalPosition(index, value));

			rect.y += 18;
			rect.x -= labelWidth;
			GUI.Label(rect, "Rotation");
			rect.x += labelWidth;
			DrawVector3Field(rect, target.GetNodeLocalRotation(index).eulerAngles,
				value => target.SetNodeLocalRotation(index, Quaternion.Euler(value)));

			rect.y += 18;
			rect.x -= labelWidth;
			GUI.Label(rect, "Tangent");

			float editorLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 13;

			rect.width = (rect.width - 4) / 3;
			rect.x += labelWidth;
			DrawFloatField(rect, target.GetNodeLocalForwardTangent(index), value => target.SetNodeLocalForwardTangent(index, value), "F");

			rect.x += rect.width + 2;
			DrawFloatField(rect, target.GetNodeLocalBackTangent(index), value => target.SetNodeLocalBackTangent(index, value), "B");

			EditorGUIUtility.labelWidth = editorLabelWidth;
		}


		protected override void DrawSplineElement(Rect rect, int index)
		{
			EditorGUI.BeginDisabledGroup(true);

			if (target.IsSplineSamplesInvalid(index))
			{
				rect.height = 16;
				EditorGUI.TextField(rect, "Samples Count", "");
				rect.y += 18;
				EditorGUI.TextField(rect, "Length", "");
			}
			else
			{
				rect.height = 16;
				EditorGUI.IntField(rect, "Samples Count", target.GetSplineSamplesCount(index));
				rect.y += 18;
				float splineLength = target.GetSplineTotalLength(index);
				float t = target.GetPathLength(index) / target.pathTotalLength;
				EditorGUI.TextField(rect, "Length", splineLength.ToString() + " (" + t.ToString() + ")");
			}

			EditorGUI.EndDisabledGroup();
		}


		int handleIndex = 0;
		int nodesCount;
		float handleSize;
		float forwardTangent, backTangent;
		Vector3 position, forwardPoint, backPoint;
		Vector3 normal;
		Quaternion rotation = Quaternion.identity;


		protected override void OnSceneGUI()
		{
			base.OnSceneGUI();

			nodesCount = target.nodesCount;

			for (int i = 0; i < nodesCount; i++)
			{
				position = target.GetNodePosition(i);
				backPoint = target.GetNodeBackPoint(i);
				forwardPoint = target.GetNodeForwardPoint(i);

				Handles.color = PathEditorSettings.lineColor;
				Handles.DrawLine(backPoint, forwardPoint);

				handleSize = HandleUtility.GetHandleSize(position);

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
					handleIndex = 0;
					nodesList.index = i;
					Repaint();
				}

				EditorKit.BeginHandleEventCheck();
				SmartDraw(
					() => Handles.FreeMoveHandle(forwardPoint, rotation, HandleUtility.GetHandleSize(forwardPoint) * PathEditorSettings.dotCapSize, Vector3.zero, Handles.CircleCap),
					value => { if (freeDrag) target.SetNodeForwardPoint(i, value); });
				if (EditorKit.EndHandleEventCheck() == HandleEvent.release)
				{
					handleIndex = 1;
					nodesList.index = i;
					Repaint();
				}

				EditorKit.BeginHandleEventCheck();
				SmartDraw(
					() => Handles.FreeMoveHandle(backPoint, rotation, HandleUtility.GetHandleSize(backPoint) * PathEditorSettings.dotCapSize, Vector3.zero, Handles.CircleCap),
					value => { if (freeDrag) target.SetNodeBackPoint(i, value); });
				if (EditorKit.EndHandleEventCheck() == HandleEvent.release)
				{
					handleIndex = -1;
					nodesList.index = i;
					Repaint();
				}
			}

			if (useTools && nodesList.index >= 0 && nodesList.index < nodesCount)
			{
				if (Tools.current == Tool.Move)
				{
					switch(handleIndex)
					{
						case 0:
							SmartDraw(
								() => Handles.PositionHandle(target.GetNodePosition(nodesList.index),
									Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : target.transform.rotation),
								value => target.SetNodePosition(nodesList.index, value));
							return;

						case 1:
							SmartDraw(
								() => Handles.PositionHandle(target.GetNodeForwardPoint(nodesList.index),
									Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : target.transform.rotation),
								value => target.SetNodeForwardPoint(nodesList.index, value));
							return;

						case -1:
							SmartDraw(
								() => Handles.PositionHandle(target.GetNodeBackPoint(nodesList.index),
									Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : target.transform.rotation),
								value => target.SetNodeBackPoint(nodesList.index, value));
							return;
					}
				}

				else if (Tools.current == Tool.Rotate)
				{
					SmartDraw(
						() => Handles.RotationHandle(target.GetNodeRotation(nodesList.index), target.GetNodePosition(nodesList.index)),
						value => target.SetNodeRotation(nodesList.index, value));
				}

				else if (Tools.current == Tool.Scale)
				{
					int index = nodesList.index;
					position = target.GetNodePosition(index);
					rotation = target.GetNodeRotation(index);
					float size = HandleUtility.GetHandleSize(position);
					Handles.color = PathEditorSettings.dotCapColor;

					SmartDraw(() => Handles.ScaleSlider(
						target.GetNodeLocalForwardTangent(index), position, rotation * Vector3.forward, rotation, size, 0),
						value => target.SetNodeLocalForwardTangent(index, value));

					SmartDraw(() => Handles.ScaleSlider(
						target.GetNodeLocalBackTangent(index), position, rotation * Vector3.back, rotation, size, 0),
						value => target.SetNodeLocalBackTangent(index, value));

					EditorKit.BeginHandleEventCheck();
					SmartDraw(() => Handles.ScaleValueHandle(1, position, rotation, size, Handles.CubeCap, 0),
						value =>
						{
							target.SetNodeLocalForwardTangent(index, value * forwardTangent);
							target.SetNodeLocalBackTangent(index, value * backTangent);
						});
					if (EditorKit.EndHandleEventCheck() == HandleEvent.select)
					{
						forwardTangent = target.GetNodeLocalForwardTangent(index);
						backTangent = target.GetNodeLocalBackTangent(index);
					}
				}
			}
		}
	}
}