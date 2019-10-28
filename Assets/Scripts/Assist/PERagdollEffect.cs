using UnityEngine;
using System.Collections;
using PETools;

public class PERagdollEffect : MonoBehaviour
{
    static int s_Layer;
    static int s_TerrainLayer;
    static GameObject s_ParticleSmall;
    static GameObject s_ParticleLarge;

    bool m_IsActive;
    [SerializeField]
    Rigidbody[] m_Rigibodys;
    [SerializeField]
    PECollision[] m_Collisions;

    public bool IsActive { set { m_IsActive = value; } }

    public void ResetRagdoll()
    {
        //PECollision[] cols = PEUtil.GetAllCmpts<PECollision>(transform);
        //for (int n = 0; n < cols.Length; n++)
        //    GameObject.DestroyImmediate(cols[n], true);

        m_Rigibodys = PEUtil.GetCmpts<Rigidbody>(transform);
        if(m_Rigibodys != null && m_Rigibodys.Length > 0)
        {
            m_Collisions = new PECollision[m_Rigibodys.Length];
            for (int i = 0; i < m_Rigibodys.Length; i++)
            {
                PECollision[] cols = m_Rigibodys[i].gameObject.GetComponents<PECollision>();
                for (int j = 0; j < cols.Length; j++)
                {
                    if (m_Collisions[i] == null)
                        m_Collisions[i] = cols[j];
                    else
                        GameObject.DestroyImmediate(cols[j], true);

                }

                if (m_Collisions[i] == null)
                    m_Collisions[i] = m_Rigibodys[i].gameObject.AddComponent<PECollision>();
            }
        }
    }

    void Awake()
    {
        if(s_ParticleSmall == null)
            s_ParticleSmall = AssetsLoader.Instance.LoadPrefabImm("Prefab/Particle/FX_enemyFall") as GameObject;

        if(s_ParticleLarge == null)
			s_ParticleLarge = AssetsLoader.Instance.LoadPrefabImm("Prefab/Particle/FX_enemyFall_large") as GameObject;

        if (s_Layer == 0)
            s_Layer = 1 << Pathea.Layer.VFVoxelTerrain
                        | 1 << Pathea.Layer.SceneStatic
                        | 1 << Pathea.Layer.Unwalkable;

        if (s_TerrainLayer == 0)
            s_TerrainLayer = 1 << Pathea.Layer.VFVoxelTerrain;

        if (m_Collisions != null && m_Collisions.Length > 0)
        {
            for (int i = 0; i < m_Collisions.Length; i++)
            {
                m_Collisions[i].enter += OnCollisionChildEnter;
            }
        }
    }

    void FixedUpdate()
    {
        if (m_Rigibodys == null || m_Rigibodys.Length == 0 || !m_IsActive)
            return;

        for (int i = 0; i < m_Rigibodys.Length; i++)
        {
            if (m_Rigibodys[i] == null) continue;

            float height;
            if (PETools.PEUtil.GetWaterSurfaceHeight(m_Rigibodys[i].worldCenterOfMass, out height))
            {
                //float h = Mathf.Max(0.0f, height - m_Rigibodys[i].worldCenterOfMass.y);

                if (!Physics.Raycast(m_Rigibodys[i].position, Vector3.up, 3.0f, s_Layer))
                {
                    float h = Mathf.Clamp(height - m_Rigibodys[i].worldCenterOfMass.y, 0.0f, 2.0f);
                    m_Rigibodys[i].AddForce((-Physics.gravity + Vector3.up * h) * m_Rigibodys[i].mass);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (m_Collisions != null && m_Collisions.Length > 0)
        {
            for (int i = 0; i < m_Collisions.Length; i++)
            {
                m_Collisions[i].enter -= OnCollisionChildEnter;
            }
        }
    }

    float GetRadius(Collider collider)
    {
        BoxCollider boxCollider = collider as BoxCollider;
        SphereCollider sphereCollider = collider as SphereCollider;
        CapsuleCollider capsuleCollider = collider as CapsuleCollider;

        if (boxCollider != null)
            return boxCollider.size.x * boxCollider.size.y * boxCollider.size.z;
        else if (sphereCollider != null)
            return sphereCollider.radius * sphereCollider.radius * sphereCollider.radius * Mathf.PI * 1.334f;
        else if (capsuleCollider != null)
            return capsuleCollider.radius * capsuleCollider.radius * Mathf.PI * capsuleCollider.height;

        return 0.0f;
    }

    void OnCollisionChildEnter(Collider col, Collision info)
    {
        int layer = 1 << info.gameObject.layer;
        if ((s_Layer & layer) == 0)
            return;

        //Debug.LogError("impulse : " + info.impulse.magnitude + " --> " + "relativeVelocity : " + info.relativeVelocity.magnitude);

        if (info.relativeVelocity.sqrMagnitude < 5.0f * 5.0f)
            return;

        GameObject obj;

        if (GetRadius(col) < 1f*1f*1f)
        //if (m_Radius < 5.0f)
            obj = GameObject.Instantiate(s_ParticleSmall, info.contacts[0].point, Quaternion.identity) as GameObject;
        else
            obj = GameObject.Instantiate(s_ParticleLarge, info.contacts[0].point, Quaternion.identity) as GameObject;

        if (obj != null)
            GameObject.Destroy(obj, 5.0f);
    }
}
