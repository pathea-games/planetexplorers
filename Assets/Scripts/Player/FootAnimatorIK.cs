using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class FootAnimatorIK : MonoBehaviour 
{
	public Transform LeftFootBone;
	public Transform RightFootBone;
	
	public float	MaxStepHeight = 0.4f;
	public float	AnimChangeSpeed = 10f;
	[HideInInspector]
	public bool	mGrounded = true;
	
	public float	BodyOffset = 0.05f;
	public float	FootOffset = 0f;
	
	float	LeftFootIKHeight;
	float	RightFootIKHeight;
	float	BodyIKHeight;
	
	public Animator	mAnimator;
	
	bool	UPD;
	Vector3 leftFootPos = Vector3.zero;
	Vector3 rightFootPos = Vector3.zero;
	Vector3 bodyPos = Vector3.zero;
	[HideInInspector]
	public Vector3 FootDir;
	
	// Use this for initialization
	void Start () 
	{
		mAnimator = GetComponent<Animator>();
		LeftFootIKHeight = 0;
		RightFootIKHeight = 0;
		BodyIKHeight = 0;
	}

	bool CheckBones()
	{
		if(null == LeftFootBone)
			LeftFootBone = AiUtil.GetChild(transform, "Bip01 L Foot");
		if(null == RightFootBone)
			RightFootBone = AiUtil.GetChild(transform, "Bip01 R Foot");
		return null != LeftFootBone && null != RightFootBone;
	}
	
	// Update is called once per frame
	void Update () 
	{
		UPD = true;
	}
	
	void OnAnimatorIK()
	{
		if (mGrounded && null != mAnimator && CheckBones())
		{
			if(UPD) //UpdateBonePos. Call once per frame
			{
				UPD = false;
				RaycastHit rayHit;
				Vector3 LFootIkPos = Vector3.zero;
				Vector3 RFootIkPos = Vector3.zero;
				if(Physics.Raycast(LeftFootBone.position + (MaxStepHeight - LeftFootBone.position.y + transform.position.y)* Vector3.up,Vector3.down,out rayHit,2 * MaxStepHeight
					,1<<Pathea.Layer.VFVoxelTerrain | 1<<Pathea.Layer.SceneStatic | 1<<Pathea.Layer.Unwalkable))
				{
					if(rayHit.distance < 2 * MaxStepHeight && !rayHit.collider.isTrigger)
					{
						LeftFootIKHeight = Mathf.Lerp(LeftFootIKHeight, rayHit.point.y - transform.position.y, AnimChangeSpeed * Time.deltaTime);
						LFootIkPos = rayHit.point;
					}
					else
						LFootIkPos = LeftFootBone.position;
					
				}
				else
				{
					LeftFootIKHeight = Mathf.Lerp(LeftFootIKHeight, 0, AnimChangeSpeed * Time.deltaTime);
					LFootIkPos = LeftFootBone.position;
				}
				
				if(Physics.Raycast(RightFootBone.position + (MaxStepHeight - RightFootBone.position.y + transform.position.y)* Vector3.up,Vector3.down,out rayHit,2 * MaxStepHeight
					,1<<Pathea.Layer.VFVoxelTerrain | 1<<Pathea.Layer.SceneStatic | 1<<Pathea.Layer.Unwalkable))
				{
					if(rayHit.distance < 2 * MaxStepHeight)
					{
						RightFootIKHeight = Mathf.Lerp(RightFootIKHeight, rayHit.point.y - transform.position.y, AnimChangeSpeed * Time.deltaTime);
						RFootIkPos = rayHit.point;
					}
					else
						RFootIkPos = RightFootBone.position;
				}
				else
					RightFootIKHeight = Mathf.Lerp(RightFootIKHeight, 0, AnimChangeSpeed * Time.deltaTime);

				FootDir = LFootIkPos - RFootIkPos;
				
				BodyIKHeight = Mathf.Lerp(BodyIKHeight, Mathf.Min(LeftFootIKHeight,RightFootIKHeight) + BodyOffset, AnimChangeSpeed * Time.deltaTime);
				leftFootPos = LeftFootBone.position + (LeftFootIKHeight + FootOffset) * Vector3.up;
				rightFootPos = RightFootBone.position + (RightFootIKHeight + FootOffset) * Vector3.up;
				bodyPos = mAnimator.bodyPosition + BodyIKHeight * Vector3.up;
			}
			// Set BonePos would call manytime per frame
			mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1f);
			mAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot,1f);
			
			mAnimator.bodyPosition = bodyPos;
			mAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);
			mAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPos);
		}
	}
}
