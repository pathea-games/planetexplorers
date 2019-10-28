using System;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 对象回收器
	/// </summary>
	public class ObjectRecycler : BaseBehaviour
	{
		[NonSerialized] public ObjectPool objectPool;
	}
}