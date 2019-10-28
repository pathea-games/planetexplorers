using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using AiAsset;
using SoundAsset;
using System.Reflection;

public partial class AiObject : SkillRunner
{
	public delegate void DelegateAiObject(AiObject aiObject);
	
    public GameObject aiObject;
    public Transform model;
    public Transform ragdoll;
    [System.NonSerialized]
    public bool isMission = false;

    protected int m_camp;
    protected int m_harm;
    protected int m_life;
    //protected int m_comfort;
    //protected int m_hungry;
    protected int m_soundId = 0;

    protected bool m_isDead;
	protected bool m_isSleep;
	protected bool m_isActive;
    protected bool m_isConceal;

    protected float m_startShowLifeBarTime;

    protected Transform mCenter;
    protected Bounds mHideBounds;

    protected Transform m_tdInfo;
    protected Animation m_animation;
    protected Animator m_animator;
    protected Rigidbody m_rigidbody;
    protected CharacterController m_controller;
    //protected AnimatorStateHolder m_AnimatorStateHolder;

    protected EffSkillBuffSum m_buffSum;
    protected EffSkillBuffMultiply m_buffSumMul;
    //protected AiTarget m_aiTarget;
    protected AiBehave m_behave;

    public event DelegateAiObject SpawnedHandlerEvent;
	public event DelegateAiObject DeathHandlerEvent;
	public event DelegateAiObject DestroyHandlerEvent;

	public event DelegateAiObject ActiveHandlerEvent;
	public event DelegateAiObject InactiveHandlerEvent;

    //public AiTarget aiTarget { get { return m_aiTarget; } }
    public Animator animator { get { return m_animator; } }
    public Rigidbody rigid { get { return m_rigidbody; } }
    public AiBehave behave { get { return m_behave; } }
    public CharacterController controller { get { return m_controller; } }

	public virtual bool isActive{get{return m_isActive;}}

    public Bounds hideBounds { get { return mHideBounds; } }

    public bool conceal { get { return m_isConceal; } set { m_isConceal = value; } }
	
    public virtual Transform tdInfo 
    { 
        get 
        { 
            return m_tdInfo; 
        } 
        set 
        { 
            m_tdInfo = value;

            if (value != null)
            {
                SetCamp(27);
            }
        } 
    }

    protected virtual void OnShowLifeBar()
    {
        
    }

    protected virtual void PlayAnimationAudio(string name)
    {
          
    }

    protected virtual void Awake()
    {
        m_motor = GetComponent<AiCharacterMotor>();
        m_controller = GetComponent<CharacterController>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_animation = GetComponentInChildren<Animation>();
        m_animator = GetComponentInChildren<Animator>();
        //m_AnimatorStateHolder = new AnimatorStateHolder(m_animator);
        //m_seeker = GetComponentInChildren<Seeker>();
        //m_aiTarget = GetComponentInChildren<AiTarget>();
		
        InitializeData();
        InitializeControllerData();
        InitAttackData();
        InitCenter();

		ActivateRagdoll(false);


    }

    void InitCenter()
    {
        Rigidbody[] rigids = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigid in rigids)
        {
            if (rigid == null)
                continue;

            if (rigid.gameObject.layer != Pathea.Layer.Ragdoll)
                continue;

            if (rigid.GetComponent<CharacterJoint>() != null)
                continue;

            mCenter = rigid.transform;
            break;
        }
    }

    public void RemoveAiBehave()
    {
        if (m_behave != null)
        {
            Destroy(m_behave.gameObject);
            m_behave = null;
        }
    }

    public void SetupAibehave(GameObject aiPrefab)
    {
        if (aiPrefab == null)
            return;

        if (m_behave != null)
        {
            Destroy(m_behave.gameObject);
            m_behave = null;
        }

        GameObject go = Instantiate(aiPrefab, transform.position, transform.rotation) as GameObject;

        if (go == null)
        {
            Debug.LogError("Instantiate ai prefab failed.");
            return;
        }

        go.transform.parent = transform;

        m_behave = go.GetComponent<AiBehave>();

        if (m_behave == null)
        {
            Debug.LogError("cant find AiBehaveTree.");
            return;
        }

        AiBehaveSingle bs = m_behave as AiBehaveSingle;
        if (bs != null)
        {
            bs.RegisterAiObject(this);
        }
        else
        {
            Debug.LogWarning("ai [" + m_behave + "] is not a AiBehaveSingle");
        }
    }

    protected virtual void Start()
    {
        //AiManager.Manager.RegisterAiObject(this);

        m_spawnPosition = transform.position;
        m_offset = transform.localPosition;

        SetupAibehave(aiObject);

        HandleTheSpawnedEvent();
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {
        m_effSkillInsts.Clear();
        m_effShareSkillInsts.Clear();
        m_buffSum.Clear();
    }

    protected virtual void OnDestroy()
    {
        //if ( AiManager.Manager == null )
        //    return;

        //AiManager.Manager.RemoveAiObject(this);

//        if (null != GameGui_N.Instance)
//            GameGui_N.Instance.mLifeShowGui.RemoveEnemy(this);

		DeathHandlerEvent = null;
        HandleTheDestroyEvent();
    }

	void RemoveItem()
	{
		//DragItemMgr.Instance.RemoveWithObject(gameObject);
	}

    public virtual void DestroyLOD(Vector3 center, Vector3 size, float delayTime = 0.0f)
    {
        Delete(delayTime);
    }

    public virtual void Delete(float delayTime = 0.0f)
    {
        //if (motor != null)
        //    motor.enabled = false;

		RemoveItem();

        AiAlpha alpha = GetComponentInChildren<AiAlpha>();
        if (alpha != null && gameObject.activeSelf)
        {
            alpha.ChangeAlphaToValue(0.0f, delayTime, 2.0f);
            GameObject.Destroy(gameObject, delayTime + 2.0f);
        }
        else
        {
            GameObject.Destroy(gameObject, delayTime);
        }
    }

    protected virtual void Update()
    {
        AiUpdate();
        SumupBuff();
    }

    void SumupBuff()
    {
        if (m_effSkillBuffManager.m_bEffBuffDirty)
        {
            m_buffSum = EffSkillBuffSum.Sumup(m_effSkillBuffManager.m_effBuffInstList);
            m_buffSumMul.ResetBuffMultiply(m_effSkillBuffManager.m_effBuffInstList);
            m_effSkillBuffManager.m_bEffBuffDirty = false;
        }
    }

    public void SetCamp(int iCamp)
    {
        m_camp = iCamp;
    }

    public void HandleTheSpawnedEvent()
    {
        if (SpawnedHandlerEvent != null)
        {
            SpawnedHandlerEvent(this);
        }
    }

    public void HandleTheDeathEvent()
    {
        if (DeathHandlerEvent != null)
        {
            DeathHandlerEvent(this);
        }
    }

    public void HandleTheDestroyEvent()
    {
        if (DestroyHandlerEvent != null)
        {
            DestroyHandlerEvent(this);
        }
    }

	public void HandleActiveEvent()
	{
		if(ActiveHandlerEvent != null)
		{
			ActiveHandlerEvent(this);
		}
	}

	public void HandleInactiveEvent()
	{
		if(InactiveHandlerEvent != null)
		{
			InactiveHandlerEvent(this);
		}
	}

	public virtual void SetOwennerView()
	{
		if(IsController && m_isActive)
		{
			if (motor != null && !m_isDead)
				motor.enabled = true;
			
			if (null != behave && !m_isDead)
				behave.enabled = true;

            if (GetComponent<Rigidbody>() != null)
                GetComponent<Rigidbody>().WakeUp();

            Animator ani = GetComponent<Animator>();
            if (ani != null && !m_isDead)
                ani.applyRootMotion = true;
		}
		else
		{
			if (motor != null)
				motor.enabled = false;

            if (null != behave)
                behave.enabled = false;

            if (GetComponent<Rigidbody>() != null)
                GetComponent<Rigidbody>().Sleep();

            Animator ani = GetComponent<Animator>();
            if (ani != null)
                ani.applyRootMotion = false;
		}
	}
	
	protected virtual void ActivateRagdoll(bool active)
    {
        if (model == null) return;

        Collider _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            if(active || m_isDead)
                _collider.enabled = false;
            else
                _collider.enabled = true;
        }

        Animator _animator = GetComponent<Animator>();
        if (_animator != null)
        {
            if (active || m_isDead)
                _animator.enabled = false;
            else
                _animator.enabled = true;
        }

        Animation _animation = model.GetComponent<Animation>();
        if (_animation != null)
        {
            if (active || m_isDead)
                _animation.enabled = false;
            else
                _animation.enabled = true;
        }

        ArmAimer aimer = GetComponent<ArmAimer>();
        if (aimer != null)
        {
            if (active || m_isDead)
                aimer.enabled = false;
            else
                aimer.enabled = true;
        }

		LegAnimator legAni = GetComponentInChildren<LegAnimator>();
		if(legAni != null && active)
		{
			legAni.enabled = false;
		}

        Rigidbody[] rigids = model.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigid in rigids)
        {
            if (active)
                rigid.WakeUp();
            else
                rigid.Sleep();

            rigid.isKinematic = !active;

            //if (active)
            //{
            //    rigid.AddForce(transform.forward * 7500, ForceMode.Force);
            //}
        }

//        Collider[] colliders = model.GetComponentsInChildren<Collider>();
//        foreach (Collider c in colliders)
//        {
//            //c.enabled = active;
//        }
    }

    void ActivateAlpha(bool value)
    {
        if (value)
        {
            SetActive(true);
            AiAlpha alpha = GetComponentInChildren<AiAlpha>();
            if (alpha != null)
            {
                alpha.ChangeAlphaToValue(1.0f, 0.0f, 2.0f);
            }

            if (IsInvoking("DeActive"))
                CancelInvoke("DeActive");
        }
        else
        {
            AiAlpha alpha = GetComponentInChildren<AiAlpha>();
            if (alpha != null)
            {
                alpha.ChangeAlphaToValue(0.0f, 0.0f, 2.0f);

                Invoke("DeActive", 2.0f);
            }
        }
    }

    protected virtual void ActivateMultiOwener(bool value)
	{
		m_isActive = value;

		SetOwennerView();

        //if (!m_isActive && aiTarget != null)
        //    aiTarget.ClearHatred();

        ActivateAlpha(value);
	}

    protected virtual void ActivateMultiProxy(bool value)
	{
		m_isActive = value;

		SetOwennerView();

        ActivateAlpha(value);
	}

	protected virtual void ActivateSingleMode(bool value)
	{
		m_isActive = value;
			
		if (motor != null)
        {
            if (m_isActive && !m_isDead)
                motor.enabled = true;
            else
                motor.enabled = false;
        }
			
		if (behave != null)
        {
            if (m_isActive && !m_isDead)
                behave.enabled = true;
            else
                behave.enabled = false;
        }
			
		Animator ani = GetComponent<Animator>();
        if (ani != null)
        {
            if (m_isActive && !m_isDead)
                ani.applyRootMotion = true;
            else
                ani.applyRootMotion = false;
        }

        //if (!m_isActive && aiTarget != null)
        //    aiTarget.ClearHatred();

        ActivateAlpha(value);
	}

    protected void NetwrokSwitchController(bool value)
    {
        if (IsController)
            ActivateMultiOwener(value);
        else
            ActivateMultiProxy(value);
    }

    public virtual void Activate(bool value)
    {
        if (m_isActive == value)
            return;

        if (GameConfig.IsMultiMode)
            NetwrokSwitchController(value);
        else
            ActivateSingleMode(value);

        if(m_isDead)
        {
            if (m_isActive)
                ActivateRagdoll(true);
            else
                ActivateRagdoll(false);
        }
    }

    public virtual void Activate(bool value, IntVector4 node)
    {

    }

	public virtual void Activate(bool value, Bounds bounds)
	{
        if (value)
            mHideBounds = new Bounds();
        else
            mHideBounds = bounds;

		if (GameConfig.IsMultiMode)
		{
			Activate(value);
		}
		else
		{
			Activate(value);
		}
	}

	void DeActive()
    {
		SetActive(false);
    }

	void SetActive(bool value)
	{
		if(gameObject.activeSelf != value)
		{
			gameObject.SetActive(value);

			if(value)
				HandleActiveEvent();
			else
				HandleInactiveEvent();
		}
	}

    public virtual Bounds arriveBound
    {
        get
        {
            return bound;
        }
    }

    public virtual Bounds attackBound
    {
        get
        {
            return bound;
        }
    }

    public virtual bool isboss { get { return false; } }

    public virtual int camp { get { return m_camp; } set { SetCamp(value); } }

    public virtual int harm { get { return m_harm; } set { m_harm = value; } }

    public virtual string xmlPath { get { return ""; } }

    public virtual bool dead { get { return m_isDead; } set { m_isDead = value; } }

    public virtual int life { get { return m_life; } set { m_life = value; } }

    public virtual int maxLife { get { return 0; } }

    public virtual float lifePercent 
    { 
        get { return (float)life / (float)maxLife; }
        set 
        { 
            life = (int)Mathf.Clamp(maxLife * value, 0.0f, maxLife);

            if (life <= 0 && !m_isDead)
            {
                m_isDead = true;

                OnDeath();
            }
        }
    }

    //public virtual int comfort { get { return 0; } set { m_comfort = value; } }

    //public virtual int maxComfort { get { return 0; } }

    //public virtual float comfortPercent { get { return 1.0f; } }

    //public virtual int hungry { get { return 0; } set { m_hungry = value; } }

    //public virtual int maxHungry { get { return 0; } }

    //public virtual float hungryPercent { get { return 1.0f; } }

    public virtual float attackRadius { get { return 0.0f; } }

    public virtual int damage { get { return 0; } }

    public virtual int buildDamage { get { return 0; } }

    public virtual int defence { get { return 0; } }

    public virtual float walkSpeed { get { return 1.0f; } }

    public virtual float runSpeed { get { return 1.0f; } }

    public virtual int defenceType { get { return 0; } }
	
	public bool sleeping{get{return m_isSleep;} set{m_isSleep = value;}}

    public bool OnBuff(Buff_Sp _sp)
    {
        return (m_buffSum.m_buffSp & (short)(1 << ((short)_sp - 1))) > 0;
    }

    public void CancelBuffSp(Buff_Sp _sp)
    {
		List<EffSkillBuffInst> insts = m_effSkillBuffManager.m_effBuffInstList.FindAll(ret => ret.m_buff.m_buffSp == (short)_sp);
		foreach (EffSkillBuffInst inst in insts) 
		{
			m_effSkillBuffManager.m_effBuffInstList.Remove(inst);
		}
    }

    //public ActionSucceedDelegate GetSucceedDelegate(string funcName)
    //{
    //    if (funcName == null)
    //    {
    //        Debug.LogError("The tree xml have null string");
    //        return null;
    //    }

    //    MethodInfo method = this.GetType().GetMethod(funcName, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(object) }, null);

    //    if (method != null && method.ReturnType == typeof(void))
    //    {
    //        return Delegate.CreateDelegate(typeof(ActionSucceedDelegate), this, method) as ActionSucceedDelegate;
    //    }

    //    Debug.LogWarning("Don't have this function --> " + funcName);
    //    return null;
    //}

    //public CanRunDecoratorDelegate GetCanRunDecoratorDelegate(string funcName)
    //{
    //    if (funcName == null)
    //    {
    //        Debug.LogError("The tree xml have null string");
    //        return null;
    //    }

    //    MethodInfo method = this.GetType().GetMethod(funcName, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(object) }, null);

    //    if (method != null && method.ReturnType == typeof(bool))
    //    {
    //        return Delegate.CreateDelegate(typeof(CanRunDecoratorDelegate), this, method) as CanRunDecoratorDelegate;
    //    }

    //    Debug.LogWarning("Don't have this function --> " + funcName);
    //    return null;
    //}

    protected virtual void AiUpdate()
    {
        //����ģʽֻ�з��������߼�
//        if (!GameConfig.IsMultiMode || IsController)
//        {
//            GetMaxThreatTarget();
//            UpdateMove();
//        }

        if(m_animation != null) 
            UpdateAnimation();

        if(m_animator != null) 
            UpdateAnimator();

        OnShowLifeBar();
    }

    protected virtual void InitializeData()
    {
        m_camp = 0;
        m_life = 1;
        m_startShowLifeBarTime = 0.0f;
        m_isDead = false;
		m_isSleep = false;
		m_isActive = true;

        m_buffSum = new EffSkillBuffSum();
        m_buffSumMul = new EffSkillBuffMultiply();
    }

    protected virtual bool ClearTarget()
    {
        return true;
    }
}

