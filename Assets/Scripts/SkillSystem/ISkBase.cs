using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Put these 2 type out of namespace in order to facinate exp compilation
public class PackBase
{
	protected virtual void PushIn(params int[] ids){}
	protected virtual void PopOut(params int[] ids){}
	public static PackBase operator+(PackBase pack, int id)
	{
		pack.PushIn(id);
		return pack;
	}
	public static PackBase operator-(PackBase pack, int id)
	{
		pack.PopOut(id);
		return pack;
	}
	
	public static PackBase operator+(PackBase pack, int[] ids)
	{
		pack.PushIn(ids);
		return pack;
	}
	public static PackBase operator-(PackBase pack, int[] ids)
	{
		pack.PopOut(ids);
		return pack;
	}
}
public interface ISkEntity
{
	ISkAttribs attribs{ get; }
}
public interface IExpFuncSet	// Extendable
{
	bool RandIn(float prob);
    void GetHurt(float dmg);
    void TryGetHurt(float dmg, float exp = 0);
	bool InCoolingForGettingHurt(float coolTime);
	bool InCoolingForConsumingStamina(float cooltime);
}
public interface ISkAttribs
{
	ISkEntity		entity{get;}
	IExpFuncSet		expFunc{get;}

	IList<float>	raws{get;}
	IList<float>	sums{get;}
	IList<bool>		modflags{get;}
	PackBase   	 	pack{get;set;}
	float			buffMul{get;set;}
	float			buffPreAdd{get;set;}
	float			buffPostAdd{get;set;}
}

public interface ISkPara{}
public interface ISkAttribsModPara : ISkPara, IList<float>{}
public interface ISkParaNet : ISkPara
{
	int TypeId{ get; }
	float[] ToFloatArray();
	void FromFloatArray(float[] data);
}

namespace SkillSystem
{
	public class NumList : IList<float>
	{
		float[] _data;
		Action<NumList, int, float> _setter;
		public NumList(int n, Action<NumList, int, float> setter = null)
		{
			_data = new float[n];	
			_setter = setter == null ? DefSetter : setter;
		}
		public Action<NumList, int, float> Setter{ 	get{return _setter; } set{ _setter = value; } }
		public static readonly Action<NumList, int, float> DefSetter = (n,i,v)=>n.Set(i, v);
		// Virtuals
		public virtual float Get(int idx){			return _data[idx];		}
		public virtual void Set(int idx, float v){	_data[idx] = v;			}
		// Interface
		public int Count{ 			get{ 			return _data.Length;	}	}
		public bool IsReadOnly{ 	get{ 			return _data.IsReadOnly;}	}
		public float this[int i]{	get{ 			return Get(i);			}	
									set{ 			_setter(this, i, value);}	}
		public void Add(float v){					throw new NotSupportedException(("NotSupported_FixedSizeCollection"));		}
		public void Clear(){						throw new NotSupportedException(("NotSupported_ReadOnlyCollection"));		}
		public void Insert(int index,float v){		throw new NotSupportedException(("NotSupported_FixedSizeCollection"));		}
		public bool Remove(float v) {				throw new NotSupportedException(("NotSupported_FixedSizeCollection"));		}		
		public void RemoveAt(int index) {			throw new NotSupportedException(("NotSupported_FixedSizeCollection"));		}
		public void CopyTo(float[] a, int index){	_data.CopyTo(a, index);							}
		public bool Contains(float v){				return Array.IndexOf(_data, v) != -1;			}
		public int IndexOf(float v){				return Array.IndexOf(_data, v);					}
		public IEnumerator GetEnumerator(){			return _data.GetEnumerator();					}
		IEnumerator<float> IEnumerable<float>.GetEnumerator(){	return (IEnumerator<float>)_data.GetEnumerator();}
	}
	public class NumListWithParent : NumList
	{
		NumList _parent;
		bool[] _useParentMasks;
		public NumListWithParent(NumList parent, bool[] useParentMasks, int cnt, Action<NumList, int, float> setter = null) : base(cnt, setter)
		{
			_parent = parent;		
			_useParentMasks = useParentMasks;
		}
		public override float Get(int idx){			return _useParentMasks[idx] ? _parent.Get(idx) : base.Get(idx);	}
		public override void Set(int idx, float val)
		{
			if(_useParentMasks[idx])
			{
				_parent.Set(idx, val);
			}
			else
			{
				base.Set (idx, val);
			}
		}
	}
}

