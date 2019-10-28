using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem;

// On model layer
public class MonsterSummoner : MonoBehaviour
{
	public float Radius{	set{ _radius = value; } }
	public Vector3 Center{	get{ return transform.position; } }
	public int MaxCntOfMonsters{		set{ _maxCntOfMonsters = value;  }}
	public int[] ProtoIdsOfMonsters{	set{ _protoIdsOfMonsters = value;}}
	public Vector3[] PosOfMonsters{ 	set{ _posOfMonsters = value;	 }}

	//static readonly float s_rayHalfLen = 10.0f;
	[SerializeField]	int _maxCntOfMonsters;
	[SerializeField]	float _radius = 64.0f;
	[SerializeField]	float _playerRadius = 168.0f;
	[SerializeField]	Vector2 _timeIntervalMinMax = new Vector2(10f, 50f);
	[SerializeField]	int[] _protoIdsOfMonsters;
	[SerializeField]	Vector3[] _posOfMonsters;

	void Start()
	{
		if (PeGameMgr.IsAdventure) {
			SceneDoodadLodCmpt lod = GetComponentInParent<SceneDoodadLodCmpt> ();
			if (lod != null && lod.Index >= 0) {
//				int townid = lod.Index;
				List<Vector3> lstPos;
				VANativeCampXML.DynamicNative[] nativeIds = VArtifactUtil.GetAllDynamicNativePoint(lod.Index, out lstPos);
				if(nativeIds==null)
					return;
				int cntNativeIds = nativeIds.Length;
				_posOfMonsters = lstPos.ToArray();
				_protoIdsOfMonsters = new int[cntNativeIds];
				for(int i = 0; i < cntNativeIds; i++){
					_protoIdsOfMonsters[i] = nativeIds[i].type == 0 ? (nativeIds[i].did|EntityProto.IdGrpMask) : nativeIds[i].did;
				}
				_maxCntOfMonsters = 8;
			}
		}
		_radius = 256.0f; //force 128
		StartCoroutine (RefreshAgents ());
	}

	IEnumerator RefreshAgents()
	{
		while (true) {
			if(PETools.PEUtil.SqrMagnitudeH (PeCreature.Instance.mainPlayer.position, Center) <=_playerRadius*_playerRadius)
			{
				int n = GetEntitiesCnt();
				if(n < _maxCntOfMonsters && 
				   _posOfMonsters != null && _posOfMonsters.Length > 0 &&
				   _protoIdsOfMonsters != null && _protoIdsOfMonsters.Length > 0){
					Vector3 pos = GetSpawnPos();
					int protoId = GetProtoId();
					if (PeGameMgr.IsAdventure){
						SceneDoodadLodCmpt lod = GetComponentInParent<SceneDoodadLodCmpt> ();
						if (lod != null && lod.Index >= 0) {
							int allyId = VArtifactTownManager.Instance.GetTownByID(lod.Index).AllyId;
							int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
							int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
							MonsterEntityCreator.CreateAdMonster(protoId,pos,allyColor,playerId);
						}
					}else{
						MonsterEntityCreator.CreateMonster(protoId, pos);
					}
					//
				}
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(_timeIntervalMinMax.x,_timeIntervalMinMax.y));
		}
	}

	Vector3 GetSpawnPos()
	{
		return _posOfMonsters [UnityEngine.Random.Range (0, _posOfMonsters.Length)];
	}
	int GetProtoId()
	{
		return _protoIdsOfMonsters [UnityEngine.Random.Range (0, _protoIdsOfMonsters.Length)];
	}
	int GetEntitiesCnt()
	{
		List<PeEntity> entities = new List<PeEntity>(EntityMgr.Instance.mDicEntity.Values);
		return entities.Count (it => {
			return it.commonCmpt != null && (it.commonCmpt.Race == ERace.Paja || it.commonCmpt.Race == ERace.Puja) && PETools.PEUtil.SqrMagnitudeH (it.position, Center) <= _radius * _radius;
		});
	}
}
