using UnityEngine;
using System.Collections;

namespace Pathea
{
	public class PEPortal : MonoBehaviour 
	{
		public Vector3 transPoint;

		public int descriptionID;

		void OnTriggerEnter(Collider other)
		{
			PeEntity entity = other.GetComponentInParent<PeEntity>();
			if(null != entity && entity == MainPlayer.Instance.entity)
				MessageBox_N.ShowYNBox(PELocalization.GetString(descriptionID), Do);
		}

		void Do()
		{
			if(PeGameMgr.IsMulti)
			{
				if (null != PlayerNetwork.mainPlayer)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerReset, transPoint);
					PlayerNetwork.mainPlayer.RequestChangeScene(0);
				}
			}
			else
				FastTravelMgr.Instance.TravelTo(transPoint);
		}
	}
}
