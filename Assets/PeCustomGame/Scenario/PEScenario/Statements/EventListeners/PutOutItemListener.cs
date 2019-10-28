using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("PUT OUT ITEM")]
    public class PutOutItemListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
        OBJECT item;	// ITEM

        // 在此初始化参数
        protected override void OnCreate()
        {
            item = Utility.ToObject(parameters["item"]);
        }

        // 打开事件监听
        public override void Listen()
        {
			if (Pathea.PeGameMgr.IsSingle)
			{
	            if (DraggingMgr.Instance != null && DraggingMgr.Instance.eventor != null)
	                DraggingMgr.Instance.eventor.Subscribe(OnResponse);
			}
			else if (Pathea.PeGameMgr.IsMulti)
			{
                // TODO: 服务器下发事件
                PlayerNetwork.mainPlayer.OnCustomPutOutItemEventHandler += OnNetResponse;
            }
		}

        // 关闭事件监听
        public override void Close()
        {
			if (Pathea.PeGameMgr.IsSingle)
			{
	            if (DraggingMgr.Instance != null && DraggingMgr.Instance.eventor != null)
	                DraggingMgr.Instance.eventor.Unsubscribe(OnResponse);
	            else
	                Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
			else if (Pathea.PeGameMgr.IsMulti)
			{
                // TODO: 服务器下发事件
                PlayerNetwork.mainPlayer.OnCustomPutOutItemEventHandler -= OnNetResponse;
            }
        }

        void OnResponse(object sender, DraggingMgr.EventArg e)
        {
            if (item.type != OBJECT.OBJECTTYPE.ItemProto)
                return;
            ItemObjDragging items = e.dragable as ItemObjDragging;

            Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            if (pkg == null)
                return;

            if (item.isAnyPrototype)
                Post();
            else if (item.isAnyPrototypeInCategory)
            {
                if (ItemAsset.ItemProto.Mgr.Instance == null)
                    return;
                ItemAsset.ItemProto proto = ItemAsset.ItemProto.Mgr.Instance.Get(items.GetItemProtoId());
                if (proto == null)
                    return;
                if (item.Group == proto.editorTypeId)
                    Post();
            }
            else if (item.isSpecificPrototype)
            {
                if (item.Id == items.GetItemProtoId())
                    Post();
            }
        }

        void OnNetResponse(ItemAsset.ItemObject itemObj)
        {
            if (item.type != OBJECT.OBJECTTYPE.ItemProto)
                return;

            if (item.isAnyPrototype)
                Post();
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
            else if (item.isSpecificPrototype)
            {
                if (item.Id == itemObj.protoId)
                    Post();
            }
        }
    }
}
