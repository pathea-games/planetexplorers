using UnityEngine;
using System.Collections;
using SkillAsset;
using System.Collections.Generic;

public partial class AiObject : SkillRunner
{
    //static float SpeedDampTime = 0.25f;
    //static float DirectionDampTime = 0.25f;

    float mLookAtWeight = 0.0f;
    float mLeftHandIKWeight = 0.0f;
    float mRightHandWeight = 0.0f;
    float mLeftFootIKWeight = 0.0f;
    float mRightFootWeight = 0.0f;

    Vector3 mLookAtPosition = Vector3.zero;
    Vector3 mLeftHandIKPosition = Vector3.zero;
    Vector3 mRightHandIKPosition = Vector3.zero;
    Vector3 mLeftFootIKPosition = Vector3.zero;
    Vector3 mRightFootIKPosition = Vector3.zero;

    Vector3 mLastPos;
    Quaternion mLastRot;

    protected virtual void UpdateAnimator()
    {
        if (m_animator == null) 
            return;

        if (m_motor != null && !(m_motor is AiAnimatorMotor))
        {
            if (!GameConfig.IsMultiMode || IsController)
            {
                UpdateMoveAnimator();
            }
        }

        if (GameConfig.IsMultiMode && !IsController)
        {
            PickDefaltAnimator();
        }
    }

    void PickDefaltAnimator()
    {
        Vector3 moveDirection = transform.position - mLastPos;
        if (moveDirection.sqrMagnitude <= 0.05f * 0.05f)
            SetFloat("Speed", 0.0f);
        else
        {
            Vector3 local = transform.InverseTransformDirection(moveDirection);
            if (local.z > PETools.PEMath.Epsilon)
                SetFloat("Speed", 1.0f, 0.25f, Time.deltaTime);
            else
                SetFloat("Speed", -1.0f, 0.25f, Time.deltaTime);
        }

        Vector3 forward = Quaternion.Inverse(mLastRot) * transform.forward;
        if (forward.x < -0.1f)
            SetFloat("Direction", -1.0f, 0.25f, Time.deltaTime);
        else if (forward.x > 0.1f)
            SetFloat("Direction", 1.0f, 0.25f, Time.deltaTime);
        else
            SetFloat("Direction", 0.0f);

        mLastPos = transform.position;
        mLastRot = transform.rotation;
    }

    public void ApplyRootMotion(bool activate)
    {
        if (m_animator == null)
            return;

        m_animator.applyRootMotion = activate;
    }

    void UpdateMoveAnimator()
    {
        Vector3 movement = motor.transform.rotation * motor.desiredMovementDirection;
        movement = Util.ProjectOntoPlane(movement, motor.transform.up);
       // Vector3 forward = Util.ProjectOntoPlane(motor.transform.forward, motor.transform.up);

        if (movement == Vector3.zero)
        {
            SetFloat("Speed", 0.0f, 0.25f, Time.deltaTime);
        }
        else
        {
            float speed = 1f;
            if (motor.maxRunSpeed - motor.maxWalkSpeed > PETools.PEMath.Epsilon)
            {
                speed = Mathf.Clamp((Mathf.Clamp(motor.maxForwardSpeed, motor.maxWalkSpeed, motor.maxRunSpeed) 
                    - motor.maxWalkSpeed) / (motor.maxRunSpeed - motor.maxWalkSpeed), 0.15f, 1f);
            }
            else
            {
                Debug.LogWarning(name + " maxRunSpeed[" + motor.maxRunSpeed + "] not big than maxWalkSpeed[" + motor.maxWalkSpeed + "].");
            }

            Vector3 moveDirection = Util.ProjectOntoPlane(motor.desiredVelocity, transform.up);
            if (Vector3.Dot(transform.forward, moveDirection.normalized) > 0)
                SetFloat("Speed", speed, 0.25f, Time.deltaTime);
            else
            {
                if (motor.desiredLookAtTran == null)
                    SetFloat("Speed", 0.0f, 0.25f, Time.deltaTime);
                else
                    SetFloat("Speed", -1.0f, 0.25f, Time.deltaTime);
            }
        }
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (m_animator == null)
            return;

        SetLookAtWeight(mLookAtWeight);
        SetLookAtPosition(mLookAtPosition);

        SetIKPositionWeight(AvatarIKGoal.LeftHand, mLeftHandIKWeight);
        SetIKPosition(AvatarIKGoal.LeftHand, mLeftHandIKPosition);

        SetIKPositionWeight(AvatarIKGoal.RightHand, mRightHandWeight);
        SetIKPosition(AvatarIKGoal.RightHand, mRightHandIKPosition);

        SetIKPositionWeight(AvatarIKGoal.LeftFoot, mLeftFootIKWeight);
        SetIKPosition(AvatarIKGoal.LeftFoot, mLeftFootIKPosition);

        SetIKPositionWeight(AvatarIKGoal.RightFoot, mRightFootWeight);
        SetIKPosition(AvatarIKGoal.RightFoot, mRightFootIKPosition);

    }

    #region
    public void SetLeftHandIKWeight(float value)
    {
        if (mLeftHandIKWeight != value)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.LeftHand, value);
            }

            mLeftHandIKWeight = value;
        }
    }

    public void SetLeftHandIKPosition(Vector3 ikPosition)
    {
        if (mLeftHandIKPosition != ikPosition)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.LeftHand, ikPosition);
            }

            mLeftHandIKPosition = ikPosition;
        }
    }

    public void SetRightHandIKWeight(float value)
    {
        if (mRightHandWeight != value)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.RightHand, value);
            }

            mRightHandWeight = value;
        }
    }

    public void SetRightHandIKPosition(Vector3 ikPosition)
    {
        if (mRightHandIKPosition != ikPosition)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.RightHand, ikPosition);
            }

            mRightHandIKPosition = ikPosition;
        }
    }

    public void SetLeftFootIKWeight(float value)
    {
        if (mLeftFootIKWeight != value)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.LeftFoot, value);
            }

            mLeftFootIKWeight = value;
        }
    }

    public void SetLeftFootIKPosition(Vector3 ikPosition)
    {
        if (mLeftFootIKPosition != ikPosition)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.LeftFoot, ikPosition);
            }

            mLeftFootIKPosition = ikPosition;
        }
    }

    public void SetRightFootIKWeight(float value)
    {
        if (mRightFootWeight != value)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.RightFoot, value);
            }

            mRightFootWeight = value;
        }
    }

    public void SetRightFootIKPosition(Vector3 ikPosition)
    {
        if (mRightFootIKPosition != ikPosition)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.RightFoot, ikPosition);
            }

            mRightFootIKPosition = ikPosition;
        }
    }

    public void LookAtWeight(float value)
    {
        if (mLookAtWeight != value)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_LookAtWeight, value);
            }

            mLookAtWeight = value;
        }
    }

    public void LookAtPosition(Vector3 ikPosition)
    {
        if (mLookAtPosition != ikPosition)
        {
            if (GameConfig.IsMultiMode && IsController)
            {
                RPCServer(EPacketType.PT_AI_LookAtPos, ikPosition);
            }

            mLookAtPosition = ikPosition;
        }
    }
    #endregion

    #region function of Animator
    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
    {
        return m_animator.GetCurrentAnimatorStateInfo(layerIndex);
    }

    public bool IsInTransition(int layerIndex)
    {
        return m_animator.IsInTransition(layerIndex);
    }
    #endregion

    #region Public animator function
    public void SetFloat(string name, float value)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetFloat_String",name,value );
        //}
        m_animator.SetFloat(name, value);
        //m_AnimatorStateHolder.SetFloat(name, value);
    }

    public void SetFloat(int id, float value)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetFloat_Int",id,value );
        //}
        m_animator.SetFloat(id, value);
        //m_AnimatorStateHolder.SetFloat(name, value);
    }

    public void SetFloat(string name, float value, float dampTime, float deltaTime)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetFloat_String_1", name, value, dampTime, deltaTime);
        //}
        m_animator.SetFloat(name, value, dampTime, deltaTime);
        //m_AnimatorStateHolder.SetFloat(name, value);
    }

    public void SetFloat(int id, float value, float dampTime, float deltaTime)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_RPC_C2S_SetFloat_Int_1", id, value, dampTime, deltaTime);
        //}
        m_animator.SetFloat(id, value, dampTime, deltaTime);
        //m_AnimatorStateHolder.SetFloat(name, value);
    }

    public void SetBool(string name, bool value)
    {
        if (GameConfig.IsMultiMode && IsController && value != m_animator.GetBool(name))
        {
			RPCServer(EPacketType.PT_AI_BoolString, name, value);
        }
        m_animator.SetBool(name, value);
        //m_AnimatorStateHolder.SetBool(name, value);
    }

    public void SetBool(int id, bool value)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
			RPCServer(EPacketType.PT_AI_BoolInt, id, value);
        }
        m_animator.SetBool(id, value);
        //m_AnimatorStateHolder.SetBool(name, value);
    }

    public void SetVector(string name, Vector3 value)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
			RPCServer(EPacketType.PT_AI_VectorString, name, value);
        }
        //m_animator.SetVector(name, value);
    }

    public void SetVector(int id, Vector3 value)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
			RPCServer(EPacketType.PT_AI_VectorInt, id, value);
        }
        //m_animator.SetVector(id, value);
    }

    public void SetInteger(string name, int value)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
			RPCServer(EPacketType.PT_AI_IntString, name, value);
        }
        m_animator.SetInteger(name, value);
    }

    public void SetInteger(int id, int value)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
			RPCServer(EPacketType.PT_AI_IntInt, id, value);
        }
        m_animator.SetInteger(id, value);
    }

    public virtual void SetLayerWeight(int layerIndex, float weight)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
			RPCServer(EPacketType.PT_AI_LayerWeight, layerIndex, weight);
        }
        m_animator.SetLayerWeight(layerIndex, weight);
    }

    public void SetLookAtWeight(float weight)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetLookAtWeight", weight);
        //}
        m_animator.SetLookAtWeight(weight);
    }

    public void SetLookAtPosition(Vector3 lookAtPosition)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetLookAtPosition", lookAtPosition);
        //}
        m_animator.SetLookAtPosition(lookAtPosition);
    }

    public void SetIKPositionWeight(AvatarIKGoal goal, float value)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetIKPositionWeight", goal,value);
        //}
        m_animator.SetIKPositionWeight(goal, value);
    }

    public void SetIKPosition(AvatarIKGoal goal, Vector3 goalPosition)
    {
        //if (GameConfig.IsMultiMode && IsController)
        //{
        //    RPC("RPC_C2S_SetIKPosition", goal, goalPosition);
        //}
        m_animator.SetIKPosition(goal, goalPosition);
    }

    public void SetIKRotationWeight(AvatarIKGoal goal, float value)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
            RPCServer(EPacketType.PT_AI_IKRotWeight, goal,value);
        }
        m_animator.SetIKRotationWeight(goal, value);
    }

    public void SetIKRotation(AvatarIKGoal goal, Quaternion goalPosition)
    {
        if (GameConfig.IsMultiMode && IsController)
        {
            RPCServer(EPacketType.PT_AI_IKRotation, goal, goalPosition);
        }
        m_animator.SetIKRotation(goal, goalPosition);
    }

    public Transform GetBoneTransform(HumanBodyBones humanBodyId)
    {
        return m_animator.GetBoneTransform(humanBodyId);
    }

    public Vector3 GetIKPosition(AvatarIKGoal goal)
    {
        return m_animator.GetIKPosition(goal);
    }

    public float GetIKPositionWeight(AvatarIKGoal goal)
    {
        return m_animator.GetIKPositionWeight(goal);
    }
    #endregion


    #region Public animator network function

    public void NetWorkSetFloat(string name, float value)
    {
        m_animator.SetFloat(name, value);
    }

    public void NetWorkSetFloat(int id, float value)
    {
        m_animator.SetFloat(id, value);
    }

    public void NetWorkSetFloat(string name, float value, float dampTime, float deltaTime)
    {
        m_animator.SetFloat(name, value, dampTime, deltaTime);
    }

    public void NetWorkSetFloat(int id, float value, float dampTime, float deltaTime)
    {
        m_animator.SetFloat(id, value, dampTime, deltaTime);
    }

    public void NetWorkSetBool(string name, bool value)
    {
        m_animator.SetBool(name, value);
    }

    public void NetWorkSetBool(int id, bool value)
    {
        m_animator.SetBool(id, value);
    }

    //public void NetWorkSetVector(string name, Vector3 value)
    //{
    //    m_animator.SetVector(name, value);
    //}

    //public void NetWorkSetVector(int id, Vector3 value)
    //{
    //    m_animator.SetVector(id, value);
    //}

    public void NetWorkSetInteger(string name, int value)
    {
        m_animator.SetInteger(name, value);
    }

    public void NetWorkSetInteger(int id, int value)
    {
        m_animator.SetInteger(id, value);
    }

    public void NetWorkSetLayerWeight(int layerIndex, float weight)
    {
        m_animator.SetLayerWeight(layerIndex, weight);
    }

    public void NetWorkSetLookAtWeight(float weight)
    {
        m_animator.SetLookAtWeight(weight);
    }

    public void NetWorkSetLookAtPosition(Vector3 lookAtPosition)
    {
        m_animator.SetLookAtPosition(lookAtPosition);
    }

    public void NetWorkSetIKPositionWeight(AvatarIKGoal goal, float value)
    {
        m_animator.SetIKPositionWeight(goal, value);
    }

    public void NetWorkSetIKPosition(AvatarIKGoal goal, Vector3 goalPosition)
    {
        m_animator.SetIKPosition(goal, goalPosition);
    }

    public void NetWorkSetIKRotationWeight(AvatarIKGoal goal, float value)
    {
        m_animator.SetIKRotationWeight(goal, value);
    }

    public void NetWorkSetIKRotation(AvatarIKGoal goal, Quaternion goalPosition)
    {
        m_animator.SetIKRotation(goal, goalPosition);
    }

    #endregion
}

public class AnimatorStateHolder
{
    Animator mAnimator;

    Dictionary<string, bool> mBoolValue = new Dictionary<string, bool>(5);
    Dictionary<string, float> mFloatValue = new Dictionary<string, float>(5);

    public AnimatorStateHolder(Animator animator)
    {
        mAnimator = animator;
    }

    public void SetBool(string name, bool value)
    {
        mBoolValue[name] = value;
    }

    public void SetFloat(string name, float value)
    {
        mFloatValue[name] = value;
    }

    public void RestoreAnimator()
    {
        if (null == mAnimator)
        {
            Debug.LogError("Animator is null.");
            return;
        }

        foreach (KeyValuePair<string, bool> kv in mBoolValue)
        {
            mAnimator.SetBool(kv.Key, kv.Value);
        }

        foreach (KeyValuePair<string, float> kv in mFloatValue)
        {
            mAnimator.SetFloat(kv.Key, kv.Value);
        }
    }
}