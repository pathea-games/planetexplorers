using UnityEngine;
using WhiteCat;

namespace Pathea
{
	public class CreationViewCmpt : ViewCmpt
	{
		CreationController _creationController;


		public void Init(CreationController creationController)
		{
			_creationController = creationController;
		}


		/// <summary>
		/// 是否存在 View 层
		/// </summary>
		public override bool hasView
		{
			get
			{
				return _creationController.collidable;
			}
		}


		/// <summary>
		/// 中心 Transform 对象. 子类实现时应当保证 View 层存在时返回有效的 transform 对象, 否则返回 null
		/// </summary>
		public override Transform centerTransform
		{
			get
			{
				return _creationController.collidable ? _creationController.centerObject : null;
			}
		}
	}
}