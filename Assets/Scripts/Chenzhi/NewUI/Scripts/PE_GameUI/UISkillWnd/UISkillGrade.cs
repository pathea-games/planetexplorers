using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HelpExtension;

public class UISkillGrade : MonoBehaviour 
{
	public Transform mContent;
	public UISprite mIcon;
	[SerializeField] UISprite mLeftLine_h;
	[SerializeField] UISprite mLeftLine_v;
	[SerializeField] UISprite mRightLine_h;
	[SerializeField] UISprite mRightLine_v;

	public Color enableColor = Color.white;
	public Color disableColor = Color.white;


	[SerializeField] GameObject mSkillItemPrefab;

	List<UISkillItem> mSkillItems = new List<UISkillItem>(10);

	public List<UISkillItem> skillItems { get{ return mSkillItems;}  }

	List<GameObject> mSkillGos = new List<GameObject>(10);

	public delegate void DNotify (UISkillItem item);
	public event DNotify onSkillItemLernBtn;
	public event DNotify onSkillItemClick;

	public static int c_SkillItemSpace = 30;

	public void SetContent (int grade, List<SkillTreeUnit> skills)
	{
		mSkillGos.RefreshItem(skills.Count, mSkillItemPrefab, mContent);
		mSkillItems.Clear();

		for (int i = 0; i < mSkillGos.Count; i++)
		{
			UISkillItem item = mSkillGos[i].GetComponent<UISkillItem>();
			int pos_y = i * -c_SkillItemSpace* 2 + (mSkillGos.Count -1)* c_SkillItemSpace;
			item.gameObject.transform.localPosition = new Vector3(0,pos_y,0); 
			item.SetCoord(grade, i);

			item.onClickLernBtn -= OnSkillItemLernBtn;
			item.onClickItemBtn -= OnSkillItemClick;
			item.onClickLernBtn += OnSkillItemLernBtn;
			item.onClickItemBtn += OnSkillItemClick;

			mSkillItems.Add(item);
		}

		SetV_LineSize((mSkillGos.Count-1) * c_SkillItemSpace * 2);
	}
	

	public void RefreshData (UISkillType.SkillTypeData data)
	{
		if (data == null )
			return;
		if (data.data.Count > 3 || data.data.Count == 0)
			return;
		
		foreach (UISkillItem item in mSkillItems)
		{
			if (data.data.ContainsKey(item.grade))
				if (item.index < data.data[item.grade].Count)
					item.data = data.data[item.grade][item.index];
		}
	}
	

	void OnSkillItemLernBtn(UISkillItem item)
	{
		if (onSkillItemLernBtn != null)
			onSkillItemLernBtn(item);
	}


	void OnSkillItemClick(UISkillItem item)
	{
		if (onSkillItemClick != null)
			onSkillItemClick(item);
	}
	
	public void SetV_LineSize(float size)
	{
		Vector3 scale = mLeftLine_v.transform.localScale;
		scale.y = size;
		mLeftLine_v.transform.localScale = scale;

		scale = mRightLine_v.transform.localScale;
		scale.y = size;
		mRightLine_v.transform.localScale = scale;
	}

	#region UINITY_INNER_FUNC

	void Update ()
	{

		bool disable = true;
		foreach (UISkillItem item in mSkillItems)
		{
			if (item.data._state != SkillState.Lock)
			{
				disable = false;
				break;
			}
		}

		if (!disable)
			mIcon.color = enableColor;
		else
			mIcon.color = disableColor;
	}
	#endregion

}
