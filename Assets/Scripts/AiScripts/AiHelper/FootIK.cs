using UnityEngine;
using System.Collections;

public class FootIK : MonoBehaviour
{
    public float ikHeight;

    Transform leftFoot;
    Transform rightFoot;

    float leftFootHeight;
    float rightFootHeight;

    Animator animator;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();

        if (GetComponent<Animation>() != null)
        {
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (leftFoot != null)
            {
                Vector3 pos = transform.InverseTransformPoint(leftFoot.position);
                leftFootHeight = pos.y;
            }

            if (rightFoot != null)
            {
                Vector3 pos = transform.InverseTransformPoint(rightFoot.position);
                rightFootHeight = pos.y;
            }
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != 0 || animator == null)
            return;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.0f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.0f);

        RaycastHit hitInfo;
        if (leftFoot != null && Physics.Raycast(leftFoot.position + Vector3.up * ikHeight,
            -Vector3.up, out hitInfo, 2 * ikHeight, AiUtil.groundedLayer))
        {
            if (Mathf.Abs(hitInfo.point.y + leftFootHeight - leftFoot.position.y) > 0.5f)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.5f);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitInfo.point + Vector3.up * leftFootHeight);
            }
        }

        if (rightFoot != null && Physics.Raycast(rightFoot.position + Vector3.up * ikHeight,
            -Vector3.up, out hitInfo, 2 * ikHeight, AiUtil.groundedLayer))
        {
            if (Mathf.Abs(hitInfo.point.y + rightFootHeight - rightFoot.position.y) > 0.5f)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.5f);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, hitInfo.point + Vector3.up * rightFootHeight);
            }
        }
    }
}
