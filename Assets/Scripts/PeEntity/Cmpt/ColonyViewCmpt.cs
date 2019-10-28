using System;
using UnityEngine;

namespace Pathea
{
	public class ColonyViewCmpt : ViewCmpt
	{
		CSBuildingLogic _logic;
		CSBuildingLogic logic
		{
			get
			{
				if (!_logic)
				{
					_logic = GetComponent<CSBuildingLogic>();
				}
				return _logic;
			}
		}


		/// <summary>
		/// 是否存在 View 层
		/// </summary>
		public override bool hasView
		{
			get{
				return logic.HasModel;
			}
		}


		/// <summary>
		/// 中心 Transform 对象. 子类实现时应当保证 View 层存在时返回有效的 transform 对象, 否则返回 null
		/// </summary>
		public override Transform centerTransform
		{get{
				if(!hasView)
					return null;
				else
					return logic.transform;
			}
		}

	}
}