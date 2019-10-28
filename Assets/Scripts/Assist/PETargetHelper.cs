//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Pathea;
//
//public class PETargetHelper : PESingleton<PETargetHelper>
//{
//    public class CombatInfo
//    {
//        PeEntity m_Caster;
//        PeEntity m_Target;
//
//        public CombatInfo(PeEntity caster, PeEntity target)
//        {
//            m_Caster = caster;
//            m_Target = target;
//        }
//
//        public bool IsCombat(PeEntity caster, PeEntity target)
//        {
//            if (IsNull()) return false;
//
//            return m_Caster.Equals(caster) && m_Target.Equals(target);
//        }
//
//        public bool Match(PeEntity entity)
//        {
//            if (IsNull()) return false;
//
//            return m_Target.Equals(entity);
//        }
//
//        public bool IsNull()
//        {
//            return m_Caster == null || m_Target == null;
//        }
//
//        public override bool Equals(object obj)
//        {
//            if (IsNull())
//                return false;
//
//            CombatInfo other = obj as CombatInfo;
//            if (other == null) return false;
//
//            return m_Caster.Equals(other.m_Caster) && m_Target.Equals(other.m_Target);
//        }
//    }
//
//    List<CombatInfo> m_Targets = new List<CombatInfo>();
//
//    bool IsInjured(PeEntity entity)
//    {
//        return m_Targets.Find(ret => ret.Match(entity)) != null;
//    }
//
//    bool IsCombat(PeEntity caster, PeEntity target)
//    {
//        return m_Targets.Find(ret => ret.IsCombat(caster, target)) != null;
//    }
//
//    void CalculateInjured(PeEntity entity)
//    {
//        if(entity != null)
//        {
//            entity.ActivateInjured(IsInjured(entity));
//        }
//    }
//
//    CombatInfo Get(PeEntity caster, PeEntity target)
//    {
//        return m_Targets.Find(ret => ret.IsCombat(caster, target));
//    }
//
//    IEnumerator RegisterEnumerator(PeEntity caster, PeEntity target, float time)
//    {
//        CombatInfo info = new CombatInfo(caster, target);
//
//        m_Targets.Add(info);
//
//        CalculateInjured(target);
//
//        yield return new WaitForSeconds(time);
//
//        m_Targets.Remove(info);
//
//        CalculateInjured(target);
//    }
//
//    public void RegisterContinues(PeEntity caster, PeEntity target, float time)
//    {
//        if (caster == null || target == null)
//            return;
//
//        PECoroutineRunner.Instance.StartCoroutine(RegisterEnumerator(caster, target, time));
//    }
//
//    public void Register(PeEntity caster, PeEntity target)
//    {
//        if (caster == null || target == null)
//            return;
//
//        if (IsCombat(caster, target))
//            return;
//        
//        m_Targets.Add(new CombatInfo(caster, target));
//
//        CalculateInjured(target);
//    }
//
//    public void Remove(PeEntity caster, PeEntity target)
//    {
//        if (caster == null || target == null)
//            return;
//
//        m_Targets.Remove(Get(caster, target));
//
//        CalculateInjured(target);
//    }
//}
