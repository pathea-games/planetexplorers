using UnityEngine;
using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;

public class MousePickableLootItem : MonoBehaviour
{
	public LootItemData data{ get { return m_Data; } }
	
	public Renderer	m_sphere;

	public Renderer m_Line;

	public UISprite m_Icon;

	public Transform m_IconRoot;

	public Transform m_LineRoot;

	static readonly float FetchSqrDis = 0.5f * 0.5f;
	static readonly float IconEnableSqrDis = 10f * 10f;

	public float stayMinTime = 1f;
	public float fadeSpeed = 1f;

	public int effectID;

	public int lootSoundID;

	public int getItemSoundID;

	AudioController m_LootAudio;

	public enum MoveState
	{
		Drop,
		Stay,
		Loot
	}
	
	LootItemData m_Data;
	
	MoveState m_MoveState;

	Transform m_Target;

	Action<int> m_EndFunc;

	//float 	m_StartMoveTime;
	//float	m_MoveTime;
	Vector3 m_MoveDir;
	float	m_Speed;
	Vector3 m_Velocity;
	//bool	m_DropToLoot;
	public float m_MaxSpeed = 20f;
	public float m_Acceleration = 10f;
	public float m_RotateSpeed = 5f;	
	public float selfRotateSpeed = 360f;

	float m_StartStayTime;
	Color m_DefaultCol;

	void Awake()
	{
		m_sphere.material = Material.Instantiate (m_sphere.material);
		m_DefaultCol = m_sphere.material.GetColor ("_Color");
	}
	
	public void SetData(LootItemData data)
	{
        m_Data = data;
        if (m_Data == null || m_Data.itemObj == null || m_Data.itemObj.protoData.icon == null)
            return;
        m_Icon.spriteName = m_Data.itemObj.protoData.icon[0];
		transform.position = m_Data.position;
		transform.rotation = Quaternion.AngleAxis (UnityEngine.Random.Range(0f, 360f), Vector3.up);
		m_Icon.alpha = 0;

		m_LineRoot.localScale = new Vector3 (m_LineRoot.localScale.x, UnityEngine.Random.Range(0.4f, 0.65f), m_LineRoot.localScale.z);
		UpdateColor ();
	}

	public void SetMoveState(MoveState state, Action<int> moveEndFunc = null, Transform lootTrans = null)
	{
		m_EndFunc = moveEndFunc;
		m_Target = lootTrans;

		switch(state)
		{
		case MoveState.Drop:
			//m_DropToLoot = false;
			InitMoveDrop();
			break;
		case MoveState.Loot:
			//m_DropToLoot = true;
			break;
		case MoveState.Stay:			
			//m_DropToLoot = false;
			InitStay();
			break;
		}
	}

	void Update()
	{
		UpdateRotate ();
		UpdateMoveState ();
	}

	void UpdateRotate()
	{
		m_IconRoot.rotation = Quaternion.AngleAxis (selfRotateSpeed * Time.deltaTime, Vector3.up) * m_IconRoot.rotation;
		m_sphere.transform.rotation = PETools.PEUtil.MainCamTransform.rotation;
		m_LineRoot.eulerAngles = new Vector3(0, PETools.PEUtil.MainCamTransform.eulerAngles.y, 0);
	}

	void UpdateMoveState()
	{
		switch(m_MoveState)
		{
		case MoveState.Drop:
			UpdateDrop();
			break;
		case MoveState.Loot:
			UpdateLoot();
			break;
		case MoveState.Stay:
			UpdateStay();
			break;
		}
	}

	void InitStay()
	{
		m_MoveState = MoveState.Stay;
		m_Line.enabled = true;
		m_Icon.enabled = true;
		m_Icon.alpha = 0;
		m_StartStayTime = Time.time;
	}

	void InitMoveDrop()
	{
		m_MoveState = MoveState.Drop;
		m_Line.enabled = false;
		m_Icon.enabled = false;
		//m_StartMoveTime = Time.time;
		m_Speed = m_MaxSpeed * UnityEngine.Random.Range(0.2f, 0.5f);
		m_MoveDir = Vector3.Slerp(UnityEngine.Random.onUnitSphere, Vector3.up, 0.7f);
		m_Velocity = m_MoveDir * m_Speed;
		//m_MoveTime = Mathf.Abs (2f * m_MoveDir.y * m_Speed / Physics.gravity.y) * 1.2f;
		transform.position += Vector3.up;
	}

	void InitMoveLoot()
	{
		if (null == m_Target)
			return;
		m_Line.enabled = false;
		m_Icon.enabled = false;
		m_MoveState = MoveState.Loot;
		Vector3 toTarget = m_Target.position - transform.position;
		m_MoveDir = Vector3.Slerp(UnityEngine.Random.onUnitSphere, -toTarget.normalized, 0.5f);
		if (m_MoveDir.y < 0)
			m_MoveDir.y = -m_MoveDir.y;
		m_Speed = m_MaxSpeed * UnityEngine.Random.Range(0.2f, 0.5f);
		if(null == m_LootAudio)
			m_LootAudio = AudioManager.instance.Create(transform.position, lootSoundID, transform, false, false);
		if(null != m_LootAudio)
			m_LootAudio.PlayAudio(0.3f);
	}

	void UpdateDrop()
	{
		if(!Physics.Raycast(transform.position + Vector3.up, Vector3.down, 50f, GameConfig.SceneLayer))
		{
			InitStay();
			return;
		}
		m_Velocity += Physics.gravity * Time.deltaTime;
		float moveDis = m_Velocity.magnitude * Time.deltaTime;
		if(Physics.Raycast(transform.position, m_Velocity.normalized, moveDis, GameConfig.SceneLayer))
		{
			InitStay();
			return;
		}
		transform.position += m_Velocity * Time.deltaTime;
		data.position = transform.position;
	}

	void UpdateLoot()
	{
		if (null == m_Target)
		{
			OnEndLoot();
			return;
		}

		m_Speed = Mathf.Clamp (m_Speed + m_Acceleration * Time.deltaTime, 0, m_MaxSpeed);
		Vector3 toTarget = m_Target.position - transform.position;
		m_MoveDir = Vector3.Slerp (m_MoveDir, toTarget.normalized, m_RotateSpeed * Time.deltaTime).normalized;
		m_Velocity = m_MoveDir * m_Speed;

		Vector3 startPos = transform.position;
		Vector3 endPos = transform.position + m_Velocity * Time.deltaTime;
		transform.position = endPos;
		Vector3 closetPos = WhiteCat.Utility.ClosestPoint (startPos, endPos, m_Target.position);
		if (Vector3.SqrMagnitude (closetPos - m_Target.position) < FetchSqrDis) 
		{
			AudioManager.instance.Create(transform.position, getItemSoundID);
			transform.position = closetPos;
			OnEndLoot();
			return;
		}
	}
	
	void UpdateStay()
	{
		if (null != MainPlayer.Instance.entity) 
		{
			float dir = (Vector3.SqrMagnitude (MainPlayer.Instance.entity.position - transform.position) < IconEnableSqrDis) ? 1f : -1f;
			m_Icon.alpha = Mathf.Clamp01 (m_Icon.alpha + dir * fadeSpeed * Time.deltaTime);
		}
		if (Time.time - m_StartStayTime > stayMinTime)
			InitMoveLoot ();
	}

	void OnEndLoot()
	{
		Pathea.Effect.EffectBuilder.Instance.Register (effectID, null, transform.position, transform.rotation, m_Target);
		if(null != m_LootAudio)
			m_LootAudio.StopAudio();
		m_MoveState = MoveState.Stay;
		if(null != m_EndFunc)
			m_EndFunc(data.id);
	}

	void UpdateColor()
	{
		Color color = m_DefaultCol;
		if (!m_Data.itemObj.protoData.isFormula)
			color = m_Data.itemObj.protoData.color;
		m_sphere.material.SetColor("_Color", color);
	}
}
