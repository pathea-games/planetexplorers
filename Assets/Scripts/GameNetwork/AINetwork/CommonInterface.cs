using UnityEngine;
using System.Collections;
using SkillAsset;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
public abstract class CommonInterface : MonoBehaviour, ISkillTarget
{
	#region ISkillTarget
	public virtual ESkillTargetType GetTargetType() { return ESkillTargetType.TYPE_SkillRunner; }
	public virtual Vector3 GetPosition() { return Vector3.zero; }
	#endregion

	#region Netlayer

	private NetworkInterface netlayer;

	internal bool IsMultiMine { get { return null == Netlayer ? false : Netlayer.IsOwner; } }
	internal bool IsMultiProxy { get { return null == Netlayer ? false : Netlayer.IsProxy; } }

	public virtual bool IsOwner { get { return null == Netlayer ? false : Netlayer.IsOwner; } }

	public virtual bool IsController { get { return null == Netlayer ? false : Netlayer.hasOwnerAuth; } }

	public NetworkInterface Netlayer { get { return netlayer; } }

	public int TeamId { get { return null == Netlayer ? 0 : Netlayer.TeamId; } }

	internal virtual uLink.NetworkView OwnerView { get { return null == Netlayer ? null : Netlayer.OwnerView; } }
	SkAliveEntity _skEntityPE = null;
	public SkAliveEntity SkEntityPE { get { return _skEntityPE; } }
	SkEntity _skEntity = null;
	public SkEntity SkEntityBase { get { return _skEntity; } }
	static Dictionary<int, CommonInterface> _comMgr = new Dictionary<int, CommonInterface>();
	public static Dictionary<int, CommonInterface> ComMgr { get { return _comMgr; } }
	internal virtual void InitNetworkLayer(NetworkInterface network, GameObject obj = null)
	{
		if (obj == null)
			obj = gameObject;

		netlayer = network;

		if (null == network)
			return;

		_comMgr[network.Id] = this;

		if (obj != null)
		{
			_skEntity = obj.GetComponent<SkEntity>();
			if (_skEntity != null)
				_skEntity.SetNet(network);
			_skEntityPE = obj.GetComponent<SkAliveEntity>();
		}
	}
	public static CommonInterface GetComByNetID(int id)
	{
		if (_comMgr.ContainsKey(id))
			return _comMgr[id];
		return null;
	}
	internal virtual void RPCServer(EPacketType type, params object[] objs)
	{
		if (null != Netlayer)
			Netlayer.RPCServer(type, objs);
	}

	internal virtual void URPCServer(EPacketType type, params object[] objs)
	{
		if (null != Netlayer)
			Netlayer.URPCServer(type, objs);
	}

	public virtual void NetworkApplyDamage(CommonInterface caster, float damage, int lifeLeft) { }
	public virtual void NetworkAiDeath(CommonInterface caster) { }
	#endregion
}
