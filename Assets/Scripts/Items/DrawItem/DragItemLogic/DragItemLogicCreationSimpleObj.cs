using UnityEngine;
using System.Collections;

public class DragItemLogicCreationSimpleObj : DragItemLogic
{
    public override void OnConstruct()
    {
        ItemScript[] scripts = GetComponentsInChildren<ItemScript>(true);
        foreach (ItemScript script in scripts)
        {
            if (script != null)
            {
                script.InitNetlayer(mNetlayer);
                script.SetItemObject(itemDrag.itemObj);
                script.id = id;

                script.OnConstruct();
            }
        }
    }

    public override void OnDestruct()
    {
        base.OnDestruct();

        ItemScript[] scripts = GetComponentsInChildren<ItemScript>(true);
        foreach (ItemScript script in scripts)
        {
            if (script != null)
            {
                script.OnDestruct();
            }
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();

        ItemScript[] scripts = GetComponentsInChildren<ItemScript>(true);
        foreach (ItemScript script in scripts)
        {
            if (script != null)
            {
                script.OnActivate();
            }
        }

		var r = GetComponent<Rigidbody>();
		r.constraints &= (~RigidbodyConstraints.FreezePositionY);

		var creation = GetComponent<WhiteCat.CreationController>();
		creation.collidable = creation.visible = true;
	}

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        ItemScript[] scripts = GetComponentsInChildren<ItemScript>(true);
        foreach (ItemScript script in scripts)
        {
            if (script != null)
            {
                script.OnDeactivate();
            }
        }

		var r = GetComponent<Rigidbody>();
		r.constraints |= RigidbodyConstraints.FreezePositionY;

		var creation = GetComponent<WhiteCat.CreationController>();
		creation.collidable = creation.visible = false;
	}
}
