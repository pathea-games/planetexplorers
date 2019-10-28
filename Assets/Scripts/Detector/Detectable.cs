//using UnityEngine;
//using System.Collections;
//using Pathea;

//namespace Pathea
//{
//    public class Detectable : MonoBehaviour
//    {
//        PeTrans mTrans;

//        ViewCmpt mView;

//        void Awake()
//        {
//            mTrans = GetComponent<PeTrans>();
//            mView = GetComponent<ViewCmpt>();
//        }

//        public SkillSystem.SkEntity GetSkEntity()
//        {
//            return GetComponent<SkillSystem.SkEntity>();
//        }

//        public PeEntity GetEntity()
//        {
//            return GetComponent<PeEntity>();
//        }

//        public virtual Transform tr { get { return GetEntity().tr; } }
//        public virtual Transform center { get { return GetEntity().centerBone; } }
//        public virtual Vector3 pos { get{return GetEntity().position;} }
//        public virtual Quaternion rot { get { return GetEntity().rotation; } }
//        public virtual float radius { get { return mTrans != null ? mTrans.bound.extents.x : 0.0f; } }
//        public virtual bool isRagdoll { get{ return GetEntity().isRagdoll; } }
//        public virtual Rigidbody modelRigidbody { get { return mView != null && mView.modelTrans != null ? mView.modelTrans.GetComponent<Rigidbody>() : null; } }
//    }
//}