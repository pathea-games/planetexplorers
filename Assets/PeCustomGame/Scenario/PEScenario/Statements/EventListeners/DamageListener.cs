using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("DAMAGE")]
    public class DamageListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		OBJECT obj;	// OBJECT
		OBJECT atk;	// OBJECT
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			obj = Utility.ToObject(parameters["object"]);
			atk = Utility.ToObject(parameters["attacker"]);
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (Pathea.PeGameMgr.IsSingle)
            	Pathea.PESkEntity.entityAttackEvent += OnResponse;
			else if (Pathea.PeGameMgr.IsMulti)
                PlayerNetwork.mainPlayer.OnCustomDamageEventHandler += OnNetResponse;	// TODO: 服务器下发的事件
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (Pathea.PeGameMgr.IsSingle)
            	Pathea.PESkEntity.entityAttackEvent -= OnResponse;
			else if (Pathea.PeGameMgr.IsMulti)
                PlayerNetwork.mainPlayer.OnCustomDamageEventHandler -= OnNetResponse;	// TODO: 服务器下发的事件
		}

        void OnResponse(SkillSystem.SkEntity self, SkillSystem.SkEntity caster, float value)
        {
			if (self == null || caster == null)
				return;
            Pathea.PeEntity selfEntity = self.GetComponent<Pathea.PeEntity>();
            Pathea.PeEntity casterEntity = caster.GetComponent<Pathea.PeEntity>();
            if (selfEntity == null || casterEntity == null)
                return;
            if (PeScenarioUtility.IsObjectContainEntity(obj, selfEntity) &&
                PeScenarioUtility.IsObjectContainEntity(atk, casterEntity))
                Post();
        }

        void OnNetResponse(int scenarioId, int casterScenarioId, float value)
        {
            if (PeScenarioUtility.IsObjectContainEntity(obj, scenarioId) &&
                PeScenarioUtility.IsObjectContainEntity(atk, casterScenarioId))
                Post();
        }
    }
}
