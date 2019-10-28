using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_PlantGrid : MonoBehaviour 
{

	public FarmPlantLogic m_Plant;

	// State
	[SerializeField]
	private UIGrid m_StateRoot;
	[SerializeField]
	private UISlicedSprite m_WateringState;
	[SerializeField]
	private UISlicedSprite m_WeedingState;
	[SerializeField]
	private UISlicedSprite m_GainState;
	[SerializeField]
	private UISlicedSprite m_DeadState;

	// Icon
	[SerializeField]
	private UISlicedSprite m_IconSprite;

	public string IconSpriteName 
	{
		get{
			return m_IconSprite.spriteName;
		}

		set{
			m_IconSprite.spriteName = value;
		}
	}

	// Delete button
	[SerializeField]
	private UIButton m_DeleteBtn;

	// Life Slider
	[SerializeField]
	private UISlider m_LifeSlider;

	public delegate void PlantGridEvent(CSUI_PlantGrid plantGrid);
	public PlantGridEvent OnDestroySelf;

	int m_State = 0; // 1 = Ripe, 2 = only watering or, 3 = only weeding, 4 = both watering and weeding

	#region NGUI_CALLBACK

	void OnActivate (bool active)
	{
		m_DeleteBtn.gameObject.SetActive(active);
	}

	void OnDeleteBtn ()
	{
		if (m_Plant == null)
			return;

		if (FarmManager.Instance != null)
			FarmManager.Instance.RemovePlant(m_Plant.mPlantInstanceId);

        DragArticleAgent.Destory(m_Plant.mPlantInstanceId);
		
		//ItemMgr.Instance.DestroyItem(m_Plant.mInstanceId); 

		//GameObject.Destroy(gameObject);

		if (OnDestroySelf != null) 
			OnDestroySelf(this);
	}

	#endregion

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_Plant == null)
			return;

		int curState = 0;
		if (m_Plant.IsRipe)
		{
			m_WateringState.gameObject.SetActive(false);
			m_WeedingState.gameObject.SetActive(false);
			m_GainState.gameObject.SetActive(true);
			m_DeadState.gameObject.SetActive(false);
			curState = 1;
		}
		else if (m_Plant.mDead)
		{
			m_WateringState.gameObject.SetActive(false);
			m_WeedingState.gameObject.SetActive(false);
			m_GainState.gameObject.SetActive(false);
			m_DeadState.gameObject.SetActive(true);
			curState = 5;
		}
		else
		{
			m_GainState.gameObject.SetActive(false);
			m_DeadState.gameObject.SetActive(false);

			m_WateringState.gameObject.SetActive(m_Plant.NeedWater);
			m_WeedingState.gameObject.SetActive(m_Plant.NeedClean);

			if (m_Plant.NeedWater)
				curState = 2;
			else if (m_Plant.NeedClean)
				curState = 3;
			else if (m_Plant.NeedWater && m_Plant.NeedClean)
				curState = 4;

		}

		if (m_State != curState)
		{
			m_StateRoot.repositionNow = true;
			m_State = curState;
		}

		if (!m_Plant.mDead)
			m_LifeSlider.sliderValue = m_Plant.GetRipePercent();
		else
			m_LifeSlider.sliderValue = 0;
	}
}
