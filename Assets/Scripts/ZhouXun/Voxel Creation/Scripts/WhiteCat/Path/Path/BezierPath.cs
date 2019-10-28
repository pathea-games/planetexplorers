using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 贝塞尔样条路径
	/// </summary>
	[AddComponentMenu("White Cat/Path/Bezier Path")]
	public class BezierPath : GenericPath<BezierNode, BezierSpline>
	{
		// Node:	...   0     1     2     3     4   ...
		// Curve:	   ...   0     1     2     3   ...


		/// <summary>
		/// 样条总数
		/// </summary>
		public override int splinesCount
		{
			get { return _isCircular ? _splines.Count : _splines.Count - 1; }
		}


		/// <summary>
		/// 节点是否可以移除
		/// </summary>
		public override bool isNodeRemovable { get { return _nodes.Count > 2; } }


		// 初始化节点表和样条表
		protected override void InitializeNodesAndSplines()
		{
			_nodes = new List<BezierNode>(4);
			_splines = new List<BezierSpline>(4);

			_nodes.Add(new BezierNode(new Vector3(0, 0, 0), Quaternion.identity, 3, 3));
			_nodes.Add(new BezierNode(new Vector3(0, 0, 10), Quaternion.identity, 3, 3));
			_splines.Add(new BezierSpline(_lengthError));
			_splines.Add(new BezierSpline(_lengthError));
		}


		// 更新样条参数
		protected override void UpdateSplineParameters(int splineIndex)
		{
			int count = _splines.Count;
			splineIndex = (splineIndex + count) % count;

			_splines[splineIndex].SetBezierParameters(
				transform.TransformPoint(_nodes[splineIndex].localPosition),
				transform.TransformPoint(_nodes[splineIndex].localForwardPoint),
				transform.TransformPoint(_nodes[(splineIndex + 1) % count].localBackPoint),
				transform.TransformPoint(_nodes[(splineIndex + 1) % count].localPosition));

			if (_invalidSplineLengthIndex > splineIndex) _invalidSplineLengthIndex = splineIndex;
		}




		/// <summary>
		/// 插入节点。index 范围是 [0, nodesCount]
		/// </summary>
		public void InsertNode(int nodeIndex, Vector3 localPosition, Quaternion localRotation, float forwardTangent, float backTangent)
		{
			CheckAndResetAllSplines();

			BezierNode node = new BezierNode(localPosition, localRotation, forwardTangent, backTangent);
			BezierSpline spline = new BezierSpline(_lengthError);

			_nodes.Insert(nodeIndex, node);
			_splines.Insert(nodeIndex, spline);

			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 插入节点。index 范围是 [0, nodesCount], 节点位置自动计算
		/// </summary>
		public override void InsertNode(int nodeIndex)
		{
			CheckAndResetAllSplines();

			Vector3 localPosition;
			Quaternion localRotation;
			float tangent;
			int count = _nodes.Count;

			if (_isCircular || (nodeIndex > 0 && nodeIndex < count))
			{
				BezierSpline spline = _splines[(nodeIndex - 1 + count) % count];
				BezierNode prevNode = _nodes[(nodeIndex - 1 + count) % count];
				BezierNode nextNode = _nodes[nodeIndex % count];

				localPosition = transform.InverseTransformPoint(spline.GetPoint(0.5f));

				localRotation = Quaternion.LookRotation(
					transform.InverseTransformVector(spline.GetDerivative(0.5f)),
					Quaternion.Slerp(prevNode.localRotation, nextNode.localRotation, 0.5f) * Vector3.up
					);

				tangent = (prevNode.localForwardTangent + nextNode.localBackTangent) * 0.5f;
			}
			else
			{
				if (nodeIndex == 0)
				{
					localRotation = _nodes[0].localRotation;

					localPosition = _nodes[0].localPosition + localRotation * Vector3.back
						* (_nodes[1].localPosition - _nodes[0].localPosition).magnitude;

					tangent = _nodes[0].localBackTangent;
				}
				else
				{
					localRotation = _nodes[count - 1].localRotation;

					localPosition = _nodes[count - 1].localPosition + localRotation * Vector3.forward
						* (_nodes[count - 2].localPosition - _nodes[count - 1].localPosition).magnitude;

					tangent = _nodes[count - 1].localForwardTangent;
				}
			}

			InsertNode(nodeIndex, localPosition, localRotation, tangent, tangent);
		}


		/// <summary>
		/// 移除节点。index 范围是 [0, nodesCount), 节点总数大于 2 才执行
		/// </summary>
		public override void RemoveNode(int nodeIndex)
		{
			if (_nodes.Count > 2)
			{
				CheckAndResetAllSplines();

				_nodes.RemoveAt(nodeIndex);
				_splines.RemoveAt(nodeIndex);

				UpdateSplineParameters(nodeIndex - 1);

				TriggerOnChangeEvents();
			}
		}




		/// <summary>
		/// 设置节点本地位置
		/// </summary>
		public override void SetNodeLocalPosition(int nodeIndex, Vector3 localPosition)
		{
			CheckAndResetAllSplines();

			_nodes[nodeIndex].localPosition = localPosition;

			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 设置节点本地旋转
		/// </summary>
		public override void SetNodeLocalRotation(int nodeIndex, Quaternion localRotation)
		{
			CheckAndResetAllSplines();

			_nodes[nodeIndex].localRotation = localRotation;

			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 获取节点本地前向切线长度
		/// </summary>
		public float GetNodeLocalForwardTangent(int nodeIndex)
		{
			return _nodes[nodeIndex].localForwardTangent;
		}


		/// <summary>
		/// 设置节点本地前向切线长度
		/// </summary>
		public void SetNodeLocalForwardTangent(int nodeIndex, float localForwardTangent)
		{
			CheckAndResetAllSplines();

			_nodes[nodeIndex].localForwardTangent = localForwardTangent;

			UpdateSplineParameters(nodeIndex);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 获取节点本地后向切线长度
		/// </summary>
		public float GetNodeLocalBackTangent(int nodeIndex)
		{
			return _nodes[nodeIndex].localBackTangent;
		}


		/// <summary>
		/// 设置节点本地后向切线长度
		/// </summary>
		public void SetNodeLocalBackTangent(int nodeIndex, float localBackTangent)
		{
			CheckAndResetAllSplines();

			_nodes[nodeIndex].localBackTangent = localBackTangent;

			UpdateSplineParameters(nodeIndex - 1);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 获取节点前向切线端点位置
		/// </summary>
		public Vector3 GetNodeForwardPoint(int nodeIndex)
		{
			return transform.TransformPoint(_nodes[nodeIndex].localForwardPoint);
		}


		/// <summary>
		/// 设置节点前向切线端点位置
		/// </summary>
		public void SetNodeForwardPoint(int nodeIndex, Vector3 forwardPoint)
		{
			CheckAndResetAllSplines();

			_nodes[nodeIndex].localForwardPoint = transform.InverseTransformPoint(forwardPoint);

			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 获取节点后向切线端点位置
		/// </summary>
		public Vector3 GetNodeBackPoint(int nodeIndex)
		{
			return transform.TransformPoint(_nodes[nodeIndex].localBackPoint);
		}


		/// <summary>
		/// 设置节点后向切线端点位置
		/// </summary>
		public void SetNodeBackPoint(int nodeIndex, Vector3 backPoint)
		{
			CheckAndResetAllSplines();

			_nodes[nodeIndex].localBackPoint = transform.InverseTransformPoint(backPoint);

			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			TriggerOnChangeEvents();
		}




		/// <summary>
		/// 获取样条某点处的旋转，旋转朝向切线方向，up 方向由节点旋转插值获得
		/// </summary>
		public override Quaternion GetSplineRotation(int splineIndex, float splineTime, bool reverseForward = false)
		{
			CheckAndResetAllSplines();
			return Quaternion.LookRotation(
				reverseForward ? -_splines[splineIndex].GetDerivative(splineTime) : _splines[splineIndex].GetDerivative(splineTime),
				transform.TransformVector(Quaternion.Slerp(
						_nodes[splineIndex].localRotation,
						_nodes[(splineIndex + 1) % _nodes.Count].localRotation,
						splineTime) * Vector3.up));
		}

	}
}