using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkillSystem
{
	// base class for skill caster and skill target
	public partial class SkEntity : MonoBehaviour
	{
		// Virtuals Methods
		public virtual void ApplySe(int seId, SkRuntimeInfo info){}
		public virtual void ApplyAnim(string anim, SkRuntimeInfo info){}
		public virtual void ApplyEmission(int emitId, SkRuntimeInfo info){}
		public virtual void ApplyEff(int effId, SkRuntimeInfo info){}
		public virtual void ApplyCamEff(int effId, SkRuntimeInfo info){}
		public virtual void ApplyRepelEff(SkRuntimeInfo info){}
		public virtual void CondTstFunc(SkFuncInOutPara funcInOut){}
		public virtual Transform GetTransform(){	return transform;	}
		public virtual Collider GetCollider(string name) { return null; }

		public float _lastestTimeOfHurtingSb = 0;
		public float _lastestTimeOfGettingHurt = 0;
		public float _lastestTimeOfConsumingStamina = 0;
		public virtual void OnHurtSb(SkInst inst, float dmg){}
		public virtual void OnGetHurt(SkInst inst, float dmg){}
		public virtual void GetCollisionInfo(out List<KeyValuePair<Collider,Collider>> colPairs){ colPairs = null; }
		public virtual void OnBuffAdd(int buffId){}
		public virtual void OnBuffRemove(int buffId){}
	}
}
