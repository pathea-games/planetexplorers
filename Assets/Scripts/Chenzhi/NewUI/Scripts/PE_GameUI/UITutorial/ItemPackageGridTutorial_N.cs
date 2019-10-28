using UnityEngine;
using System.Collections;

public class ItemPackageGridTutorial_N : MonoBehaviour
{
    private int m_ProtoID;
    public int ProtoID { get { return m_ProtoID; } }

    #region public Methods

    public void SetProtoID(int protoID)
    {
        m_ProtoID = protoID;
    }
    #endregion
}
