using UnityEngine;
using System.Collections;

public class RailwayStation : MonoBehaviour
{
    public Transform mLinkPoint;
    public Transform mJointPoint;
    public CrossLine mLine;

    public int pointId = Railway.Manager.InvalId;

    [SerializeField]
    RailwayStation m_LinkStation;

    public Railway.Point Point
    {
        get
        {
            return Railway.Manager.Instance.GetPoint(pointId);
        }
    }

    public void SetPos(Vector3 pos)
    {
        transform.position = pos;
        UpdateLink();
    }

    public void SetRot(Vector3 rot)
    {
        transform.rotation = Quaternion.Euler(rot);
        //transform.localRotation = Quaternion.Euler(rot);
        UpdateLink();
    }

    public void LinkTo(RailwayStation targetStation)
    {
        m_LinkStation = targetStation;
        UpdateLink();
    }

    public void UpdateLink()
    {
        if (null == m_LinkStation)
        {
            BreakLink();
        }
        else
        {
            EstablishLink(m_LinkStation.mLinkPoint.position, mLinkPoint.position);
        }
    }

    void EstablishLink(Vector3 pos1, Vector3 pos2)
    {
        mLine.m_Begin = pos1;
        mLine.m_End = pos2;

        if (!mLine.gameObject.activeSelf)
        {
            mLine.gameObject.SetActive(true);
        }
    }

    void BreakLink()
    {
        if (mLine.gameObject.activeSelf)
        {
            mLine.gameObject.SetActive(false);
        }
    }
}
