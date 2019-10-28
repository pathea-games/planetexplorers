using UnityEngine;
using System.Collections;
using System.IO;

namespace Pathea
{
    public class StateInjuredCmpt : PeCmpt
    {
        float mLevel;

        public float Level
        {
            get { return mLevel; }
        }

        //0-1 is injured level
        public void SetLevel(float level)
        {
            if (level < PETools.PEMath.Epsilon)
            {
                level = 0f;
            }

            mLevel = level;

            DoInjured();
        }

        void DoInjured()
        {
            if (Mathf.Approximately(mLevel, 1f))
            {
                //AttackMode = EAttackMode.Passive;
                //SetAiActive(false);
                //SetFloat("InjuredLevel", 2f);
                //animator.Play("InjuredSit");
            }
            else
            {
                //AttackMode = EAttackMode.Defence;
                //SetAiActive(true);
                //SetFloat("InjuredLevel", injuredLevel);
            }
        }

        public override void Start()
        {
			base.Start ();
			DoInjured();
        }

        public override void Serialize(BinaryWriter w)
        {
            w.Write((float)mLevel);
        }

        public override void Deserialize(BinaryReader r)
        {
            mLevel = r.ReadSingle();
        }
    }
}