using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
    public class AnimatorCmpt : PeCmpt, IPeMsg
    {
        Animator m_Animator;

        Dictionary<int, bool> m_Bools;
        Dictionary<string, float> m_Floats;
        Dictionary<string, int> m_Integers;
        Dictionary<int, float> m_Layers;

		float m_Speed = 1f;
		
		public event Action<string> AnimEvtString;

		public Quaternion	m_LastRot;
		public Vector3		m_LastMove;

        HashSet<int> m_Parameters;

        public Animator animator
        {
            get { return m_Animator; }
            set
            {
                if (m_Animator != value)
                {
                    m_Animator = value;

                    if (m_Animator != null)
                    {
                        InitParameters();
                        InitAnimator();
                    }
                }
            }
        }

		public float speed
		{
			get
			{
				return (null != animator)?animator.speed:m_Speed;
			}
			set
			{
				m_Speed = value;
				if(null != animator)
					animator.speed = m_Speed;
			}
		}

		public bool hasAnimator{ get { return null != m_Animator; } }

        public bool ContainsParameter(string name)
        {
			return m_Parameters != null ? m_Parameters.Contains (Animator.StringToHash(name)) : false;
        }

        public bool ContainsParameter(int paraHash)
        {
            return m_Parameters != null ? m_Parameters.Contains(paraHash) : false;
        }

        #region Set
        public void SetBool(string name, bool value)
        {
            if (string.IsNullOrEmpty(name))
                return;

            SetBool(Animator.StringToHash(name), value);
        }

        public void SetBool(int name, bool value)
        {
            m_Bools[name] = value;

            if (m_Animator != null && ContainsParameter(name))
                m_Animator.SetBool(name, value);
        }

        public void SetFloat(string name, float value)
        {
            if (string.IsNullOrEmpty(name))
                return;

			m_Floats[name] = value;

            if (m_Animator != null && ContainsParameter(name))
                m_Animator.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
        {
            if (string.IsNullOrEmpty(name))
                return;

            m_Integers[name] = value;

            if (m_Animator != null && ContainsParameter(name))
                m_Animator.SetInteger(name, value);
        }

        public void SetTrigger(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (m_Animator != null && ContainsParameter(name))
                m_Animator.SetTrigger(name);
        }

		public void ResetTrigger(string name)
		{
			if (string.IsNullOrEmpty(name))
				return;

            if (m_Animator != null && ContainsParameter(name))
			    m_Animator.ResetTrigger(name);
		}

        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (m_Animator == null || layerIndex < 0 || layerIndex >= m_Animator.layerCount)
                return;

			float w = -1.0f;
			if (!m_Layers.TryGetValue(layerIndex, out w) || Mathf.Approximately(w, weight))
            {
				m_Layers[layerIndex] = weight;
                if (m_Animator != null)
                    m_Animator.SetLayerWeight(layerIndex, weight);
            }
        }

        public void SetLayerWeight(string layerName, float weight)
        {
            if(null == m_Animator)
                return;

            SetLayerWeight(GetLayerIndex(layerName), weight);
        }
        #endregion

        #region Get
        public bool GetBool(string name)
        {
            bool ret = false;
            //lw:一些动作是自动结束，m_Bools.TryGetValue获取值却为true
            //if (!m_Bools.TryGetValue(Animator.StringToHash(name), out ret))
            //{

            //}
            if (m_Animator != null && ContainsParameter(name))
                ret = m_Animator.GetBool(name);

            return ret;
        }

        public float GetFloat(string name)
        {
			float ret = 0.0f;
            if (!m_Floats.TryGetValue(name, out ret))
            {
                if (m_Animator != null && ContainsParameter(name)){
					ret = m_Animator.GetFloat(name);
				}
            }
			return ret;
        }

        public int GetInteger(string name)
        {
			int ret = 0;
            if (!m_Integers.TryGetValue(name, out ret))
            {
                if (m_Animator != null && ContainsParameter(name))
					ret = m_Animator.GetInteger(name);
            }
			return ret;
        }

        public float GetLayerWeight(int layerIndex)
        {
			float ret = 0.0f;
            if (!m_Layers.TryGetValue(layerIndex, out ret))
            {
                if (m_Animator != null)
					ret = m_Animator.GetLayerWeight(layerIndex);
            }
			return ret;
        }

        public AnimatorStateInfo GetAnimatorStateInfo(int layerIndex)
        {
            if (m_Animator != null)
                return m_Animator.GetCurrentAnimatorStateInfo(layerIndex);
            else
                return new AnimatorStateInfo();
        }

		public bool IsAnimPlaying (string animName, int layer = -1)
		{
			if(null != m_Animator)
			{
				if(-1 == layer)
				{
					for(int i = 0; i < m_Animator.layerCount; i++)
					{
						string layerName = m_Animator.GetLayerName(i);
						if(m_Animator.GetCurrentAnimatorStateInfo(i).IsName(layerName + "." + animName)
						   || (m_Animator.IsInTransition(i) && m_Animator.GetNextAnimatorStateInfo(i).IsName(layerName + "." + animName)))
							return true;
					}
				}
				else if(layer >= 0 && layer < m_Animator.layerCount)
				{
					string layerName = m_Animator.GetLayerName(layer);
					return m_Animator.GetCurrentAnimatorStateInfo(layer).IsName(layerName + "." + animName)
						|| (m_Animator.IsInTransition(layer) && m_Animator.GetNextAnimatorStateInfo(layer).IsName(layerName + "." + animName));
				}
			}
			return false;
		}

		public bool IsInTransition(int layer)
		{
			if(null != m_Animator)
			{
				if(layer < m_Animator.layerCount)
					return m_Animator.IsInTransition(layer);
			}
			return false;
		}

		public int GetLayerCount()
		{
			if(null != m_Animator)
				return m_Animator.layerCount;
			return 0;
		}

        public int GetLayerIndex(string layerName)
        {
            if (null != m_Animator)
                return m_Animator.GetLayerIndex(layerName);

            return -1;
        }

        public string GetLayerName(int layerIndex)
        {
            if (null != m_Animator)
                return m_Animator.GetLayerName(layerIndex);

            return "";
        }
        #endregion

        #region private function
        void InitParameters()
        {
            AnimatorControllerParameter[] parameters = m_Animator.parameters;

            int length = parameters.Length;
			m_Parameters = new HashSet<int>();

			for(int i = 0; i < length; i++)
				m_Parameters.Add(Animator.StringToHash(parameters[i].name));
        }

        void InitAnimator()
        {
            for (int i = 0; i < m_Animator.layerCount; i++)
            {
                if (!m_Layers.ContainsKey(i))
                    m_Layers.Add(i, m_Animator.GetLayerWeight(i));
                else
                    m_Animator.SetLayerWeight(i, m_Layers[i]);
            }

            foreach (KeyValuePair<int, bool> kvp in m_Bools)
            {
                if(ContainsParameter(kvp.Key))
                    m_Animator.SetBool(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<string, float> kvp in m_Floats)
            {
                if (ContainsParameter(kvp.Key))
                    m_Animator.SetFloat(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<string, int> kvp in m_Integers)
            {
                if (ContainsParameter(kvp.Key))
                    m_Animator.SetInteger(kvp.Key, kvp.Value);
            }
        }
        #endregion

		#region Anim Event
		public void AnimEvent(string para)
		{
			if(null != AnimEvtString)
				AnimEvtString(para);
		}
        #endregion

        public override void Awake()
        {
            base.Awake();

            m_Bools = new Dictionary<int, bool>();
            m_Floats = new Dictionary<string, float>();
            m_Integers = new Dictionary<string, int>();
            m_Layers = new Dictionary<int, float>();
        }

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.View_Model_Build:
					if(isActiveAndEnabled){
						StartCoroutine(FindAnimator(args[0] as GameObject, 0.5f));
					}
                    break;
                default:
                    break;
            }
        }

		static Queue<Transform> toCheck = new Queue<Transform> (32);
		IEnumerator FindAnimator(GameObject obj, float delayTime)
		{
			if (delayTime > 0) {
				yield return new WaitForSeconds (delayTime);
			}
			if(null == obj)
				yield break;

			toCheck.Clear ();
			toCheck.Enqueue (obj.transform);
			while (toCheck.Count > 0) {
				Transform tran = toCheck.Dequeue();
				animator = tran.GetComponent<Animator> ();
				if (animator != null) {
					break;
				}
				foreach (Transform t in tran) {
					toCheck.Enqueue(t);
				}
			}
			speed = m_Speed;
		}
	}
}

