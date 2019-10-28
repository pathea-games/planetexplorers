using UnityEngine;
using System.Collections;

public class TreeCutDown : MonoBehaviour
{
    const int scaling = 30;
    [SerializeField]
    private AnimationCurve animaCur;
    private float startTime;
    private float downTime;

    private Vector3 downDirection;
    private Vector3[] footsPos;
    private float height;
    private int radius;
    private bool lockRotate = false;
    private bool lockDown = true;
    private bool lockPartical = false;
    private const float particalDelayTime = 2;

    private static AnimationCurve anim;
    private static AnimationCurve Anim
    {
        get
        {
            if (anim == null)
                anim = new AnimationCurve(((GameObject)Resources.Load("Prefab/Item/Other/CutDownTreeCurve")).GetComponent<TreeCutDown>().animaCur.keys);
            return TreeCutDown.anim;
        }
    }

    public void SetDirection(Vector3 casterPos, float height, float radius, Vector3[] footsPos = null)
    {
        if (footsPos == null)
        {
            Vector3 foward = this.transform.position - casterPos;
            foward = new Vector3(foward.x, 0, foward.z);
            Vector3 left = Vector3.Cross(foward.normalized, Vector3.up);

            Vector3 downDir = (foward + left).normalized;
            downDirection = Vector3.Cross(Vector3.up, downDir).normalized;
        }
        else
        {
            if (footsPos.Length == 2)
            {
                Vector3 v = footsPos[0] - footsPos[1];
                Vector3 vertical = Vector3.Cross(v, Vector3.up);
                if (Vector3.Dot(vertical, casterPos - (footsPos[0] / 2 + footsPos[1] / 2)) > 0)
                    downDirection = (new Vector3(-v.x, 0, -v.z)).normalized;
                else
                    downDirection = (new Vector3(v.x, 0, v.z)).normalized;
                radius /= 2;
                this.footsPos = footsPos;
            }
        }

        System.Math.Ceiling(radius);
        this.radius = radius >= 7 ? 8 : radius >= 5 ? 6 : radius >= 3 ? 4 : 2;
        this.height = height;

        animaCur = Anim;
        startTime = Time.time;
    }

    private void PlayRootPartical()
    {
        if (!lockPartical && Time.time - startTime > particalDelayTime)
        {
            if (footsPos == null)
            {
                RaycastHit hit;
                Physics.Raycast(transform.position + (Vector3.up) * 10, Vector3.down, out hit, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
                Pathea.Effect.EffectBuilder.Instance.Register(179 + radius / 2, null,
                    (hit.collider == null ? transform.position : hit.point) + new Vector3(0, -0.2f, 0), Quaternion.identity);
            }
            else
            {
                RaycastHit hit1, hit2;
                Physics.Raycast(footsPos[0] + (Vector3.up) * 10, Vector3.down, out hit1, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
                Physics.Raycast(footsPos[1] + (Vector3.up) * 10, Vector3.down, out hit2, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
                Pathea.Effect.EffectBuilder.Instance.Register(179 + radius / 2, null,
                    (hit1.collider == null ? footsPos[0] : hit1.point) + new Vector3(0, -0.2f, 0), Quaternion.identity);
                Pathea.Effect.EffectBuilder.Instance.Register(179 + radius / 2, null,
                    (hit2.collider == null ? footsPos[1] : hit2.point) + new Vector3(0, -0.2f, 0), Quaternion.identity);
            }
            AudioManager.instance.Create(transform.position, radius < 5 ? 947 : radius < 7 ? 945 : 939);
            lockPartical = true;
        }
    }

    void Update()
    {
        if (downDirection == Vector3.zero)
            return;
        PlayRootPartical();
        if (!lockRotate)
        {
            transform.Rotate(downDirection, Time.deltaTime * scaling * animaCur.Evaluate(Time.time - startTime));

            Vector3 dir = Vector3.Cross(downDirection, transform.up).normalized;
            RaycastHit hit1, hit2;
            Physics.Raycast(transform.position + transform.up * height, dir, out hit1, height * 0.2f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
            Physics.Raycast(transform.position + transform.up * height * 0.5f, dir, out hit2, height * 0.1f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
            if (hit1.collider != null || hit2.collider != null)
            {
                if (hit1.collider != null)
                    Pathea.Effect.EffectBuilder.Instance.Register(175 + radius / 2, null, hit1.point + new Vector3(0, 1, 0), Quaternion.identity);
                else
                    Pathea.Effect.EffectBuilder.Instance.Register(175 + radius / 2, null, hit2.point + new Vector3(0, 1, 0), Quaternion.identity);

                AudioManager.instance.Create(transform.position, radius < 5 ? 948 : radius < 7 ? 946 : 940);
                lockRotate = true;
                GameObject.Destroy(this.gameObject, 10f);
                downTime = Time.time;
            }
            if (Time.time - startTime > 6f)     //防止地形没有的情况下树木接触不到地面，一直旋转
            {
                lockRotate = true;
                GameObject.Destroy(this.gameObject, 10f);
                downTime = Time.time;
            }
        }
        else
        {
            if (!lockDown)
                transform.Translate(Vector3.down * 0.01f * radius * Time.deltaTime * scaling, Space.World);
            else if (Time.time - downTime > 1)
                lockDown = false;
            else
                transform.Rotate(downDirection, Time.deltaTime * scaling * animaCur.Evaluate(Time.time - downTime + 7));
        }
    }
}
