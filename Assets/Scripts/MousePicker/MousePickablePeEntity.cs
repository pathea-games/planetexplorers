using System;

public class MousePickablePeEntity : MousePickable
{
	public event Action<MousePickablePeEntity> pickBodyEventor;

    void Awake()
    {
        priority = MousePicker.EPriority.Level3;
        //delayTime = 1.0f;
    }

    protected override void CheckOperate()
    {
		base.CheckOperate();

		Pathea.PeEntity mono = GetComponent<Pathea.PeEntity>();

		if (PeInput.Get (PeInput.LogicFunction.PickBody) && null != pickBodyEventor)
			pickBodyEventor(this);

		if(PeInput.Get(PeInput.LogicFunction.TalkToNpc))
		{
			Pathea.EntityMgr.NPCTalkEvent ee = new Pathea.EntityMgr.NPCTalkEvent();
			if(null != mono)
			{
				ee.entity = mono;
				Pathea.EntityMgr.Instance.npcTalkEventor.Dispatch(ee);
			}
		}

        if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
        {

            Pathea.EntityMgr.RMouseClickEntityEvent ee = new Pathea.EntityMgr.RMouseClickEntityEvent();
            if (null != mono)
            {
                ee.entity = mono;

                Pathea.EntityMgr.Instance.eventor.Dispatch(ee);
            }
        }

		if (MissionManager.Instance != null)
		{
	        if (PeInput.Get(PeInput.LogicFunction.TalkToNpc))
	        {
	            if (MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID83) && gameObject.name == "scene_Dien_viyus_ship_on01(Clone)")
	            {
	                if (Pathea.PeGameMgr.IsMulti)
	                    MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID83);
	                else
	                {
	                    MissionManager.Instance.CompleteMission(MissionManager.m_SpecialMissionID83);
	                }
	            }
	        }
		}
    }

    protected override string tipsText
    {
        get
        {
            //Pathea.MonoPeEntity mono = GetComponent<Pathea.MonoPeEntity>();
            //if (null == mono)
            //{
            //    return null;
            //}

            //Pathea.PeEntity e = mono.Impl;
            //if (null == e)
            //{
            //    return null;
            //}

            //return e.ToString();
            return null;
        }
    }
}