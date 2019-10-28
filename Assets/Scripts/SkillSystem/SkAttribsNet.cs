using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;

namespace SkillSystem
{
	public partial class SkAttribs : ISkAttribs 
	{
		static List<AttribType> _sendDValue = new List<AttribType>();
		SkNetworkInterface _net;
		bool _fromNet = false;
		bool _lockModifyBySingle = true;
        int _netCaster;
        public void SetNetCaster(int caster)
        {
            _netCaster = caster;
        }
		public bool FromNet
		{
			set{
				_fromNet = value;
			}
			get
			{
				return _fromNet;
			}
		}
		public bool LockModifyBySingle
		{
			set{
				_lockModifyBySingle = value;
			}
			get
			{
				return _lockModifyBySingle;
			}
		}
		void AddToSendDValue(AttribType type)
		{
			if(!_sendDValue.Contains(type))
				_sendDValue.Add(type);
		}
		static bool IsSendDValue(AttribType type)
		{
			return _sendDValue.Contains(type);
		}
		private void RawSetterNet(int idx, float v)
		{
			NumList r = (NumList)_raws;
			float oldValue = r.Get(idx);
			float dValue = v - oldValue;
			bool bRaw = true;
			if(FromNet)
			{
				r.Set(idx, v);
				_dirties[idx] = true;
			}
			else
			{
				bool bSendDValue = false;
				if( !CheckAttrNet((AttribType)idx,oldValue,v,r,out bSendDValue))
					return;
                if(!bSendDValue)
                    v = CheckAttrMax((AttribType)idx, oldValue,  v, (NumList)_sums);
                SkEntity entity = GetModCaster(idx);
				if(_net != null && LockModifyBySingle)
				{
					//Debug.Log("attr was locked, please wait for net init data inx = "+idx +" , v = " +v);
					return;
				}
				if(!bSendDValue)
				{
					dValue = v;
					r.Set(idx, v);
					_dirties[idx] = true;
				}
				if(entity != null && _net != null)
				{
					if(entity.IsController() && !_net.IsStaticNet())
					{
						_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr,(byte)idx,dValue,entity.GetId(), bRaw,bSendDValue);
					}
				}
				else if(_net != null && _net.hasOwnerAuth)
				{
					_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx,dValue,-1,bRaw,bSendDValue);
				}
                else if (idx == 95 && _net is MapObjNetwork)
                {
                    _net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, dValue, -1, bRaw, bSendDValue);
                }
            }
		}
		private void SumSetterNet(int idx, float v)
		{
			NumList s = (NumList)_sums;
			float oldValue = s.Get(idx);
			float dValue = v - oldValue;
			bool bRaw = false;
			SkEntity entity = GetModCaster(idx);
			if(FromNet)
			{
				s.Set(idx, v);
				OnAlterNumAttribs(idx, oldValue, v);
			}
			else
			{
				bool bSendDValue = false;
				if(  !CheckAttrNet((AttribType)idx,oldValue,v,s,out bSendDValue))
					return;
                if (bSendDValue && dValue == 0)
                    return;
                if (!bSendDValue)
                    v = CheckAttrMax((AttribType)idx, oldValue, v, (NumList)_sums);
                if (_net != null && LockModifyBySingle)
				{
					//Debug.Log("attr was locked, please wait for net init data inx = "+idx +" , v = " +v);
					return;
				}
				if(!bSendDValue)
				{
					dValue = v;
					s.Set(idx, v);
					OnAlterNumAttribs(idx, oldValue, v);
				}
				if(entity != null && _net != null)
				{	if(entity.IsController() && !_net.IsStaticNet())
					{
						_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx,dValue,entity.GetId(),bRaw,bSendDValue);
					}
				}
				else if(_net != null && _net.hasOwnerAuth)
				{
					_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx,dValue,-1,bRaw,bSendDValue);
				}
                else if(idx == 95 && _net is MapObjNetwork)
                {
                    _net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, dValue, -1, bRaw, bSendDValue);
                }
			}
		}
		public void SwitchSeterToNet( SkNetworkInterface net)
		{
			_net = net;
			((NumList)_raws).Setter = (n,i,v)=>RawSetterNet(i, v);
			((NumList)_sums).Setter = (n,i,v)=>SumSetterNet(i, v);
		}
		bool CheckAttrNet( AttribType attType, float oldVal, float newVal,NumList r,out bool bSendDValue)
		{
			bSendDValue = IsSendDValue(attType);
			switch(attType)
			{
			case AttribType.Stamina:
				if(newVal < 0)
					newVal = 0;
				else if(newVal > r[(int)(AttribType.StaminaMax)])
					newVal = r[(int)(AttribType.StaminaMax)];
				if (newVal == oldVal)
					return false;
				break;
			case AttribType.Comfort:
				if(newVal < 0)
					newVal = 0;
				else if(newVal > r[(int)(AttribType.ComfortMax)])
					newVal = r[(int)(AttribType.ComfortMax)];		
				if (newVal == oldVal)
					return false;
				break;
			case AttribType.Oxygen:
				if(newVal < 0)
					newVal = 0;
				else if(newVal > r[(int)(AttribType.OxygenMax)])
					newVal = r[(int)(AttribType.OxygenMax)];	
				if (newVal == oldVal)
					return false;
				break;
			case AttribType.Hunger:
				if(newVal < 0)
					newVal = 0;
				else if(newVal > r[(int)(AttribType.HungerMax)])
					newVal = r[(int)(AttribType.HungerMax)];	
				if (newVal == oldVal)
					return false;
				break;
			case AttribType.Rigid:
				if(newVal < 0)
					newVal = 0;
				else if(newVal > r[(int)(AttribType.RigidMax)])
					newVal = r[(int)(AttribType.RigidMax)];	
				if (newVal == oldVal)
					return false;
				break;
			case AttribType.Hitfly:
				if(newVal < 0)
					newVal = 0;
				else if(newVal > r[(int)(AttribType.HitflyMax)])
					newVal = r[(int)(AttribType.HitflyMax)];		
				if (newVal == oldVal)
					return false;
				break;
			}
			return true;
		}

        float CheckAttrMax(AttribType attType, float oldVal, float newVal, NumList r)
        {            
            switch (attType)
            {
                case AttribType.Stamina:
                    if (newVal < 0)
                        newVal = 0;
                    else if (newVal > r[(int)(AttribType.StaminaMax)])
                        newVal = r[(int)(AttribType.StaminaMax)];
                    break;
                case AttribType.Comfort:
                    if (newVal < 0)
                        newVal = 0;
                    else if (newVal > r[(int)(AttribType.ComfortMax)])
                        newVal = r[(int)(AttribType.ComfortMax)];
                    break;
                case AttribType.Oxygen:
                    if (newVal < 0)
                        newVal = 0;
                    else if (newVal > r[(int)(AttribType.OxygenMax)])
                        newVal = r[(int)(AttribType.OxygenMax)];
                    break;
                case AttribType.Hunger:
                    if (newVal < 0)
                        newVal = 0;
                    else if (newVal > r[(int)(AttribType.HungerMax)])
                        newVal = r[(int)(AttribType.HungerMax)];
                    break;
                case AttribType.Rigid:
                    if (newVal < 0)
                        newVal = 0;
                    else if (newVal > r[(int)(AttribType.RigidMax)])
                        newVal = r[(int)(AttribType.RigidMax)];
                    break;
                case AttribType.Hitfly:
                    if (newVal < 0)
                        newVal = 0;
                    else if (newVal > r[(int)(AttribType.HitflyMax)])
                        newVal = r[(int)(AttribType.HitflyMax)];
                    break;
            }
            return newVal;
        }
    }
}

