using UnityEngine;
using System.Collections;

using Pathea;

public class DragItemLogicFlag : DragItemLogic
{
	public PESkEntity FlagSkEntity;
	public PeTrans FlagTrans;
	public PeEntity FlagEntity;

	private GameObject _mainGo;

	public int TeamId { get { return mNetlayer.TeamId; } }
	public int InstanceId { get { return itemDrag.itemObj.instanceId; } }
	public int protoId { get { return itemDrag.itemObj.protoId; } }

	void Awake()
	{
		FlagEntity = gameObject.AddComponent<PeEntity>();
		FlagTrans = gameObject.AddComponent<PeTrans>();
		FlagSkEntity = gameObject.AddComponent<PESkEntity>();

		FlagSkEntity.onHpChange += OnHpChange;
		FlagSkEntity.deathEvent += OnDeathEvent;
		FlagSkEntity.InitEntity();
	}

	public override void OnActivate()
	{
		base.OnActivate();

		ItemScript s = GetComponentInChildren<ItemScript>();
		if (null != s)
			s.OnActivate();
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		ItemScript s = GetComponentInChildren<ItemScript>();
		if (null != s)
			s.OnDeactivate();
	}

	public override void OnConstruct()
	{
		base.OnConstruct();

		_mainGo = itemDrag.CreateViewGameObject(null);
		if (_mainGo == null)
			return;

		FlagTrans.SetModel(_mainGo.transform);
		_mainGo.transform.parent = transform;

		_mainGo.transform.position = transform.position;
		_mainGo.transform.rotation = transform.rotation;
		_mainGo.transform.localScale = transform.localScale;

		ItemScript itemScript = _mainGo.GetComponentInChildren<ItemScript>();
		if (null != itemScript)
		{
			itemScript.SetItemObject(itemDrag.itemObj);
			itemScript.InitNetlayer(mNetlayer);
			itemScript.id = id;
			itemScript.OnConstruct();
		}
	}

	public override void OnDestruct()
	{
		ItemScript s = GetComponentInChildren<ItemScript>();
		if (null != s)
			s.OnDestruct();

		if (_mainGo != null)
			GameObject.Destroy(_mainGo);

		base.OnDestruct();
	}

	public void OnHpChange(SkillSystem.SkEntity caster, float hpChange)
	{
	}

	public void OnDeathEvent(SkillSystem.SkEntity skSelf, SkillSystem.SkEntity skCaster)
	{
	}
}
