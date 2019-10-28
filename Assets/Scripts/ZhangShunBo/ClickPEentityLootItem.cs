using UnityEngine;
using System.Collections.Generic;

public class ClickPEentityLootItem : MousePickableChildCollider 
{
    struct ItemSampleInfo
    {
        public int protoId;
        public int num;
    }

    public System.Action<int> onClick;
    private Pathea.PeEntity entity;
    private List<ItemSampleInfo> items = new List<ItemSampleInfo>();
    private bool clicked = false;

	protected override void OnStart ()
	{
        base.OnStart();
        entity = GetComponent<Pathea.PeEntity>();

        MousePickablePeEntity tmp = entity.GetComponent<MousePickablePeEntity>();
        if (null != tmp)
            Component.Destroy(tmp);

        operateDistance = 5f;
	}

	void OnClickPeEntity() 
    {
        if (!clicked)
        {
            if (onClick != null)
                onClick.Invoke(entity.Id);
            for (int i = 0; i < items.Count; i++)
                LootItemMgr.Instance.AddLootItem(entity.position, items[i].protoId, items[i].num);
            PeLogicGlobal.Instance.DestroyEntity(entity.skEntity);
            clicked = true;
            Component.Destroy(this);
        }
    }

    public void AddItem(int protoId,int num) 
    {
        ItemSampleInfo tmp = new ItemSampleInfo { protoId = protoId, num = num };
        items.Add(tmp);
    }

    protected override void CheckOperate()
    {
        //base.CheckOperate();
        if(PeInput.Get (PeInput.LogicFunction.PickBody))
        {
            OnClickPeEntity();
        }
    }
}
