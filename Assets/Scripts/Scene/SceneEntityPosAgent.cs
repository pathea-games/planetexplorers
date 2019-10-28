using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;

public class SceneEntityPosAgent : ISceneObjAgent
{
	public class AgentInfo
	{
		public static AgentInfo s_defAgentInfo = new AgentInfo ();
		public virtual void OnSuceededToCreate(SceneEntityPosAgent agent)
		{
			// Default behavior: 
			LodCmpt entityLodCmpt = agent.entity.lodCmpt;
			if(entityLodCmpt != null){
				entityLodCmpt.onDestruct += (e)=>{agent.DestroyEntity();};
			}
		}
		public virtual void OnFailedToCreate(SceneEntityPosAgent agent)
		{
			SceneMan.RemoveSceneObj (agent);
		}
	}

	const int MaxTimes = 100;
	const float OneWait = 0.05f;
	public const float PosYAssignedMin = PETools.PEMath.Epsilon;
	public const float PosYTBD = 0.0f;
	public enum EStep
	{
		NotCreated,
		InCreating,
		Created,
		Destroyed,
	}

	EStep _step = EStep.NotCreated;

	Vector3 _pos;
	Vector3 _scl;
	Quaternion _rot;
	EntityType _type;
    bool _canRide = true;

	public bool FixPos{ get; set; }
	public int protoId{ get; set; }
	public PeEntity entity{ get; set; }
	public AgentInfo spInfo{ get; set;}
	public SceneEntityPosAgent(Vector3 pos, EntityType type, int protoid = -1)
	{
		_pos = pos;
		_scl = Vector3.one;
		_rot = Quaternion.identity;
		_type = type;
		protoId = protoid;
		spInfo = AgentInfo.s_defAgentInfo;
	}
	public SceneEntityPosAgent(Vector3 pos, Vector3 scl, Quaternion rot, EntityType type, int protoid = -1)
	{
		_pos = pos;
		_scl = scl;
		_rot = rot;
		_type = type;
		protoId = protoid;
		spInfo = AgentInfo.s_defAgentInfo;
	}

	// Interface
	public void OnConstruct()
	{
		if (!NeedToActivate) {
			if (entity == null && _step != EStep.InCreating) {
				CreateEntityStatic();
			}
		}
	}
	public void OnActivate()
	{
		if (NeedToActivate) {
			if (entity == null && _step != EStep.InCreating) {
				SceneMan.self.StartCoroutine (TryCreateEntityUnstatic ());
			}
		}
	}

	public void OnDeactivate(){}
	public void OnDestruct(){}
	public int Id { 			get; set; }
    public int ScenarioId { get; set; }
    public GameObject Go{ 		get { return entity == null ? null : entity.gameObject; } }
	public Vector3 Pos  {		get { return _pos; } set { _pos = value; } }

    public bool   canRide { get { return _canRide; } set { _canRide = value; } }
	public IBoundInScene Bound{	get	{ return null; } }
	public bool NeedToActivate{	get { return _type != EntityType.EntityType_Doodad;	} }	// only doodad pos agent not need to activate
	public bool TstYOnActivate{	get { return _type != EntityType.EntityType_Doodad && _pos.y >= PosYAssignedMin; } }
	// Extension
	public bool IsIdle	{ 		get { return _step != EStep.InCreating; } }
	public EStep Step	{ 		get { return _step; } }
    public Vector3 Scl	{ 		get { return _scl; } }
	public Quaternion Rot{ 		get { return _rot; } }

	// Other methods/property
	void CreateEntityStatic()
	{
		_step = EStep.InCreating;

		DoodadEntityCreator.CreateDoodad (this);
		if(entity == null){
			Debug.LogWarning("[EntityPosAgent]:Failed to create entity "+_type.ToString()+protoId);
		}
		// now doodad's lod(SceneDoodadLodCmpt) would handle doodads, posAgent not needed longer after dooad created.
		SceneMan.RemoveSceneObj (this);
		_step = EStep.Created;
	}
	bool CheckSetAvailablePos()
	{
		if (!NeedToActivate || FixPos)	//fixed spawn
			return true;

		RaycastHit[] hitInfos = SceneMan.DependenceHitTst(this);
		if (hitInfos != null && hitInfos.Length > 0) {
			if (hitInfos.Length > 1) 
			{
				PeEntity mainPlayer = PeCreature.Instance.mainPlayer;
				if(null != mainPlayer)
				{
					float[] dHeight = new float[hitInfos.Length];
					float totalDHeight = 0;
					int hitIndex = -1;
					for(int i = 0; i < hitInfos.Length; ++i)
					{
						dHeight[i] = Mathf.Abs(hitInfos [i].point.y - mainPlayer.position.y);
						if(dHeight[i] > PETools.PEMath.Epsilon)
						{
							dHeight[i] = 1f/dHeight[i];
							totalDHeight += dHeight[i];
						}
						else
						{
							hitIndex = i;
							break;
						}
					}

					if(hitIndex < 0)
					{
						float randomValue = UnityEngine.Random.Range(0, totalDHeight);
						float countDHeight = 0;
						for(int i = 0; i < dHeight.Length; ++i)
						{
							if(randomValue < dHeight[i] + countDHeight)
							{
								hitIndex = i;
								break;
							}
							countDHeight += dHeight[i];
						}
					}
					_pos.y = hitInfos [hitIndex].point.y + 1;
					dHeight = null;
				}
				else
					_pos.y = Mathf.Max (hitInfos [0].point.y, hitInfos [1].point.y) + 1;
			}
			else
			{
				_pos.y = hitInfos [0].point.y + 1;
			}
			return true;
		}
		return false;
	}
	IEnumerator TryCreateEntityUnstatic()
	{
		_step = EStep.InCreating;

		int n = 0;
		while(n++ < MaxTimes)
		{
			if(CheckSetAvailablePos())
			{
				switch(_type)
				{
				case EntityType.EntityType_Npc: 		NpcEntityCreator.CreateNpc(this); break;
				case EntityType.EntityType_Monster: 	MonsterEntityCreator.CreateMonster(this); break;
				}
				
				if(entity != null){
					spInfo.OnSuceededToCreate (this);
				}else{
					//if(protoId >= 0){	// Airborne monster would go here to remove its agent
					//	Debug.LogWarning("[EntityPosAgent]:Failed to create entity "+_type.ToString()+protoId);
					//}
					spInfo.OnFailedToCreate (this);
				}
				break;
			}				
			yield return new WaitForSeconds(OneWait);
		}
		if(n >= MaxTimes)
		{
			Debug.LogWarning("[EntityPosAgent]:Failed to create entity "+_type.ToString()+protoId+" during " + n*OneWait);
		}

		_step = EStep.Created;
	}
	public void DestroyEntity()
	{
		if (entity != null) {
			PeCreature.Instance.Destory (entity.Id);
			entity = null;
		}
	}
}
