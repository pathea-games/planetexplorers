//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Pathea;

//public abstract class CreationController : PeCmpt 
//{
//	public bool m_Active = false;
//	internal int _creationID;
//	//CreationData _creationData;
//	public CreationData m_CreationData 
//	{
//		get{
//				//if(_creationData == null)
//				//{
//					//_creationData = CreationMgr.GetCreation(_creationID);
//				//}
//			return CreationMgr.GetCreation(_creationID);
//			}
//		set
//		{
//			_creationID = value.m_ObjectID;
//		}
//	}
//	public ENetCharacter m_NetChar = ENetCharacter.nrOwner;

//	public abstract void Init(CreationData crd, int itemInstanceId);
	
//	/// <summary>
//	/// Owner's state serialization for network sync
//	/// </summary>
//	/// <returns>The owner's state.</returns>
//	public virtual byte[] GetState() { return null; }
//	/// <summary>
//	/// Apply state to a proxy CreationController
//	/// </summary>
//	/// <param name="buffer">Serialized state byte buffer</param>
//	public virtual void SetState(byte[] buffer) {}

//	protected virtual void Awake () {}
//	protected virtual void Start () {}
//	protected virtual void OnDestroy () {}
//	protected virtual void OnEnable () {}
//	protected virtual void OnDisable () {}
//	protected virtual void Update () {}
//	protected virtual void FixedUpdate () {}

//	public void LoadParts<T> (ref T member) where T : VCPartFunc
//	{
//		T[] arr = GetComponentsInChildren<T>(false);
//		if ( arr.Length > 0 )
//			member = arr[0];
//		else
//			member = null;
//		arr = null;
//	}
//	public void LoadParts<T> (ref List<T> list_member) where T : VCPartFunc
//	{
//		T[] arr = GetComponentsInChildren<T>(false);
//		list_member = new List<T> ();
//		foreach ( T t in arr )
//		{
//			list_member.Add(t);
//		}
//		arr = null;
//	}
//}
