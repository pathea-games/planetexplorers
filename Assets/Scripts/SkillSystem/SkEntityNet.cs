using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SkillSystem
{
	public class SkBeModified
	{
		public List< int > indexList = new List<int>();
		public List< float > valueList = new List<float>();
		public List< int > casterIdList = new List<int> ();
		public void Clear()
		{
			indexList.Clear ();
			valueList.Clear ();
			casterIdList.Clear ();
		}
		public bool HaveModifyData()
		{
			if(indexList.Count >0 || valueList.Count > 0 )
				return true;
			return false;
		}
	}
	
	public class SKCanLoop
	{
		public int _casterId = 0;
		public int _skillId = 0;
		public bool _bLoop = false;
		public bool _bFailedRecv = false;
		public void Reset()
		{
			_casterId = 0;
			_skillId = 0;
			_bLoop = false;
			_bFailedRecv = false;
		}
	}
	public partial class SkEntity : MonoBehaviour
	{
		#region Network
		internal NetworkInterface _net;
		SKCanLoop _skCanLoop = new SKCanLoop();
		internal SkBeModified _beModified = new SkBeModified();
		public void SetNet(NetworkInterface rpcnet, bool isSwitch = true)
		{
			_net = rpcnet;
			if (isSwitch && _attribs != null && _net is SkNetworkInterface && !((SkNetworkInterface)_net).IsStaticNet())
				_attribs.SwitchSeterToNet ((SkNetworkInterface)_net );
		}
		public int GetId()
		{
			if(_net != null)
				return _net.Id;
			return 0;
		}
		public bool IsController()
		{
			if (_net != null)
			{
				if (_net is SubTerrainNetwork || _net is VoxelTerrainNetwork)
					return true;
				return _net.hasOwnerAuth;
			}
			return false;
		}

		public bool IsStaticNet()
		{
			if(_net != null)
			{
				if(((SkNetworkInterface)_net).IsStaticNet())
					return true;
			}
			return false;
		}
		public void Kill(bool eventOff,bool bBoth = true)
		{
			SetAttribute((int)Pathea.AttribType.Hp,0f,eventOff,bBoth);
		}
		public void SendBLoop(int skId,int targetId,bool bLoop)
		{
			if( IsController() )
			{
				_net.RPCServer(EPacketType.PT_InGame_SKBLoop,skId,targetId,bLoop);
			}
		}
		public void SetBLoop(bool bLoop,int skId)
		{
			_skCanLoop._bLoop = bLoop;
			_skCanLoop._skillId = skId;
			_skCanLoop._casterId = this.GetId ();
			_skCanLoop._bFailedRecv = true;
		}
		public void SetCondRet(SkFuncInOutPara funcInOut)
		{
			if(_skCanLoop._bFailedRecv && _skCanLoop._casterId == funcInOut._inst._caster.GetId() && _skCanLoop._skillId == funcInOut._inst.SkillID)
			{
				funcInOut._ret = _skCanLoop._bLoop;
				_skCanLoop.Reset();
			}
		}
		
		void OnNetAlterAttribs(int idx, float oldValue, float newValue)
		{
//			if(IsController())
//			{
//				if(SkInst.Current != null && SkInst.Current.Caster != null && oldValue != newValue)
//					_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr,idx,newValue,SkInst.Current.Caster.GetId());
//			}
		}
		public void SendFellTree( int proType,Vector3 pos, float height,float width )
		{
			if(IsController())
			{
				_net.RPCServer(EPacketType.PT_InGame_SKFellTree,proType,pos,height,width);
			}
		}
		public void SendStartSkill(SkEntity target, int id,float[] para = null)
		{
			_skCanLoop.Reset();
			if( IsController() )
			{
				if(para != null && para.Length > 0)
				{
					if(target != null && target.GetId() != 0)
						_net.RPCServer(EPacketType.PT_InGame_SKStartSkill,id, target.GetId(),true,para);
					else
						_net.RPCServer(EPacketType.PT_InGame_SKStartSkill,id,0,true,para);
				}
				else
				{
					if(target != null && target.GetId() != 0)
						_net.RPCServer(EPacketType.PT_InGame_SKStartSkill,id, target.GetId(),false);
					else
						_net.RPCServer(EPacketType.PT_InGame_SKStartSkill,id,0,false);
				}
			}
		}

		#endregion
		#region data
		public void Import(byte[] data)
		{
			if (null == data || data.Length <= 0)
				return;
			using (MemoryStream ms = new MemoryStream(data))
			{
				using (BinaryReader _in = new BinaryReader(ms))
				{
					//int count;
					//_attribs.ResetAll();
					int iSize = _in.ReadInt32();
					for (int i = 0; i < iSize; i++)
					{
						int type = _in.ReadInt32();
						float value = _in.ReadSingle();
						SetAttribute(type,value,true, true,0);
						type = _in.ReadInt32();
						value = _in.ReadSingle();
						SetAttribute(type,value,true, false,0);
					}
					_in.Close();
				}                
				ms.Close();
			}
		}
		public byte[] Export()
		{
			byte[] va = null;
			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter _out = new BinaryWriter(ms))
				{
					int nSize = 0;
					for (int i = 0; i < attribs.raws.Count; i++)
					{
						if(GetAttribute(i,false) <= PETools.PEMath.Epsilon)
							continue;
						nSize++;
					}
					_out.Write(nSize);
					for (int i = 0; i < attribs.raws.Count; i++)
					{
						if(GetAttribute(i,false) <= PETools.PEMath.Epsilon || GetAttribute(i,true) <= PETools.PEMath.Epsilon)
							continue;
						_out.Write(i);
						_out.Write(GetAttribute(i,false));
						_out.Write(i);
						_out.Write(GetAttribute(i,true));
					}
					_out.Close();
					va = ms.ToArray();
					ms.Close();
				}
			}
			return va;
		}
		#endregion
	}
}

