using UnityEngine;

public abstract class OperatableItem : MousePickableChildCollider
{
    const int SqrOperateMaxDistance = 6 * 6;

    [SerializeField]
    protected int m_id;

    //Pathea.PeTrans playerTrans = null;

    public virtual bool Operate()
    {
        return true;
    }

    public virtual bool Init(int id)
    {
        m_id = id;
        return true;
    }

    public override string ToString()
    {
        return "[OperatableItem:" + m_id + "]";
    }

    protected override void CheckOperate()
    {
        base.CheckOperate();
        if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) || PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
        {
            Operate();
        }
    }

    protected override string tipsText
    {
        get
        {
            return PELocalization.GetString(8000141);
        }
    }

//	new void Start ()
//	{
////		Pathea.PeEntity entity = gameObject.GetComponentInParent<Pathea.PeEntity>();
////		Init(entity.Id);
//	}

	protected override void OnStart ()
	{
		base.OnStart ();
		Pathea.PeEntity entity = gameObject.GetComponentInParent<Pathea.PeEntity>();
		if(null != entity)
			Init(entity.Id);
	}

}

