using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("DEATH")]
    public class DeathListener : ScenarioRTL.EventListener
    {
		// 在此列举参数
		OBJECT obj;	// OBJECT

		// 在此初始化参数
		protected override void OnCreate()
		{
			obj = Utility.ToObject(parameters["object"]);
		}
        
        // 打开事件监听
        public override void Listen()
        {
            if (Pathea.PeGameMgr.IsSingle)
                Pathea.PESkEntity.entityDeadEvent += OnResponse;
            else if (Pathea.PeGameMgr.IsMulti)
                PlayerNetwork.mainPlayer.OnCustomDeathEventHandler += OnNetResponse;   // TODO: 服务器下发的事件
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (Pathea.PeGameMgr.IsSingle)
				Pathea.PESkEntity.entityDeadEvent -= OnResponse;
			else if (Pathea.PeGameMgr.IsMulti)
                PlayerNetwork.mainPlayer.OnCustomDeathEventHandler -= OnNetResponse;	// TODO: 服务器下发的事件
        }

        void OnResponse(SkillSystem.SkEntity sk) 
        {
            Pathea.PeEntity entity = sk.GetComponent<Pathea.PeEntity>();
            if (entity == null)
                return;
            if (PeScenarioUtility.IsObjectContainEntity(obj, entity))
                Post();
        }

        void OnNetResponse(int scenarioId)
        {
            if (PeScenarioUtility.IsObjectContainEntity(obj, scenarioId))
                Post();
        }
    }
}
