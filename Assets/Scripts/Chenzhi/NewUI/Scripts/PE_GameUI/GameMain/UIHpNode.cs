using UnityEngine;
using System.Collections;

public class UIHpNode : MonoBehaviour 
{
	[SerializeField] UILabel mLabel;

	public Color color;
	public Vector3 worldPostion;
	public string text { set{mLabel.text = value;} }
	public bool isHurt = true;
	[SerializeField] AnimationCurve acScale;
	[SerializeField] AnimationCurve acAlpha;
	[SerializeField] float speed = 0.5f;
	[SerializeField] float maxShowTime = 0.8f; 
	float time = 0;
	void Start()
	{
		time = 0;
		mLabel.enabled = true;
		mLabel.color = color;
		mLabel.enabled = false;
	}

	void Update()
	{
		if (!CanShow())
		{
			time = maxShowTime + 1; 
		}

		if (color.a < 0.01f && time > maxShowTime)
		{
			mLabel.enabled = false;
			this.enabled = false;
			UIHpChange.Instance.RemoveNode(this);
			return; 
		}

		float speed_scale = Mathf.Pow(Mathf.Clamp(Vector3.Distance( PETools.PEUtil.MainCamTransform.position, worldPostion ), 12, 1000), 0.7f);
		worldPostion.y += Time.deltaTime * speed * speed_scale;
		//color.a = 1 -  (time /maxShowTime);
		color.a = acAlpha.Evaluate(time);

		//		Vector3 eye = PETools.PEUtil.MainCamTransform.position;
		//		Vector3 dir = PETools.PEUtil.MainCamTransform.forward;
		//		Vector3 ofs = worldPostion - eye;
		//		float dot = Vector3.Dot(ofs, dir);

		Vector3 pos = Camera.main.WorldToScreenPoint(worldPostion);

		if (pos.z > 1)
		{
			mLabel.enabled = true;

			mLabel.color = color;;
			pos.z = -1;
			transform.localPosition = pos;
			if (isHurt)
			{
				float scale = 1 + acScale.Evaluate(time);
				transform.localScale = new Vector3(scale,scale,1);
			}
		}
		else 
			mLabel.enabled = false;

		time += Time.deltaTime;
	}

	bool CanShow()
	{
		if ( GameUI.Instance == null )
			return false;
        //lz-2016.11.03 游戏暂停的时候不显示
		if ( !UIHpChange.Instance.m_ShowHPChange || GameConfig.IsInVCE ||GameUI.Instance.mMainPlayer == null|| Pathea.PeGameMgr.gamePause)
			return false;
		return true;
	}

}
