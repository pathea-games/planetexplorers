using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IKTransFollow : MonoBehaviour 
{
	[SerializeField,SetProperty("FollowTarget")]
    Transform m_FollowTarget;

    public Transform FollowTarget
    {
        get { return m_FollowTarget; }
        set
        {
            m_FollowTarget = value;
            SyncInfo();
        }
    }

    [ContextMenu("ExecSyncInfo")]
    public void SyncInfo()
    {
        if (null != m_FollowTarget)
        {
            transform.position = m_FollowTarget.position;
            transform.rotation = m_FollowTarget.rotation;
        }
        else
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }


    #region mono methods

    void Update () 
	{
        SyncInfo();
    }

    void LateUpdate()
    {
        SyncInfo();
    }

    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one *0.1f);
    }
    #endregion
}
