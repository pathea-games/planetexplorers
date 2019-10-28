using UnityEngine;
using System.Collections;
using WhiteCat;

public class DragItemMousePickTowerCreation : DragItemMousePickCreation
{

    AITurretController _controller;

    public void Init(AITurretController controller)
    {
        _controller = controller;
    }

    protected override void InitCmd(CmdList cmdList)
    {
        base.InitCmd(cmdList);
		if (null != itemObj && null != itemObj.protoData && itemObj.protoData.unchargeable)
            cmdList.Add("Refill", OnRefill);
    }

    void OnRefill()
    {
        int addAmmoNum = GameUI.Instance.mItemOp.AmmoNum;
        if (addAmmoNum <= 0)
            return;

        if (_controller == null)
            return;

        if (pkg == null)
            return;

        int maxhas = GetMaxRefillNum();
        if (maxhas < addAmmoNum)//???
        {
            GameUI.Instance.mItemOp.SetRefill(Mathf.FloorToInt(_controller.energy), Mathf.FloorToInt(_controller.maxEnergy), GetMaxRefillNum());
            return;
        }

        if (!pkg.Destroy(_controller.bulletProtoId, addAmmoNum))
            return;

        _controller.SetEnergy(_controller.energy + addAmmoNum);
        //GameUI.Instance.mItemOp.Hide();
        GameUI.Instance.mItemOp.SetRefill(Mathf.FloorToInt(_controller.energy), Mathf.FloorToInt(_controller.maxEnergy), GetMaxRefillNum());
    }

    protected override void CheckOperate()
    {
        base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd() && null != itemObj && null != itemObj.protoData && itemObj.protoData.unchargeable)
        {
            GameUI.Instance.mItemOp.SetRefill(Mathf.FloorToInt(_controller.energy), Mathf.FloorToInt(_controller.maxEnergy), GetMaxRefillNum());
        }
    }

    public int GetMaxRefillNum()
    {
        if (_controller == null)
            return 0;

        if (pkg == null)
            return 0;

        int countInPkg = pkg.GetCount(_controller.bulletProtoId);
        int countNeed = Mathf.FloorToInt(_controller.maxEnergy) - Mathf.FloorToInt(_controller.energy);
        return countNeed > countInPkg ? countInPkg : countNeed;
    }

    protected override string tipsText
    {
        get
        {
            string tips="";

            //lz-2016.11.04 ISO炮塔的名字增加能源量和耐久度显示
            if (_controller != null)
            {
                if (null != itemObj && null != itemObj.protoData)
                {
                    //lz-2016.11.04 dragName为空，所以这里用protoData.name
                    tips = "[5CB0FF]" + itemObj.protoData.name + "[-]" + "\n" + PELocalization.GetString(8000129);
                }
                tips += "\n" + Mathf.FloorToInt(_controller.energy) + "/" + Mathf.FloorToInt(_controller.maxEnergy);
                tips += string.Format("\n{0} {1}/{2}", PELocalization.GetString(82220001), Mathf.FloorToInt(_controller.hp), Mathf.FloorToInt(_controller.maxHp));
            }
            return tips;
        }
    }

}
