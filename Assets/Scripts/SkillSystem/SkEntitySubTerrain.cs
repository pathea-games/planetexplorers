using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea;

public interface ISkSubTerrain
{
	GlobalTreeInfo treeInfo{ get; }
}

// TODO : make LSubTerrainMgr/RSubTerrainMgr derived from this class
public class SkEntitySubTerrain : SkEntity
{
	static SkEntitySubTerrain m_Instance;

	public static SkEntitySubTerrain Instance
	{
		get
		{
			if(null == m_Instance)
			{
				m_Instance = (new GameObject("SkEntitySubTerrain")).AddComponent<SkEntitySubTerrain>();
				m_Instance.InitSkEntity();
				if(GameConfig.IsMultiMode && SubTerrainNetwork.Instance != null)
				{
					SubTerrainNetwork.Instance.Init();
				}
			}
			return m_Instance;
		}
	}

    event Action<SkEntity, GlobalTreeInfo> onTreeCutDown;

    public void AddListener(Action<SkEntity, GlobalTreeInfo> listener)
    {
        onTreeCutDown += listener;
    }

    public void RemoveListener(Action<SkEntity, GlobalTreeInfo> listener)
    {
        onTreeCutDown -= listener;
    }

	Dictionary<Vector3, float> treeHPInfos = new Dictionary<Vector3, float>();

	public float GetTreeHP(Vector3 treeInfo)
	{
		if(!treeHPInfos.ContainsKey(treeInfo))
			treeHPInfos[treeInfo] = 255f;
		return treeHPInfos[treeInfo];
	}

	//
	public void SetTreeHp(Vector3 tree, float hp)
	{
		foreach(Vector3 treeInfo in treeHPInfos.Keys)
		{
			if(treeInfo == tree)
			{
				treeHPInfos[treeInfo] = hp;
				break;
			}
		}
	}

//	GlobalTreeInfo _targetTree = null;
//	float lastTime = -1;
	private void InitSkEntity()
	{
		Init(onAlterAttribs, null, 4);
	}
	private void onAlterAttribs(int idx, float oldValue, float newValue)
	{
//		if(_targetTree == null)	return;
//		Debug.Log("Cutting a tree........");

		/* when the tree's hp has been cut to zero
		if (null != LSubTerrainMgr.Instance)
		{
			LSubTerrainMgr.DeleteTree(mGroundItem);
			LSubTerrainMgr.RefreshAllLayerTerrains();
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			RSubTerrainMgr.DeleteTree(mGroundItem._treeInfo);
			RSubTerrainMgr.RefreshAllLayerTerrains();
		}
		*/
		
		if(idx != 2)	return;

		float damage = _attribs.sums[0];
		float resourceBonus = _attribs.sums[1];
		bool returnResource = _attribs.sums[2] > 0.0f;

		SkEntity caster = GetCasterToModAttrib(idx);
		if(null != caster)
		{
			ISkSubTerrain subTerrain = caster as ISkSubTerrain;
			if(null != subTerrain && null != subTerrain.treeInfo)
			{
				treeHPInfos[subTerrain.treeInfo.WorldPos] = DigTerrainManager.Fell(subTerrain.treeInfo, damage, GetTreeHP(subTerrain.treeInfo.WorldPos));
				if(GameConfig.IsMultiMode)
				{
					caster.SendFellTree(subTerrain.treeInfo._treeInfo.m_protoTypeIdx, subTerrain.treeInfo.WorldPos,subTerrain.treeInfo._treeInfo.m_heightScale,subTerrain.treeInfo._treeInfo.m_widthScale);
				}
				else
				{
					if(treeHPInfos[subTerrain.treeInfo.WorldPos] <= 0)
					{
                        OnTreeCutDown(caster, subTerrain.treeInfo);
						DigTerrainManager.RemoveTree(subTerrain.treeInfo);
						if(returnResource)
						{
							bool bGetSpItems = false;
							if(caster is SkAliveEntity)
							{
								SkAliveEntity alive = (SkAliveEntity)caster;
								if(alive.Entity.proto == EEntityProto.Player)
								{
									SkillTreeUnitMgr mgr = alive.Entity.GetCmpt<SkillTreeUnitMgr>();
									bGetSpItems = mgr.CheckMinerGetRare();
								}
							}
							Dictionary<int, int> itemGet = DigTerrainManager.GetTreeResouce(subTerrain.treeInfo, resourceBonus,bGetSpItems);
							if(itemGet.Count > 0)
							{
								List<int> itemsArray = new List<int>(itemGet.Count*2);
								foreach(int intemID in itemGet.Keys)
								{
									itemsArray.Add(intemID);
									itemsArray.Add(itemGet[intemID]);
								}
                                GetSpecialItem.PlantItemAdd(ref itemsArray);  //植物特殊道具添加
								caster._attribs.pack += itemsArray.ToArray();
							}
						}
					}
				}
			}
		}
	}

    public void OnTreeCutDown(SkEntity skEntity, GlobalTreeInfo treeInfo)
    {
        if (null != onTreeCutDown)
        {
            onTreeCutDown(skEntity, treeInfo);
        }
    }
//	private void CondToCut(SkFuncInOutPara para)
//	{
//		float curTime = Time.time;
//		if(curTime - lastTime > 1f)
//		{
//			if(!TryGetTreeToCut(para._inst._caster, out _targetTree))
//			{
//				para._ret = false;
//				return;
//			}
//		}
//
//		para._ret = Input.GetKey(KeyCode.Mouse0) && _targetTree != null;
//		if(para._ret)
//		{
//			lastTime = curTime;
//		}
//	}
//	private bool TryGetTreeToCut(SkEntity caster, out GlobalTreeInfo tree)
//	{
//		Transform transCaster = caster.transform;
//		tree = null;
//		if(null != LSubTerrainMgr.Instance)
//		{
//			tree = LSubTerrainMgr.RayCast(new Ray(transCaster.position + Vector3.up, transCaster.forward), 1f);
//			return true;
//		}
//		else if(null != RSubTerrainMgr.Instance)
//		{
//			TreeInfo findTree = RSubTerrainMgr.RayCast(new Ray(transCaster.position + Vector3.up, transCaster.forward), 1f);
//			if(null != findTree)
//			{
//				tree = new GlobalTreeInfo(-1, findTree);
//				return true;
//			}
//		}
//		return false;
//	}
}
