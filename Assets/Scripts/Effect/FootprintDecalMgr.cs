using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NaturalResAsset;

public class FootprintDecalMgr : Pathea.MonoLikeSingleton<FootprintDecalMgr>
{
	List<FootprintDecalMan> _reqDecals = new List<FootprintDecalMan>();
	int _layerToFootprintTerra = Pathea.Layer.VFVoxelTerrain;
	int _layerToFootprintMetal = Pathea.Layer.SceneStatic;
	int _layerMaskToFootprint = 0;
	int _lastSoundID;
	static readonly int[] _metalClipIds = new int[]{4097, 4098, 4099, 4100, };
	static readonly int[] _stoneClipIds = new int[]{4501,4502,4503,4504,};
	int[] _nonTerClipIds = _metalClipIds;

	protected override void OnInit()
	{
		_layerMaskToFootprint = 0;
		_layerToFootprintTerra = Pathea.Layer.VFVoxelTerrain;
		_layerToFootprintMetal = -1;
		_nonTerClipIds = _metalClipIds;
		if (Pathea.PeGameMgr.IsAdventure) {
			if (RandomDungenMgr.Instance!= null && RandomDungenMgrData.dungeonBaseData != null && RandomDungenMgrData.InDungeon){
				DungeonType dType = RandomDunGenUtil.GetDungeonType();
				if(dType == DungeonType.Iron || dType == DungeonType.Cave) {
					_layerToFootprintTerra = -1;
					_layerToFootprintMetal = Pathea.Layer.SceneStatic;
					_nonTerClipIds = _stoneClipIds;
				}
				//if(dType == DungeonType.Cave) {
				//	_layerToFootprintTerra = Pathea.Layer.SceneStatic;
				//	_layerToFootprintMetal = -1;
				//}
			}
		} else if (Pathea.PeGameMgr.IsStory) {
			if(Pathea.PeGameMgr.IsSingle || Pathea.PeGameMgr.IsTutorial){
				if(Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand){
					_layerToFootprintTerra = -1;
					_layerToFootprintMetal = Pathea.Layer.SceneStatic;
				}
			}
			if(Pathea.PeGameMgr.IsMulti) {
				if (PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand){
					_layerToFootprintTerra = -1;
					_layerToFootprintMetal = Pathea.Layer.SceneStatic;
				}
			}
		}
		if (_layerToFootprintTerra >= 0)			_layerMaskToFootprint |= (1 << _layerToFootprintTerra);
		if (_layerToFootprintMetal >= 0)			_layerMaskToFootprint |= (1 << _layerToFootprintMetal);
	}
	public override void OnDestroy()
	{
		base.OnDestroy ();
	}
	
	public override void Update()
	{
		int n = _reqDecals.Count;
		for (int i = n-1; i >= 0; i--) {
			if(_reqDecals[i] == null){
				_reqDecals.RemoveAt(i);
			} else {
				Update(_reqDecals[i]);
			}
		}
	}

	public void Register(FootprintDecalMan reqDecal)
	{
		_reqDecals.Add (reqDecal);
	}
	public void Unregister(FootprintDecalMan reqDecal)
	{
		_reqDecals.Remove (reqDecal);
	}
	void Update(FootprintDecalMan reqDecal)
	{
		float fCurFootDistance = 0f;
		bool bPlaceAFootprint = false;
		
		bool bPlayerInMove = reqDecal._ctrlr.velocity.magnitude > reqDecal._thresVelOfMove;
		if (bPlayerInMove)
		{
			//Vector3 vecMoveDir = mMoveDirection;													vecMoveDir.y = 0;
			Vector3 vecMoveDir = reqDecal._ctrlr.velocity;											vecMoveDir.y = 0;
			Vector3 vecFootDistance = reqDecal._lrFoot[0].position - reqDecal._lrFoot[1].position;	vecFootDistance.y = 0;
			fCurFootDistance = Vector3.Dot(vecFootDistance,vecMoveDir.normalized);
			if((0==reqDecal._curFoot&&reqDecal._fpLastLRFootDistance>0&&fCurFootDistance<reqDecal._fpLastLRFootDistance) ||
			   (1==reqDecal._curFoot&&reqDecal._fpLastLRFootDistance<0&&fCurFootDistance>reqDecal._fpLastLRFootDistance) )
			{
				bPlaceAFootprint = true;
			}
			reqDecal._fpbFootInMove[0] = reqDecal._fpbFootInMove[1] = true;
			reqDecal._fpbPlayerInMove = true;
		}
		else
		{
			if(reqDecal._fpbPlayerInMove)
			{
				reqDecal._fpLastFootsPos[0] = reqDecal._lrFoot[0].position;
				reqDecal._fpLastFootsPos[1] = reqDecal._lrFoot[1].position;
				reqDecal._fpbPlayerInMove = false;
			}
			float sqrDist = Vector3.Magnitude(reqDecal._lrFoot[reqDecal._curFoot].position - reqDecal._fpLastFootsPos[reqDecal._curFoot]);
			if(reqDecal._fpbFootInMove[reqDecal._curFoot])
			{
				if(sqrDist < 0.02f)
					reqDecal._fpbFootInMove[reqDecal._curFoot] = false;
				reqDecal._fpLastFootsPos[reqDecal._curFoot] = reqDecal._lrFoot[reqDecal._curFoot].position;
			}
			else if(sqrDist > 0.04f)
			{
				reqDecal._fpbFootInMove[reqDecal._curFoot] = true;
				bPlaceAFootprint = true;
			}
		}
		
		if(bPlaceAFootprint || reqDecal._ctrlr.fallGround)
		{
			Transform curFoot = reqDecal._lrFoot[reqDecal._curFoot];
			Ray ray = new Ray(curFoot.position+Vector3.up*reqDecal._rayLength*0.5f, Vector3.down);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, reqDecal._rayLength, (1<<_layerToFootprintTerra)|(1<<_layerToFootprintMetal)))
			{
				Vector3 pos = hit.point+(hit.normal*0.02f);
				int hitLayer = hit.transform.gameObject.layer;
				if(hitLayer == _layerToFootprintTerra)
				{
					Vector3 vecFootprintDir = Quaternion.Euler(0,90,0)*curFoot.forward;
					vecFootprintDir.y = 0;
					int idxFoot = reqDecal._curFpIdx[reqDecal._curFoot];
					Quaternion rot = Quaternion.FromToRotation (Vector3.up, hit.normal)*Quaternion.FromToRotation(Vector3.forward,vecFootprintDir);
					if(reqDecal._fpGoUpdates[reqDecal._curFoot,idxFoot] == null)
					{
						GameObject obj = MonoBehaviour.Instantiate(reqDecal._fpSeedGoLR, pos, rot) as GameObject;
						obj.transform.parent = reqDecal.FootPrintsParent;
						reqDecal._fpGoUpdates[reqDecal._curFoot,idxFoot] = obj.GetComponent<FootprintDecal>();
					}
					else
					{
						reqDecal._fpGoUpdates[reqDecal._curFoot,idxFoot].Reset(pos, rot);
					}
					reqDecal._curFpIdx[reqDecal._curFoot] = (idxFoot+1)%reqDecal._fpGoUpdates.GetLength(1);
				}
				//Sound effect
				if(bPlayerInMove || reqDecal._ctrlr.fallGround)
				{
					int soundID = 0;
					if(hitLayer == _layerToFootprintMetal){
						if ((reqDecal._mmc == null || !reqDecal._mmc.GetMaskState(Pathea.PEActionMask.SwordAttack)) ){
							int idx = UnityEngine.Random.Range(0, _nonTerClipIds.Length);
							soundID = _nonTerClipIds[idx];
							if(_lastSoundID == soundID){
								idx += UnityEngine.Random.Range(1, _nonTerClipIds.Length);
								if(idx >= _nonTerClipIds.Length){
									idx -= _nonTerClipIds.Length;
								}
								soundID = _nonTerClipIds[idx];
							}
							AudioManager.instance.Create(hit.point, soundID);
							_lastSoundID = soundID;
						}
					}else{ // _layerToFootprintTerr
						int vType = 8;
						if(hitLayer == Pathea.Layer.VFVoxelTerrain){
							VFVoxel groundVoxel = VFVoxelTerrain.self.Voxels.SafeRead((int)hit.point.x, (int)hit.point.y, (int)hit.point.z);
							vType = groundVoxel.Type;
						}
						NaturalRes res = NaturalResAsset.NaturalRes.GetTerrainResData(vType);					
						if (res != null)
						{
							if (res.mGroundEffectID > 0){
								Pathea.Effect.EffectBuilder.Instance.Register(res.mGroundEffectID, null, reqDecal.transform);
							}
							if (null != res.mGroundSoundIDs && res.mGroundSoundIDs.Length > 0 
							    && (reqDecal._mmc == null || !reqDecal._mmc.GetMaskState(Pathea.PEActionMask.SwordAttack)) )
							{
								if(res.mGroundSoundIDs.Length > 1)
								{
									int idx = UnityEngine.Random.Range(0, res.mGroundSoundIDs.Length);
									soundID = res.mGroundSoundIDs[idx];
									if(_lastSoundID == soundID){
										idx += UnityEngine.Random.Range(1, res.mGroundSoundIDs.Length);
										if(idx >= res.mGroundSoundIDs.Length){
											idx -= res.mGroundSoundIDs.Length;
										}
										soundID = res.mGroundSoundIDs[idx];
									}
								}
								else
									soundID = res.mGroundSoundIDs[0];
								AudioManager.instance.Create(pos, soundID);
								_lastSoundID = soundID;
							}
						}
					}
				}
			}
			
			reqDecal._curFoot = (reqDecal._curFoot+1)&1;
		}
		reqDecal._fpLastLRFootDistance = fCurFootDistance;
	}
}
