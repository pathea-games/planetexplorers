using UnityEngine;

namespace WhiteCat
{
	public class VCPPivot : VCSimpleObjectPart
	{
		[SerializeField]
		UIFilledSprite fillSprite;

		[SerializeField]
		[GetSet("Angle")]
		float angle;


		Transform rootTrans;
		Vector3 originalPos;
		bool direction;
		float time01;


		public float Angle
		{
			get { return angle; }
			set
			{
				if (fillSprite)
				{
					if (value < 0)
					{
						fillSprite.transform.localEulerAngles = new Vector3(0, 180f, -value);
					}
					else
					{
						fillSprite.transform.localEulerAngles = new Vector3(0, 0, value);
					}
					fillSprite.fillAmount = Mathf.Abs(value) / 360f;
				}

				angle = value;
			}
		}


		protected override void Awake()
		{
			base.Awake();
			transform.localEulerAngles=new Vector3(0f, 0f, 0f);
		}


		public override CmdList GetCmdList()
		{
			var list = base.GetCmdList();
			list.Add("Rotate Pivot", Rotate);
			return list;
		}


		public void Init(Transform rootTrans)
		{
			originalPos = rootTrans.localPosition;
            enabled = false;
			this.rootTrans = rootTrans;
			direction = false;
			time01 = 0f;
		}


		void Rotate()
		{
			direction = !direction;

			enabled = true;
		}


		void FixedUpdate()
		{
			float delta01 = Time.deltaTime * PEVCConfig.instance.pivotRotateSpeed / Mathf.Abs(angle);
			if (direction) time01 += delta01;
			else time01 -= delta01;
			time01 = Mathf.Clamp01(time01);

			float angleY = Mathf.SmoothStep(0, angle, time01);
			rootTrans.localPosition = originalPos;
			rootTrans.localRotation = Quaternion.identity;
			rootTrans.RotateAround(rootTrans.parent.position, Vector3.up, angleY);

			if ((direction && time01 == 1f) || (!direction && time01 == 0f))
			{
				enabled = false;
			}
		}
	}
}