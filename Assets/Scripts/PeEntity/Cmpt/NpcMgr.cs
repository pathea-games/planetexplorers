using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Pathea
{
	public enum  ECsNpcState
	{
		None,
		Working,
	    InMission,
		OutOfRadiu
	}


    public  class NPCConstNum
    {
        public const float ATK_MIN_HP = 0.2f;
		
		public const float IK_Aim_AngleH = 80.0f;
		public const float IK_Aim_AngleH_Idle = 60.0f;
		public const float IK_Aim_Height = 1.5f;
		public const float IK_Aim_Heiget_UpY = 0.05f;
		public const float IK_Aim_Heiget_DownY = 0.15f;
		public const float IK_Aim_Distance_Min = 1.0f;
		public const float IK_Aim_Distance_Max = 10.0f;
        public const float IK_Aim_Lerpdefault = 100.0f;
        public static readonly float IK_Aim_Lerpspeed_0 = 2.0f;

        public static readonly float IK_Aim_FadeInTimedefault = 0.05f;
        public static readonly float IK_Aim_FadeOutTimedefault = 0.05f;
        public static readonly float IK_Aim_FadeInTime_0 = 1.0f;
        public static readonly float IK_Aim_FadeOutTime_0 = 3.0f;

        public static readonly float Fix_Distance_Max = 6.0f;

     


    }

	public class NpcMgr 
	{
        public static  int[] InFeildBuff = new int[] { 30200053, 30200046 };
        public static  int[] RecruitBuff = new int[] { 30200049, 30200050 };

		public static bool CallBackColonyNpcImmediately(PeEntity entity)
		{
			if(entity == null)
				return false;

            if (entity.NpcCmpt == null) //||  == null || entity.NpcCmpt.Creater.Assembly == null
				return false;

            CSCreator creator =  entity.NpcCmpt.Creater;
            if(creator == null)
                creator = CSMain.GetCreator(CSConst.ciDefMgCamp);

            if (creator == null || creator.Assembly == null)
                return false;

			//in assembly radius
			if(!IsOutRadiu(entity.position,creator.Assembly.Position, creator.Assembly.Radius))
				return true;

            Vector3 backPos = PETools.PEUtil.GetRandomPositionOnGroundForWander(creator.Assembly.Position, creator.Assembly.Radius * 0.7f, creator.Assembly.Radius);
			if (backPos == Vector3.zero)
                backPos = creator.Assembly.Position;

			entity.NpcCmpt.Req_Translate(backPos);
			return true;
		}

		public static bool CallBackCampsieNpcImmediately(PeEntity entity)
		{

			if(entity == null)
				return false;
			
			if(entity.NpcCmpt == null)
				return false;

			return false;
		}

        public static bool CallBackNpcToMainPlayer(PeEntity entity)
        {

            if (PeCreature.Instance == null || PeCreature.Instance.mainPlayer == null)
                return false;

            Vector3 pos = PeCreature.Instance.mainPlayer.position;
            pos.z += 2;
            pos.y += 1;

            if (entity.NpcCmpt != null)
            {
                entity.NpcCmpt.Req_Translate(pos);
                return true;
            }
            return false;

        }

		public static bool CallBackToFixPiontImmediately(PeEntity entity)
		{

			if(entity == null)
				return false;
			
			if(entity.NpcCmpt == null || entity.NpcCmpt.FixedPointPos == Vector3.zero)
				return false;

			entity.NpcCmpt.Req_Translate(entity.NpcCmpt.FixedPointPos);
			return true;
		}

		// only ColonyNpc
		public static bool ColonyNpcLostController(PeEntity entity)
		{
			if(entity == null)
				return false;

            if (entity.NpcCmpt == null) 
                return false;

            CSCreator creator = entity.NpcCmpt.Creater;
			if (creator == null || creator.Assembly == null)
				return false;

			if(entity.NpcCmpt.Processing)
				return false;

            float distance = PETools.PEUtil.MagnitudeH(entity.position, creator.Assembly.Position);
            if (distance > creator.Assembly.Radius)
			{
                Vector3 backPos = PETools.PEUtil.GetRandomPositionOnGroundForWander(creator.Assembly.Position, creator.Assembly.Radius * 0.7f, creator.Assembly.Radius);
				if(backPos == Vector3.zero)
					backPos = creator.Assembly.Position;

				entity.NpcCmpt.Req_Translate(backPos);
				return true;
			}
			return false;

		}

		public static void GetRandomPathForCsWander(PeEntity npc,Vector3 center, Vector3 direction, float minRadius, float maxRadius,OnPathDelegate callback = null)
		{
			if (AstarPath.active != null)
			{
                Pathfinding.RandomPath path = Pathfinding.RandomPath.Construct(npc.position, (int)Random.Range(minRadius, maxRadius) * 100, callback);
				path.spread = 40000;
				path.aimStrength = 1f;
                path.aim = PETools.PEUtil.GetRandomPosition(npc.position, direction, minRadius, maxRadius, -75.0f, 75.0f);
				AstarPath.StartPath(path);
				return;
			}
			return ;
		}

		public static bool IsIncenterAraound(Vector3 center,float radiu,Vector3 target)
		{
			float r0 = (target.x - center.x)*(target.x - center.x) + (target.y - center.y)*(target.y - center.y) + (target.z - center.y)*(target.y - center.y);
			return r0 <= radiu * radiu;
		}

        public static bool IsOutRadiu(Vector3 slf,Vector3 centor,float radiu)
        {
            float ra = PETools.PEUtil.Magnitude(slf,centor);
            return ra > radiu;
        }

		public static bool CallBackColonyNpcToPlayer(PeEntity entity,out ECsNpcState state)
		{
			state = ECsNpcState.None;
			if(entity == null || entity.NpcCmpt == null)
				return false;

			if(entity.NpcCmpt.Job != ENpcJob.Resident)
			{
				state = ECsNpcState.Working;
				return false;
			}

			if(entity.NpcCmpt.Creater == null || entity.NpcCmpt.Creater.Assembly == null)
			{
				if(entity.NpcCmpt.BaseNpcOutMission)
					state = ECsNpcState.InMission;

				return false;
			}

			float r0= PETools.PEUtil.Magnitude(PeCreature.Instance.mainPlayer.position,entity.NpcCmpt.Creater.Assembly.Position);
			if(r0 > entity.NpcCmpt.Creater.Assembly.Radius)
			{
				state = ECsNpcState.OutOfRadiu;
				return false;
			}

			if (entity.target != null)
				entity.target.ClearEnemy();
			
			
			if (entity.biologyViewCmpt != null)
				entity.biologyViewCmpt.Fadein();
			
			Vector3 pos = PeCreature.Instance.mainPlayer.position;
			pos.z += 2;
			pos.y += 1;
			entity.NpcCmpt.Req_Translate(pos);
			return true;
		}

        public static bool NpcMissionReady(PeEntity npc)
        {
            if (npc == null)
                return false;

            if (npc.NpcCmpt == null)
                return false;

            CSCreator mCScreator = npc.NpcCmpt.Creater;
            if(mCScreator == null)
                mCScreator = CSMain.GetCreator(CSConst.ciDefMgCamp);

            if (mCScreator == null || mCScreator.Assembly == null)
                return false;

            ItemAsset.ItemObject item;
            if (npc.UseItem != null && NpcEatDb.CanEatSthFromStorages(npc, mCScreator.Assembly.Storages, out item))
            {
                npc.UseItem.Use(item);
            }

            for (int i = 0; i < RecruitBuff.Length; i++)
            {
                npc.skEntity.CancelBuffById(RecruitBuff[i]);
            }

            for (int i = 0; i < InFeildBuff.Length; i++)
            {
                SkillSystem.SkEntity.MountBuff(npc.skEntity, InFeildBuff[i], new List<int>(), new List<float>());
            }
            return true;
        }

        public static void NpcMissionFinish(PeEntity npc)
        {
            for (int i = 0; i < InFeildBuff.Length; i++)
            {
                npc.skEntity.CancelBuffById(InFeildBuff[i]);
            }

            for (int i = 0; i < RecruitBuff.Length; i++)
            {
                SkillSystem.SkEntity.MountBuff(npc.skEntity, RecruitBuff[i], new List<int>(), new List<float>());
            }
        }
	}
}
