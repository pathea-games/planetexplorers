using UnityEngine;
using UnityEngine.UI;
using Pathea;

namespace WhiteCat
{

	public class LockUI : MonoBehaviour
	{
		[SerializeField] Image _background;
		[SerializeField] Image _progress;
		[SerializeField] Image _cross;
		[SerializeField] TweenInterpolator _crossShow;
		[SerializeField] TweenInterpolator _crossLocking;
		[SerializeField] TweenInterpolator _crossLocked;

		RectTransform rectTrans;
		RectTransform rectTransParent;
		int lastShowType;
		bool visible;

		PESkEntity lastTarget;
		PeTrans lastTrans;
		CreationController lastCreationController;

        float[] alphas = new float[3];
        bool hiding;
        static LockUI _instance;
        public static LockUI instance { get { return _instance; } }


		Vector3 targetPosition
		{
			get
			{
				if (lastTrans)
				{
					return lastTrans.center;
                }
				if (lastCreationController)
				{
					return lastCreationController.boundsCenterInWorld;
                }
				return lastTarget.transform.position;
            }
		}


        #region mono methods

        void Awake()
		{
            _instance = this;
            hiding = false;

			rectTrans = transform as RectTransform;
			rectTransParent = transform.parent as RectTransform;
			lastShowType = -1;

			visible = false;
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.SetActive(visible);
			}
		}


		void LateUpdate()
		{
			int showType = 0; // 0 不显示, 1 可攻击, 2 锁定中, 3 已锁定

			var carrier = CarrierController.playerDriving;
			PESkEntity target = null;

			if (carrier)
			{
				if (carrier.lockedTarget)
				{
					showType = 3;
					target = carrier.lockedTarget;
				}
				else if (carrier.timeToLock > 0f)
				{
					showType = 2;
					target = carrier.targetToLock;
				}
				else if (carrier.aimEntity)
				{
					showType = 1;
					target = carrier.aimEntity;
                }
            }

            if (showType != lastShowType && !hiding)
			{
				lastShowType = showType;

				_background.enabled = showType == 1 || showType == 2;
				_progress.enabled = showType == 2 || showType == 3;
				_cross.enabled = true;

				_crossShow.enabled = showType == 0 || showType == 1;
				_crossLocking.enabled = showType == 2;
                _crossLocked.enabled = showType == 3;

				if (_crossShow.enabled)
				{
					_crossShow.speed = showType == 0 ? -1f : 1f;
				}
				else if (_crossLocking.enabled)
				{
					Color c = _cross.color;
					c.a = 1f;
					_cross.color = c;
					_cross.transform.localScale = Vector3.one;
					_crossLocking.normalizedTime = 0;
                }
				else if (_crossLocked.enabled)
				{
					Color c = _cross.color;
					c.a = 1f;
					_cross.color = c;
					_cross.transform.localEulerAngles = Vector3.zero;
                }
			}

			if(target)
			{
				if (target != lastTarget)
				{
					lastTarget = target;
					lastTrans = target.GetComponent<PeTrans>();
					lastCreationController = target.GetComponent<CreationController>();
                }

				var camera = Camera.main;
				Vector3 targetPosition = this.targetPosition;

				if(visible != (Vector3.Dot(camera.transform.forward, targetPosition - camera.transform.position) > 1f))
				{
					visible = !visible;
					for (int i = 0; i < transform.childCount; i++)
					{
						transform.GetChild(i).gameObject.SetActive(visible);
					}
				}

				if (visible)
				{
					_progress.fillAmount = 1f - carrier.timeToLock / PEVCConfig.instance.lockTargetDuration;
					rectTrans.anchoredPosition = camera.WorldToScreenPoint(targetPosition) / rectTransParent.localScale.x;
				}
			}
		}

        #endregion

        #region public methods
        public void HideWhenUIPopup()
        {
            hiding = true;

            Color bgColor=_background.color;
            Color progresColor=_progress.color;
            Color crossColor=_cross.color;

            alphas[0] = bgColor.a;
            alphas[1] = progresColor.a;
            alphas[2] = crossColor.a;

            bgColor.a=0f;
            progresColor.a=0f;
            crossColor.a=0f;

            _background.color = bgColor;
            _progress.color = progresColor;
            _cross.color = crossColor;
        }


        public void ShowWhenUIDisappear()
        {
            hiding = false;

            Color bgColor = _background.color;
            Color progresColor = _progress.color;
            Color crossColor = _cross.color;

            bgColor.a = alphas[0];
            progresColor.a = alphas[1];
            crossColor.a = alphas[2];

            _background.color = bgColor;
            _progress.color = progresColor;
            _cross.color = crossColor;
        }
        #endregion
    }

}