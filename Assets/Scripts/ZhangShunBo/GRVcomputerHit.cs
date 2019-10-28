using Pathea;

public class GRVcomputerHit : PEHitDetector
{
    protected override void OnHit()
    {
        if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(10046))
        {
            MissionCommonData data = MissionManager.GetMissionCommonData(10046);
            if (data == null)
                return;

            PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
            MissionManager.Instance.SetGetTakeMission(10046, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
        }
    }
}
