using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public enum EAttackMode
{
    Attack,
    Defence,
    Passive,
    Max
}

namespace Pathea
{
    public enum PeSex
    {
        Undefined = 0,
        Female = 1,
        Male = 2,
        Max
    }

    public static class PeGender
    {
        public static PeSex Random()
        {
            return (PeSex)UnityEngine.Random.Range(1, 3);
        }

        public static PeSex Convert(int v)
        {
            if (v == 0)
            {
				return PeSex.Undefined;
            }
            else if (v == 1)
            {
                return PeSex.Female;
            }
            else if (v == 2)
            {
                return PeSex.Male;
            }
            else
            {
                return PeSex.Max;
            }
        }

        public static PeSex Convert(CustomCharactor.ESex v)
        {
            if (v == CustomCharactor.ESex.Female)
            {
                return PeSex.Female;
            }
            else if (v == CustomCharactor.ESex.Male)
            {
                return PeSex.Male;
            }
            else
            {
                return PeSex.Max;
            }
        }

        public static CustomCharactor.ESex Convert(PeSex v)
        {
            if (v == PeSex.Male)
            {
                return CustomCharactor.ESex.Male;
            }
            else if (v == PeSex.Female)
            {
                return CustomCharactor.ESex.Female;
            }
            else
            {
                return CustomCharactor.ESex.Max;
            }
        }

        public static bool IsMatch(PeSex sex, PeSex require)
        {
			if (sex == PeSex.Undefined)
            {
                return true;
            }

            if (sex == require)
            {
                return true;
            }

            return false;
        }
    }

    public enum ERace
    {
        None,
        Mankind,
        Monster,
        Puja,
        Paja,
        Alien,
        Tower,
        Building,
        NPC,
        Neutral,
        Max
    }

    public enum EIdentity
    {
        None,
        Player,
        Npc,
        Neutral,
        Max
    }

	public class CommonCmpt : PeCmpt 
	{
        AnimatorCmpt m_Animator;

        EIdentity m_IdentityProto;
        public EIdentity IdentityProto
        {
            get { return m_IdentityProto; }
            set 
            { 
                m_IdentityProto = value;

                Identity = value;
            }
        }

        ERace m_Race;
        public ERace Race
        {
            get { return m_Race; }
            set { m_Race = value; }
        }

        EIdentity m_Identity;
        public EIdentity Identity
        {
            get { return m_Identity; }
            set { m_Identity = value; }
        }

        bool m_IsBoss = false;
        public bool IsBoss
        {
            get { return m_IsBoss; }
            set { m_IsBoss = value; }
        }

        GameObject m_TDObj = null;
        public GameObject TDObj
        {
            get { return m_TDObj; }
            set { m_TDObj = value; }
        }

        Vector3 m_TDpos;
        public Vector3 TDpos
        {
            get { return m_TDpos; }
            set { m_TDpos = value; }
        }


        int m_OwnerID;
        public int OwnerID
        {
            get { return m_OwnerID; }
            set { m_OwnerID = value; }
        }

        int m_ItemDropId;
        public int ItemDropId
        {
            get { return m_ItemDropId; }
            set { m_ItemDropId = value; }
        }

        public bool IsPlayer
        {
            get { return m_Identity == EIdentity.Player; }
        }

        public bool IsNpc
        {
            get { return m_Identity == EIdentity.Npc; }
        }

        public bool isPlayerOrNpc
        {
            get { return m_Identity == EIdentity.Player || m_Identity == EIdentity.Npc; }
        }

        #region override
        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);
            invincible = r.ReadBoolean();
            sex = (PeSex)r.ReadInt32();
            m_OwnerID = r.ReadInt32();
            m_Race = (ERace)r.ReadInt32();
            m_Identity = (EIdentity)r.ReadInt32();
            m_ItemDropId = r.ReadInt32();

            if (Entity.version < PeEntity.VERSION_0002)
            {
                int entityPrototype = r.ReadInt32();
                int entityPrototypeId = r.ReadInt32();

                if (entityPrototype != -1)
                {
                    Entity.entityProto = new EntityProto()
                    {
                        proto = (EEntityProto)entityPrototype,
                        protoId = entityPrototypeId
                    };
                }
            }
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);
            w.Write(invincible);
            w.Write((int)sex);
            w.Write(m_OwnerID);
            w.Write((int)m_Race);
            w.Write((int)m_Identity);
            w.Write((int)m_ItemDropId);

            //if (entityProto != null)
            //{
            //    w.Write((int)entityProto.proto);
            //    w.Write((int)entityProto.protoId);
            //}
            //else
            //{
            //    w.Write((int)-1);
            //    w.Write((int)-1);
            //}
        }

        #endregion

        public bool invincible { get; set; }

        public PeSex sex { get; set; }

        public EntityProto entityProto
        {
            get
            {
                return Entity.entityProto;
            }
        }

        public object userData { get; set; }

        public override void Start()
        {
            base.Start();

            m_Animator = GetComponent<AnimatorCmpt>();
            if (m_Animator != null)
            {
                m_Animator.SetInteger("Owner", OwnerID);
                m_Animator.SetInteger("Sex", (int)sex);
            }
        }
    }
}
