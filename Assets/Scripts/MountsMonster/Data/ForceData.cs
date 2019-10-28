using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class ForceData
    {
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        public int _campID;
        public int _damageID;
        public int _defaultPlyerID;

        public ForceData(int campid = 0,int damageid = 0,int defaulplayerid = 0)
        {
            _campID = campid;
            _damageID = damageid;
            _defaultPlyerID = defaulplayerid;
        }

        public ForceData Copy()
        {
            return new ForceData(_campID,_damageID,_defaultPlyerID);
        }

        public void Export(System.IO.BinaryWriter bw)
        {
            bw.Write(CURRENT_VERSION);
            bw.Write(_campID);
            bw.Write(_damageID);
            bw.Write(_defaultPlyerID);
        }

        public void Import(System.IO.BinaryReader r)
        {
            r.ReadInt32();
            _campID = r.ReadInt32();
            _damageID = r.ReadInt32();
            _defaultPlyerID = r.ReadInt32();
        }
    }

    public enum MountsSkillKey
    {
        Mskill_L,
        Mskill_Space,
        Mskill_pounce,
        Mskill_tame,
        Max
    }


    public class BaseSkillData
    {
        const int VERSION_0000 = 0;
        const int VERSION_0001 = 1;
        const int CURRENT_VERSION = VERSION_0001;

        public int m_SkillL;//mouse right
        public int m_SKillIDSpace;//space
        public int m_Skillpounce;//space_1
        public string m_PounceAnim;

        public bool canUse()
        {
            return canAttack() || canSpace() || canProunce();
        }

        public bool canAttack()
        {
            return m_SkillL != 0;
        }

        public bool canSpace()
        {
            return m_SKillIDSpace != 0;
        }

        public bool canProunce()
        {
            return m_Skillpounce != 0 && !string.IsNullOrEmpty(m_PounceAnim);
        }

        public BaseSkillData CopyTo()
        {
            BaseSkillData _data = new BaseSkillData();
            _data.m_SKillIDSpace = m_SKillIDSpace;
            _data.m_SkillL = m_SkillL;
            _data.m_Skillpounce = m_Skillpounce;
            _data.m_PounceAnim = m_PounceAnim;
            return _data;
        }
        public void Reset(int skill0 = 0,int space =0,int prounce = 0)
        {
            m_SkillL = skill0;
            m_SKillIDSpace = space;
            m_Skillpounce = prounce;
            m_PounceAnim = MountsSkillDb.GetpounceAnim(prounce);
        }

        public BaseSkillData(int skill0 = 0, int space = 0,int prounce = 0)
        {
            m_SkillL = skill0;
            m_SKillIDSpace = space;
            m_Skillpounce = prounce;
            m_PounceAnim = MountsSkillDb.GetpounceAnim(prounce);
        }

        public void Export(System.IO.BinaryWriter bw)
        {
            bw.Write(CURRENT_VERSION);
            bw.Write(m_SkillL);
            bw.Write(m_SKillIDSpace);
            if(CURRENT_VERSION >= VERSION_0001)
            {
                bw.Write(m_Skillpounce);
                PETools.Serialize.WriteNullableString(bw, m_PounceAnim);
            }
            
        }

        public void Import(System.IO.BinaryReader r)
        {
            int version = r.ReadInt32();
            m_SkillL = r.ReadInt32();
            m_SKillIDSpace = r.ReadInt32();
            if(version >= VERSION_0001)
            {
                m_Skillpounce = r.ReadInt32();
                m_PounceAnim = PETools.Serialize.ReadNullableString(r);
            }
        }
    }
}


