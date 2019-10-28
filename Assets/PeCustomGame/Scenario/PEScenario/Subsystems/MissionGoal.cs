using UnityEngine;
using System.Collections.Generic;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
	public abstract class MissionGoal
	{
		public int id;
		public int missionId;
		public string text;
		public abstract bool achieved { get; set; }
		public System.Action<int, int> onAchieve;

		private bool _last_achieved = false;
		public void Update ()
		{
			if (!_last_achieved && achieved)
			{
				if (onAchieve != null)
					onAchieve(id, missionId);
			}
			_last_achieved = achieved;
		}
		public virtual void Init () {}
		public virtual void Free () {}
	}

	public class MissionGoal_Bool : MissionGoal
	{
		private bool _achieved = false;
		public override bool achieved
		{
			get { return _achieved; }
			set { _achieved = value; }
		}
	}

	public class MissionGoal_Item : MissionGoal
	{
		public OBJECT item;
		public ECompare compare;
		public int target;
		public int current
		{
			get
			{
				if (PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
				{
					PeEntity p = PeCreature.Instance.mainPlayer;
					if (item.isSpecificPrototype)
						return p.packageCmpt.GetItemCount(item.Id);             //某种item
					else if (item.isAnyPrototypeInCategory)
						return p.packageCmpt.GetCountByEditorType(item.Group);  //某大类item
					else if (item.isAnyPrototype)
						return p.packageCmpt.GetAllItemsCount();                //全部item
				}
				return 0;
			}
		}

		public override bool achieved
		{
			get { return Utility.Compare(current, target, compare); }
			set { }
		}
	}

	public class MissionGoal_Kill : MissionGoal
	{
		public OBJECT monster;
		public ECompare compare;
		public int target;
		private int _current;
		public int current { get { return _current; } set { _current = value; } }

		public override bool achieved
		{
			get { return Utility.Compare(current, target, compare); }
			set { }
		}

		public override void Init ()
		{
			Pathea.PESkEntity.entityAttackEvent += OnDamage;
		}

		public override void Free ()
		{
			Pathea.PESkEntity.entityAttackEvent -= OnDamage;
		}

		void OnDamage(SkillSystem.SkEntity self, SkillSystem.SkEntity caster, float value) 
		{
			if (PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
			{
				PeEntity p = PeCreature.Instance.mainPlayer;
				PeEntity targetEntity = self.GetComponent<Pathea.PeEntity>();
				PeEntity casterEntity = caster.GetComponent<Pathea.PeEntity>();
				if (targetEntity == null || casterEntity != p)
					return;
				float hp = targetEntity.GetAttribute(AttribType.Hp);
				if (hp <= 0 && hp + value > 0)
				{
					if (PeScenarioUtility.IsObjectContainEntity(monster, targetEntity))
						_current++;
				}
			}
		}
	}
}