using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Pathea
{
    public enum EMsg
    {
        Null,

        Lod_Collider_Created,
        Lod_Collider_Destroying,

		View_FirstPerson,

        View_Prefab_Build,
        View_Prefab_Destroy,

        View_Injured_Build,
        View_Injured_Destroy,

        View_Model_Build,
        View_Model_Destroy,
        View_Model_AttachJoint,
        View_Model_ReattachJoint,
        View_Model_DeatchJoint,

        View_Ragdoll_Build,
        View_Ragdoll_Destroy,
        View_Ragdoll_AttachJoint,
        View_Ragdoll_ReattachJoint,
        View_Ragdoll_DeatchJoint,

        View_Ragdoll_Fall_Begin,
        View_Ragdoll_Fall_Finished,
        View_Ragdoll_Getup_Begin,
        View_Ragdoll_Getup_Finished,

        View_Trigger_Add,
        View_Trigger_Remove,

        Trans_Real,
        Trans_Simulator,

		Trans_Pos_set,

        Action_Whacked,
        Action_Repulsed,
        Action_Wentfly,
        Action_Knocked,
		Action_GetOnVehicle,
		Action_DurabilityDeficiency,

		Battle_EnterShootMode,
		Battle_ExitShootMode,
		Battle_PauseShootMode,
		Battle_ContinueShootMode,
		Battle_OnShoot,
		Battle_HPChange,
		Battle_Attack,
		Battle_OnAttack,
		Battle_BeAttacked,
		Battle_EquipAttack,
		Battle_AttackHit,
		Battle_TargetSkill,

		Camera_ChangeMode,
		
		State_Die,
		State_Revive,
		state_Water,

		Build_BuildMode,

		UI_ShowChange,
	
		Skill_CheckLoop,
		Skill_Event,
        Skill_Interrupt,
        Net_Begin,
		Net_Hit = Net_Begin,
		Net_End,
        Net_Instantiate,
		Net_Controller,
		Net_Proxy,
        Net_Destroy,
        Max
    }

    public interface IPeMsg
    {
        void OnMsg(EMsg msg, params object[] args);
    }
}