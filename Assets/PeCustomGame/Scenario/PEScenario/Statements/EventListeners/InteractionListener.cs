using UnityEngine;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
    [Statement("INTERACTION")]
    public class InteractionListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
        OBJECT obj;	// NPOBJECT

        // 在此初始化参数
        protected override void OnCreate()
        {
            obj = Utility.ToObject(parameters["object"]);
        }

        // 打开事件监听
        public override void Listen()
        {
            if (EntityMgr.Instance != null && EntityMgr.Instance.eventor != null)
                EntityMgr.Instance.eventor.Subscribe(OnResponse);
        }

        // 关闭事件监听
        public override void Close()
        {
            if (EntityMgr.Instance != null && EntityMgr.Instance.eventor != null)
                EntityMgr.Instance.eventor.Unsubscribe(OnResponse);
            else
                Debug.LogError("Try to close eventlistener, but source has been destroyed");
        }

        void OnResponse(object sender, EntityMgr.RMouseClickEntityEvent e)
        {
            if (e.entity == null)
                return;

            if (PeScenarioUtility.IsObjectContainEntity(obj, e.entity))
                Post();
            //if (obj.isAnyNpo && e.entity != null)
            //    Post();
            //else
            //{
            //    PeEntity entity = PeScenarioUtility.GetEntity(obj);
            //    if (entity == e.entity)
            //        Post();
            //}
        }
    }
}
