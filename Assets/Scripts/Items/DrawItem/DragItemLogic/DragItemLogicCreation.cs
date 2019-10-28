using Pathea;

public class DragItemLogicCreation : DragItemLogic
{
    #region itemscript

    public override void OnActivate()
    {
        DragCreationLodCmpt s = GetComponent<DragCreationLodCmpt>();
        if (null != s)
        {
            s.Activate(this);
        }
    }

    public override void OnDeactivate()
    {
        DragCreationLodCmpt s = GetComponent<DragCreationLodCmpt>();
        if (null != s)
        {
            s.Deactivate(this);
        }
    }

    public override void OnConstruct()
    {
        DragCreationLodCmpt s = GetComponent<DragCreationLodCmpt>();
        if (null != s)
        {
            s.Construct(this);
        }
    }

    public override void OnDestruct()
    {
        DragCreationLodCmpt s = GetComponent<DragCreationLodCmpt>();
        if (null != s)
        {
            s.Destruct(this);
        }
    }

    #endregion
}