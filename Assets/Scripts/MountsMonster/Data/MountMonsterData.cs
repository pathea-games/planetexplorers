using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MountMonsterData
{

    const int VERSION0 = 0;
    const int VERSION1 = 1;
    const int CUR_VERSION = VERSION1;

    public MountMonsterData()
    {
        _mountsForce = new Pathea.ForceData();
        _mountsSkill = new Pathea.BaseSkillData();
    }

    public Pathea.PeEntity _monster;

    public Pathea.ForceData _mountsForce;
    public Pathea.BaseSkillData _mountsSkill;

    public float _hp;
    public Vector3 _curPostion;
    public Quaternion _rotation;
    public Vector3 _scale;
    public int _protoId;
    public Pathea.ECtrlType _eCtrltype;


    public void RefreshData()
    {
        if (_monster == null) return;

       _curPostion = _monster.peTrans.position;
       _rotation = _monster.peTrans.rotation;
       _scale = _monster.peTrans.scale;
       _hp = _monster.HPPercent;
       _protoId = _monster.ProtoID;
        _eCtrltype = _monster.monstermountCtrl.ctrlType;

       _mountsForce = _monster.monstermountCtrl.m_MountsForceDb.Copy();
       _mountsSkill = _monster.monstermountCtrl.m_SkillData.CopyTo();
    }

    public void Import(BinaryReader r)
    {
        int version = r.ReadInt32();
        if (version >= VERSION1)
        {
            _mountsForce.Import(r);
            _mountsSkill.Import(r);
            _hp = r.ReadSingle();

            _curPostion = PETools.Serialize.ReadVector3(r);
            _rotation = PETools.Serialize.ReadQuaternion(r);
            _scale = PETools.Serialize.ReadVector3(r);
            _protoId = r.ReadInt32();

            if (version >= VERSION1)
                _eCtrltype = (Pathea.ECtrlType)r.ReadInt32();

            _monster = Pathea.PeEntityCreator.Instance.CreateMountsMonster(this);
        }

        
    }

    public void Export(BinaryWriter w)
    {
        w.Write(CUR_VERSION);
        if(CUR_VERSION >= VERSION1)
        {
            _mountsForce.Export(w);
            _mountsSkill.Export(w);

            w.Write(_hp);
            PETools.Serialize.WriteVector3(w, _curPostion);
            PETools.Serialize.WriteQuaternion(w, _rotation);
            PETools.Serialize.WriteVector3(w, _scale);
            w.Write(_protoId);
            w.Write((int)_eCtrltype);
        }
    }
}

public class RelationshipData
{
    const int VERSION0 = 0;
    const int CUR_VERSION = VERSION0;

    public int _playerId;
    public int _mountsProtoId;

    public  List<MountMonsterData> mMountsDataList = new List<MountMonsterData>();

    public void AddData(Pathea.PeEntity mounts)
    {
        MountMonsterData _data = new MountMonsterData();
        _data._monster = mounts;
        _data._curPostion = mounts.peTrans.position;
        _data._rotation = mounts.peTrans.rotation;
        _data._scale = mounts.peTrans.scale;
        _data._hp = mounts.HPPercent;
        _data._protoId = mounts.ProtoID;

        _data._mountsForce = mounts.monstermountCtrl.m_MountsForceDb.Copy();
        _data._mountsSkill = mounts.monstermountCtrl.m_SkillData.CopyTo();
        mMountsDataList.Add(_data);
    }

    public void RefleshRelationData()
    {
        for(int i=0;i<mMountsDataList.Count;i++)
        {
            mMountsDataList[i].RefreshData();
        }
    }

    public void Clear()
    {
        mMountsDataList.Clear();
    }
    public void Import(BinaryReader r)
    {
        Clear();

        if (mMountsDataList == null)
            mMountsDataList = new List<MountMonsterData>();

        int version = r.ReadInt32();
        if(version >= VERSION0)
        {
            _playerId = r.ReadInt32();
            _mountsProtoId = r.ReadInt32();
            int count = r.ReadInt32();
            for (int i=0;i<count;i++)
            {
                MountMonsterData _data = new MountMonsterData();
                _data.Import(r);
                mMountsDataList.Add(_data);
            }
        }
    }

    public void Export(BinaryWriter w)
    {
        RefleshRelationData();
        w.Write(CUR_VERSION);
        if(CUR_VERSION >= VERSION0)
        {
            w.Write(_playerId);
            w.Write(_mountsProtoId);
            w.Write(mMountsDataList.Count);
            for (int i = 0; i < mMountsDataList.Count; i++)
            {
                mMountsDataList[i].Export(w);
            }
        }
    }


    public bool RecoverRelationship()
    {
        Pathea.PeEntity player = Pathea.EntityMgr.Instance.Get(_playerId);
        if (player == null)
            return false;

        for(int i=0;i<mMountsDataList.Count;i++)
        {
            if(mMountsDataList[i] != null && mMountsDataList[i]._monster != null 
                && mMountsDataList[i]._monster.biologyViewCmpt != null && mMountsDataList[i]._monster.biologyViewCmpt.biologyViewRoot != null 
                && mMountsDataList[i]._monster.biologyViewCmpt.biologyViewRoot.modelController != null)
            {
                MousePickRides rides = mMountsDataList[i]._monster.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
                if (rides)
                {
                    mMountsDataList[i]._monster.monstermountCtrl.LoadCtrl(player, rides);
                    return true;
                }
            }
        }
        return false;
    }
}

public static class RelationshipDataMgr
{
    public const int VERSION0 = 0;

    public const int CUR_VERSION = VERSION0;

    public static List<RelationshipData> mRelationship = new List<RelationshipData>();

    public static void AddRelationship(Pathea.PeEntity player,Pathea.PeEntity mounts)
    {
        if (mRelationship.Count >= 1 || player == null || mounts == null) return;
        RelationshipData _data = new RelationshipData();
        _data._playerId = player.Id;
        _data._mountsProtoId = mounts.ProtoID;
        _data.AddData(mounts);

        mRelationship.Add(_data);
    }

    public static void RemoveRalationship(int playerId,int mountsprotoId)
    {
        int n = mRelationship.Count;
        for (int i = n-1;i>=0; i--)
        {
            if (mRelationship[i]._playerId == playerId && mRelationship[i]._mountsProtoId == mountsprotoId)
            {
                mRelationship.Remove(mRelationship[i]);
            }
        }  
    }

    public static void RecoverRelationship(int playerId)
    {
        int n = mRelationship.Count;
        for (int i=n-1;i>=0; i--)
        {
            if(mRelationship[i]._playerId == playerId)
            {
                if (!mRelationship[i].RecoverRelationship())
                    mRelationship.Remove(mRelationship[i]);
            }
        }
    }


    public static void Clear()
    {
        mRelationship.Clear();
    }

    public static void Import(byte[] buffer)
    {
        Clear();
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader r = new BinaryReader(ms);
        int version = r.ReadInt32();
        if (CUR_VERSION != version)
        {
            Debug.LogWarning("The version of ColonyrecordMgr is newer than the record.");
        }

        int cnt0 = r.ReadInt32();
        if (version >= VERSION0)
        {
            for (int i = 0; i < cnt0; i++)
            {
                RelationshipData data1 = new RelationshipData();
                data1.Import(r);
                mRelationship.Add(data1);
            }
        }
    }

    public static void Export(BinaryWriter w)
    {
        w.Write(CUR_VERSION);
        w.Write(mRelationship.Count);
        for(int i=0;i<mRelationship.Count;i++)
        {
            mRelationship[i].Export(w);
        }
       
    }
}


