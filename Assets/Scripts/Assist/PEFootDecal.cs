using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class PEFootDecal : MonoBehaviour
{
    [System.Serializable]
    public class FootDecal
    {
        public Transform tr;

        Vector3 m_CurPosition;
        Vector3 m_PrePosition;

        Vector3 m_CurVelocity;

        bool m_Dirty;

        Stack<Vector3> m_UpHeights   = new Stack<Vector3>();
        Stack<Vector3> m_DownHeights = new Stack<Vector3>();

        public bool Dirty { set { m_Dirty = value; } }

        void UpdateLand(int musicID)
        {
            float v_y  = m_CurVelocity.y;
            float v_xz = Mathf.Sqrt(m_CurVelocity.x*m_CurVelocity.x + m_CurVelocity.z*m_CurVelocity.z);

            //Debug.LogError(tr.name + " : " + "Y = " + v_y + " --> XZ = " + v_xz);
            if (!m_Dirty)
            {
                m_DownHeights.Clear();

                if (v_y > PETools.PEMath.Epsilon && v_xz > 0.05f)
                    m_UpHeights.Push(m_CurVelocity);
                else
                {
                    m_Dirty = m_UpHeights.Count > 5;
                    m_UpHeights.Clear();
                }
            }
            else
            {
                m_UpHeights.Clear();

                if (v_y < -PETools.PEMath.Epsilon)
                    m_DownHeights.Push(m_CurVelocity);
                else
                {
                    if (m_DownHeights.Count > 5)
                    {
                        m_Dirty = false;

                        OnLand(musicID);
                    }

                    m_DownHeights.Clear();
                }
            }
        }

        void UpdateFan(int musicID)
        {
            if (!m_Dirty)
            {
                if (m_CurVelocity.y < -PETools.PEMath.Epsilon)
                    m_DownHeights.Push(m_CurVelocity);
                else
                    m_DownHeights.Clear();

                m_Dirty = m_DownHeights.Count > 5;

                if(m_Dirty)
                    m_UpHeights.Clear();

                return;
            }
            else
            {
                if (m_CurVelocity.y > PETools.PEMath.Epsilon)
                    m_UpHeights.Push(m_CurVelocity);
                else
                {
                    if (m_UpHeights.Count > 5)
                    {
                        m_Dirty = false;
                        m_DownHeights.Clear();

                        OnFan(musicID);
                    }

                    m_UpHeights.Clear();
                }
            }
        }

        bool CanAddFootStep()
        {
            int num = 0;
            PeEntity player = PeCreature.Instance.mainPlayer;
            if(player != null)
            {
                int lenth = AudioManager.instance.m_animFootStepAudios.Count;
                for (int i = 0; i < lenth; i++)
                {
                    AudioController audioCtrl = AudioManager.instance.m_animFootStepAudios[i];
                    if(audioCtrl != null && audioCtrl.mAudio != null)
                    {
                        float distance = Vector3.Distance(player.position, audioCtrl.transform.position);
                        if (distance < audioCtrl.mAudio.maxDistance)
                            num++;
                    }
                }
            }

            return num <= 3;
        }

        void OnLand(int musicID)
        {
            if(CanAddFootStep() && Random.value < 0.6f)
                AudioManager.instance.CreateFootStepAudio(tr.position, musicID);
        }

        void OnFan(int musicID)
        {
            if(CanAddFootStep() && Random.value < 0.6f)
                AudioManager.instance.CreateFootStepAudio(tr.position, musicID);
        }

        public void Update(Transform parent, int musicID, bool isSky)
        {
            m_CurPosition = tr.position;
            m_CurPosition.y = parent.InverseTransformPoint(tr.position).y;

            m_CurVelocity = m_CurPosition - m_PrePosition;

            if (isSky)
                UpdateFan(musicID);
            else
                UpdateLand(musicID);

            m_PrePosition = m_CurPosition;
        }
    }

    public static int LandForward       = Animator.StringToHash("Base Layer.Locomotion.Forward");
    public static int LandBackward      = Animator.StringToHash("Base Layer.Locomotion.Backward");
    public static int SkyForward        = Animator.StringToHash("Base Layer.Locomotion_Sky.Forward");
    public static int SkyBackward       = Animator.StringToHash("Base Layer.Locomotion_Sky.Backward");
    public static int SkyRotLeft        = Animator.StringToHash("Base Layer.Locomotion_Sky.RotLeft");
    public static int SkyRotRight       = Animator.StringToHash("Base Layer.Locomotion_Sky.RotRight");
    public static int SkyIdle           = Animator.StringToHash("Base Layer.Locomotion_Sky.Idle");
    public static int SkyIdleAttack     = Animator.StringToHash("Base Layer.Locomotion_Sky.AttackIdle");

    //static List<AudioController> s_FootAudios = new List<AudioController>();

    public bool isSky;
    public int musicID;
    public FootDecal[] foots;

    Animator m_Animator;
    PeEntity m_Entity;

    bool IsUpdate()
    {
        if (m_Entity == null || m_Entity.IsDeath() || m_Entity.isRagdoll)
            return false;

        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();

        if (m_Animator == null || m_Animator.IsInTransition(0))
            return false;

        int hash = m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

        if (isSky)
            return hash == SkyForward || hash == SkyBackward || hash == SkyRotLeft || hash == SkyRotRight || hash == SkyIdle || hash == SkyIdleAttack;
        else
            return hash == LandForward || hash == LandBackward;
    }

    void Start()
    {
        m_Entity = GetComponentInParent<PeEntity>();
    }

    void LateUpdate()
    {
        if (IsUpdate())
        {
            for (int i = 0; i < foots.Length; i++)
                foots[i].Update(transform, musicID, isSky);
        }
    }
}
