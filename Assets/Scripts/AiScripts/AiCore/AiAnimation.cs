using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using AiAsset;

public partial class AiObject : SkillRunner
{
    protected string m_currentAnimName = "";

	bool m_updateDefaultAnimation = true;

    public bool isPlaying
    {
        get
        {
            if (m_animation == null)
                return false;
            else
                return m_animation.isPlaying;
        }
    }

    public Animation anim { get { return m_animation; } }

	public bool updateDefaultAnimation{set{m_updateDefaultAnimation = value;}}

    protected virtual void PickDefaltAnimation()
    {

    }

	public virtual float animationTime{get{return 0.0f;}}

    public void CrossFade(string name, bool isPlay = true)
    {
        if (m_animation != null)
        {
            if(isPlay)
            {
                PlayAiAnimation(name);
            }
        }
        else
        {
            if (m_animator != null)
            {
                SetBool(name, isPlay);
            }
        }
    }

    public void PlayDefaultAnimation(string name)
    {
        if (m_animation == null)
            return;

        if (m_animation[name] == null)
            return;

        if (m_animation.IsPlaying(name))
            return;

        m_animation.CrossFade(name);
    }


    public virtual void PlayAiAnimation(string name)
    {
        if (m_animation == null
            || m_animation[name] == null
            || m_currentAnimName == name)
            return;

        m_currentAnimName = name;
        m_animation.CrossFade(name);
        PlayAnimationAudio(name);

        if (GameConfig.IsMultiMode && IsController)
            RPCServer(EPacketType.PT_AI_Animation, name);
    }

    public AnimationState GetAnimationState(string name)
    {
        if (m_animation == null)
            return null;

        return m_animation[name];
    }

    public void ClearAnimation()
    {
        m_currentAnimName = "";
    }

    public virtual bool IsPlaying(string name)
    {
        if (m_animation != null)
        {
            return m_animation.IsPlaying(name);
        }

        if (m_animator != null)
        {
            for (int i = 0; i < m_animator.layerCount; i++)
            {
                AnimatorStateInfo state = m_animator.GetCurrentAnimatorStateInfo(i);
                int hashValue = Animator.StringToHash(m_animator.GetLayerName(i) + "." + name);
                if (state.fullPathHash == hashValue) 
					return true;
            }
        }

        return false;
    }

    public bool IsPlayingAny(string name)
    {
        if (m_animation == null)
            return false;

        int count = GetAnimationCountByName(name);
        if (count <= 0)
            return false;

        for (int i = 0; i < count; i++)
        {
            if (m_animation.IsPlaying(name + i))
                return true;
        }

        return false;
    }

    public int GetAnimationCountByName(string name)
    {
        if (m_animation == null)
            return 0;
        int count = 0;
        foreach (AnimationState state in m_animation)
        {
            if (IsExist(state.name, name) && name != state.name)
            {
                count++;
            }
        }

        return count;
    }

    protected virtual void UpdateAnimation()
    {
        if (m_animation == null)
            return;

        if (IsPickDefaltAnimation() /*&& !m_isDead*/)
        {
            PickDefaltAnimation();
        }
    }

    protected bool IsPickDefaltAnimation()
    {
        if (!m_updateDefaultAnimation || m_animation == null || m_isSleep)
            return false;

        if (m_currentAnimName == "") 
            return true;

        if (IsPlaying(m_currentAnimName))
            return false;

        m_currentAnimName = "";
        return true;
    }

    protected AnimationState GetCurrentAnimationState()
    {
        if (m_animation == null || !m_animation.isPlaying) return null;

        foreach (AnimationState state in m_animation)
        {
            if (IsPlaying(state.name))
                return state;
        }

        return null;
    }

    public List<AnimationState> GetSimilarSequence(string name)
    {
        if (m_animation == null)
            return null;

        List<AnimationState> _sequence = new List<AnimationState>();
        foreach (AnimationState state in m_animation)
        {
            if (IsExist(state.name, name) && name != state.name)
            {
                _sequence.Add(state);
            }
        }

        return _sequence;
    }

    private bool IsExist(string whole, string subString)
    {
        if (!whole.StartsWith(subString)) return false;

        string str = whole.Substring(subString.Length);

        int result;
        return AiMath.IsNumberic(str, out result);
    }
}
