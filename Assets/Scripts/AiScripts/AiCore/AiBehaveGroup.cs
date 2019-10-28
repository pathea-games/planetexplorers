using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tree = Behave.Runtime.Tree;

public class AiBehaveGroup : AiBehave
{
    SPGroup mAiGroup;

    public SPGroup aiGroup { get { return mAiGroup; } }

    public override bool isPause
    {
        set
        {
            base.isPause = value;

            if (value && mAiGroup != null)
            {
                mAiGroup.ClearMoveAndRotation();
            }
        }
    }

    public override bool isGroup
    {
        get
        {
            return true;
        }
    }

	public override bool isActive {
		get {
			return base.isActive && mAiGroup != null;
		}
	}

    public void RegisterSPGroup(SPGroup spGroup)
    {
        mAiGroup = spGroup;

        //AiImplementGroup[] impGroups = GetComponentsInChildren<AiImplementGroup>();
        //foreach (AiImplementGroup imp in impGroups)
        //{
        //    imp.Initialize(spGroup);
        //}
    }
}
