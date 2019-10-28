using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragItemMousePickDoor : DragItemMousePick 
{
    ItemScript_Door mDoor;
    ItemScript_Door door
    {
        get
        {
            if (mDoor == null)
            {
                mDoor = GetComponent<ItemScript_Door>();
            }
            return mDoor;
        }
    }

    protected override void InitCmd(CmdList cmdList)
	{
        base.InitCmd(cmdList);

        if (door.IsOpen)
        {
            cmdList.Add("Shut", ShutDoor);
        }
        else
        {
            cmdList.Add("Open", OpenDoor);
        }
	}
	
	void OpenDoor()
    {
        HideItemOpGui();

        door.OpenDoor();
    }
	
	void ShutDoor()
    {
        HideItemOpGui();

        door.ShutDoor();
    }

    protected override void CheckOperate()
	{
		base.CheckOperate ();
		if(PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
		{
            if (door.IsOpen)
            {
                ShutDoor();
            }
            else
            {
                OpenDoor();
            }
		}
	}

    protected override string tipsText
    {
		get {
            if (door.IsOpen)
            {
                return base.tipsText + "\n" + PELocalization.GetString(8000127);
            }
            else
            {
                return base.tipsText + "\n" + PELocalization.GetString(8000126);
            }
		}
	}
}
