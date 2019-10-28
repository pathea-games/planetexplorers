using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 对象池
	/// </summary>
	[AddComponentMenu("White Cat/Object Pool")]
	public class ObjectPool : BaseBehaviour
	{
		[SerializeField][Editable(true, false)] GameObject _original;	// 复制源对象
		[SerializeField][Editable(true, false)] Transform _parent;		// 对象父级
		[SerializeField][Editable(true, false)] int _quantity;			// 对象数量

		[NonSerialized] List<GameObject> _list;

		
		/// <summary>
		/// 数量
		/// </summary>
		public int quantity { get { return _quantity; } }


		/// <summary>
		/// 添加对象
		/// </summary>
		/// <param name="quantity">添加的对象数量</param>
		public void AddObjects(int quantity)
		{
			GameObject go;
			while (quantity > 0)
			{
				go = Instantiate(_original) as GameObject;
				go.SetActive(false);
				go.transform.parent = _parent;
				go.AddComponent<ObjectRecycler>().objectPool = this;
				_list.Add(go);
				quantity--;
			}
			_quantity = _list.Count;
		}


		/// <summary>
		/// 取出一个对象
		/// </summary>
		/// <returns>取出的对象</returns>
		public GameObject TakeOut()
		{
			if (_quantity == 0)
			{
				AddObjects(1);
				Debug.Log("The ObjectPool is empty, a new GameObject had been created immediately.");
			}

			GameObject go = _list[_quantity - 1];
			_list.RemoveAt(_quantity - 1);
			_quantity--;

			go.SetActive(true);
			return go;
		}


		/// <summary>
		/// 回收游戏对象
		/// </summary>
		/// <param name="gameObject">要执行回收的对象</param>
		public static void Recycle(GameObject go)
		{
			ObjectPool objectPool = go.GetComponent<ObjectRecycler>().objectPool;
			go.SetActive(false);
			go.transform.parent = objectPool._parent;
			objectPool._quantity++;
			objectPool._list.Add(go);
		}


		// 初始化
		void Awake()
		{
			_list = new List<GameObject>(_quantity < 1 ? 4 : _quantity);
			AddObjects(_quantity);
		}
	}

}