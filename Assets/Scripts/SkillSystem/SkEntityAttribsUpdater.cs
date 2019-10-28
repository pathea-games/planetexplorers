using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkillSystem
{
	public class SkEntityAttribsUpdater : Pathea.MonoLikeSingleton<SkEntityAttribsUpdater>
	{
		const int UpdateFrameInterval = 8; // temporally 8 used; it can be increased for the sake of performance
		int _prevFrmCnt = 0;
		List<SkEntity> _reqEntities = new List<SkEntity>();

		public override void Update ()
		{
			if(Time.frameCount < _prevFrmCnt + UpdateFrameInterval)
				return;

			int n = _reqEntities.Count;
			for (int i = n-1; i >= 0; i--) {
				if(_reqEntities[i] == null || _reqEntities[i].Equals(null)){
					_reqEntities.RemoveAt(i);
				} else {
					_reqEntities[i].UpdateAttribs();
				}
			}
			_prevFrmCnt = Time.frameCount;
		}

		public void Register(SkEntity entity)
		{
			//entity.InvokeRepeating ("UpdateAttribs", 0, 0.1f);
			_reqEntities.Add (entity);
		}
		public void Unregister(SkEntity entity)
		{
			_reqEntities.Remove (entity);
		}
	}
}
