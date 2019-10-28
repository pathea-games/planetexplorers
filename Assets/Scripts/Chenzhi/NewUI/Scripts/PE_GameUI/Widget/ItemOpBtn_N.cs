using UnityEngine;
using System.Collections;
using ItemAsset;
using System.Collections.Generic;

public class ItemOpBtn_N : MonoBehaviour
{
	public UILabel 	mButtonName;
    private string m_CmdStr="";

    //lz-2016.07.01 因为DragItemMousePick都是挂在预制物体上的，见DragItemMousePickOperate第12行，m_ButtonName都是填在预制物体上的，有的写在代码里的，所以就这样兼容的本地化
    //lz-2016.07.01 本地化用 key=cmd, value=本地化id
    private Dictionary<string, int> m_DicCmds = new Dictionary<string, int> 
    {
        {"Turn",8000559},
        {"Get",8000560},
        {"Sleep",8000561},
        {"Get On",8000562},
        {"Open",8000563},
        {"Shut",8000564},
        {"Water",8000565},
        {"Clean",8000566},
        {"Remove",8000567},
        {"Refill",8000568},
        {"Rotate Pivot",8000569},
        {"Turn Off",8000570},
        {"Turn On",8000571},
        {"Sit",8000572},
    };

	public void InitButton(string cmdStr, GameObject parentObj)
	{
        this.m_CmdStr = cmdStr;
        if (m_DicCmds.ContainsKey(cmdStr))
        {
            this.mButtonName.text = PELocalization.GetString(this.m_DicCmds[cmdStr]);
        }
        else
        {
            this.mButtonName.text = cmdStr;
        }
        this.mButtonName.MakePixelPerfect();
	}
	
	void OnClick()
	{
		if(Input.GetMouseButtonUp(0))
            GameUI.Instance.mItemOp.CallFunction(this.m_CmdStr);
	}
}
