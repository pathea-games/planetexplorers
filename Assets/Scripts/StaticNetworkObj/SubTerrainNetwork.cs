using UnityEngine;
using System.Collections;

public class SubTerrainNetwork : SkNetworkInterface {

	static SubTerrainNetwork _instance;
	public static SubTerrainNetwork Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = GameObject.Find("SKSubTerrainNetwork");
				
				if (obj != null)
					_instance = obj.GetComponent<SubTerrainNetwork>();
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
		OnSpawned(SkEntitySubTerrain.Instance.gameObject);
		if(runner != null && runner.SkEntityPE != null)
			runner.SkEntityPE.SetNet (this);
	}
}
