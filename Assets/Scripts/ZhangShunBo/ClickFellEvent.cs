using UnityEngine;
using System.Collections;

public class ClickFellEvent : MousePickableChildCollider
{
    [SerializeField]
    float m_TreeDefaultOpDis=2;
    protected override void OnStart()
    {
        operateDistance = m_TreeDefaultOpDis;
        base.OnStart();
    }
    protected override void CheckOperate() { }
}
