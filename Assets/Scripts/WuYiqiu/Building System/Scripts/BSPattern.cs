using Mono.Data.SqliteClient;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BSPattern 
{
	public EBSVoxelType type = EBSVoxelType.Voxel;

	public int 			ID;
	public int 			size;
	public string		IconName;
	public string		MeshPath;

	public bool   		canRotate = false;

	public BSVoxel[,,]	voxelList;

	public static Dictionary<int, BSPattern> s_tblPatterns;

	public Material	   MeshMat = null;

	public static void LoadBrush()
	{
		s_tblPatterns = new Dictionary<int, BSPattern>();
		
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("BlockBrush");
		while (reader.Read())
		{
			BSPattern addBrush = new BSPattern(reader);
			s_tblPatterns[addBrush.ID] = addBrush;
		}

		foreach (KeyValuePair<int, BSPattern> kvp in s_tblPatterns)
		{
			s_DefaultB1 = kvp.Value;
			break;
		}
	}

	public static BSPattern Get(int id)
	{
		if(s_tblPatterns.ContainsKey(id))
			return s_tblPatterns[id];
		return null;
	}

	public BSPattern()
	{

	}
	public BSPattern(SqliteDataReader reader)
	{
		ID 		  = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
		size 	  = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Size")));
		IconName  = reader.GetString(reader.GetOrdinal("IconName"));
		MeshPath  = reader.GetString(reader.GetOrdinal("MeshPath"));
		voxelList = new BSVoxel[size, size, size];
		type 	  = EBSVoxelType.Block;
		
		string blockString = reader.GetString(reader.GetOrdinal("Blocks"));
		
		string[] _blockList = blockString.Split(';');
		int count = 0;
		
		for(int y = 0; y < size; y++)
		{
			for(int z = 0; z < size; z++)
			{
				for(int x = 0; x < size; x++)
				{
					string[] valueList = _blockList[count].Split(',');
					if (type == EBSVoxelType.Block)
					{
						voxelList[x, y, z] = new BSVoxel(BSVoxel.MakeBlockType(Convert.ToInt32(valueList[0]), Convert.ToInt32(valueList[1])), 0);
					}
					else if (type == EBSVoxelType.Voxel)
					{
						voxelList[x, y, z] = new BSVoxel(255, 2);
					}

					count++;
				}
			}
		}

	}

	public void SetMaterial (byte matType)
	{
		for (int i = 0; i < size; i++)
			for (int j = 0; j < size; j++)
				for (int k = 0; k < size; k++)
					voxelList[i, j, k].materialType = matType;
	}

	#region  Predefined_Brush

	// Voxel
	public static BSPattern DefaultV1  
	{
		get
		{
			if (s_DefaultV1 == null)
			{
				s_DefaultV1 = new BSPattern();
				s_DefaultV1.ID = -1;
				s_DefaultV1.size = 1;
				s_DefaultV1.voxelList = new BSVoxel[1, 1, 1];
				s_DefaultV1.voxelList[0, 0, 0] = new BSVoxel(255, 2);
				s_DefaultV1 .type = EBSVoxelType.Voxel;
			}
			
			return s_DefaultV1;
		}
	}

	public static BSPattern DefaultV2
	{
		get
		{
			if (s_DefaultV2 == null)
			{
				s_DefaultV2 = new BSPattern();
				s_DefaultV2.ID = -2;
				s_DefaultV2.size = 2;
				s_DefaultV2.voxelList = new BSVoxel[2, 2, 2];

				for (int i = 0; i < 2; i++)
					for (int j = 0; j < 2; j++)
						for (int k = 0; k < 2; k++)
							s_DefaultV2.voxelList[i, j, k] = new BSVoxel(255, 2);
				s_DefaultV2.type = EBSVoxelType.Voxel;
			}

			return s_DefaultV2;
		}
	}

	public static BSPattern DefaultV3
	{
		get
		{
			if (s_DefaultV3 == null)
			{
				s_DefaultV3 = new BSPattern();
				s_DefaultV3.ID = -3;
				s_DefaultV3.size = 3;
				s_DefaultV3.voxelList = new BSVoxel[3, 3, 3];
				
				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++)
						for (int k = 0; k < 3; k++)
							s_DefaultV3.voxelList[i, j, k] = new BSVoxel(255, 2);
				s_DefaultV3.type = EBSVoxelType.Voxel;
			}

			return s_DefaultV3;
		}
	}

	private static BSPattern s_DefaultV1;
	private static BSPattern s_DefaultV2;
	private static BSPattern s_DefaultV3;

	// block
	public static BSPattern DefaultB1  
	{
		get
		{
			if (s_DefaultB1 == null)
			{
				s_DefaultB1 = new BSPattern();
				s_DefaultB1.ID = -1;
				s_DefaultB1.size = 1;
				s_DefaultB1.voxelList = new BSVoxel[1, 1, 1];
				s_DefaultB1.voxelList[0, 0, 0] = new BSVoxel(BSVoxel.MakeBlockType(1, 0), 0);
				s_DefaultB1 .type = EBSVoxelType.Block;
			}
			
			return s_DefaultB1;
		}
	}
	
	public static BSPattern DefaultB2
	{
		get
		{
			if (s_DefaultB2 == null)
			{
				s_DefaultB2 = new BSPattern();
				s_DefaultB2.ID = -2;
				s_DefaultB2.size = 2;
				s_DefaultB2.voxelList = new BSVoxel[2, 2, 2];
				
				for (int i = 0; i < 2; i++)
					for (int j = 0; j < 2; j++)
						for (int k = 0; k < 2; k++)
							s_DefaultB2.voxelList[i, j, k] = new BSVoxel(BSVoxel.MakeBlockType(1, 0), 0);
				s_DefaultB2.type = EBSVoxelType.Block;
			}

			return s_DefaultB2;
		}
	}
	
	public static BSPattern DefaultB3
	{
		get
		{
			if (s_DefaultB3 == null)
			{
				s_DefaultB3 = new BSPattern();
				s_DefaultB3.ID = -3;
				s_DefaultB3.size = 3;
				s_DefaultB3.voxelList = new BSVoxel[3, 3, 3];
				
				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++)
						for (int k = 0; k < 3; k++)
							s_DefaultB3.voxelList[i, j, k] = new BSVoxel(BSVoxel.MakeBlockType(1, 0), 0);
				s_DefaultB3.type = EBSVoxelType.Block;
			}

			return s_DefaultB3;
		}
	}


	private static BSPattern s_DefaultB1;
	private static BSPattern s_DefaultB2;
	private static BSPattern s_DefaultB3;

	#endregion
}
