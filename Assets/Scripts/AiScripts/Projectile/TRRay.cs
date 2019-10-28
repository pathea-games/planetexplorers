using UnityEngine;
using System.Collections;

public class TRRay : Trajectory
{
    public float speed;

    Vector3 moveDirection;

    Vector3 GetLocal()
    {
        if (m_Index == 0)
            return new Vector3(0, 0, 1);
        else if (m_Index == 1)
            return new Vector3(1, 0, 0);
        else if (m_Index == 2)
            return new Vector3(0, 0, -1);
        else if (m_Index == 3)
            return new Vector3(-1, 0, 0);

        return new Vector3(0, 0, 1);
    }

    void Start()
    {
        moveDirection = transform.rotation * GetLocal();
		Debug.DrawRay (transform.position, moveDirection.normalized * 5.0f, Color.cyan);
    }

    public override Vector3 Track(float deltaTime)
    {
        if (moveDirection == Vector3.zero && m_Emitter != null)
            moveDirection = m_Emitter.forward;

        return moveDirection.normalized * speed * deltaTime;
    }

	public override Quaternion Rotate (float deltaTime)
	{
		return Quaternion.LookRotation (moveDirection);
	}
}
