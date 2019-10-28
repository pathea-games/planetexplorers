using UnityEngine;
using System.Collections;

public class VoxelTerrainNetwork : SkNetworkInterface {

	static VoxelTerrainNetwork _instance;
	public static VoxelTerrainNetwork Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = GameObject.Find("SKVoxelTerrainNetwork");
				
				if (obj != null)
					_instance = obj.GetComponent<VoxelTerrainNetwork>();
				else
					return null;
			}
			return _instance;
		}
	}
	protected override void OnPEAwake()
	{
		_instance = this;
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();

		BindSkAction();
	}

	public void Init()
	{
		_id = OwnerView.viewID.id;
		OnSpawned(VFVoxelTerrain.self.gameObject);
		if(runner != null && runner.SkEntityBase != null)
		{
            runner.SkEntityBase.SetAttribute((int)Pathea.AttribType.DefaultPlayerID, 10f);
		}
	}
}
