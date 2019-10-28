using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkillSystem
{
	public enum ESkParaType
	{
		UseItemPara,
		CarrierCanonPara,
		CarrierCollisionPara,
		ShootTarget
	}
	public static class SkParaNet
	{
		public static float[] ToFloatArray(ISkParaNet obj)
		{
			return obj.ToFloatArray();
		}
		public static ISkParaNet FromFloatArray(float[] data)
		{
			int tid = (int)data[0];
			ISkParaNet para = null;
			switch(tid)
			{
			case (int)ESkParaType.UseItemPara:
				para = new SkUseItemPara();
				para.FromFloatArray(data);
				break;
			case (int)ESkParaType.CarrierCanonPara:
				para = new SkCarrierCanonPara();
				para.FromFloatArray(data);
				break;
			case (int)ESkParaType.CarrierCollisionPara:
				para = new SkCarrierCollisionPara();
				para.FromFloatArray(data);
				break;
			case (int)ESkParaType.ShootTarget:
				para = new ShootTargetPara();
				para.FromFloatArray(data);
				break;
			}
			return para;
		}
	}
	public class SkUseItemPara : ISkAttribsModPara, ISkParaNet
	{
		float[] _data;
		public SkUseItemPara(){}
		public SkUseItemPara(List<int> idxList, List<float> valList)
		{
			_data = new float[idxList.Max()+1];
			int cnt = idxList.Count;
			for(int j = 0; j< cnt; j++)
			{
				int idx = idxList[j];
				_data[idx] = valList[j];
			}
		}
		// Interface
		public int Count{ 			get{ 			return _data.Length;	}	}
		public bool IsReadOnly{ 	get{ 			return _data.IsReadOnly;}	}
		public float this[int i]{	get{ 			return _data[i];			}	
									set{ 			_data[i] = value;		}	}
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

		public int TypeId{ get{ return (int)ESkParaType.UseItemPara; } }
		public float[] ToFloatArray()
		{
			int len = _data.Length;
			float[] data = new float[len+1];
			data[0] = TypeId + 0.1f;
			Array.Copy(_data, 0, data, 1, len);
			return data;
		}
		public void FromFloatArray(float[] data)
		{
			int len = data.Length - 1;
			_data = new float[len];
			Array.Copy(data, 1, _data, 0, len);
		}
	}



	public class SkCarrierCanonPara : ISkParaNet
	{
		public int _idxCanon;
		public SkCarrierCanonPara(){}
		public SkCarrierCanonPara(int idx){ _idxCanon = idx; }

		public int TypeId{ get{ return (int)ESkParaType.CarrierCanonPara; } }
		public float[] ToFloatArray()
		{
			float[] data = new float[2];
			data[0] = TypeId + 0.1f;
			data[1] = _idxCanon + 0.1f;
			return data;
		}
		public void FromFloatArray(float[] data)
		{
			_idxCanon = (int)data[1];
		}
	}



	public class SkCarrierCollisionPara : ISkParaNet, ISkAttribsModPara
	{
		float[] _data;

		public SkCarrierCollisionPara(float momentum) { _data=new float[2]{(int)ESkParaType.CarrierCollisionPara + 0.1f, momentum}; }
		public SkCarrierCollisionPara() {  }

		public int TypeId { get { return (int)ESkParaType.CarrierCollisionPara; } }

		public float[] ToFloatArray() { return _data; }

		public void FromFloatArray(float[] data) { _data = data; }


		// Interface
		public int Count{ 			get{ 			return _data.Length;	}	}
		public bool IsReadOnly{ 	get{ 			return _data.IsReadOnly;}	}
		public float this[int i]{	get{ 			return _data[i];			}	
			set{ 			_data[i] = value;		}	}
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



	public class ShootTargetPara : ISkParaNet
	{
		public Vector3 m_TargetPos;
		public ShootTargetPara() { m_TargetPos = Vector3.zero; }

		#region ISkParaNet implementation

		public int TypeId{ get{ return (int)ESkParaType.ShootTarget; } }

		public float[] ToFloatArray ()
		{
			float[] data = new float[4];
			data[0] = TypeId + 0.1f;
			data[1] = m_TargetPos.x;
			data[2] = m_TargetPos.y;
			data[3] = m_TargetPos.z;
			return data;
		}

		public void FromFloatArray (float[] data)
		{
			m_TargetPos.x = data[1];
			m_TargetPos.y = data[2];
			m_TargetPos.z = data[3];
		}
		#endregion
	}
}
