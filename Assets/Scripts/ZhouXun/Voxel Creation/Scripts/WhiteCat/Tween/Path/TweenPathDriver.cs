using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 对路径驱动器位置插值
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Path/Driver")]
	[RequireComponent(typeof(PathDriver))]
	public class TweenPathDriver : TweenBase
	{
		[SerializeField][GetSet("from")] float _from;
		[SerializeField][GetSet("to")] float _to;
		float _original;


		PathDriver _driver;
		PathDriver driver { get { return _driver ? _driver : _driver = GetComponent<PathDriver>(); } }


		public float from
		{
			get { return _from; }
			set
			{
				if (driver.path && !driver.path.isCircular)
				{
					_from = Mathf.Clamp(value, 0, driver.path.pathTotalLength);
				}
				else _from = value;
			}
		}


		public float to
		{
			get { return _to; }
			set
			{
				if (driver.path && !driver.path.isCircular)
				{
					_to = Mathf.Clamp(value, 0, driver.path.pathTotalLength);
				}
				else _to = value;
			}
		}


		public override void OnTween(float factor)
		{
			driver.location = (to - from) * factor + from;
		}


		public override void OnRecord() { _original = driver.location; }

		public override void OnRestore() { driver.location = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent() { from = driver.location; }

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent() { to = driver.location; }

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom() { driver.location = from; }

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo() { driver.location = to; }


#if UNITY_EDITOR

		const float discSize = 0.2f, arrowSize = 0.7f;
		static Color beginColor = new Color(1, 0.5f, 0);
		static Color endColor = new Color(0, 0.75f, 1);

		static Vector3 position, direction;
		static int index;
		static float time, handleSize, size;


		void OnDrawGizmosSelected()
		{
			if (driver.path)
			{
				index = -1;
				driver.path.GetPathPositionAtPathLength(from, ref index, ref time);
				position = driver.path.GetSplinePoint(index, time);
				direction = driver.path.GetSplineDerivative(index, time);
				handleSize = UnityEditor.HandleUtility.GetHandleSize(position);
				size = handleSize * arrowSize;

				UnityEditor.Handles.color = beginColor;
				UnityEditor.Handles.DrawWireDisc(position, direction, handleSize * discSize);
				UnityEditor.Handles.ArrowCap(0, position, Quaternion.LookRotation(from > to ? -direction : direction), size);

				index = -1;
				driver.path.GetPathPositionAtPathLength(to, ref index, ref time);
				position = driver.path.GetSplinePoint(index, time);
				direction = driver.path.GetSplineDerivative(index, time);
				handleSize = UnityEditor.HandleUtility.GetHandleSize(position);
				size = handleSize * arrowSize;

				UnityEditor.Handles.color = endColor;
				UnityEditor.Handles.DrawWireDisc(position, direction, handleSize * discSize);
				if (from > to) direction = -direction;
				UnityEditor.Handles.ArrowCap(0, position - direction.normalized * size, Quaternion.LookRotation(direction), size);
			}
		}

#endif
	}
}