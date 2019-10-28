using UnityEngine;
using System.Collections;

public class UISkillTypeBtn : MonoBehaviour 
{
	[SerializeField] UISlicedSprite  uiSprite;
	public Color enableColor = Color.white;
	public Color disableColor = Color.white;

	public Color hoverColor = Color.blue;
	
	public string spriteName { get { return uiSprite.spriteName;} set { uiSprite.spriteName = value;} }

	public int index = -1;

	public delegate void DClickEvent (UISkillTypeBtn btn);
	public event DClickEvent onBtnClick;

	private Color _color = Color.white;

	public void SetEnable (bool enable)
	{
		if (enable)
			uiSprite.color = enableColor;
		else
			uiSprite.color = disableColor;

		_color = uiSprite.color;
	}

	void OnClick()
	{
		if (onBtnClick != null)
			onBtnClick(this);

		Debug.Log("Click Skill Type Button");
	}

	void OnHover(bool isOver)
	{
		if (isOver)
		{
			uiSprite.color = Color.Lerp(_color, hoverColor, 0.2f);
		}
		else
		{
			uiSprite.color = _color;
		}
	}
}
