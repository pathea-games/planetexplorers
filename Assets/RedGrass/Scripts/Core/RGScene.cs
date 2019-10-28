
//#define DRAW_TAIL_NODE_IN_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea.Maths;

namespace RedGrass
{
	public partial class RGScene : MonoBehaviour 
	{
		public RGDataSource data { get; private set; }

		INTVECTOR2 mCenter = new INTVECTOR2(-10000, -10000);
		//INTVECTOR2 mSafeCenter = new INTVECTOR2(-10000, -10000);

		public EvniAsset evniAsset;

		public Transform tracedObj;

		public ChunkDataIO dataIO;

		public bool refreshNow;

		public bool cullObjs = true;


		public void Init ()
		{
			RedGrassInstance.Init();

			data = new RGDataSource();
			data.Init(evniAsset, dataIO);

			RGPoolSig.Init();

			// Render Part Init
			mTree = new RGLODQuadTree(evniAsset);
		}

		public void Free ()
		{
			if (data != null)
			{
				data.Free();
				data = null;
			}

			RGPoolSig.Destroy();
		}

		INTVECTOR2 _curCenter;
		bool DefaultRefreshDataDetect ()
		{
			bool result = false;
			Transform trans = tracedObj == null ? PETools.PEUtil.MainCamTransform : tracedObj;

			Vector3 pos = trans.position;
			int cur_chunk_x = Mathf.FloorToInt(pos.x) >> evniAsset.SHIFT;
			int cur_chunk_z = Mathf.FloorToInt(pos.z) >> evniAsset.SHIFT;

			if (mCenter.x != cur_chunk_x || mCenter.y != cur_chunk_z)
			{
				result = true;
				_curCenter.x = cur_chunk_x;
				_curCenter.y = cur_chunk_z;
			}

			if (result)
			{
				if (dataIO.isActive())
				{
					dataIO.StopProcess();
				}
			}

			return result;

		}

		void UpdateDataSource()
		{
			IRefreshDataDetect detect = dataIO as IRefreshDataDetect;

			// No detect part? then the default is used.
			if (detect == null)
			{
				if (DefaultRefreshDataDetect())
				{
					data.ReqsUpdate(_curCenter.x, _curCenter.y );

					mCenter.x = _curCenter.x;
					mCenter.y = _curCenter.y;
				}
			}
			else
			{
				if (detect.CanRefresh(this))
				{
					Transform trans = tracedObj == null ? PETools.PEUtil.MainCamTransform : tracedObj;
					
					Vector3 pos = trans.position;
					int cur_chunk_x = Mathf.FloorToInt(pos.x) >> evniAsset.SHIFT;
					int cur_chunk_z = Mathf.FloorToInt(pos.z) >> evniAsset.SHIFT;



					data.ReqsUpdate(cur_chunk_x, cur_chunk_z );
					mCenter.x = cur_chunk_x;
					mCenter.y = cur_chunk_z;
				}
			}

		}

		INTBOUNDS3 GetSafeBound (int center_cx, int center_cz)
		{
			int min_cx = evniAsset.XStart >> evniAsset.SHIFT;
			int min_cz = evniAsset.ZStart >> evniAsset.SHIFT;
			int max_cx = evniAsset.XEnd >> evniAsset.SHIFT;
			int max_cz = evniAsset.ZEnd >> evniAsset.SHIFT;

			int expand = evniAsset.DataExpandNum;
			min_cx = Mathf.Max(min_cx, center_cx - expand);
			min_cz = Mathf.Max(min_cz, center_cz - expand);
			max_cx = Mathf.Min(max_cx, center_cx + expand);
			max_cz = Mathf.Min(max_cz, center_cz + expand);

			return  new INTBOUNDS3(new INTVECTOR3(min_cx, 0, min_cz), new INTVECTOR3(max_cx, 0, max_cz));

		}

		public void RefreshImmediately ()
		{
			Transform trans = tracedObj == null ? PETools.PEUtil.MainCamTransform : tracedObj;
			
			Vector3 pos = trans.position;
			int cur_chunk_x = Mathf.FloorToInt(pos.x) >> evniAsset.SHIFT;
			int cur_chunk_z = Mathf.FloorToInt(pos.z) >> evniAsset.SHIFT;
			data.RefreshImmediately(cur_chunk_x, cur_chunk_z);
			
			mCenter = new INTVECTOR2(cur_chunk_x, cur_chunk_z);
			
			UpdateMeshImmediately();
		}

		#region UNITY_INNER_FUNC

		void Awake ()
		{
			Init();

			Init_Render();
		}

		void OnDestroy()
		{
			Free();
		}

		void Start ()
		{
			UpdateDataSource();
		
		}


		void Update ()
		{
			Profiler.BeginSample ("Refresh Dirty Reqs");
			RefreshDirtyReqs();
			Profiler.EndSample (); 

			Profiler.BeginSample ("Update DataSource");
			UpdateDataSource();

			if (!data.reqsOutput.IsEmpty() && !data.IsProcessReqs)
			{
				data.SumbmitReqs(mCenter);

				mUpdateMesh = true;

			}
			Profiler.EndSample ();

			Profiler.BeginSample ("Update Mesh");
			UpdateMesh();
			Profiler.EndSample (); 



			#if DRAW_TAIL_NODE_IN_EDITOR
			Profiler.BeginSample ("Draw Area");
			DrawAreaInEditor();
			Profiler.EndSample();
			#endif

			if (cullObjs)
				CullObject();

			if (refreshNow)
			{
				RefreshImmediately();
				refreshNow = false;
			}

		}

		#endregion
	}
}
