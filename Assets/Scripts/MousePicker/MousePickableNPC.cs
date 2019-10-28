using UnityEngine;
using Pathea;

public class MousePickableNPC : MousePickablePeEntity 
{
	//PeEntity m_SelfEntity;

	Vector3 mainPlayerPos { get { return (null != PeCreature.Instance.mainPlayer) ? PeCreature.Instance.mainPlayer.position : Vector3.zero; } }

	public NpcCmpt npc{ get; set;}

	protected override void OnStart ()
	{
		base.OnStart ();
		//m_SelfEntity = GetComponent<PeEntity>();
		npc = GetComponent<NpcCmpt>();
		operateDistance = GameConfig.NPCControlDistance;
	}

	protected override void CheckOperate ()
	{
		if (null != npc && npc.CanHanded)
		{
			if(PeInput.Get(PeInput.LogicFunction.InteractWithItem))
			{
				if(null != PeCreature.Instance && null != PeCreature.Instance.mainPlayer)
				{
					MotionMgrCmpt motion = PeCreature.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
					if(null != motion)
					{
						PEActionParamN param = PEActionParamN.param;
						param.n = npc.Entity.Id;
						motion.DoAction(PEActionType.Hand, param);
					}
				}
			}
		}
		else
			base.CheckOperate ();

	}
}
