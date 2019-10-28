using ItemAsset;
using UnityEngine;

public class NpcStorage
{
    //avoid to confilct with solar and repair machine.
    const int NpcStorageIDBegin = 100000;
    static CSUI_StorageMain storageMain = null;
    
    ItemPackage mPackage;

    int mId;

    public void Init(int id)
    {
        mId = NpcStorageIDBegin + id;
        //mNpc.MouseCtrl.MouseEvent.SubscribeEvent(OnRMouseClicked);

        UINpcStorageCtrl.Instance.btnClose += btnClose;
        UINpcStorageCtrl.Instance.btnPageItem += btnPageItem;
        UINpcStorageCtrl.Instance.btnPageEquipment += btnPageEquipment;
        UINpcStorageCtrl.Instance.btnPageResource += btnPageResource;
    }

    public ItemPackage Package
    {
        get
        {
            if (null == mPackage)
            {
                CSEntityAttr attr = new CSEntityAttr();
                attr.m_InstanceId = mId;
                attr.m_Type = (int)CSConst.etStorage;

                CSEntity cse = null;
                if (CSConst.rrtSucceed == CSMain.GetCreator(CSConst.ciDefNoMgCamp).CreateEntity(attr, out cse))
                {
                    CSStorage storage = cse as CSStorage;
                    if (null != storage)
                    {
                        mPackage = storage.m_Package;
                    }
                    else
                    {
                        Debug.LogError("Get CSStorage failed by "+mId);
                    }
                }
            }

            return mPackage;
        }

        set
        {
            mPackage = value;
        }
    }

    //void OnRMouseClicked(NpcMouseCtrl.MouseCtrlEvent arg)
    //{
    //    //Open();
    //}

    public void Reset()
    {
        if (null == StorageMain)
        {
            return;
        }

        StorageMain.RestItems();
    }

    static CSUI_StorageMain StorageMain
    {
        get
        {
            if (null == storageMain)
            {
                GameObject storageObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CS_StorageMain")) as GameObject;
                GameObject gameui = GameObject.Find("GameUIRoot/UICamera/Game");
                if (null == gameui)
                {
                    Debug.LogError("cant find GameUIRoot/UICamera/Game");
                    return null;
                }

                storageObject.transform.parent = UINpcStorageCtrl.Instance.mContent.transform;
                storageObject.transform.localPosition = new Vector3(-125f, 82, 0);
                storageObject.transform.localScale = Vector3.one;

                storageMain = storageObject.GetComponent<CSUI_StorageMain>();
            }

            return storageMain;
        }
    }
    
    public void Open(/*AiNpcObject npc*/)
    {
        //if (null == StorageMain)
        //{
        //    return;
        //}

        //StorageMain.SetPackage(Package);
        //StorageMain.SetType(0, 0);

        //NpcRandom npcRandom = npc as NpcRandom;

        //if (null == npcRandom)
        //{
        //    UINpcStorageCtrl.Instance.SetICO(npc.m_NpcIcon);
        //}
        //else
        //{
        //    UINpcStorageCtrl.Instance.SetICO(npcRandom.GetHeadTex());
        //}

        //UINpcStorageCtrl.Instance.SetNpcName(npc.NpcName);
        //UINpcStorageCtrl.Instance.SetTabIndex(0);
        //UINpcStorageCtrl.Instance.Show();

        //if (!GameUI.Instance.mUIItemPackageCtrl.isShow)
        //{
        //    GameUI.Instance.mUIItemPackageCtrl.Show();
        //}
        //GameUI.Instance.mUIItemPackageCtrl.ResetItem(0, 0);
    }

    void btnClose()
    {
        
    }
    void btnPageItem()
    {
        storageMain.SetType(0, 0);
		GameUI.Instance.mItemPackageCtrl.ResetItem(0, 0);
    }
    void btnPageEquipment()
    {
        storageMain.SetType(1, 0);
		GameUI.Instance.mItemPackageCtrl.ResetItem(1, 0);
    }
    void btnPageResource()
    {
        storageMain.SetType(2, 0);
		GameUI.Instance.mItemPackageCtrl.ResetItem(2, 0);
    }
}