using UnityEngine;

namespace WhiteCat
{
	public class UIViewController : MonoBehaviour
	{
		[SerializeField] float _startDistance = 2.5f;
		[SerializeField] float _startYaw = 22.5f;
		[SerializeField] float _startPitch = 5f;

		[SerializeField] Vector3 _targetOffset = new Vector3(0, 0.8f, 0f);
		[SerializeField] float _mouseMoveToAngle = 0.25f;
		[SerializeField] float _mouseScrollToDistance = 0.5f;
		[SerializeField] float _halfPitchRange = 45f;
		[SerializeField] float _minDistance = 0.5f;
		[SerializeField] float _maxDistance = 5f;
        [SerializeField] float _rootDistance = 5f; //lz-2016.07.22 当前距离

		[SerializeField] Transform _leftBottom;
		[SerializeField] Transform _rightTop;

		PeViewController _peViewController;
		Transform _target;
        Transform _viewRoot;
		Transform _viewCamera;

		Camera _uiCamera;
		Vector2 _lbPoint;
		Vector2 _rtPoint;
		Vector2 _cursor;
		
		bool _rightButtonPressing = false;

		float _distance;
        [SerializeField]
		float _yaw;
        [SerializeField]
		float _pitch;


		public void Init(PeViewController viewController)
		{
			_peViewController = viewController;
            _viewRoot = viewController.transform;
			_viewCamera = viewController.viewCam.transform;
        }

        /// <summary>
        /// lz-2016.07.21 获取视窗的宽高
        /// </summary>
        /// <returns></returns>
        public Vector2 GetViewSize()
        {
            if (_uiCamera == null) _uiCamera = GetComponentInParent<Camera>();
            _lbPoint = _uiCamera.WorldToScreenPoint(_leftBottom.position);
            _rtPoint = _uiCamera.WorldToScreenPoint(_rightTop.position);
            return new Vector2(Mathf.Abs(_lbPoint.x) + Mathf.Abs(_lbPoint.x), Mathf.Abs(_lbPoint.y) + Mathf.Abs(_lbPoint.y));
        }


        public void SetCameraToBastView(Vector3 centerPos, float bestDistance,float bestYaw,float scaleFactor)
        {
            if (_peViewController == null || _peViewController.target == null) return;
            _target = _peViewController.target;
            float scaleFactorDis = bestDistance * scaleFactor;
            _minDistance = bestDistance -scaleFactorDis ;
            _maxDistance = bestDistance +scaleFactorDis;
            _rootDistance = bestDistance;
            _targetOffset = centerPos-_target.position;
            _distance=_rootDistance;
            _yaw = bestYaw;
            _pitch = 0;
        }

		void OnEnable()
		{
			_distance = _startDistance;
			_yaw = _startYaw;
			_pitch = _startPitch;
        }


		void Update()
		{
			if (_peViewController == null || _peViewController.target == null) return;
			_target = _peViewController.target;

			if (!_peViewController.transform.IsChildOf(_target.parent))
			{
				_peViewController.transform.SetParent(_target.parent, true);
			}

			// 相机控制

			if (_uiCamera == null) _uiCamera = GetComponentInParent<Camera>();
			_lbPoint = _uiCamera.WorldToScreenPoint(_leftBottom.position);
			_rtPoint = _uiCamera.WorldToScreenPoint(_rightTop.position);
			_cursor = Input.mousePosition;

			bool mouseInView = _cursor.x > _lbPoint.x && _cursor.x < _rtPoint.x && _cursor.y > _lbPoint.y && _cursor.y < _rtPoint.y;

			if (_rightButtonPressing)
			{
				if (!Input.GetMouseButton(1))
                {
					_rightButtonPressing = false;
				}

				// 转动
				_yaw += Input.GetAxis("Mouse X") * _mouseMoveToAngle;
				_pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * _mouseMoveToAngle, -_halfPitchRange, _halfPitchRange);
            }
			else
			{
				if (mouseInView && Input.GetMouseButtonDown(1))
				{
					_rightButtonPressing = true;
                }
			}

			// 缩放
			if (mouseInView)
			{
				_distance = Mathf.Clamp(_distance - Input.GetAxis("Mouse ScrollWheel") * _mouseScrollToDistance, _minDistance, _maxDistance);
            }

			// 更新灯光和相机位置
            Vector3 p = new Vector3(0, _rootDistance * Mathf.Sin(_pitch * Mathf.Deg2Rad), 0);
            float xz = Mathf.Sqrt(_rootDistance * _rootDistance - p.y * p.y);
			p.x = xz * Mathf.Sin(_yaw * Mathf.Deg2Rad);
			p.z = xz * Mathf.Cos(_yaw * Mathf.Deg2Rad);

			_viewRoot.position = _target.TransformPoint(_targetOffset + p);
			_viewRoot.LookAt(_target.TransformPoint(_targetOffset));

			p.x = 0;
			p.y = 0;
            p.z = _rootDistance - _distance;
			_viewCamera.localPosition = p;
		}
	}
}
