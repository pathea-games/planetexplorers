using UnityEngine;
using System.Collections;

public class GunAimer : MonoBehaviour {
	
	public Transform aimPivot;
	public Transform aimWeapon;
	public Transform aimTarget;
	public float effect = 1;

    public Transform bowPivot;
    public Transform singlePistolPivot;
    public Transform doublePistolPivot;

    public AnimationClip fireBowAnimation;
    public AnimationClip fireSinglePistolAnimation;
    public AnimationClip fireDoublePistolAnimation;

    public Transform reload;
    public AnimationClip reloadAnimation;
	
	private Vector3 aimDirection = Vector3.zero;
	private LayerMask mask;

    private bool aimEnable = false;

    public void SetGunAimerEnable(bool isEnable, Transform aimWeaponRelative, ItemAsset.EquipType equipType)
    {
        aimEnable = isEnable;
        if (aimEnable)
        {
            if (aimWeaponRelative != null)
            {
                aimWeapon.position = aimWeaponRelative.position;
                aimWeapon.rotation = aimWeaponRelative.rotation;
            }

            switch (equipType)
            {
                case ItemAsset.EquipType.Null:
                    aimEnable = false;
                    break;
                case ItemAsset.EquipType.Bow:
                    if (bowPivot != null)
                        aimPivot = bowPivot;
                    else
                        aimEnable = false;
                    break;
                case ItemAsset.EquipType.HandGun:
                    if (singlePistolPivot != null)
                        aimPivot = singlePistolPivot;
                    else
                        aimEnable = false;
                    break;
                case ItemAsset.EquipType.Rifle:
                    if (doublePistolPivot != null)
                        aimPivot = doublePistolPivot;
                    else
                        aimEnable = false;
                    break;
                default:
                    aimEnable = false;
                    break;
            }
        }
    }

	void Start () {
		// Add player's own layer to mask
		mask = 1 << gameObject.layer;
		// Add Igbore Raycast layer to mask
		mask |= 1 << LayerMask.NameToLayer("Ignore Raycast");
		// Invert mask
		mask = ~mask;

        if (fireBowAnimation != null)
        {
            AnimationState fireAS = GetComponent<Animation>()[fireBowAnimation.name];
            fireAS.wrapMode = WrapMode.Once;
            fireAS.layer = 101; // we put it in a separate layer than impact animations
//            fireAS.blendMode = AnimationBlendMode.Additive;
        }

        if (fireSinglePistolAnimation != null)
        {
            AnimationState fireAS = GetComponent<Animation>()[fireSinglePistolAnimation.name];
            fireAS.wrapMode = WrapMode.Once;
            fireAS.layer = 101; // we put it in a separate layer than impact animations
//            fireAS.blendMode = AnimationBlendMode.Additive;
        }

        if (fireDoublePistolAnimation != null)
        {
            AnimationState fireAS = GetComponent<Animation>()[fireDoublePistolAnimation.name];
            fireAS.wrapMode = WrapMode.Once;
            fireAS.layer = 101; // we put it in a separate layer than impact animations
//            fireAS.blendMode = AnimationBlendMode.Additive;
        }

        if (reload != null && reloadAnimation != null)
        {
            AnimationState a = GetComponent<Animation>()[reloadAnimation.name];
            a.wrapMode = WrapMode.Once;
            a.layer = 101;
//            a.blendMode = AnimationBlendMode.Additive;
//            a.AddMixingTransform(reload);
        }
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (effect <= 0 || !aimEnable || aimWeapon == null 
            || aimPivot == null || aimTarget == null)
			return;
		
		Vector3 origPos = aimWeapon.position;
		Quaternion origRot = aimWeapon.rotation;
		
		// Find pivot
		Vector3 pivot = aimPivot.position;
		
		Transform aimSpace = transform;
		
		// Find current aim direction in character space, prior to adjustment
		Vector3 pivotWeaponDirection = Quaternion.Inverse(aimSpace.rotation) * (aimWeapon.position - pivot);
		
		// Find desired aim direction in character space
		Vector3 pivotTargetDirection = Quaternion.Inverse(aimSpace.rotation) * (aimTarget.position - pivot);
		// Move direction smoothly
		pivotTargetDirection = aimDirection = Vector3.Slerp(aimDirection, pivotTargetDirection, 15*Time.deltaTime);
		
		// Get aiming rotation needed
		Quaternion rotation = Quaternion.FromToRotation(pivotWeaponDirection, pivotTargetDirection);
		
		RotateTransformAroundPointInOtherTransformSpace (aimWeapon, rotation, pivot, aimSpace);
		
		//float distFraction = 0;
		
		// Calculates horizontal angle by projecting pivotWeaponDirection and
		// pivotTargetDirection on XZ plane and taking angle between them
		Vector3 weaponDir = pivotWeaponDirection;
		Vector3 targetDir = pivotTargetDirection;
		weaponDir.y = 0;
		targetDir.y = 0;
		///*float rotY = */Vector3.Angle(weaponDir, targetDir);
		
		// Calculate distFraction based on horizontal (XZ) rotation angle
		//distFraction = 1 - (rotY / 400);
		
		//aimWeapon.position = pivot + (aimWeapon.position - pivot) * distFraction;
				
		if (effect <= 1) {
			aimWeapon.position = Vector3.Lerp(origPos, aimWeapon.position, effect);
			aimWeapon.rotation = Quaternion.Slerp(origRot, aimWeapon.rotation, effect);
		}
	}
	
	void RotateTransformAroundPointInOtherTransformSpace (Transform toRotate, Quaternion rotation, Vector3 pivot, Transform space) {
		Vector3 pivotToWeapon = toRotate.position - pivot;
		
		Vector3 globalPositionDelta = - pivotToWeapon + space.rotation * (rotation * (Quaternion.Inverse(space.rotation) * pivotToWeapon));
		
		toRotate.position += globalPositionDelta;
		toRotate.rotation = space.rotation * rotation * Quaternion.Inverse(space.rotation) * toRotate.rotation;
	}
}
