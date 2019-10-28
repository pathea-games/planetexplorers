using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class BSIsoBrush : BSBrush 
{
    public static System.Action onBrushDo;

	public string File_Name = "No Name";

	private BIsoCursor m_Cursor;

	public bool Gen = false;

	private BSMath.DrawTarget m_Target;
	private int m_Rot;

	public delegate void VoidEvent();
	public event VoidEvent onCancelClick;

	// For History
	private List<BSVoxel> m_NewVoxel = new List<BSVoxel>();
	private List<IntVector3> m_Indexes = new List<IntVector3>();
	private List<BSVoxel> m_OldVoxel = new List<BSVoxel>();
	private Dictionary<IntVector3, int>  m_VoxelMap = new Dictionary<IntVector3, int>();

	void OnDestroy ()
	{
		if (m_Cursor != null)
		{
			Destroy(m_Cursor.gameObject);
			m_Cursor = null;
		}
	}

	void Update ()
	{
        if (GameConfig.IsInVCE)
            return;

		if (Input.GetKeyDown(KeyCode.O))
			Gen = true;
		
		if (Gen)
		{
			if (m_Cursor != null)
			{
				Destroy(m_Cursor.gameObject);
			}
			m_Cursor = BIsoCursor.CreateIsoCursor(GameConfig.GetUserDataPath() + BSSaveIsoBrush.s_IsoPath + File_Name + BSSaveIsoBrush.s_Ext);
			m_Rot = 0;
			Gen = false;
		}

		if (m_Cursor == null)
			return;

		if (BSInput.s_Cancel)
		{
			Cancel();
			if (onCancelClick != null)
				onCancelClick();
			return;
		}

		if (m_Cursor != null)
			m_Cursor.gameObject.SetActive(true);

		if (m_Cursor.ISO.m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			if (BSMath.RayCastDrawTarget(BSInput.s_PickRay, BuildingMan.Blocks, out m_Target, BSMath.MC_ISO_VALUE,  true,  BuildingMan.Datas))
			{
				m_Cursor.gameObject.SetActive(true);

				Vector3 offset = Vector3.zero;
				offset.x =  (m_Cursor.ISO.m_HeadInfo.xSize ) / 2  * BSBlock45Data.s_Scale;
				offset.z =  (m_Cursor.ISO.m_HeadInfo.zSize )/ 2  * BSBlock45Data.s_Scale;
				
				
				if (Input.GetKeyDown(KeyCode.T))
				{
					m_Rot = ++m_Rot > 3 ? 0 : m_Rot;
				}
				
				
				m_Cursor.SetOriginOffset(-offset);
				m_Cursor.transform.rotation = Quaternion.Euler(0, 90* m_Rot, 0);
				
				// Adjust Offset
				FocusAjust();
				
				m_Cursor.transform.position = m_Target.cursor + m_FocusOffset * BSBlock45Data.s_Scale;

				if (Input.GetMouseButtonDown(0))
				{
					if (BSInput.s_MouseOnUI)
						return;

					if (m_Cursor.gizmoTrigger.RayCast)
						return;

					// For History
					m_OldVoxel.Clear();
					m_NewVoxel.Clear();
					m_Indexes.Clear();
					m_VoxelMap.Clear();
					
					m_Cursor.OutputVoxels(Vector3.zero, OnOutputBlocks);

					// Extra Extendable
					FindExtraExtendableVoxels(dataSource, m_NewVoxel, m_OldVoxel, m_Indexes, m_VoxelMap);
					
					// For History
					BSAction action = new BSAction();
					BSVoxelModify modify = new BSVoxelModify(m_Indexes.ToArray(), m_OldVoxel.ToArray(), m_NewVoxel.ToArray(), BuildingMan.Blocks, EBSBrushMode.Add); 
					if ( !modify.IsNull())
					{
						action.AddModify(modify);
					}

                    if (action.Do())
                    {
                        BSHistory.AddAction(action);
                        if (onBrushDo != null)
                            onBrushDo();
                    }
				}
			}
			else
				m_Cursor.gameObject.SetActive(false);
		}
		else if (m_Cursor.ISO.m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			if (BSMath.RayCastDrawTarget(BSInput.s_PickRay, BuildingMan.Voxels, out m_Target, 1, true, BuildingMan.Datas))
			{
				m_Cursor.gameObject.SetActive(false);
				Debug.Log("Draw building Iso dont support the voxel");
				return;
			}
			else
				m_Cursor.gameObject.SetActive(false);
		}

	}

	private void OnOutputBlocks(Dictionary<int, BSVoxel> voxels, Vector3 originalPos)
	{
		List<IntVector3> posLst = new List<IntVector3>();
		List<B45Block> blockLst = new List<B45Block>();
		foreach ( KeyValuePair<int, BSVoxel> kvp in voxels )
		{
			posLst.Add(BSIsoData.KeyToIPos(kvp.Key));
			blockLst.Add(kvp.Value.ToBlock());
		}
		B45Block.RepositionBlocks(posLst, blockLst, m_Rot, originalPos);

		int n = posLst.Count;
		for(int i = 0; i < n; i++)
		{
			IntVector3 inpos = posLst[i];
			m_Indexes.Add(inpos);
			m_OldVoxel.Add(BuildingMan.Blocks.SafeRead(inpos.x, inpos.y, inpos.z));
			m_NewVoxel.Add(new BSVoxel(blockLst[i]));
			m_VoxelMap[inpos] = 0;
		}
	}
	/*
	private void OnOutputBlocks (float x, float y, float z, Vector3 original_pos, BSVoxel voxel)
	{
		// rote if need
		Vector3 offset = Quaternion.Euler(0, 90* m_Rot, 0) * new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
		IntVector3 inpos = new IntVector3(Mathf.FloorToInt( (original_pos.x ) * BSBlock45Data.s_ScaleInverted) + Mathf.FloorToInt(offset.x),
		                                  Mathf.FloorToInt( (original_pos.y ) * BSBlock45Data.s_ScaleInverted) + Mathf.FloorToInt(offset.y),
		                                  Mathf.FloorToInt( (original_pos.z ) * BSBlock45Data.s_ScaleInverted) + Mathf.FloorToInt(offset.z) );

		voxel = new BSVoxel( B45Block.TurnTo(voxel.ToBlock(), ((voxel.blockType & 0X3) + m_Rot) % 4));

		m_Indexes.Add(inpos);
		m_OldVoxel.Add(BuildingMan.Blocks.SafeRead(inpos.x, inpos.y, inpos.z));
		m_NewVoxel.Add(voxel);
	}
	*/

	protected override void Do ()
	{
	
	}


	public override void Cancel ()
	{
		if (m_Cursor != null)
		{
			Destroy(m_Cursor.gameObject);
		}
	}
	

	private Vector3 _prveMousePos = Vector3.zero;
	private Vector3 m_FocusOffset;
	void FocusAjust ()
	{
		if (_prveMousePos == Input.mousePosition)
		{
			if (BSInput.s_Up)
				m_FocusOffset += Vector3.up;
			else if (BSInput.s_Down)
				m_FocusOffset -= Vector3.up;
			else if (BSInput.s_Right)
				m_FocusOffset += Vector3.right;
			else if (BSInput.s_Left)
				m_FocusOffset += Vector3.left;
			else if (BSInput.s_Forward)
				m_FocusOffset += Vector3.forward;
			else if (BSInput.s_Back)
				m_FocusOffset += Vector3.back;
			
		}
		else
		{
			_prveMousePos = Input.mousePosition;
			m_FocusOffset.x = 0;
			m_FocusOffset.y = 0;
			m_FocusOffset.z = 0;
		}
	}
}
