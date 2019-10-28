using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class NetCmpt : PeCmpt, IPeMsg
    {
        public NetworkInterface network;

        public bool IsPlayer { get { return network is PlayerNetwork; } }
		public bool IsController { get { return null == network ? false : network.hasOwnerAuth; } }

        public override void Awake()
        {
            base.Awake();

            Entity.netCmpt = this;
        }

        public void OnMsg(EMsg msg, params object[] args)
        {
			network.OnPeMsg(msg, args);
		}

		public void SetController(bool isController)
		{
			if (isController)
			{
				Entity.SendMsg(EMsg.Net_Controller);
			}
			else
			{
				Entity.SendMsg(EMsg.Net_Proxy);
			}
		}

		public void RequestUseItem(int objId)
		{
			if (network is PlayerNetwork)
			{
				PlayerNetwork player = (PlayerNetwork)network;
				player.RequestUseItem(objId);
			}
			else if (network is AiAdNpcNetwork)
			{
				AiAdNpcNetwork npc = (AiAdNpcNetwork)network;
				npc.RequestNpcUseItem(objId);
			}
		}
    }

    namespace PeEntityExtNet
    {
        public static class PeEntityExtNet
        {
			public static NetworkInterface GetNetwork(this PeEntity entity)
            {
                if (null == entity)
                {
                    return null;
                }

                NetCmpt net = entity.GetCmpt<NetCmpt>();

                if (null == net)
                {
                    return null;
                }

                return net.network;
            }
        }
    }
}