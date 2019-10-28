using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HelpExtension;

public class UISkillTypeCtrl : MonoBehaviour 
{
	[SerializeField] UIGrid  btnGrid;
	[SerializeField] GameObject btnPrefab;
	[SerializeField] GameObject bgTracer;
	[SerializeField] GameObject bg;
	[SerializeField] UILabel    infoContent;

	[SerializeField] UIPanel panel;
	

	List<GameObject>   mSkillBtnGos = new List<GameObject>();
	List<UISkillTypeBtn>  mSkillBtnItems = new List<UISkillTypeBtn>();
	
	public delegate void DSkillBtnNotify (UISkillTypeBtn btn);
	public event DSkillBtnNotify onSetBtnActive;

	public string desc { get {return infoContent.text;} set {infoContent.text = value;}}

	// SetContent
	public void SetContent (int count, DSkillBtnNotify setContent)
	{
		mSkillBtnGos.RefreshItem(count, btnPrefab, btnGrid.transform);

		mSkillBtnItems.Clear();
		if (setContent != null)
		{
			for (int i = 0; i < count; i++)
			{
				UISkillTypeBtn btn = mSkillBtnGos[i].GetComponent<UISkillTypeBtn>();
				mSkillBtnItems.Add(btn);
				btn.index = i;
				btn.onBtnClick -= OnSkillBtnClick;
				btn.onBtnClick += OnSkillBtnClick;
				if (setContent != null)
					setContent(btn);


			}
		}

		btnGrid.repositionNow = true;

		// Set Info wnd position and size
		float btn_size = mSkillBtnGos.Count * btnGrid.cellWidth + btnGrid.transform.position.x;
		float bg_x = btn_size + 20;
		float bg_xSize = panel.clipRange.z - bg_x;
		Vector3 bg_pos = bgTracer.transform.localPosition;
		Vector3 bg_scale = bgTracer.transform.localScale;
		bg_pos.x = bg_x;
		bg_scale.x = bg_xSize;
		bgTracer.transform.localPosition = bg_pos;
		bgTracer.transform.localScale = bg_scale;

		bg.transform.position = bgTracer.transform.position;
		bg.transform.localScale = bgTracer.transform.localScale;

		infoContent.lineWidth = (int)(bg_xSize - 31);
	}

	public void SetActiveBtn(int index)
	{
		if (index >= mSkillBtnItems.Count || index < 0)
		{
			Debug.LogError("The giving index is error");
			return;
		}

		mSkillBtnItems[index].SetEnable(true);
		
		for (int i = 0; i < mSkillBtnItems.Count; i++)
		{
			if (i == index)
				continue;
			mSkillBtnItems[i].SetEnable(false);
			
		}

		if (onSetBtnActive != null)
			onSetBtnActive(mSkillBtnItems[index]);


	}


	#region CALLBACK

	void OnSkillBtnClick(UISkillTypeBtn btn)
	{
		SetActiveBtn(btn.index);
	}

	#endregion
}
