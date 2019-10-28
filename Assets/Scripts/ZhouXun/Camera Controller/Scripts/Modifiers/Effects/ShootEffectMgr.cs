using UnityEngine;
using System.Collections;

using Pathea.Effect;
using SkillSystem;

public class ShootEffectMgr : MonoBehaviour, ISkEffectEntity
{
    public enum EffectType
    {
        EffectType_Shoot,
        EffectType_BowShoot,
        EffectType_LaserShoot,
        EffectType_ShotgunShoot,
    }

    public SkInst m_SkInst;
    public SkInst Inst { set {m_SkInst = value;} }
    public EffectType m_Type;

    void Start()
    {
        int tmp = (int)m_Type;
		if(null != PECameraMan.Instance && null != EntityCreateMgr.Instance)
		{
	        switch (tmp)
	        {
	            case 0:
	                PECameraMan.Instance.ShootEffect(EntityCreateMgr.Instance.GetPlayerDir());
	                break;
	            case 1:
	                PECameraMan.Instance.BowShootEffect(EntityCreateMgr.Instance.GetPlayerDir());
	                break;
	            case 2:
	                PECameraMan.Instance.LaserShootEffect(EntityCreateMgr.Instance.GetPlayerDir());
	                break;
	            case 3:
	                PECameraMan.Instance.ShotgunShootEffect(EntityCreateMgr.Instance.GetPlayerDir());
	                break;
	        }
		}
    }
}
