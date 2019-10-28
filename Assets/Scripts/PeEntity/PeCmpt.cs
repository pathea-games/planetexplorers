using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Pathea
{
    public interface IPeCmpt
    {
        void Serialize(BinaryWriter w);
        void Deserialize(BinaryReader r);

        string GetTypeName();
    }

    public abstract class PeCmpt:MonoBehaviour, IPeCmpt
    {
        PeEntity mEntity;
        public PeEntity Entity
        {
            get
            {
                if (mEntity == null)
                {
                    //lw:2017.1.16:entity被删除后访问Comonent报错
                    mEntity = this != null ? GetComponent<PeEntity>() : null;
                }
                return mEntity;
            }
        }

        public virtual void Serialize(BinaryWriter w){}
        public virtual void Deserialize(BinaryReader r){}

		public virtual void Awake() { if(this is IPeMsg) Entity.AddMsgListener(this as IPeMsg); }
		public virtual void Start() { CmptMgr.Instance.AddCmpt (this);  }
        public virtual void OnUpdate() { }
		public virtual void OnDestroy() { if(null != Entity && this is IPeMsg) Entity.RemoveMsgListener(this as IPeMsg); }


        void IPeCmpt.Serialize(BinaryWriter w)
        {
            Serialize(w);
        }

        void IPeCmpt.Deserialize(BinaryReader r)
        {
            Deserialize(r);
        }

        string IPeCmpt.GetTypeName()
        {
            return GetType().Name;
        }
    }
}