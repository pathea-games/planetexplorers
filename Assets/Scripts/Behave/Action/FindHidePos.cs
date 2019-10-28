using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	public class FindHidePos  
	{
		Transform _player;
		//float _radius;

		static float PLAYER_R = 3.0f;
		//static  float ENEMY_R = 10.0f;
		static float K_R = 0.1f;
		static float K_R1 = 30.0f;

		private bool _bNeedHide = false;
		public bool bNeedHide {get{return _bNeedHide;}}
		private float _mEnemyR;
		public FindHidePos(float radius,bool needHide ,float enemyR = 10.0f)
		{
			//_radius = radius;
			_bNeedHide = needHide;
			_mEnemyR = enemyR;
		}

		public Vector3 GetHideDir(Vector3 _playerPos,Vector3 npcPos,List<Enemy> hideEntities)
		{
			Vector3 mDir = Vector3.zero;
			Vector3 playerDir =_playerPos - npcPos;
			_bNeedHide = false;

			bool bhideEnmy = false;
			bool bawayPlayer = false;
			for(int i=0; i < hideEntities.Count;i++)
			{
				Vector3 temDir;
                Vector3 enemyPos = hideEntities[i].position;
                enemyPos.y = npcPos.y;
                temDir = npcPos - enemyPos;
				if(temDir.magnitude < _mEnemyR)
				{
					bhideEnmy = true;
					mDir = mDir + _mEnemyR/temDir.magnitude * temDir.normalized;
				}
				
			}
			if(playerDir.magnitude < PLAYER_R)
				bawayPlayer = true;

			if(bawayPlayer)
			{
				_bNeedHide = true;
                mDir = mDir + PLAYER_R / playerDir.magnitude * (npcPos - _playerPos).normalized;
				return mDir;
			}

			if(!bhideEnmy)
			{
				_bNeedHide = false;
				return Vector3.zero;
			}

			_bNeedHide = true;
			if(playerDir.magnitude > PLAYER_R + K_R1)
				mDir = mDir + K_R * playerDir.magnitude * playerDir.normalized;

			return mDir;
		}
	}

}
