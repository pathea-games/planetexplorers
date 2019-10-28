using UnityEngine;
using Pathea;

public class OperatableItemPowerPlantSolar : OperatableItem
{
    public override bool Operate()
    {
        if (!base.Operate())
        {
            return false;
        }

        //Debug.Log(this + "operated");

        if (!TutorialData.AddActiveTutorialID(TutorialData.PlantSolarId))
        {
            GameUI.Instance.mPowerPlantSolar.OpenWnd(GetComponent<CSPowerPlantObject>());
        }

        return true;
    }

    public override bool Init(int id)
    {
        if (!base.Init(id))
        {
            return false;
        }
        if (CSMain.GetCreator(CSConst.ciDefNoMgCamp) == null)
        {
            CSMain.InitOperatItemEvent += PostInit;
            return true;
        }
        else
        {
            CSPowerPlantObject ppo = GetComponent<CSPowerPlantObject>();
            return CSConst.rrtSucceed == ppo.Init(id, CSMain.GetCreator(CSConst.ciDefNoMgCamp), false);
        }
    }

    public void PostInit()
    {
        CSPowerPlantObject ppo = GetComponent<CSPowerPlantObject>();

        ppo.Init(m_id, CSMain.GetCreator(CSConst.ciDefNoMgCamp), false);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        CSMain.InitOperatItemEvent -= PostInit;
    }
}
