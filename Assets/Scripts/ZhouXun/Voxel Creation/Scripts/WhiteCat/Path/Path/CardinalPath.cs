using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 基数样条路径
	/// </summary>
	[AddComponentMenu("White Cat/Path/Cardinal Path")]
	public class CardinalPath : GenericPath<CardinalNode, CardinalSpline>
	{
		// Node:	...   0     1     2     3     4   ...
		// Curve:	  ...   n-1    0     1     2   ...


		/// <summary>
		/// 样条总数
		/// </summary>
		public override int splinesCount
		{
			get { return _isCircular ? _splines.Count : _splines.Count - 3; }
		}


		/// <summary>
		/// 节点是否可以移除
		/// </summary>
		public override bool isNodeRemovable { get { return _nodes.Count > 4; } }


		// 初始化节点表和样条表
		protected override void InitializeNodesAndSplines()
		{
			_nodes = new List<CardinalNode>(4);
			_splines = new List<CardinalSpline>(4);

			for (int i = -1; i < 3; i++)
			{
				_nodes.Add(new CardinalNode(new Vector3(0, 0, i * 10), Quaternion.identity));
				_splines.Add(new CardinalSpline(_lengthError, 0.5f));
			}
		}


		// 更新样条参数
		protected override void UpdateSplineParameters(int splineIndex)
		{
			int count = _splines.Count;
			splineIndex = (splineIndex + count) % count;

			_splines[splineIndex].SetCardinalParameters(
				transform.TransformPoint(_nodes[splineIndex].localPosition),
				transform.TransformPoint(_nodes[(splineIndex + 1) % count].localPosition),
				transform.TransformPoint(_nodes[(splineIndex + 2) % count].localPosition),
				transform.TransformPoint(_nodes[(splineIndex + 3) % count].localPosition),
				_splines[splineIndex].tension);

			if (_invalidSplineLengthIndex > splineIndex) _invalidSplineLengthIndex = splineIndex;
		}


		// 更新节点旋转
		void UpdateNodeRotation(int nodeIndex)
		{
			nodeIndex = (nodeIndex + _nodes.Count) % _nodes.Count;
			int prev = nodeIndex == 0 ? _nodes.Count - 1 : nodeIndex - 1;
			int next = (nodeIndex + 1) % _nodes.Count;

			_nodes[nodeIndex].localRotation = Quaternion.LookRotation(
				_nodes[next].localPosition - _nodes[prev].localPosition,
				_nodes[nodeIndex].localRotation * Vector3.up);
		}




		/// <summary>
		/// 插入节点。index 范围是 [0, nodesCount]
		/// </summary>
		public void InsertNode(int nodeIndex, Vector3 localPosition, Vector3 localUpwards)
		{
			CheckAndResetAllSplines();

			int prev = nodeIndex == 0 ? _nodes.Count - 1 : nodeIndex - 1;
			int next = nodeIndex % _nodes.Count;

			CardinalNode node = new CardinalNode(localPosition,
				Quaternion.LookRotation(_nodes[next].localPosition - _nodes[prev].localPosition, localUpwards));

			CardinalSpline spline = new CardinalSpline(_lengthError, _splines[prev].tension);

			_nodes.Insert(nodeIndex, node);
			_splines.Insert(nodeIndex, spline);

			UpdateSplineParameters(nodeIndex - 3);
			UpdateSplineParameters(nodeIndex - 2);
			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			UpdateNodeRotation(nodeIndex - 1);
			UpdateNodeRotation(nodeIndex + 1);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 插入节点。index 范围是 [0, nodesCount], 节点位置和旋转自动计算
		/// </summary>
		public override void InsertNode(int nodeIndex)
		{
			CheckAndResetAllSplines();

			Vector3 localPosition, localUpwards;
			int count = _nodes.Count;

			if (_isCircular || (nodeIndex > 0 && nodeIndex < count))
			{
				localPosition = transform.InverseTransformPoint(
					_splines[(nodeIndex - 2 + count) % count].GetPoint(0.5f));

				localUpwards = Quaternion.Slerp(
					_nodes[(nodeIndex - 1 + count) % count].localRotation,
					_nodes[nodeIndex % count].localRotation, 0.5f
					) * Vector3.up;
			}
			else
			{
				if (nodeIndex == 0)
				{
					localPosition = _nodes[0].localPosition * 2 - _nodes[1].localPosition;
					localUpwards = _nodes[0].localRotation * Vector3.up;
				}
				else
				{
					localPosition = _nodes[count - 1].localPosition * 2 - _nodes[count - 2].localPosition;
					localUpwards = _nodes[count - 1].localRotation * Vector3.up;
				}
			}

			InsertNode(nodeIndex, localPosition, localUpwards);
		}


		/// <summary>
		/// 移除节点。index 范围是 [0, nodesCount), 节点总数大于 4 才执行
		/// </summary>
		public override void RemoveNode(int nodeIndex)
		{
			if (_nodes.Count > 4)
			{
				CheckAndResetAllSplines();

				_nodes.RemoveAt(nodeIndex);
				_splines.RemoveAt(nodeIndex);

				UpdateSplineParameters(nodeIndex - 3);
				UpdateSplineParameters(nodeIndex - 2);
				UpdateSplineParameters(nodeIndex - 1);

				UpdateNodeRotation(nodeIndex);
				UpdateNodeRotation(nodeIndex - 1);

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

			UpdateSplineParameters(nodeIndex - 3);
			UpdateSplineParameters(nodeIndex - 2);
			UpdateSplineParameters(nodeIndex - 1);
			UpdateSplineParameters(nodeIndex);

			UpdateNodeRotation(nodeIndex - 1);
			UpdateNodeRotation(nodeIndex + 1);

			TriggerOnChangeEvents();
		}


		/// <summary>
		/// 设置节点本地旋转
		/// </summary>
		public override void SetNodeLocalRotation(int nodeIndex, Quaternion localRotation)
		{
			_nodes[nodeIndex].localRotation = Quaternion.LookRotation(
				_nodes[nodeIndex].localRotation * Vector3.forward,
				localRotation * Vector3.up);

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
						_nodes[(splineIndex + 1) % _nodes.Count].localRotation,
						_nodes[(splineIndex + 2) % _nodes.Count].localRotation,
						splineTime) * Vector3.up));
		}


		/// <summary>
		/// 获取样条张力
		/// </summary>
		public float GetSplineTension(int splineIndex)
		{
			return _splines[splineIndex].tension;
		}


		/// <summary>
		/// 设置样条张力
		/// </summary>
		public void SetSplineTension(int splineIndex, float tension)
		{
			CheckAndResetAllSplines();

			_splines[splineIndex].tension = tension;

			UpdateSplineParameters(splineIndex);

			TriggerOnChangeEvents();
		}
	}
}