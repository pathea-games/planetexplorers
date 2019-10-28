using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

public class AnimClip{
	public bool m_isLoop;
	public string m_frm_Name;
	public int m_frm_Start;
	public int m_frm_End;
	public int m_frm_Sound;
}

public class AnimData {
	public enum AnimId{
		Base_walk_1,
		Base_run_1,
		Base_turnL_1,
		Base_turnR_1,
		Base_startJump_0,
		Base_jump_1,
		Base_jumpEnd_0,
		Base_idle_1,
		Base_idle0_0,
		Base_idle1_0,
		Attack_aTKIdle_1,
		Attack_attack0_0,
		Attack_attack1_0,
		Attack_attack2_0,
		Attack_attack3_0,
		Attack_attack4_0,
		Attack_attack5_0,
		Attack_attack6_0,
		Attack_attack7_0,
		Attack_attack8_0,
		Attack_hit0_0,
		Attack_hit1_0,
		Attack_death0_0,
		Attack_death1_0,
		Rest_idle2_0,
		Rest_sleepdown0_0,
		Rest_sleep0_1,
		Rest_sleepup0_0,
		Rest_sleepdown1_0,
		Rest_sleep1_1,
		Rest_sleepup1_0,
		Rest_other0_0,
		Rest_other1_1,
		Rest_other2_0,
		Life_idle3_0,
		Life_idle4_0,
		Life_idle5_0,
		Life_alert_0,
		Life_call0_0,
		Life_call1_0,
		Life_threat0_0,
		Life_threat1_0,
		Life_layegg_0,
        BaseAlien_gunidle_1,
        BaseAlien_gunrun_1,
        BaseAlien_gunpull_0,
        BaseAlien_gunless_0,
        BaseAlien_swordidle_1,
        BaseAlien_swordrun_1,
        BaseAlien_swordpull_0,
        BaseAlien_swordless_0,
		AnimId_MAX
	}
	
	public int m_modelId;
	public string m_modelName;
	public AnimClip[] m_animClips;
	
	public static bool isLoaded = false;
	public static List<AnimData> s_tblAnimData;
	public static void LoadData(){
		s_tblAnimData = new List<AnimData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ai_anim_clip");
		int animMax = (int)AnimId.AnimId_MAX;
		if(reader.FieldCount != 3*animMax+2){
			Debug.LogError("'AnimDataTbl' : Unrecognized table format");
		}
		while(reader.Read()){
			AnimData animData = new AnimData();
			animData.m_animClips = new AnimClip[animMax];
			animData.m_modelId = Convert.ToInt32(reader.GetString(0));
			animData.m_modelName = reader.GetString(1);

			for(int i = 0; i < animMax; i++){
				animData.m_animClips[i] = new AnimClip();
				string animName = reader.GetName(2+i*3);
				int foundIndex = animName.IndexOf('_');
				string[] strTmp = ((AnimId)i).ToString().Split ('_');
				animData.m_animClips[i].m_isLoop = Convert.ToInt32 (strTmp[2]) != 0;
				animData.m_animClips[i].m_frm_Name = animName.Substring(0, foundIndex).Trim();
				animData.m_animClips[i].m_frm_Start = Convert.ToInt32(reader.GetString(2+i*3));
				animData.m_animClips[i].m_frm_End = Convert.ToInt32(reader.GetString(3+i*3));
				animData.m_animClips[i].m_frm_Sound = Convert.ToInt32(reader.GetString(4+i*3));
			}
			s_tblAnimData.Add(animData);
		}
		
		isLoaded = true;
	}	
	
	public static AnimData LoadAnimDataFromModelID(int modelID){
		
		foreach(AnimData data in s_tblAnimData){
			if(data.m_modelId == modelID)
				return data;
		}
		
		return null;
	}
	
	public static AnimClip LoadAnimSoundFromModelID(int modelID, string animName){
		
		foreach(AnimData data in s_tblAnimData){
			if(data.m_modelId == modelID){
				foreach(AnimClip clip in data.m_animClips){
					if(clip.m_frm_Name.ToLower() == animName.ToLower())
						return clip;
				}
			}
		}
		
		return null;
	}
	
	public static AnimData LoadAnimDataFromModelName(string modelName){
		
		foreach(AnimData data in s_tblAnimData){
			if(data.m_modelName == modelName)
				return data;
		}
		
		return null;
	}
}

public class AnimSoundData
{
    public int id;
    public string name;
    public string soundStr;

    Dictionary<string, int> mSoundList = new Dictionary<string, int>();

    static List<AnimSoundData> s_tblAnimSoundData = new List<AnimSoundData>();

    public static int GetAnimationSound(int modelId, string anim)
    {
        AnimSoundData data = s_tblAnimSoundData.Find(ret => ret != null && ret.id == modelId);
        if (data != null && data.mSoundList.ContainsKey(anim))
            return data.mSoundList[anim];

        return -1;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MonsterAnimationSound");
        while (reader.Read())
        {
            AnimSoundData data = new AnimSoundData();
            data.id = reader.GetInt32(reader.GetOrdinal("model_id"));
            data.name = reader.GetString(reader.GetOrdinal("model_name"));
            data.soundStr = reader.GetString(reader.GetOrdinal("anim_music"));

            data.InitSoundList(data.soundStr);

			s_tblAnimSoundData.Add(data);
        }
    }

    void InitSoundList(string str)
    {
        string[] anims = AiUtil.Split(str, ';');
        foreach (string ite in anims)
        {
            string[] soundData = AiUtil.Split(ite, '_');
            if (soundData.Length == 2)
            {
                string anim = soundData[0];
                int soundid = Convert.ToInt32(soundData[1]);

                if (!mSoundList.ContainsKey(anim))
                    mSoundList.Add(anim, soundid);
            }
        }
    }
}
