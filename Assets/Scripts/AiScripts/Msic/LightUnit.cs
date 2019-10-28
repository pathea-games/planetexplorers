using UnityEngine;
using System.Collections;

public class LightUnit : MonoBehaviour
{
    public Light lamp;
	public LightShadows shadowsBak{ get; private set; }
	public LightRenderMode renderModeBak{ get; private set; }

    Transform m_Trans;
    Vector3 m_Position;
    Vector3 m_Forward;

    void Start()
    {
        m_Trans = transform;
        lamp = GetComponent<Light>();
		shadowsBak = lamp.shadows;
		renderModeBak = lamp.renderMode;
        if (LightMgr.Instance != null)
        {
            LightMgr.Instance.Registerlight(this);
        }
    }

    void LateUpdate()
    {
        m_Forward = m_Trans.forward;
        m_Position = m_Trans.position;
    }

    void OnDestroy()
    {
        if (LightMgr.Instance != null)
        {
            LightMgr.Instance.RemoveLight(this);
        }
    }

    public Vector3 GetPositionOutOfLight(Vector3 pos)
    {
        if (lamp == null)
            return Vector3.zero;

        switch (lamp.type)
        {
            case LightType.Spot:

                for (int i = 0; i < 10; i++)
                {
                    Vector3 v = PETools.PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 0.5f*lamp.range, 2.0f*lamp.range, -90.0f, 90.0f);
                    if (Vector3.Angle(m_Forward, v - m_Position) >= lamp.spotAngle)
                        return v;
                }

                return PETools.PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 0.5f * lamp.range, 2.0f * lamp.range, -90.0f, 90.0f);
            case LightType.Directional:
                break;
            case LightType.Point:
                return PETools.PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 1.5f*lamp.range, 3.0f*lamp.range, -75.0f, 75.0f);
            case LightType.Area:
                //float radius = Mathf.Max(light.areaSize.x, light.areaSize.y);
                //return PETools.PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 1.5f*radius, 3.0f*radius, -75.0f, 75.0f);
                return Vector3.zero;
            default:
                break;
        }

        return Vector3.zero;
    }

    public bool IsInLight(Vector3 point)
    {
        if (lamp == null || !lamp.isActiveAndEnabled)
            return false;

        switch (lamp.type)
        {
            case LightType.Spot:
                float sqrDis = PETools.PEUtil.SqrMagnitude(m_Position, point);
                float angle = Vector3.Angle(m_Forward, point - m_Position);
                return sqrDis <= lamp.range*lamp.range && angle <= lamp.spotAngle;
            case LightType.Directional:
                return false;
            case LightType.Point:
                return PETools.PEUtil.SqrMagnitude(m_Position, point) <= lamp.range*lamp.range;
            case LightType.Area:
                //Vector3 v = transform.InverseTransformPoint(point);
                //return Mathf.Abs(v.x) <= light.areaSize.x && Mathf.Abs(v.y) <= light.areaSize.y;
                return false;
            default:
                break;
        }

        return false;
    }

    public bool IsInLight(Transform tr)
    {
        if (tr == null)
            return false;

        return IsInLight(tr.position);
    }

    public bool IsInLight(Transform tr, Bounds bounds)
    {
        if (tr == null || bounds.size == Vector3.zero || lamp == null)
            return false;

        if (PETools.PEUtil.SqrMagnitude(m_Position, tr.position, false) > lamp.range * 2 * lamp.range * 2)
            return false;

        Vector3 v1 = bounds.center;
        Vector3 v2 = bounds.center + bounds.extents.y * new Vector3(0, 1, 0);
        Vector3 v3 = bounds.center - bounds.extents.y * new Vector3(0, 1, 0);

        //Debug.DrawLine(m_Position, tr.TransformPoint(v1), Color.cyan);
        //Debug.DrawLine(m_Position, tr.TransformPoint(v2), Color.cyan);
        //Debug.DrawLine(m_Position, tr.TransformPoint(v3), Color.cyan);

        if (IsInLight(tr.TransformPoint(v1)))
            return true;

        if (IsInLight(tr.TransformPoint(v2)))
            return true;

        if (IsInLight(tr.TransformPoint(v3)))
            return true;

        Vector3[] vert = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            vert[i] = bounds.center;
            if ((i & 1) == 0)
                vert[i] -= bounds.extents.x * new Vector3(1, 0, 0);
            else
                vert[i] += bounds.extents.x * new Vector3(1, 0, 0);
            if ((i & 2) == 0)
                vert[i] -= bounds.extents.y * new Vector3(0, 1, 0);
            else
                vert[i] += bounds.extents.y * new Vector3(0, 1, 0);
            if ((i & 4) == 0)
                vert[i] -= bounds.extents.z * new Vector3(0, 0, 1);
            else
                vert[i] += bounds.extents.z * new Vector3(0, 0, 1);

            vert[i] = tr.TransformPoint(vert[i]);
            //Debug.DrawLine(m_Position, vert[i], Color.cyan);
            if (IsInLight(vert[i])) return true;
        }

        return false;
    }
}
