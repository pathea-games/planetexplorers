using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BSMiscBrush : BSSelectBrush 
{

	public string IsoSavedName = "No Name";

	private bool _extruding = false;
	private float _startExtrudeTime = 0;


    //const float intervalTime = 0.2f;
    //private float _curTime = 0;
    //private bool _down = false;

    protected override bool AfterSelectionUpdate ()
	{
		if (!m_Selecting)
		{
			if (m_SelectionBoxes.Count == 0)
				return true;

			if (!_extruding)
			{
				// Do delelete voxel
				if (BSInput.s_Delete)
				{
					DeleteVoxel();
				}
				// Do Save
				else if (Input.GetKeyDown(KeyCode.Period))
				{
					BSIsoData iso = null;
					SaveToIso(IsoSavedName, new byte[0], out iso);
				}
			}

            if (!_extruding)
            {
                // Do ExtrudeSelection
                if (BSInput.s_Shift && BSInput.s_Left)
                {
                    ExtrudeSelection(-1, 0, 0);
                    _startExtrudeTime = 0;
                }
                if (BSInput.s_Shift && BSInput.s_Right)
                {
                    ExtrudeSelection(1, 0, 0);
                    _startExtrudeTime = 0;
                }
                if (BSInput.s_Shift && BSInput.s_Up)
                {
                    ExtrudeSelection(0, 1, 0);
                    _startExtrudeTime = 0;
                }
                if (BSInput.s_Shift && BSInput.s_Down)
                {
                    ExtrudeSelection(0, -1, 0);
                    _startExtrudeTime = 0;
                }
                if (BSInput.s_Shift && BSInput.s_Forward)
                {
                    ExtrudeSelection(0, 0, 1);
                    _startExtrudeTime = 0;
                }
                if (BSInput.s_Shift && BSInput.s_Back)
                {
                    ExtrudeSelection(0, 0, -1);
                    _startExtrudeTime = 0; 
                }
            }

			if (_extruding)
			{
				if (_startExtrudeTime > 0.3f)
				{
					_startExtrudeTime = 0;
					_extruding = false;
					return true;
				}
				else
				{
					_startExtrudeTime += Time.deltaTime;
					return false;
				}
			}
		}

		return true;
	}

	public void DeleteVoxel ()
	{
		if (m_SelectionBoxes.Count == 0)
			return;

		List<BSVoxel> new_voxels = new List<BSVoxel>();
		List<IntVector3> indexes = new List<IntVector3>();
		List<BSVoxel> old_voxels = new List<BSVoxel>();

		foreach (BSTools.SelBox box in m_SelectionBoxes)
		{
			for (int x = box.m_Box.xMin; x <=  box.m_Box.xMax; x++ )
			{
				for (int y = box.m_Box.yMin; y <= box.m_Box.yMax; y++)
				{
					for (int z = box.m_Box.zMin; z <= box.m_Box.zMax; z++)
					{
						BSVoxel voxel = dataSource.Read(x, y, z);

						new_voxels.Add(new BSVoxel());
						indexes.Add(new IntVector3(x, y, z));
						old_voxels.Add(voxel);
					}
				}
			}
		}


		// Select modify 
		ClearSelection(m_Action);

		// Modity modify
		if (indexes.Count != 0)
		{
			BSVoxelModify vm = new BSVoxelModify(indexes.ToArray(), old_voxels.ToArray(), new_voxels.ToArray(), dataSource, EBSBrushMode.Subtract);

			m_Action.AddModify(vm);
			vm.Redo();

		}


	}

	public bool SaveToIso(string IsoName, byte[] icon_tex, out BSIsoData outData)
	{
		outData = null;
		if (m_SelectionBoxes.Count == 0)
			return false;

		// Only the block can save to be ISO
		if (pattern.type != EBSVoxelType.Block)
		{
			Debug.LogWarning("The iso is not support the Voxel");
			return false;
		}

		BSIsoData iso = new BSIsoData();
		iso.Init(pattern.type);
		iso.m_HeadInfo.Name = IsoName;

		BSTools.IntBox bound = BSTools.SelBox.CalculateBound(m_SelectionBoxes);

		iso.m_HeadInfo.xSize = bound.xMax - bound.xMin + 1;
		iso.m_HeadInfo.ySize = bound.yMax - bound.yMin + 1;
		iso.m_HeadInfo.zSize = bound.zMax - bound.zMin + 1;
		iso.m_HeadInfo.IconTex = icon_tex;

		foreach (BSTools.SelBox box in m_SelectionBoxes)
		{
			for (int x = box.m_Box.xMin; x <=  box.m_Box.xMax; x++ )
			{
				for (int y = box.m_Box.yMin; y <= box.m_Box.yMax; y++)
				{
					for (int z = box.m_Box.zMin; z <= box.m_Box.zMax; z++)
					{
						BSVoxel voxel = dataSource.SafeRead(x, y, z);
						int key = BSIsoData.IPosToKey(x - bound.xMin, y - bound.yMin, z - bound.zMin);
						if (!dataSource.VoxelIsZero(voxel, 1))
							iso.m_Voxels.Add(key, voxel);
					}
				}
			}
		}

		if (iso.m_Voxels.Count == 0)
			return false;

		iso.CaclCosts();

		string FilePath = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath;
		/*bool r = */SaveFile(FilePath, iso);
		
		if (SaveFile(FilePath, iso))
		{
			ClearSelection(m_Action);
			outData = iso;
			return true;
		}
		else
			return false;

	}

	bool SaveFile (string file_path, BSIsoData iso)
	{
		if (!Directory.Exists(file_path))
			Directory.CreateDirectory(file_path);
		
		file_path +=  iso.m_HeadInfo.Name + BuildingMan.s_IsoExt;

		try
		{
			using(FileStream fileStream = new FileStream(file_path, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter bw = new BinaryWriter(fileStream);
				byte[] datas = iso.Export();
				bw.Write(datas);
				bw.Close();
			}

			Debug.Log("Save building ISO successfully");
			return true;
		}
		catch (System.Exception)
		{
			//			new PeTipMsg ("Failed to create file, please check  the name", PeTipMsg.EMsgLevel.Error, PeTipMsg.EMsgType.Misc);
			return false;
		}
	}


	protected void ExtrudeSelection(int x, int y, int z)
	{
		if (x == 0 && y == 0 && z == 0)
			return;

		if (m_PrevDS != dataSource)
			return;

		_extruding = true;

		Dictionary<IntVector3, byte> new_selection = new Dictionary<IntVector3, byte>();

		//BSVoxelModify modify = new BSVoxelModify(
		List<IntVector3> indexes = new List<IntVector3>();
		List<BSVoxel>  old_voxels = new List<BSVoxel>();
		List<BSVoxel> new_voxels = new List<BSVoxel>();
		Dictionary<IntVector3, int> refVoxelMap = new Dictionary<IntVector3, int>();

		foreach (KeyValuePair<IntVector3, byte> kvp in m_Selections)
		{
			IntVector3 ipos = new IntVector3(kvp.Key);
			ipos.x += x;
			ipos.y += y;
			ipos.z += z;


			BSVoxel new_voxel = dataSource.SafeRead(kvp.Key.x, kvp.Key.y, kvp.Key.z);
			BSVoxel old_voxel = dataSource.SafeRead(ipos.x, ipos.y, ipos.z);

			indexes.Add(ipos);
			old_voxels.Add(old_voxel);
			new_voxels.Add(new_voxel);

			new_selection.Add(ipos, kvp.Value);
			refVoxelMap.Add(ipos, 0);
		}

		// Extra Extendable
		FindExtraExtendableVoxels(dataSource, new_voxels, old_voxels, indexes, refVoxelMap);



		BSVoxelModify modify = new BSVoxelModify(indexes.ToArray(), old_voxels.ToArray(), new_voxels.ToArray(), dataSource, EBSBrushMode.Add);
		m_Action.AddModify(modify);
		modify.Redo();

		Dictionary<IntVector3, byte> old_selection = new Dictionary<IntVector3, byte>(m_Selections);
		BSSelectedBoxModify sm = new BSSelectedBoxModify(old_selection, new_selection, this);
		m_Action.AddModify(sm);
		sm.Redo();

	}
}
