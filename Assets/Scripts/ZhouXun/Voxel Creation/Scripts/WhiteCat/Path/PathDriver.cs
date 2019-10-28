using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 路径驱动器
	/// </summary>
	[AddComponentMenu("White Cat/Path/Path Driver")]
	[ExecuteInEditMode]
	public class PathDriver : BaseBehaviour
	{
		[SerializeField][GetSet("path")] Path _path;
		[SerializeField][GetSet("location")] float _location;

		public RotationMode rotationMode;
		public bool reverseForward = false;
		[Direction] public Vector3 constantUp = Vector3.up;

		int _splineIndex = -1;
		float _splineTime;


		public Path path
		{
			get { return _path; }
			set
			{
				if (value != _path)
				{
					if (value && value.gameObject == gameObject)
					{
#if UNITY_EDITOR
						UnityEditor.EditorUtility.DisplayDialog("Error", "Can not set path from same game object.", "OK");
#endif
						return;
					}

					if (_path) _path.onChange -= Sync;
					_path = value;
					if (_path) _path.onChange += Sync;
				}
			}
		}


		public float location
		{
			get { return _location; }
			set
			{
				_location = value;
				if (_path) Sync();
			}
		}

		public float normalizedLocation
		{
			get { return _location / path.pathTotalLength; }
			set
			{
				_location = value * path.pathTotalLength;
				if (_path) Sync();
			}
		}


		void OnDestroy()
		{
			if (_path) _path.onChange -= Sync;
		}


		void Sync()
		{
			if (!_path.isCircular) _location = Mathf.Clamp(_location, 0, _path.pathTotalLength);

			_path.GetPathPositionAtPathLength(_location, ref _splineIndex, ref _splineTime);

			transform.position = _path.GetSplinePoint(_splineIndex, _splineTime);

			switch (rotationMode)
			{
				case RotationMode.ConstantUp:
					transform.rotation = _path.GetSplineRotation(_splineIndex, _splineTime, constantUp, reverseForward);
					return;

				case RotationMode.SlerpNodes:
					transform.rotation = _path.GetSplineRotation(_splineIndex, _splineTime, reverseForward);
					return;

				case RotationMode.MinimizeDelta:
					transform.rotation = _path.GetSplineRotation(_splineIndex, _splineTime, transform.up, reverseForward);
					return;
			}
		}


#if UNITY_EDITOR

		const float _arrowSize = 1.0f;
		static Color _arrowColor = new Color(0, 1, 0, 0.4f);


		void OnDrawGizmosSelected()
		{
			if (rotationMode == RotationMode.ConstantUp)
			{
				Vector3 pos = transform.position;
				float showSize = UnityEditor.HandleUtility.GetHandleSize(pos) * _arrowSize;
				UnityEditor.Handles.color = _arrowColor;
				UnityEditor.Handles.ArrowCap(0, pos, Quaternion.LookRotation(constantUp), showSize);
			}
		}

#endif
	}
}