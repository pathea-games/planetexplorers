/*  Author: Mark Davis
 * 
 *  This is the main voxel terrain class for use within a project.
 *  It will work as is, but it's designed to provide starting point for your project's unique requirements.
 * 
 */

using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

/// <summary>
/// This is the main voxel terrain class for use within a project.
/// </summary>
/// <remarks>
/// It will work as is, but it's designed to provide a starting point for your project's unique requirements.
/// </remarks>

public partial class VFVoxelTerrain : SkillSystem.SkEntity, IVxChunkHelperProc, ILODNodeDataMan
{

	#region VoxelInAttack_Management
	class VoxelInAttack{
		public float invalidTimePoint;
		public byte volume;
		public byte type;
	}
	const float atkActiveTime = 10;
	Dictionary<IntVector3, VoxelInAttack> listVoxelInAttack = new Dictionary<IntVector3, VoxelInAttack>();
	bool AttackVoxel(int vx, int vy, int vz, byte volumeDec)
	{
        // Tip: Switch between Voxels.Read and Voxels.SafeRead (with bounds check) to enabled/disable falling through the world.
       	VFVoxel existingVoxel = Voxels.SafeRead(vx, vy, vz);
		NaturalResAsset.NaturalRes res = NaturalResAsset.NaturalRes.GetTerrainResData(existingVoxel.Type);
        if (res == null)
        {
            Debug.LogWarning("Failed to get NaturalRes !! ResType = " + existingVoxel.Type + " ---> position = " + vx + " " + vy + "" + vz);
            return false;
        }
		
		float curTimePoint = Time.time;
		IntVector3 voxelPos = new IntVector3(vx, vy, vz);
		VoxelInAttack voxelInAtk = null;
		if(listVoxelInAttack.TryGetValue(voxelPos, out voxelInAtk))
		{
			int volumeLeft = (curTimePoint < voxelInAtk.invalidTimePoint && existingVoxel.Type == voxelInAtk.type) ?
								(int)(voxelInAtk.volume - volumeDec * res.m_duration) :
								(int)(existingVoxel.Volume - volumeDec * res.m_duration);
			if(volumeLeft > 0)
			{
				voxelInAtk.invalidTimePoint = curTimePoint + atkActiveTime;
				voxelInAtk.volume = (byte)volumeLeft;
				voxelInAtk.type = existingVoxel.Type;
				return false;
			}
			else
			{
				listVoxelInAttack.Remove(voxelPos);
				Voxels.SafeWrite(vx, vy, vz, new VFVoxel(0));
                //AlterVoxelInBuild()
				return true;
			}
		}
		else
		{
			int volumeLeft = (int)(existingVoxel.Volume - volumeDec * res.m_duration);
			if(volumeLeft > 0)
			{
				voxelInAtk = new VoxelInAttack();
				voxelInAtk.invalidTimePoint = curTimePoint + atkActiveTime;
				voxelInAtk.volume = (byte)volumeLeft;
				voxelInAtk.type = existingVoxel.Type;
				List<IntVector3> keyList = new List<IntVector3>();
				for(int i = 0; i < listVoxelInAttack.Count; i++)
				{
					KeyValuePair<IntVector3, VoxelInAttack> keyValue = listVoxelInAttack.ElementAt(i);
					if(curTimePoint < keyValue.Value.invalidTimePoint)
						break;

					keyList.Add(keyValue.Key);
				}
				for(int i = 0; i < keyList.Count; i++)
				{
					listVoxelInAttack.Remove(keyList[i]);
				}
				listVoxelInAttack.Add(voxelPos, voxelInAtk);
				return false;
			}
			else
			{
				Voxels.SafeWrite(vx, vy, vz, new VFVoxel(0));
				return true;
			}
		}
		
//		return false;
	}
	#endregion
	#region Alter_Voxel_FuncSet
	public void AlterVoxel(int vx, int vy, int vz, VFVoxel voxel, bool writeType, bool writeVolume)
    {
        // Tip: Switch between Voxels.Read and Voxels.SafeRead (with bounds check) to enabled/disable falling through the world.

        VFVoxel existingVoxel = Voxels.SafeRead(vx, vy, vz);

        if (existingVoxel.Volume < VoxelTerrainConstants._isolevel)
        {
            existingVoxel.Volume = 0;
        }

        Voxels.SafeWrite(vx, vy, vz, new VFVoxel((byte)Mathf.Clamp((int)existingVoxel.Volume + (writeVolume ? (int)voxel.Volume : 0), 0, 255),
            writeType ? voxel.Type : existingVoxel.Type));
    }
		
	/// <summary>
	/// Manipulates a voxel and updates neighbor chunks if the voxel lies on a border.
	/// </summary>
	/// <remarks>
	/// See preceding function.
	/// </remarks>
	public void AlterVoxel(Vector3 position, VFVoxel voxel, bool writeType, bool writeVolume)
	{
		AlterVoxel((int)position.x, (int)position.y, (int)position.z, voxel, writeType, writeVolume);
	}
	
	// Note: now this function is invoked to mod/dig terrain
	public void AlterVoxelInBuild(int vx, int vy, int vz, VFVoxel voxel)
	{
		Voxels.SafeWrite(vx, vy, vz, voxel);
		int n = Voxels.DirtyChunkList.Count;
		for(int i = 0; i < n; i++)
		{
			SaveLoad.AddChunkToSaveList(Voxels.DirtyChunkList[i]);
		}
	}
	public void AlterVoxelInBuild(Vector3 position, VFVoxel voxel){
		AlterVoxelInBuild((int)position.x, (int)position.y, (int)position.z, voxel);
	}
	
	// Note: now this function is invoked to attack building
	public void AlterVoxelInChunk(int vx, int vy, int vz, VFVoxel voxel, bool writeType, bool writeVolume)
    {
		if(!AttackVoxel(vx, vy, vz, voxel.Volume))
			return;

		//Instantiate effect
        int index = UnityEngine.Random.Range(1, 3);
        string path = "Prefab/Particle/FX_voxel_block_collapsing_0" + index;

        GameObject particleResources = Resources.Load(path) as GameObject;
		if (particleResources != null)
		{
			GameObject particle = GameObject.Instantiate(particleResources, new Vector3(vx, vy, vz), Quaternion.identity) as GameObject;
			GameObject.Destroy(particle, 2.5f);
		}
		
		int n = Voxels.DirtyChunkList.Count;
		for(int i = 0; i < n; i++)
		{
			SaveLoad.AddChunkToSaveList(Voxels.DirtyChunkList[i]);
		}
    }
	
	public void AlterVoxelListInChunk(List<IntVector3> intPos, float durDec){
		
		float tmpDur = Mathf.Clamp(durDec, 0, 255);
		VFVoxel _voxel = new VFVoxel((byte)tmpDur, 0);

		int n = intPos.Count;
		for(int i = 0 ; i < n; i++)
		{
			IntVector3 pos = intPos[i];
			AlterVoxelInChunk(pos.x, pos.y, pos.z, _voxel, true, true);
		}
	}
	
	public void AlterVoxelBox(int x, int y, int z, int width, int height, int depth, float volume, byte voxelType, bool writeType)
	{
		int x1 = x - width / 2;
		int y1 = y - height / 2;
		int z1 = z - depth / 2;
		int x2 = x + width / 2;
		int y2 = y + height / 2;
		int z2 = z + depth / 2;

		for (int px = x1; px < x2; ++px)
		{
			for (int py = y1; py < y2; ++py)
			{
				for (int pz = z1; pz < z2; ++pz)
				{
					VFVoxel voxel = new VFVoxel(VFVoxel.ToNormByte(volume), voxelType);
					AlterVoxel(px, py, pz, voxel, writeType, true);
				}
			}
		}
	}

	public void AlterVoxelSphere(int x, int y, int z, int radius, float coreVolume, byte voxelType, bool writeType)
	{
		int x1 = x - radius;
		int y1 = y - radius;
		int z1 = z - radius;
		int x2 = x + radius;
		int y2 = y + radius;
		int z2 = z + radius;

		float radiusf = (float)radius;

		for (int px = x1; px < x2; ++px)
		{
			for (int py = y1; py < y2; ++py)
			{
				for (int pz = z1; pz < z2; ++pz)
				{
					Vector3 v = new Vector3(px - x, py - y, pz - z);
					float bpos = radiusf - v.magnitude;
					if (bpos > 0)
					{
						VFVoxel voxel = new VFVoxel( VFVoxel.ToNormByte( (bpos >= coreVolume) ? coreVolume : (bpos % coreVolume) ), voxelType);
						AlterVoxel(px, py, pz, voxel, writeType, true);
					}
				}
			}
		}
	}
	#endregion	
	public VFVoxel GetRaycastHitVoxel(RaycastHit hitInfo, out IntVector3 voxelPos)
	{
		//(hitInfo.point - transform.localPosition) / VoxelTerrainConstants._scale;
		Vector3 vPos = hitInfo.point;
		
		if(0.05f > Mathf.Abs(hitInfo.normal.normalized.x))
			vPos.x = Mathf.RoundToInt(vPos.x);
		else
			vPos.x = (hitInfo.normal.x > 0)?Mathf.Floor(vPos.x):Mathf.Ceil(vPos.x);
		if(0.05f > Mathf.Abs(hitInfo.normal.normalized.y))
			vPos.y = Mathf.RoundToInt(vPos.y);
		else
			vPos.y = (hitInfo.normal.y > 0)?Mathf.Floor(vPos.y):Mathf.Ceil(vPos.y);
		if(0.05f > Mathf.Abs(hitInfo.normal.normalized.z))
			vPos.z = Mathf.RoundToInt(vPos.z);
		else
			vPos.z = (hitInfo.normal.z > 0)?Mathf.Floor(vPos.z):Mathf.Ceil(vPos.z);
		
		voxelPos = new Vector3(Mathf.Round(vPos.x), Mathf.Round(vPos.y), Mathf.Round(vPos.z));
#if false		//VOXEL_OFFSET
		voxelPos.x += 1;
		voxelPos.y += 1;
		voxelPos.z += 1;
#endif
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		float dis = 0;
		while(voxel.Volume==0)
		{
			dis += 0.1f;
			if(dis > 1.5f)
				break;
			Vector3 hitPoint = hitInfo.point - hitInfo.normal*dis;
			voxelPos = new Vector3(Mathf.Round(hitPoint.x), Mathf.Round(hitPoint.y), Mathf.Round(hitPoint.z));
#if false			// VOXEL_OFFSET
			voxelPos.x += 1;
			voxelPos.y += 1;
			voxelPos.z += 1;
#endif
			voxel = VFVoxelTerrain.self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		}
		return voxel;
	}
}

