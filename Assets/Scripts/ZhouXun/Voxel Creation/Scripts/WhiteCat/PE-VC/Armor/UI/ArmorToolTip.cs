using UnityEngine;

namespace WhiteCat
{

	public class ArmorToolTip : MonoBehaviour
	{
		[SerializeField] UILabel _label;
		[SerializeField] Transform _background;
		[SerializeField] Vector2 _offset;
		[SerializeField] float _widthExtend = 16f;
		[SerializeField] float _showDelay = 1.25f;
		[SerializeField] float _ignoreInterval = 1.25f;

		Collider _lastCollider;
		float _time = -1f;
		float _lastHideTime = 0f;


		void UpdateText()
		{
			if (_lastCollider)
			{
				var tip = _lastCollider.GetComponent<UITip>();
				if (tip)
				{
					gameObject.SetActive(true);

					_label.text = tip.text;
					var size = _label.font.size * _label.font.CalculatePrintedSize(tip.text, _label.supportEncoding, _label.symbolStyle);
					var scale = _background.localScale;
					scale.x = size.x + _widthExtend;
					_background.localScale = scale;
				}
				else
				{
					gameObject.SetActive(false);
					_lastHideTime = Time.unscaledTime;
				}
			}
			else
			{
				gameObject.SetActive(false);
				_lastHideTime = Time.unscaledTime;
            }
		}


		public void UpdateToolTip(Vector2 mousePosition)
		{
			var newCollider = UICamera.lastHit.collider;
            if (newCollider != _lastCollider)
			{
				_lastCollider = newCollider;
				if (newCollider)
				{
					if (gameObject.activeSelf || Time.unscaledTime - _lastHideTime < _ignoreInterval)
					{
						UpdateText();
                    }
					else
					{
						if (newCollider.GetComponent<UITip>())
						{
							_time = _showDelay;
						}
					}
				}
				else
				{
					UpdateText();
				}
			}

			if (_time > 0f)
			{
				_time -= Time.unscaledDeltaTime;
				if (_time <= 0f)
				{
					UpdateText();
				}
            }

			mousePosition.x = (int)(mousePosition.x + _offset.x);
			mousePosition.y = (int)(mousePosition.y + _offset.y);
			transform.localPosition = mousePosition;
        }
	}

}