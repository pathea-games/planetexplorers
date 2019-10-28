// Custom game UI Select Window
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using PeCustom.Ext;

public class UICustomGameSelectWnd : MonoBehaviour, IListReceiver
{
	[SerializeField] GameObject		content;


	// Left Window Content
	[SerializeField] UIEfficientGrid	mapItemGrid;
	[SerializeField] UILabel			pathLb;
	[SerializeField] Transform  		mask;

	public string Path { get { return pathLb.text;} set { pathLb.text = value;} }
	
	UICustomMapItem mSelectedItem = null;
	public UICustomMapItem selectedItem { get { return mSelectedItem;} }

	// Right Top Content
	[System.Serializable]
	public class CMapInfo
	{
		public UITexture 	texture;
		public UILabel 		name;
		public UILabel		size;
		public GameObject root;

	}

	[SerializeField] CMapInfo mapInfo;


	// Right Bottom Content
	[System.Serializable]
	public class CPlayerInfo
	{
		public UIPopupList playerList;
		public GameObject root;
	}
	[SerializeField] CPlayerInfo  playerInfo;

	// Effect Componet
	[SerializeField] UITweenBufferAlpha alphaTweener;
	[SerializeField] TweenScale		scaleTweener;

	[SerializeField] UIHintBox hintBox;
	public UIHintBox HintBox { get { return hintBox;} }

	/// <summary> 选择了某张具体地图触发/// </summary>
	public event Action<CMapInfo, CPlayerInfo, UICustomMapItem> onMapItemClick;

	public event Action<UICustomMapItem> onMapItemDoubleClick;
	
	public event Func<bool> onStartBtnClick;

	//public event Action onCancelBtnClick;

	public event Action<int> onPlayerSelectedChanged;

	public event Action onInit;

	public event Action onOpen;

	public event Action onClose;

	public event Func<bool> onBack;

	public event Action<int, UICustomMapItem> onMapItemSetContent;

	public event Action<UICustomMapItem> onMapItemClearContent;

	public void ClearMapItem ()
	{
		mapItemGrid.UpdateList(0, this);

	}
	
	public void CreateMapItem (int count)
	{
		mapItemGrid.UpdateList(count, this);
	}

	public void Init ()
	{
        mapItemGrid.itemGoPool.Init();
		if (onInit != null)
			onInit();

		hintBox.onOpen += OnHintBoxOpen;
		hintBox.onClose += OnHintBoxClose;
	}

	public void Open ()
	{
		if (onOpen != null)
			onOpen();

		scaleTweener.Play(true);

		alphaTweener.Play(true);
		content.SetActive(true);
	}

	public void Close ()
	{
		alphaTweener.Play(false);
		scaleTweener.Play(false);
	}

	void OnAlphaTweenFinished(UITweener tween)
	{
		if (tween.direction == AnimationOrTween.Direction.Reverse)
		{
			content.SetActive(false);

			if (onClose != null)
				onClose();
		}
	}
	

	#region CALLBACK_FUNC


	void OnMapItemClick (UICustomMapItem item)
	{

		for (int i = 0; i < mapItemGrid.Gos.Count; i++)
		{
			UICustomMapItem it = mapItemGrid.Gos[i].GetComponent<UICustomMapItem>();
			it.IsSelected = false;
		}

		item.IsSelected = true;
		mSelectedItem = item;

		if (item.IsFile)
		{
			// Hide All Right Information
			if (mapInfo.root.activeSelf)
				mapInfo.root.SetActive(false);

            if (playerInfo.root != null)
            {
                if (playerInfo.root.activeSelf)
                    playerInfo.root.SetActive(false);
            }
		}
		else
		{
			if (!mapInfo.root.activeSelf)
				mapInfo.root.SetActive(true);

            if (playerInfo.root != null)
            {
                if (!playerInfo.root.activeSelf)
                    playerInfo.root.SetActive(true);
            }

			if (onMapItemClick != null)
				onMapItemClick(mapInfo, playerInfo, item);
		}
	}

	void OnMapItemDbClick (UICustomMapItem item)
	{
		if (onMapItemDoubleClick != null)
			onMapItemDoubleClick(item);

		if (item.IsFile)
		{
			mSelectedItem = null;
		}
		else
		{
			OnMapItemClick(item);
		}
	}

	void OnStartBtnClick ()
	{
		if (onStartBtnClick != null && onStartBtnClick())
		{
			
		}
		else
			Close();
	}

	void OnCancelBtnClick ()
	{
		Close();

	}

	void OnClose ()
	{
		Close();
	}

	void OnBackClick ()
	{
		if (onBack != null)
		{
			if (onBack())
			{
				if (mSelectedItem != null)
				{
					mSelectedItem.IsSelected = false;
					mSelectedItem = null;
				}

				mapInfo.root.SetActive(false);

                if (playerInfo.root != null)
                {
                    playerInfo.root.SetActive(false);
                }
			}
		}

	}
	


	void OnPlayerSelectChanged (string select)
	{
		if (onPlayerSelectedChanged != null)
		{
			int index = playerInfo.playerList.items.FindIndex(item0 => item0 == select);
			onPlayerSelectedChanged(index);
		}
	}


	void OnHintBoxOpen ()
	{
		Vector3 pos = mask.localPosition;
		pos.z = (gameObject.transform.localPosition.z + hintBox.transform.localPosition.z) * 0.5f;
		mask.localPosition = pos; 
	}

	void OnHintBoxClose()
	{
		Vector3 pos = mask.localPosition;
		pos.z = 0;
		mask.localPosition = pos; 
	}
	#endregion

	#region IListReceiver_func
	void IListReceiver.SetContent (int index, GameObject go)
	{
		UICustomMapItem item = go.GetComponent<UICustomMapItem>();
		item.index = index;
		item.onClick -= OnMapItemClick;
		item.onDoubleClick -= OnMapItemDbClick;
		item.onClick += OnMapItemClick;
		item.onDoubleClick += OnMapItemDbClick;

		if (onMapItemSetContent != null)
			onMapItemSetContent(index, item);
	}

	void IListReceiver.ClearContent(GameObject go)
	{
		UICustomMapItem item = go.GetComponent<UICustomMapItem>();
		item.index = -1;
		item.IsSelected = false;
		item.onClick -= OnMapItemClick;
		item.onDoubleClick -= OnMapItemDbClick;

		if (onMapItemClearContent != null)
			onMapItemClearContent(item);
	}
	#endregion

	#region UNITY_INNER_FUNC

	void Awake()
	{
		Init();
	}

	
	#endregion
}
