using UnityEngine;
using System.Collections;

public class ItemDraggingConnection : ItemDraggingArticle
{
	protected override bool canPutUp { get { return true; } }
    public override bool OnDragging(Ray cameraRay)
    {
        if(false == base.OnDragging(cameraRay))
        {
            return false;
        }

        AutoSnap();

        return true;
    }

    void AutoSnap()
    {
        ItemScript_Connection itemSc = fhitInfo.transform.GetComponent<ItemScript_Connection>();

        if (null != itemSc && itemSc.mConnectionPoint.Count > 0)
        {
            float nearDis = 10f;
            foreach (Vector3 offsetPos in itemSc.mConnectionPoint)
            {
                float nowDis = (fhitInfo.transform.position + fhitInfo.transform.rotation * offsetPos - fhitInfo.point).magnitude;
                if (nearDis > nowDis)
                {
                    nearDis = nowDis;
                    rootGameObject.transform.position = fhitInfo.transform.position + fhitInfo.transform.rotation * offsetPos;
                }
            }
        }
    }
}
