using UnityEngine;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
    [Statement("USE ITEM")]
    public class UseItemListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
        OBJECT item;	// ITEM

        // 在此初始化参数
        protected override void OnCreate()
        {
            item = Utility.ToObject(parameters["item"]);
        }


        UseItemCmpt uic;
        // 打开事件监听
        public override void Listen()
        {
            if (PeCreature.Instance == null || PeCreature.Instance.mainPlayer == null)
                return;

			if (PeGameMgr.IsSingle)
			{
	            uic = PeCreature.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
	            if (uic == null)
	                return;
	            uic.eventor.Subscribe(OnResponse);
			}
			else if (PeGameMgr.IsMulti)
			{
                // TODO: 服务器下发事件
                PlayerNetwork.mainPlayer.OnCustomUseItemEventHandler += OnNetResponse;
			}
        }

        // 关闭事件监听
        public override void Close()
        {
			if (PeGameMgr.IsSingle)
			{
	            if (uic != null && uic.eventor != null)
	                uic.eventor.Unsubscribe(OnResponse);
	            else
	                Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
			else if (PeGameMgr.IsMulti)
			{
                // TODO: 服务器下发事件
                PlayerNetwork.mainPlayer.OnCustomUseItemEventHandler -= OnNetResponse;
            }
		}

        void OnResponse(object sender, UseItemCmpt.EventArg e)
        {
            if (item.type != OBJECT.OBJECTTYPE.ItemProto)
                return;
            if (item.isSpecificPrototype)
            {
                if (item.Id == e.itemObj.protoId)
                    Post();
            }
            else if (item.isAnyPrototypeInCategory)
            {
                if (ItemAsset.ItemProto.Mgr.Instance == null)
                    return;
                ItemAsset.ItemProto proto = ItemAsset.ItemProto.Mgr.Instance.Get(e.itemObj.protoId);
                if (proto == null)
                    return;
                if (item.Group == proto.editorTypeId)
                    Post();
            }
            else if (item.isAnyPrototype && e.itemObj != null)
                Post();
        }

        void OnNetResponse(ItemAsset.ItemObject itemObj)
        {
            if (item.type != OBJECT.OBJECTTYPE.ItemProto)
                return;
            if (item.isSpecificPrototype)
            {
                if (item.Id == itemObj.protoId)
                    Post();
            }
            else if (item.isAnyPrototypeInCategory)
            {
                if (ItemAsset.ItemProto.Mgr.Instance == null)
                    return;
                ItemAsset.ItemProto proto = ItemAsset.ItemProto.Mgr.Instance.Get(itemObj.protoId);
                if (proto == null)
                    return;
                if (item.Group == proto.editorTypeId)
                    Post();
            }
            else if (item.isAnyPrototype && itemObj != null)
                Post();
        }
    }
}
