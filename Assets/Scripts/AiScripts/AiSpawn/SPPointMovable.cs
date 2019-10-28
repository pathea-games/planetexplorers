using UnityEngine;
using System.Collections;

public class SPPointMovable : SPPoint
{
    Transform mTarget;
    Vector3 mPos;

    public Transform target { set { mTarget = value; } }
    public Vector3 targetPos { set { mPos = value; } }

    bool IsMove()
    {
        if (clone == null && !spawning && !death)
            return true;

        return false; ;
    }

    IEnumerator Move()
    {
        while (true)
        {
            if (IsMove())
            {
                Vector3 targetPosition = mTarget != null ? mTarget.position : mPos;
                if(targetPosition != Vector3.zero)
                {
                    Vector3 direction = targetPosition - position;
                    if(direction.sqrMagnitude > 5.0f*5.0f)
                    {
                        position += direction.normalized * 2.0f;
                        //position = Vector3.Lerp(position, targetPosition, 0.1f);
                        AttachEventFromMesh();
                        AttachCollider();
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Start()
    {
        StartCoroutine(Move());
    }
}
