using UnityEngine;
using System;

namespace WhiteCat
{
	public class UIBoneFocus : MonoBehaviour
	{
		[SerializeField] UIWidget _sprite;
		[SerializeField] Vector3 _boneOffset = new Vector3(0.1f, 0f, 0f);
		[SerializeField] TweenInterpolator _interpolator;
		[SerializeField] TweenInterpolator _highlightAnim;

		bool _visible = false;
		bool _highlight = false;
		bool _pressing = false;
//		bool _focus = false;

		const float panelWidth = 310f;
		const float panelHeight = 480f;
		const float sqrRadius = 361f;
		const float minX = 19f;
		const float minY = 19f;
		const float maxX = panelWidth - minX;
		const float maxY = panelHeight - minY;

		static Color normalColor = new Color(1f, 1f, 1f, 0.9f);
		static Color highlightColor = new Color(0.25f, 1f, 1f, 1f);
		static Color pressingColor = new Color(0.25f, 0.75f, 0.85f, 1f);


		void Awake()
		{
			_sprite.color = normalColor;
		}


		void OnEnable()
		{
			_visible = true;
            _interpolator.speed = 1f;
			_interpolator.isPlaying = true;
        }


		void OnDisable()
		{
			_visible = false;
			_interpolator.speed = -1f;
			_interpolator.isPlaying = true;
        }


		public bool highlight
		{
			get { return _highlight; }
			set
			{
				_highlight = value;

				if (value)
				{
					_highlightAnim.speed = 1f;
					_highlightAnim.isPlaying = true;
					_sprite.color = highlightColor;
				}
				else
				{
					_highlightAnim.speed = -1f;
					_highlightAnim.isPlaying = true;
					_sprite.color = normalColor;
				}
			}
		}


		public bool pressing
		{
			get { return _pressing; }
			set
			{
				_pressing = value;

				if (value)
				{
					transform.localScale = new Vector3(0.95f, 0.95f, 1f);
					_sprite.color = pressingColor;
				}
				else
				{
					transform.localScale = new Vector3(1f, 1f, 1f);
					_sprite.color = highlightColor;
				}
			}
		}


		public bool isHover(Vector2 mouseLocalPosition)
		{
			if (_visible)
			{
				Vector3 p = transform.localPosition;
				p.x -= mouseLocalPosition.x;
				p.y -= mouseLocalPosition.y;
				return ((p.x * p.x) + (p.y * p.y)) < sqrRadius;
			}
			else return false;
		}


		public void UpdatePosition(Transform bone, Camera viewCamera)
		{
			Vector3 p = viewCamera.WorldToViewportPoint(bone.TransformPoint(_boneOffset));
			p.x = (int)(p.x * panelWidth);
			p.y = (int)(p.y * panelHeight);
			p.z = 0f;
            transform.localPosition = p;

			if(_visible != (p.x > minX && p.y > minY && p.x < maxX && p.y < maxY))
			{
				_visible = !_visible;
				if (_visible)
				{
					_interpolator.speed = 1f;
					_interpolator.isPlaying = true;
				}
				else
				{
					_interpolator.speed = -1f;
					_interpolator.isPlaying = true;
				}
			}
		}

    }
}