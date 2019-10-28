using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System;

namespace CustomData
{
	[AddComponentMenu("Pathea Network/CustomData")]

	#region Enums
	public enum EMoneyType
	{
		Null = 0,
		Meat,
		Digital,
	}
	public enum ENetworkState
	{
		Null = 0,
		Loading,
		Ready,
		Gameing,
	}

	public enum ServerLength
	{
		AccountLength = 32,
		PasswordLength = 32,
		EmailLength = 64,
		MaxRoleNum = 6
	}

	public enum ServerCreateType
	{
		LocalCreated = 1,
		ServerCreated
	}

	public enum EServerStatus
	{
        None = 0,
        Prepared = 1,
        Initialized = 2,
        Gameing = 4,
    }

	public enum EMsgType
	{
		ToAll = 0,
		ToOne,
		ToTeam,
		ToGuild,
        ToMe,
	}

	public enum ETerrainOperatorType
	{
		TERRAINOP_BUILD,
		TERRAINOP_DIG
	}

	public enum TDType : int
	{
		TASK = 0,
		DOTA,
		TIMER
	}

	public enum ESceneObjType : byte
	{
		NONE = 0,
		ITEM,
		DOODAD,
		EFFECT,
		DROPITEM
	}
    public enum EItemClass
    {
        MONSTERBEACON = 19,

        FURNITURECOMMON = 20,
        FURNITURE_BED = 21,
        FURNITURE_CHAIR = 22,
        FURNITURE_RAMP = 23,
        FURNITURE_DOOR = 24,

        TOWER_FOUR = 25,
        TOWER = 26,

        COLONY_CORE = 52,

        MOTOBIKE = 62,
        CAR = 63,
        CART = 64,
        SHIPS = 65,
        ROBOT = 86,
        AITURRET = 87,
        ENERGYTOWER = 88,

        AIRPLANE = 66,
        AIRCRAFT = 67,

        FLAGS = 72,
        PLANT_SEED = 73
    }
    #endregion

    #region User Custom Data
    public class RoleInfo
	{
		public ulong steamId = 0;
		public int roleID = -1;
		public int winrate = 0;
		public int deletedFlag = 0;
		public byte level = 1;
		public byte sex = 1;

		public byte[] appearData;
		public byte[] nudeData;

		public string name = "NickName";
		public float lobbyExp;
		public override string ToString()
		{
			return string.Format("[RoleInfo][name:{0}, sex:{1}, level:{2}]", name, sex, level);
		}

		public static void WriteRoleInfo(uLink.BitStream stream, object obj, params object[] codecOptions)
		{
			RoleInfo info = (RoleInfo)obj;
			stream.Write<ulong>(info.steamId);
			stream.Write<byte>(info.level);
			stream.Write<int>(info.winrate);
			stream.Write<byte>(info.sex);
            stream.Write<string>(info.name);
            stream.Write<int>(info.deletedFlag);
			stream.Write<int>(info.roleID);
			stream.Write<byte[]>(info.appearData);
			stream.Write<byte[]>(info.nudeData);
			stream.Write<float>(info.lobbyExp);
		}

		public static object ReadRoleInfo(uLink.BitStream stream, params object[] codecOptions)
		{
			RoleInfo info = new RoleInfo();
			info.steamId = stream.Read<ulong>();
			info.level = stream.Read<byte>();
			info.winrate = stream.Read<int>();
			info.sex = stream.Read<byte>();
			info.name = stream.Read<string>();
            info.deletedFlag = stream.Read<int>();
			info.roleID = stream.Read<int>();
			info.appearData = stream.Read<byte[]>();
			info.nudeData = stream.Read<byte[]>();
			info.lobbyExp = stream.Read<float>();
			return info;
		}

		public CustomCharactor.CustomData CreateCustomData()
		{
			CustomCharactor.CustomData data = new CustomCharactor.CustomData();
			data.sex = sex == 1 ? CustomCharactor.ESex.Female : CustomCharactor.ESex.Male;
			data.appearData = new AppearBlendShape.AppearData();
			data.appearData.Deserialize(appearData);
			data.nudeAvatarData = new CustomCharactor.AvatarData();
			data.nudeAvatarData.Deserialize(nudeData);
			data.charactorName = name;
			return data;
		}
	}

    public class RoleInfoProxy
    {
        public byte level = 1;
        public byte sex = 1;

        public int winrate = 0;
		public int roleID = -1;

		public ulong steamId = 0;

        public string name = "NickName";
		public float lobbyExp = 0;

        public override string ToString()
        {
            return string.Format("[RoleInfo][name:{0}, sex:{1}, level:{2}]", name, sex, level);
        }

        public RoleInfoProxy()
        {
        }

        public static void WriteRoleInfoProxy(uLink.BitStream stream, object obj, params object[] codecOptions)
        {
            RoleInfoProxy info = obj as RoleInfoProxy;
			stream.Write<ulong>(info.steamId);
            stream.Write<byte>(info.level);
            stream.Write<int>(info.winrate);
			stream.Write<int>(info.roleID);
            stream.Write<byte>(info.sex);
            stream.Write<string>(info.name);
			stream.Write<float>(info.lobbyExp);
        }

        public static object ReadRoleInfoProxy(uLink.BitStream stream, params object[] codecOptions)
        {
            RoleInfoProxy info = new RoleInfoProxy();
			info.steamId = stream.Read<ulong>();
            info.level = stream.Read<byte>();
            info.winrate = stream.Read<int>();
			info.roleID = stream.Read<int>();
            info.sex = stream.Read<byte>();
            info.name = stream.Read<string>();
			info.lobbyExp = stream.Read<float>();
            return info;
        }
    }


	public class TMsgInfo
	{
		public EMsgType msgtype;
		public int sendRoleId;
		public int recvRoleId;
		public string msg;

		public static void WriteMsg(uLink.BitStream stream, object obj, params object[] codecOptions)
		{
			TMsgInfo msg = (TMsgInfo)obj;
			stream.Write<EMsgType>(msg.msgtype);
			stream.Write<int>(msg.sendRoleId);
			stream.Write<int>(msg.recvRoleId);
			stream.Write<string>(msg.msg);
		}

		public static object ReadMsg(uLink.BitStream stream, params object[] codecOptions)
		{
			TMsgInfo msg = new TMsgInfo();
			msg.msgtype = stream.Read<EMsgType>();
			msg.sendRoleId = stream.Read<int>();
			msg.recvRoleId = stream.Read<int>();
			msg.msg = stream.Read<string>();
			return msg;
		}
	}

	public class CreationOriginData
	{
		//internal int OwnerID;
		internal int ObjectID;
		internal int Seed;
		internal ulong HashCode;
		internal float HP;
		internal float Fuel;
		internal ulong SteamId;
		public static void Serialize(uLink.BitStream stream, object obj, params object[] codecOptions)
		{
			var creation = (CreationOriginData)obj;
			stream.Write<ulong>(creation.SteamId);
			stream.Write<int>(creation.ObjectID);
			stream.Write<ulong>(creation.HashCode);
			stream.Write<int>(creation.Seed);
			stream.Write<float>(creation.Fuel);
			stream.Write<float>(creation.HP);
		}

		public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			var creation = new CreationOriginData();
			creation.SteamId = stream.Read<ulong> ();
			creation.ObjectID = stream.Read<int>();
			creation.HashCode = stream.Read<ulong>();
			creation.Seed = stream.Read<int>();
			creation.Fuel = stream.Read<float>();
			creation.HP = stream.Read<float>();
			return creation;
		}
	}
	
	public class BuildCube
	{
		private Vector3 _startPos;
		private Vector3 _endPos;
		private bool _straightMode;
		private bool _dragHeight;
		private bool _deleteMode;
		private byte _rotation;
		private int _heightLength;
		private int _id;
		private int _matIndex;
		private IntVector3 _pivot;
		
		public Vector3 StartPos { get { return _startPos; } }
		public Vector3 EndPos { get { return _endPos; } }
		public bool StraightMode { get { return _straightMode; } }
		public bool DragHeight { get { return _dragHeight; } }
		public bool DeleteMode { get { return _deleteMode; } }
		public byte Rotation { get { return _rotation; } }
		public int HeightLength { get { return _heightLength; } }
		public int ID { get { return _id; } }
		public int MatIndex { get { return _matIndex; } }
		public IntVector3 Pivot { get { return _pivot; } }
		
		public virtual bool Equals(BuildCube bc)
		{
			if (bc._startPos.Equals(_startPos)
				&& bc._endPos.Equals(_endPos)
				&& bc._straightMode == _straightMode
				&& bc._dragHeight == _dragHeight
				&& bc._deleteMode == _deleteMode
				&& bc._rotation == _rotation
				&& bc._heightLength == _heightLength
				&& bc._id == _id
				&& bc._matIndex == _matIndex
				&& bc._pivot.Equals(_pivot))
				return true;
			
			return false;
		}
		
		public void Init(
			Vector3 startPos,
			Vector3 endPos,
			bool straightMode,
			bool dragHeight,
			bool deleteMode,
			byte rotation,
			int heightLength,
			int id,
			int matIndex,
			IntVector3 pivot)
		{
			_startPos = startPos;
			_endPos = endPos;
			_straightMode = straightMode;
			_dragHeight = dragHeight;
			_deleteMode = deleteMode;
			_rotation = rotation;
			_heightLength = heightLength;
			_id = id;
			_matIndex = matIndex;
			_pivot = pivot;
		}
		
		public void Init(BuildCube bc)
		{
			_startPos = bc._startPos;
			_endPos = bc._endPos;
			_straightMode = bc._straightMode;
			_dragHeight = bc._dragHeight;
			_deleteMode = bc._deleteMode;
			_rotation = bc._rotation;
			_heightLength = bc._heightLength;
			_id = bc._id;
			_matIndex = bc._matIndex;
			_pivot = bc._pivot;
		}
		
		public BuildCube()
		{
		}
		
		public byte[] ToBuffer()
		{
			using (BinaryWriter bw = new BinaryWriter(new MemoryStream(64)))
			{
				bw.Write(_startPos.x);
				bw.Write(_startPos.y);
				bw.Write(_startPos.z);
				bw.Write(_endPos.x);
				bw.Write(_endPos.y);
				bw.Write(_endPos.z);
				bw.Write(_straightMode);
				bw.Write(_dragHeight);
				bw.Write(_deleteMode);
				bw.Write(_rotation);
				bw.Write(_heightLength);
				bw.Write(_id);
				bw.Write(_matIndex);
				bw.Write(_pivot.x);
				bw.Write(_pivot.y);
				bw.Write(_pivot.z);
				bw.Flush();
				
				MemoryStream ms = bw.BaseStream as MemoryStream;
				byte[] data = ms.ToArray();
				return data;
			}
		}
		
		public void FromBuffer(byte[] buffer)
		{
			using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
			{
				_startPos.x = br.ReadSingle();
				_startPos.y = br.ReadSingle();
				_startPos.z = br.ReadSingle();
				_endPos.x = br.ReadSingle();
				_endPos.y = br.ReadSingle();
				_endPos.z = br.ReadSingle();
				_straightMode = br.ReadBoolean();
				_dragHeight = br.ReadBoolean();
				_deleteMode = br.ReadBoolean();
				_rotation = br.ReadByte();
				_heightLength = br.ReadInt32();
				_id = br.ReadInt32();
				_matIndex = br.ReadInt32();
				int x = br.ReadInt32();
				int y = br.ReadInt32();
				int z = br.ReadInt32();

				_pivot = new IntVector3(x, y, z);
			}
		}
	}

	public class RegisteredISO
	{
		internal ulong _hashCode;
		internal ulong UGCHandle;
		internal string _isoName;

		public static void Serialize(uLink.BitStream stream, object obj, params object[] codecOptions)
		{
			var iso = (RegisteredISO)obj;
			stream.Write(iso._hashCode);
			stream.Write(iso._isoName);
			stream.Write(iso.UGCHandle);
		}

		public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			var iso = new RegisteredISO();
			iso._hashCode = stream.Read<ulong>();
			iso._isoName = stream.Read<string>();
			iso.UGCHandle = stream.Read<ulong>();

			return iso;
		}
	}
	public class MapObj
	{
		public Vector3 pos;
		public int objID;
		//public float rotY;
		//public int pathID;
		
		internal static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			var item = new MapObj();
			item.pos = stream.Read<Vector3>();
			item.objID = stream.Read< int >();
			//item.rotY = stream.Read<float>();
			//item.pathID = stream.Read<int>();
			return item;
		}
		
		internal static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
		{
			var item = (MapObj)value;
			
			stream.Write<Vector3>(item.pos);
			stream.Write< int > (item.objID);
			//stream.Write<float>(item.rotY);
			//stream.Write<int>(item.pathID);
		}
	}

	public class NPCType
	{
		public const int AD_NPC = 1;    //随机npc
		public const int TOWN_NPC = 2;   //城镇npc
		public const int BUILDING_NPC = 3;   //建筑npc
		public const int STRD_NPC = 4;//故事模式随机NPC
		public const int ST_NPC = 5;//故事模式主线NPC
		public const int CUSTOM_NPC = 6; //
	}

	public class HistoryStruct
	{
		public int m_Day;
		public string m_Value;
		public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			var item = new HistoryStruct();
			item.m_Day = stream.Read<int>();
			item.m_Value = stream.Read< string > ();
			return item;
		}
		public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
		{
			var item = (HistoryStruct)value;
			
			stream.Write<int>(item.m_Day);
			stream.Write< string > (item.m_Value);
		}
	}
	public class CompoudItem
	{
		public float curTime;
		public float time=-1;
		public int itemID;
		public int itemCnt;
        public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			var item = new CompoudItem();
			item.curTime = stream.Read<float>();
			item.time = stream.Read< float > ();
			item.itemID = stream.Read< int > ();
			item.itemCnt = stream.Read< int > ();
			return item;
		}
		public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
		{
			var item = (CompoudItem)value;
			
			stream.Write<float>(item.curTime);
			stream.Write< float > (item.time);
			stream.Write< int > (item.itemID);
			stream.Write< int > (item.itemCnt);
		}

		public bool IsFinished{
			get{return curTime>=time;}
		}
	}

	public class LobbyShopData
	{
		public int id;
		public int itemtype;
		public int price;
		public int rebate;
		public int tab;
		public bool bind;
		public bool bshow;
		public int forbid;
		
		public static void Serialize(uLink.BitStream stream, object obj, params object[] codecOptions)
		{
			LobbyShopData data = (LobbyShopData)obj;
			stream.Write<int>(data.id);
			stream.Write<int>(data.itemtype);
			stream.Write<int>(data.price);
			stream.Write<int>(data.rebate);
			stream.Write<int>(data.tab);
			stream.Write<bool>(data.bind);
			stream.Write<bool>(data.bshow);
			stream.Write<int>(data.forbid);
		}
		public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			LobbyShopData data = new LobbyShopData();
			data.id = stream.Read<int>();
			data.itemtype = stream.Read<int>();
			data.price = stream.Read<int>();
			data.rebate = stream.Read<int>();
			data.tab = stream.Read<int>();
			data.bind = stream.Read<bool>();
			data.bshow = stream.Read<bool>();
			data.forbid = stream.Read<int>();
			return data;
		}
	}

    public class IsoData
    {
        public ulong isohashcode;
        public ulong uploader;

        public static void WriteMsg(uLink.BitStream stream, object obj, params object[] codecOptions)
        {
            IsoData msg = (IsoData)obj;
            stream.Write<ulong>(msg.isohashcode);
            stream.Write<ulong>(msg.uploader);
        }

        public static object ReadMsg(uLink.BitStream stream, params object[] codecOptions)
        {
            IsoData msg = new IsoData();
            msg.isohashcode = stream.Read<ulong>();
            msg.uploader = stream.Read<ulong>();
            return msg;
        }
    }

    public class SceneObject
	{
		protected int _objId;
		protected int _protoId;
		protected int _worldId;
        protected int _scenarioId;
        protected ESceneObjType _type;
		protected Vector3 _pos;
		protected Vector3 _scale;
		protected Quaternion _rot;

		public int Id { get { return _objId; } }
		public int ProtoId { get { return _protoId; } }
		public int WorldId { get { return _worldId; } }
        public int ScenarioId { get { return _scenarioId; } }
        public ESceneObjType Type { get { return _type; } }
		public Vector3 Pos { get { return _pos; } }
		public Vector3 Scale { get { return _scale; } }
		public Quaternion Rot { get { return _rot; } }

		public SceneObject()
		{
            _scenarioId = -1;
        }

		public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
		{
			SceneObject obj = new SceneObject();
			obj._objId = stream.Read<int>();
			obj._protoId = stream.Read<int>();
			obj._worldId = stream.Read<int>();
			obj._type = stream.Read<ESceneObjType>();
			obj._pos = stream.Read<Vector3>();
			obj._scale = stream.Read<Vector3>();
			obj._rot = stream.Read<Quaternion>();
            obj._scenarioId = stream.Read<int>();

            return obj;
		}

		public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
		{
			SceneObject obj = value as SceneObject;
			stream.Write(obj._objId);
			stream.Write(obj._protoId);
			stream.Write(obj._worldId);
			stream.Write(obj._type);
			stream.Write(obj._pos);
			stream.Write(obj._scale);
			stream.Write(obj._rot);
            stream.Write(obj._scenarioId);
        }
    }
	#endregion
}
