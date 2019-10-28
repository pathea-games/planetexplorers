using UnityEngine;
using System.Collections;

public class ClickGatherEvent : MousePickableChildCollider
{
    protected override void OnStart()
    {
        operateDistance = 2;
        base.OnStart();
    }
    protected override void CheckOperate() { }
}
