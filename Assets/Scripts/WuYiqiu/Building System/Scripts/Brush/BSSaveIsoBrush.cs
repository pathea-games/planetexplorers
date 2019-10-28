using UnityEngine;
using System.Collections;
using System.IO;

public class BSSaveIsoBrush : BSSelectBrush 
{
	public string IsoName = "No Name";

	public static string s_IsoPath = "/PlanetExplorers/BuildingIso/";

	public static string s_Ext = ".biso";

	protected override bool AfterSelectionUpdate ()
	{
		return true;
//		if (!m_Selecting)
//		{
//			// Do delelete
//			if (Input.GetKeyDown(KeyCode.Insert))
//			{
//
//				BSIsoData iso = new BSIsoData();
//				iso.Init(pattern.type);
//				iso.m_HeadInfo.Name = IsoName;
//
//				BSTools.IntBox bound = BSTools.SelBox.CalculateBound(s_SelectionBoxes);
//
//				int center_x = bound.xMin + (bound.xMax - bound.xMin + 1) / 2;
//				int center_y = bound.yMin;
//				int center_z = bound.zMin + (bound.zMax - bound.zMin + 1) / 2;
//
//				iso.m_HeadInfo.xSize = bound.xMax - bound.xMin + 1;
//				iso.m_HeadInfo.ySize = bound.yMax - bound.yMin + 1;
//				iso.m_HeadInfo.zSize = bound.zMax - bound.zMin + 1;
//
//				foreach (BSTools.SelBox box in s_SelectionBoxes)
//				{
//					for (int x = box.m_Box.xMin; x <=  box.m_Box.xMax; x++ )
//					{
//						for (int y = box.m_Box.yMin; y <= box.m_Box.yMax; y++)
//						{
//							for (int z = box.m_Box.zMin; z <= box.m_Box.zMax; z++)
//							{
//								BSVoxel voxel = dataSource.SafeRead(x, y, z);
//								int key = BSIsoData.IPosToKey(x - center_x, y - center_y, z - center_z);
//								iso.m_Voxels.Add(key, voxel);
//							}
//						}
//					}
//				}
//
//				string FilePath = GameConfig.GetUserDataPath() + s_IsoPath;
//				SaveFile(FilePath, iso);
//
//				
//				ClearSelection();
//
//				Debug.Log("");
//			}
//			
//			
//		}
	}

	void SaveFile (string file_path, BSIsoData iso)
	{
		if (!Directory.Exists(file_path))
			Directory.CreateDirectory(file_path);

		file_path +=  iso.m_HeadInfo.Name + s_Ext;
		
		using(FileStream fileStream = new FileStream(file_path, FileMode.Create, FileAccess.Write))
		{
			BinaryWriter bw = new BinaryWriter(fileStream);
			byte[] datas = iso.Export();
			bw.Write(datas);
			bw.Close();
		}
	}

}
