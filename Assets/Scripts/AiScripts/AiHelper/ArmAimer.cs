using UnityEngine;
using System.Collections;

public class ArmAimer : MonoBehaviour
{
    public Transform aimPivot;
    public Transform aimWeapon;
    public Transform aimWeaponModel;
    public Vector3 aimTarget;
    public float effect;

    public Transform IKLeft;
    public Transform IKRight;

    private Vector3 aimDirection = Vector3.zero;
    private LayerMask mask;

    Transform armIKSrcLeft;
    Transform armIKSrcRight;

    Vector3 localPosition;
    Quaternion localRotation;
    float distance;

    void Start()
    {
        // Add player's own layer to mask
        mask = 1 << gameObject.layer;
        // Add Igbore Raycast layer to mask
        mask |= 1 << LayerMask.NameToLayer("Ignore Raycast");
        // Invert mask
        mask = ~mask;

        armIKSrcLeft  = AiUtil.GetChild(transform, "IKHandLeft");
        armIKSrcRight = AiUtil.GetChild(transform, "IKHandRight");

        localPosition = aimWeapon.localPosition;
        localRotation = aimWeapon.localRotation;
        distance = Vector3.Distance(aimPivot.position, aimWeapon.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (effect <= PETools.PEMath.Epsilon)
        {
            aimWeapon.localPosition = localPosition;
            aimWeapon.localRotation = localRotation;
            return;
        }

        aimWeapon.position = aimWeaponModel.position;
        aimWeapon.rotation = aimWeaponModel.rotation;

        Vector3 origPos = aimWeapon.position;
        Quaternion origRot = aimWeapon.rotation;

        // Find pivot
        Vector3 pivot = aimPivot.position;

        Transform aimSpace = transform;

        // Find current aim direction in character space, prior to adjustment
        Vector3 pivotWeaponDirection = Quaternion.Inverse(aimSpace.rotation) * (aimWeapon.position - pivot);

        // Find desired aim direction in character space
        //Vector3 pivotTargetDirection = Quaternion.Inverse(aimSpace.rotation) * (aimTarget.position - pivot);
        Vector3 pivotTargetDirection = Vector3.zero;
        if (aimTarget == Vector3.zero)
            pivotTargetDirection = Quaternion.Inverse(aimSpace.rotation) * aimSpace.forward;
        else
            pivotTargetDirection = Quaternion.Inverse(aimSpace.rotation) * (aimTarget - pivot);

        // Move direction smoothly
        pivotTargetDirection = aimDirection = Vector3.Slerp(aimDirection, pivotTargetDirection, 15 * Time.deltaTime);

        // Get aiming rotation needed
        Quaternion rotation = Quaternion.FromToRotation(pivotWeaponDirection, aimDirection);

        RotateTransformAroundPointInOtherTransformSpace(aimWeapon, rotation, pivot, aimSpace);

        //float distFraction = 0;

        // Calculates horizontal angle by projecting pivotWeaponDirection and
        // pivotTargetDirection on XZ plane and taking angle between them
        Vector3 weaponDir = pivotWeaponDirection;
        Vector3 targetDir = pivotTargetDirection;
        weaponDir.y = 0;
        targetDir.y = 0;
//        float rotY = Vector3.Angle(weaponDir, targetDir);

        // Calculate distFraction based on horizontal (XZ) rotation angle
        //distFraction = 1 - (rotY / 400);

        //aimWeapon.position = pivot + (aimWeapon.position - pivot) * distFraction;
        aimWeapon.position = pivot + (aimWeapon.position - pivot).normalized * distance;

        //AiNative native = GetComponent<AiNative>();
        //if(native is AiPaja)
        //    aimWeapon.rotation = Quaternion.FromToRotation(aimWeapon.forward, aimWeapon.position - pivot) * aimWeapon.rotation;
        //else
        //    aimWeapon.rotation = Quaternion.FromToRotation(-aimWeapon.right, aimWeapon.position - pivot) * aimWeapon.rotation;


        //Quaternion rot = Quaternion.FromToRotation(aimWeapon.forward, aimSpace.forward);
        //aimWeapon.rotation = rot * Quaternion.LookRotation(aimDirection) * Quaternion.Inverse(rot) * aimWeapon.rotation;

        if (effect <= 1)
        {
            aimWeapon.position = Vector3.Lerp(origPos, aimWeapon.position, effect);
            aimWeapon.rotation = Quaternion.Slerp(origRot, aimWeapon.rotation, effect);
        }

        //RecordIKPosition();
    }

    void LateUpdate()
    {
        if (effect <= PETools.PEMath.Epsilon)
            return;

        if (aimWeapon != null && aimWeaponModel != null)
        {
            aimWeaponModel.position = aimWeapon.position;
            aimWeaponModel.rotation = aimWeapon.rotation;
        }
    }

    void RotateTransformAroundPointInOtherTransformSpace(Transform toRotate, Quaternion rotation, Vector3 pivot, Transform space)
    {
        Vector3 pivotToWeapon = toRotate.position - pivot;

        Vector3 globalPositionDelta = -pivotToWeapon + space.rotation * (rotation * (Quaternion.Inverse(space.rotation) * pivotToWeapon));

        toRotate.position += globalPositionDelta;
        toRotate.rotation = space.rotation * rotation * Quaternion.Inverse(space.rotation) * toRotate.rotation;
    }

    void RecordIKPosition()
    {
        if (IKLeft != null && armIKSrcLeft != null)
        {
            IKLeft.position = armIKSrcLeft.position;
            IKLeft.rotation = armIKSrcLeft.rotation;
        }

        if (IKRight != null && armIKSrcRight != null)
        {
            IKRight.position = armIKSrcRight.position;
            IKRight.rotation = armIKSrcRight.rotation;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        //if (layerIndex != 1)
        //    return;

        AiObject aiObject = GetComponent<AiObject>();
        if (aiObject == null)
            return;

        if (GameConfig.IsMultiMode && !aiObject.IsController)
            return;

        if (effect <= PETools.PEMath.Epsilon)
        {
            aiObject.LookAtWeight(0.0f);
            aiObject.SetLeftHandIKWeight(0.0f);
            aiObject.SetRightHandIKWeight(0.0f);
        }
        else
        {
            aiObject.LookAtWeight(effect);

            Vector3 lookAtPosition = Vector3.zero;
            if (aimTarget != Vector3.zero)
                lookAtPosition = aimTarget;
            else
            {
                Transform eye = aiObject.GetBoneTransform(HumanBodyBones.Head);
                Vector3 eyePosition = transform.InverseTransformPoint(eye.position);
                lookAtPosition = transform.TransformPoint(new Vector3(0.0f, eyePosition.y, 1.0f));
            }

            aiObject.LookAtPosition(lookAtPosition);

            aiObject.SetLeftHandIKWeight(1.0f);
            aiObject.SetRightHandIKWeight(1.0f);

            aiObject.SetLeftHandIKPosition(IKLeft.position);
            aiObject.SetRightHandIKPosition(IKRight.position);
        }
    }
}
