using UnityEngine;
using Pathea;
using SkillSystem;
using Pathea.Projectile;

namespace WhiteCat
{
	public class CreationSkEntity : PESkEntity
	{
		BehaviourController _controller;


		public void Init(BehaviourController controller)
		{
			_controller = controller;
		}


		public override void ApplyEmission(int emitId, SkRuntimeInfo inst)
		{
			if (inst.Para is SkCarrierCanonPara) _controller.GetWeapon(inst.Para).PlayEffects();

			//Transform trans = null;
			if (inst.Target != null)
			{
				//PeTrans peTrans = inst.Target.GetComponent<PeTrans>();
				//if (peTrans != null) trans = peTrans.trans;
			}

			ProjectileBuilder.Instance.Register(emitId, transform, inst, 0, false);
		}


        public PeEntity driver
        {
            get { return _controller is CarrierController ? (_controller as CarrierController).driver : null; }
        }
	}
}
