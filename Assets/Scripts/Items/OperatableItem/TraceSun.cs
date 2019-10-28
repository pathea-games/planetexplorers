using System.Collections;
using UnityEngine;

public class TraceSun : MonoBehaviour
{
    public Transform chassis;
    public Transform pitchPivot;
    float pitchAngle = 60.0f;

    Vector3 aimDirection = Vector3.zero;

    //IEnumerator Start()
    //{
    //    while (true)
    //    {
    //        UpdateRotation();

    //        yield return new WaitForSeconds(60f);
    //    }
    //}

    Vector3 SunDirection()
    {
		return Vector3.up;
       // return NVWeatherSys.Instance.Sun.m_Transform.forward;  [Edit by zx]
    }

    void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        Vector3 direction = SunDirection();

        if (direction == Vector3.zero)
        {
            return;
        }

        if (direction.y < 0f)
        {
            return;
        }

        if (chassis != null)
        {
            Vector3 targetDirection = direction;
            targetDirection.y = 0.0f;

            Vector3 pivotTargetDirection = targetDirection;

            aimDirection = Vector3.Slerp(aimDirection, pivotTargetDirection, 15 * Time.deltaTime);
            

            if (aimDirection != Vector3.zero)
            {
                Vector3 forward = chassis.forward;
                forward.y = 0.0f;
                Quaternion rotation = Quaternion.FromToRotation(forward, aimDirection);
                chassis.rotation *= rotation;
            }
        }

        if (pitchPivot != null)
        {
            Vector3 _vec = AiAsset.AiMath.ProjectOntoPlane(direction, pitchPivot.up);

            if (Vector3.Dot(_vec.normalized, pitchPivot.forward) > Mathf.Cos(Mathf.Deg2Rad * pitchAngle))
            {
                Vector3 aimDirection = Vector3.Slerp(pitchPivot.forward, _vec, 15 * Time.deltaTime);
                Quaternion rot = Quaternion.FromToRotation(pitchPivot.forward, aimDirection);
                pitchPivot.rotation = rot * pitchPivot.rotation;
            }
        }
    }
}
