using UnityEngine;
using System.Collections;

public class GroundEffect_RK : MonoBehaviour
{
    public Transform dirtyLeft;
    public Transform dirtyRight;
    public int particleId;
    void Start()
    {
        Animation anim = GetComponentInChildren<Animation>();
        if (anim != null)
        {
            AnimationEvent _event = new AnimationEvent();
            _event.time = 0.5f;
            _event.functionName = "GoundDirtyEffect";

            AnimationClip _clip = anim["skywalk"].clip;
            if (_clip != null) _clip.AddEvent(_event);

            AnimationClip _clip1 = anim["skyaTKIdle"].clip;
            if (_clip1 != null) _clip1.AddEvent(_event);
			
			AnimationClip _clip2 = anim["skyrun"].clip;
            if (_clip2 != null) _clip2.AddEvent(_event);
			
			AnimationClip _clip3 = anim["skyidle"].clip;
            if (_clip3 != null) _clip3.AddEvent(_event);
        }
    }

    public void GoundDirtyEffect()
    {
        if (dirtyLeft == null || dirtyRight == null || particleId <= 0) return;

        RaycastHit hitInfo;
        if (Physics.Raycast(dirtyLeft.position, -Vector3.up, out hitInfo, 8, AiUtil.groundedLayer))
        {
            EffectManager.Instance.Instantiate(particleId, hitInfo.point, Quaternion.identity, null);
        }

        if (Physics.Raycast(dirtyRight.position, -Vector3.up, out hitInfo, 8, AiUtil.groundedLayer))
        {
            EffectManager.Instance.Instantiate(particleId, hitInfo.point, Quaternion.identity, null);
        }
    }
}
