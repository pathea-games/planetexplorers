using UnityEngine;
using RootMotion.FinalIK;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Pathea.Operate
{

    public class PERide : Operation_Single
    {
        [HideInInspector]
        [SerializeField]
        public string Ainm= "RideMax";
        [HideInInspector]
        [SerializeField]
        public float MonsterScaleMin = 0.6f;
        [HideInInspector]
        [SerializeField]
        public float MonsterScaleMax = 1.5f;
        [HideInInspector]
        [SerializeField]
        public AnimationCurve IkLeapCurve = new AnimationCurve(new Keyframe(0f, 0f),new Keyframe(1f, 1f));
        public Vector3 RidePos { get {return transform.position; } }
        public Quaternion RideRotation { get { return transform.rotation; } }

        #region mono methods
        void Start()
        {
            AdjustIkByMonsterScale();
        }

        #endregion

        #region override methods

        public override bool Do(IOperator oper)
        {
            PEActionParamVQSNS param = PEActionParamVQSNS.param;
            param.vec = transform.position;
            param.q = transform.rotation;
            param.strAnima = Ainm;
            PeEntity monsterEnitity = GetComponentInParent<PeEntity>();
            param.enitytID = null==monsterEnitity?-1: monsterEnitity.Id;
            param.boneStr = transform.name;
            return Do(oper, param);
        }

        public override bool CanOperate(Transform trans)
        {
            return null==base.Operator;
        }

        public override bool Do(IOperator oper, PEActionParam para)
        {
            return oper.DoAction(PEActionType.Ride, para);
        }

        public override bool UnDo(IOperator oper)
        {
            oper.EndAction(PEActionType.Ride);
            return true;
        }

        public override bool CanOperateMask(EOperationMask mask)
        {
            return base.CanOperateMask(mask);
        }

        public override EOperationMask GetOperateMask()
        {
            return EOperationMask.Ride;
        }

        public override bool StartOperate(IOperator oper, EOperationMask mask)
        {
            if (null == oper || oper.Equals(null)) return false;
            if (m_Mask == mask)
            {
                Operator = oper;
                oper.Operate = this;
                if (!Do(oper))
                {
                    Operator = null;
                    oper.Operate = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        public override bool StopOperate(IOperator oper, EOperationMask mask)
        {
            if (null == oper || oper.Equals(null)) return false;
            if (m_Mask == mask)
            {
                if (UnDo(oper))
                {
                    Operator = null;
                    oper.Operate = null;
                    return true;
                }
                return false;
            }
            return true;
        }

        [ContextMenu("AdjustIkByMonsterScale")]
        public void AdjustIkByMonsterScale()
        {
            PeEntity monsterEntity = transform.GetComponentInParent<PeEntity>();
            if (monsterEntity)
            {
                if (monsterEntity.biologyViewCmpt && monsterEntity.biologyViewCmpt.biologyViewRoot)
                {
                    FullBodyBipedIK monsterFullIK = monsterEntity.GetComponentInChildren<FullBodyBipedIK>();
                    if (monsterFullIK)
                    {
                        Transform minIKRoot = monsterFullIK.transform.FindChild("BeRideMin");
                        Transform maxIKRoot = monsterFullIK.transform.FindChild("BeRideMax");
                        Transform runIKRoot = monsterFullIK.transform.FindChild("BeRide");
                        if (minIKRoot && maxIKRoot && runIKRoot)
                        {
                            Dictionary<FullBodyBipedEffector, Transform> minIKTransDic, maxIKTransDic, runIKTransDic;
                            minIKTransDic = maxIKTransDic = runIKTransDic = null;
                            GetIkTransByRoot(minIKRoot, out minIKTransDic);
                            GetIkTransByRoot(maxIKRoot, out maxIKTransDic);
                            GetIkTransByRoot(runIKRoot, out runIKTransDic);
                            if (null!=minIKTransDic&& minIKTransDic.Count>0 && null != maxIKTransDic && maxIKTransDic.Count > 0&& null != runIKTransDic && runIKTransDic.Count > 0)
                            {
                                Transform min, max;
                                float curMonsterProportionScale = (monsterEntity.biologyViewCmpt.biologyViewRoot.transform.localScale.x - MonsterScaleMin) / (MonsterScaleMax - MonsterScaleMin);
                                curMonsterProportionScale = Mathf.Clamp01(IkLeapCurve.Evaluate(curMonsterProportionScale));
                                foreach (var item in runIKTransDic)
                                {
                                    min = max = null;
                                    if (minIKTransDic.ContainsKey(item.Key))
                                    {
                                        if (maxIKTransDic.ContainsKey(item.Key))
                                        {
                                            min = minIKTransDic[item.Key];
                                            max = maxIKTransDic[item.Key];
                                            item.Value.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, curMonsterProportionScale);
                                            item.Value.localRotation = Quaternion.Lerp(min.localRotation, max.localRotation, curMonsterProportionScale);
                                        }
                                        else
                                            Debug.LogFormat("PERide:{0}-> maxIKTransDic->{1} not exist ! ", monsterEntity.name, item.Key.ToString());
                                    }
                                    else
                                        Debug.LogFormat("PERide:{0}-> minIKTransDic->{1} not exist ! ", monsterEntity.name, item.Key.ToString());
                                }
                            }
                            else
                                Debug.LogFormat("PERide:{0} ik trans not full ! ", monsterEntity.name);
                        }
                        else
                            Debug.LogFormat("PERide:{0} ik configura not completed ! ", monsterEntity.name);
                    }
                    else
                        Debug.LogFormat("PERide: {0} not have FullBodyBipedIK ! ", monsterEntity.name);
                }
                else
                    Debug.LogFormat("PERide: {0} not have biologyViewCmpt ! ", monsterEntity.name);
            }
            else
                Debug.LogFormat("PERide: monsterEntity is null!");
        }

        #endregion

        #region private methods

        private void GetIkTransByRoot(Transform transRoot, out Dictionary<FullBodyBipedEffector, Transform> bonesDic)
        {
            bonesDic = new Dictionary<FullBodyBipedEffector, Transform>();
            InteractionTarget[] targetArray = transRoot.GetComponentsInChildren<InteractionTarget>(true);
            if (null != targetArray && targetArray.Length > 0)
            {
                for (int i = 0; i < targetArray.Length; i++)
                {
                    bonesDic.Add(targetArray[i].effectorType, targetArray[i].transform);
                }
            }
        }

        #endregion

#if UNITY_EDITOR
        #region Gizmos
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
        }
        #endregion
#endif
    }

}
