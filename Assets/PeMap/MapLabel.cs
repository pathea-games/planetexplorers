using Mono.Data.SqliteClient;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Pathea;
using System.Collections;


namespace PeMap
{
    public class MyBaseList<T> where T : class, new()
    {
        protected List<T> mList;

        public MyBaseList(int capacity)
        {
            mList = new List<T>(capacity);
        }

        public void ForEach(System.Action<T> action)
        {
            mList.ForEach(action);
        }

        public T Find(System.Predicate<T> predicate)
        {
            return mList.Find(predicate);
        }

        public List<T> FindAll(System.Predicate<T> predicate)
        {
            return mList.FindAll(predicate);
        }

        public void Add(T item)
        {
            mList.Add(item);
        }

        public bool Remove(T item)
        {
            return mList.Remove(item);
        }
    }

    public class MyBaseListSingleton<T0, T>:Pathea.MonoLikeSingleton<T0>
        where T0:class, new()        
    {
        public List<T> mList = new List<T>();

        public void ForEach(System.Action<T> action)
        {
            mList.ForEach(action);
        }

        public void RemoveAll(Predicate<T> match)
        {
            mList.RemoveAll(match);
        }

        public T Find(System.Predicate<T> predicate)
        {
            return mList.Find(predicate);
        }

		public List<T> FindAll(System.Predicate<T> predicate)
		{
			return mList.FindAll(predicate);
		}
		
		public virtual bool Add(T item)
        {
            if (item != null)
            {
                //lz-2016.07.28 不允许添加重复的
                int index = mList.FindIndex(a => ((ILabel)item).CompareTo(((ILabel)a)));
                if (index >= 0) return false;
                mList.Add(item);
                return true;
            }
            else
            {
                Debug.Log("MapLabel Item is null: "+item);
                return false;
            }
        }

        public virtual bool Remove(T item)
        {
            if (null == item)
                return false;
            int index = mList.FindIndex(a => ((ILabel)item).CompareTo(((ILabel)a)));
            if (index >= 0)
            {
                mList.RemoveAt(index);
                return true;
            }
            return false;
        }
    }

    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }

    public class MyListArchiveSingleton<T0, T> : Pathea.ArchivableSingleton<T0>
        where T0 : class, new()
        where T : class, ISerializable, new()
    {
        List<T> mList = new List<T>();

        public void ForEach(System.Action<T> action)
        {
            mList.ForEach(action);
        }

        public T Find(System.Predicate<T> predicate)
        {
            return mList.Find(predicate);
        }

        public virtual void Add(T item)
        {
            mList.Add(item);
        }

        public virtual bool Remove(T item)
        {
            return mList.Remove(item);
        }

        protected override void SetData(byte[] data)        
        {
            PETools.Serialize.Import(data, (r) =>
            {
                int count = r.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    byte[] itemBuff = PETools.Serialize.ReadBytes(r);
                    if (null != itemBuff)
                    {
                        T item = new T();

                        item.Deserialize(itemBuff);

                        Add(item);
                    }
                }
            });
        }

        protected override void WriteData(BinaryWriter w)
        {
            w.Write(mList.Count);

            mList.ForEach((item) =>
            {
                PETools.Serialize.WriteBytes(item.Serialize(), w);
            });
        }
    }

    public enum ELabelType
    {
        FastTravel,
        User,
        Npc,
        Vehicle,
        Mark,
		Mission,
        Revive,
        Max
    }

    public enum EShow
    {
        BigMap,
        MinMap,
        All,
        Max
    }

    public interface ILabel
    {
        int GetIcon();
        Vector3 GetPos();
        string GetText();
        bool FastTravel();
        ELabelType GetType();
        bool NeedArrow();
        float GetRadius();
        EShow GetShow();
        //lz-2016.06.02 增加比对方法，每个实现ILabel接口的对象必须实现比对方法，不然移除的时候会出问题
        bool CompareTo(ILabel label);
    }


    public class LabelMgr : MyBaseListSingleton<LabelMgr, ILabel>
    {
        public class Args:PeEvent.EventArg
        {
            public bool add;
            public ILabel label;
        }

        PeEvent.Event<Args> mEventor = new PeEvent.Event<Args>();
        public PeEvent.Event<Args> eventor
        {
            get
            {
                return mEventor;
            }
        }

        public override bool Add(ILabel item)
        {
            if(base.Add(item))
            {
                eventor.Dispatch(new Args() {add=true, label = item });
                return true;
            }
            return false;
        }

        public override bool  Remove(ILabel item)
        {
            if (base.Remove(item))
            {
                eventor.Dispatch(new Args() { add = false, label = item });
                return true;
            }
            return false;
        }
    }

    public class ArchivableLabelMgr<T0, T> : MyListArchiveSingleton<T0, T>
        where T0 : class, new()
        where T : class, ILabel, ISerializable, new()
    {
        public override void Add(T item)
        {
            if (LabelMgr.Instance.Add(item))
            {
                base.Add(item);
            }
        }

        public override bool Remove(T item)
        {
            if (LabelMgr.Instance.Remove(item))
            {
                return base.Remove(item);
            }
            return false;
        }
    }

    public class UserLabel : ILabel, ISerializable
    {
        public Vector3 pos;
        public string text;
        public int icon;
		public int playerID;
		public byte index;

        int ILabel.GetIcon()
        {
            return icon;
        }

        Vector3 ILabel.GetPos()
        {
            return pos;
        }

        string ILabel.GetText()
        {
            return text;
        }

        bool ILabel.FastTravel()
        {
            return false;            
        }

        ELabelType ILabel.GetType()
        {
            return ELabelType.User;            
        }

        bool ILabel.NeedArrow()
        {
            return false;
        }

        float ILabel.GetRadius()
        {
            return -1f;
        }

        EShow ILabel.GetShow()
        {
            return EShow.BigMap;
        }

        byte[] ISerializable.Serialize()
        {
            return PETools.Serialize.Export((w) =>
            {
                PETools.Serialize.WriteVector3(w, pos);
                w.Write(text);
                w.Write(icon);
                w.Write(index);
            });
        }

        void ISerializable.Deserialize(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                pos = PETools.Serialize.ReadVector3(r);
                text = r.ReadString();
                icon = r.ReadInt32();
                //lz-2016.07.30 以前没有存Index这个版本以后加上
                if (Pathea.ArchiveMgr.Instance.GetCurArvhiveVersion() >= Pathea.Archive.Header.Version_5)
                {
                    index = r.ReadByte();
                }
            });
        }

        public class Mgr : ArchivableLabelMgr<Mgr, UserLabel>
        {
            protected override string GetArchiveKey()
            {
                return "ArchiveKeyMapUserLabel";
            }
        }
        #region lz-2016.06.02
        public bool CompareTo(ILabel label)
        {
            if (label is UserLabel)
            {
                UserLabel userLabel = (UserLabel)label;
                if (pos == userLabel.pos)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        #endregion
    }

    public class LabelMark : ILabel
    {
        public Vector3 pos;
        public string text;
        public int icon;
        public ELabelType labelType;
        public bool needArrow;
        public float radius = -1f;

        int ILabel.GetIcon()
        {
            return icon;
        }

        Vector3 ILabel.GetPos()
        {
            return pos;
        }

        string ILabel.GetText()
        {
            return text;
        }

        bool ILabel.FastTravel()
        {
            return false;
        }

        ELabelType ILabel.GetType()
        {
            return labelType;
        }

        bool ILabel.NeedArrow()
        {
            return needArrow;
        }

        float ILabel.GetRadius()
        {
            return radius;
        }

        EShow ILabel.GetShow()
        {
            return EShow.All;
        }

        #region lz-2016.06.02 

        public bool CompareTo(ILabel label)
        {
            if (label is LabelMark)
            {
                LabelMark marklabel = (LabelMark)label;
                if (this.pos == marklabel.pos && this.icon == marklabel.icon)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        #endregion
    }

    public class StaticPoint : PeMap.ILabel, ISerializable
    {
        public int ID = 0;
        public int soundID = 0;
        public int campId = -1;
        public float distance = 100f;
        public string text = "default";
        public int textId = -1;
        public int icon = 10;
        public bool fastTravel = true;
        public Vector3 position;

        bool discoverd = false;

        void AddToLabelMgr()
        {
            LabelMgr.Instance.Add(this);
        }

        int PeMap.ILabel.GetIcon()
        {
            return icon;
        }

        Vector3 PeMap.ILabel.GetPos()
        {
            return position;
        }

        string PeMap.ILabel.GetText()
        {
            if (-1 != textId)
            {
                return PELocalization.GetString(textId);
            }

            return text;
        }

        bool PeMap.ILabel.FastTravel()
        {
            return fastTravel;
        }

        ELabelType ILabel.GetType()
        {
            if (fastTravel)
            {
                return ELabelType.FastTravel;
            }

            return ELabelType.Mark;
        }

        bool ILabel.NeedArrow()
        {
            return false;
        }

        float ILabel.GetRadius()
        {
            return -1f;
        }

        EShow ILabel.GetShow()
        {
            return EShow.All;
        }

        byte[] ISerializable.Serialize()
        {
            return PETools.Serialize.Export((w) =>
            {
                PETools.Serialize.WriteVector3(w, position);
                w.Write((float)distance);
                w.Write((int)textId);
                w.Write((string)text);
                w.Write((int)icon);
                w.Write((bool)fastTravel);
                w.Write((bool)discoverd);
                w.Write(campId);
                //lz-2016.07.30 这个地方一定要记得把ID写进去，因为这个是唯一标识，在匹配的时候要用到
                w.Write((int)ID);
            });
        }

        void ISerializable.Deserialize(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                position = PETools.Serialize.ReadVector3(r);
                distance = r.ReadSingle();
                textId = r.ReadInt32();
                text = r.ReadString();
                icon = r.ReadInt32();
                fastTravel = r.ReadBoolean();
                discoverd = r.ReadBoolean();
                if (Pathea.ArchiveMgr.Instance.GetCurArvhiveVersion() >= Pathea.Archive.Header.Version_2)
                {
                    campId = r.ReadInt32();
                }
                else
                {
                    campId = StoryStaticPoint.GetCamp(textId);
                }
                if (Pathea.ArchiveMgr.Instance.GetCurArvhiveVersion() >= Pathea.Archive.Header.Version_5)
                {
                    ID = r.ReadInt32();
                }
                else
                {
                    ID = StoryStaticPoint.GetIDByNameID(textId);
                }
                
            });

            if (discoverd)
            {
                AddToLabelMgr();
            }
        }

        public class Mgr : MyListArchiveSingleton<Mgr, StaticPoint>
        {
            public void Tick(Vector3 observerPos)
            {
                ForEach((item) =>
                {
                    if (item.discoverd == false && Vector3.Distance(observerPos, item.position) < item.distance)
                    {
                        item.discoverd = true;
                        item.AddToLabelMgr();
						if(PeGameMgr.IsMultiStory)
						{
							PlayerNetwork.mainPlayer.RequestAddFountMapLable(item.ID);
						}
                    }
                });
            }

            public void UnveilAll()
            {
                ForEach((item) =>
                {
                    if (item.discoverd == false)
                    {
                        item.discoverd = true;
                        item.AddToLabelMgr();
                    }
                });
            }

            public int GetMapSoundID(Vector3 position)
            {
                int soundID = 0;

                ForEach((item) =>
                {
                    if(Vector3.Distance(position, item.position) <= item.distance && item.soundID > 0)
                    {
                        soundID = item.soundID;
                    }
                });

                return soundID;
            }
        }
        
        public static void StaticPointBeFound(int id)
        {
            StaticPoint tmp = StaticPoint.Mgr.Instance.Find(delegate(StaticPoint sp)
            {
                if (sp.ID == id)
                    return true;
                return false;
            });
			if (tmp != null && tmp.discoverd == false) 
            {
                tmp.discoverd = true;
                tmp.AddToLabelMgr();
            }
        }

        #region lz-2016.06.02 比对方法，查找和移除用

        public bool CompareTo(ILabel label)
        {
            if (label is StaticPoint)
            {
                StaticPoint staticPointlabel = (StaticPoint)label;
                if (this.ID == staticPointlabel.ID)
                    return true;
                return false;
            }
            else
                return false;
        }
        #endregion
    }

    public class StoryStaticPoint
    {
        public static int GetCamp(int nameId)
        {
            SqliteDataReader reader = LocalDatabase.Instance.SelectWhereSingle("MapIcon", "*", "Name", " = ", "'" + nameId + "'");

            if (reader.Read())
            {
                return Convert.ToInt32(reader.GetString(reader.GetOrdinal("Camp")));
            }

            return -1;
        }

        //lz-2016.07.30  因为以前没有存ID，以前的版本就这样来读
        public static int GetIDByNameID(int nameId)
        {
            SqliteDataReader reader = LocalDatabase.Instance.SelectWhereSingle("MapIcon", "*", "Name", " = ", "'" + nameId + "'");

            if (reader.Read())
            {
                return Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            }

            return -1;
        }

        public static void Load()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MapIcon");
            while (reader.Read())
            {
                StaticPoint point = new StaticPoint();
                point.ID = reader.GetInt32(0);
                point.textId = Convert.ToInt32(reader.GetString(1));
                string[] posStr = reader.GetString(2).Split(',');
                point.position = new Vector3(Convert.ToSingle(posStr[0]), Convert.ToSingle(posStr[1]), Convert.ToSingle(posStr[2]));
                point.icon = Convert.ToInt32(reader.GetString(3));
                point.distance = Convert.ToSingle(reader.GetString(4));
                point.campId = Convert.ToInt32(reader.GetString(5));
                if (point.campId >= 0)
                {
                    point.fastTravel = true;
                }
                else
                {
                    point.fastTravel = false;
                }

                point.soundID = Convert.ToInt32(reader.GetString(6));
                StaticPoint.Mgr.Instance.Add(point);
            }
        }
    }

	public class TowerMark : ILabel, ISerializable
	{
		public int ID = 0;
		public int campId = -1;
		public string text = "default";
		public int icon = PeMap.MapIcon.Turret;
		public Vector3 position;

		#region ILabel implementation

		public int GetIcon ()
		{
			return icon;
		}

		public Vector3 GetPos ()
		{
			return position;
		}

		public string GetText ()
		{
			return text;
		}

		public bool FastTravel ()
		{
			return false;
		}

		ELabelType ILabel.GetType ()
		{
			return ELabelType.Mark;
		}

		public bool NeedArrow ()
		{
			return false;
		}

		public float GetRadius ()
		{
			return -1;
		}

		public EShow GetShow ()
		{
			return EShow.All;
		}

		public bool CompareTo (ILabel label)
		{
			TowerMark towerMask = label as TowerMark;
			return null != towerMask && towerMask.ID == ID;
		}

		#endregion

		#region ISerializable implementation

		public byte[] Serialize ()
		{ 
			return PETools.Serialize.Export((w) =>
			{
				w.Write(ID);
				PETools.Serialize.WriteVector3(w, position);
				w.Write(campId);
				w.Write(icon);
				w.Write(text);
			}
			);
		}

		public void Deserialize (byte[] data)
		{ 
			PETools.Serialize.Import(data, (r) =>
			                                      {
				ID = r.ReadInt32();
				position = PETools.Serialize.ReadVector3(r);
				campId = r.ReadInt32();
				icon = r.ReadInt32();
				text = r.ReadString();
			});
			LabelMgr.Instance.Add(this);
		}

		#endregion

		public class Mgr : MyListArchiveSingleton<Mgr, TowerMark>
		{

		}
	}
		
	public class MonsterBeaconMark : ILabel
	{
		string text = "MonsterBeacon";
		int icon = PeMap.MapIcon.MonsterBeacon;
		public Vector3 Position;
        //lz-2016.09.06 是否是怪物攻城
        public bool IsMonsterSiege=false;

        #region ILabel implementation

        public int GetIcon ()
		{
			return icon;
		}
		
		public Vector3 GetPos ()
		{
			return Position;
		}
		
		public string GetText ()
		{
			return text;
		}
		
		public bool FastTravel ()
		{
			return false;
		}
		
		ELabelType ILabel.GetType ()
		{
			return ELabelType.Mark;
		}
		
		public bool NeedArrow ()
		{
			return false;
		}
		
		public float GetRadius ()
		{
			return -1;
		}
		
		public EShow GetShow ()
		{
			return EShow.All;
		}
		
		public bool CompareTo (ILabel label)
		{
            MonsterBeaconMark mark = label as MonsterBeaconMark;
            return null != mark && mark.Position == Position;
        }
		
		#endregion
	}

	public class CarrierMark : ILabel
	{
		int icon = PeMap.MapIcon.WorldMapVehiclesTag;
		public WhiteCat.CarrierController carrierController;
		
		#region ILabel implementation
		
		public int GetIcon ()
		{
			return icon;
		}
		
		public Vector3 GetPos ()
		{
			if(null != carrierController)
				return carrierController.creationController.boundsCenterInWorld;
			return Vector3.zero;
		}
		
		public string GetText ()
		{
			if(null != carrierController)
				return carrierController.creationController.creationData.m_IsoData.m_HeadInfo.Name;
			return "";
		}
		
		public bool FastTravel ()
		{
			return false;
		}
		
		ELabelType ILabel.GetType ()
		{
			return ELabelType.Vehicle;
		}
		
		public bool NeedArrow ()
		{
			return false;
		}
		
		public float GetRadius ()
		{
			return -1;
		}
		
		public EShow GetShow ()
		{
			return EShow.All;
		}
		
		public bool CompareTo (ILabel label)
		{
			CarrierMark mark = label as CarrierMark;
			return null != mark && mark.carrierController == carrierController;
		}
		
		#endregion
	}
	
    public enum EMapIcon
    {
        TransPoint = 1,
        Custom,
        Npc,
        Vehicle,
        Mark
    }


    public class MapIcon
    {
		public const int None = 0;  					// 未知 
        public const int UserDefine1 = 1;				// 自定义标记1
		public const int UserDefine2 = 2;				// 自定义标记2
		public const int UserDefine3 = 3;				// 自定义标记3
		public const int UserDefine4 = 4;				// 自定义标记4
        public const int Camp = 5;						// 可传送营地
		public const int WorldMapVehiclesTag = 6;		// 大地图载具
		public const int Player = 7;					// 玩家
		public const int Turret = 8;					// 玩家炮台
		public const int Servant = 9;					// 仆从
		public const int FlagIcon = 10;					// 多人旗帜
		public const int AllyPlayer = 11;				// 多人队友
		public const int OppositePlayer = 12;			// 多人敌方
		public const int TaskTarget = 13;				// 任务完成地点
		public const int Npc = 14;						// Npc
		public const int TaskUnCmplt = 26;				// 未完成的任务
		public const int PlayerBase = 16;				// 玩家基地
		public const int MonsterBoss = 17;				// Boss
	 	public const int Monster = 18;					// 小怪

		public const int PujaBase = 19;					// puja营地
		public const int Puja = 20;						// puja怪
		public const int PujaBoss = 21;					// pujaBoss

		public const int PajaBase = 22;					// paja营地
		public const int Paja = 23;				     	// paja怪
		public const int PajaBoss = 24;             	// pajaBoss
		public const int TaskGetable = 25;				// 可接的任务		
		public const int TaskCmplt = 26;				// 完成的任务
		public const int AdventureCamp = 27;			// 历险模式可传送城镇
		public const int Nutral	= 28;					// 多人中立

		public const int CrashSite = 29;				// 各种坠机处
		public const int GlantTree = 30;				// 巨树
		public const int RockFormation = 31;			// 石堆、小坑（陈真、老雷、wiles等）
		public const int MonsterNest = 32;				// 各种BOSS怪巢穴 火熊/黑暗怪/层背兽
		public const int Relic = 33;					// 遗迹
		public const int LargeDoline = 34;				// 天坑
		public const int Lake = 35;						// 大禺湖/河
		public const int BrokenDam = 36;				// 残破的大坝
		public const int DesertCity = 37;				// 沙漠城市废墟
		public const int Waterfall = 38;				// 大瀑布
		public const int VirusSpacecraft = 39;			// 病毒释放船（第三方外星人）
		public const int PujaGate = 40;					// PUJA城墙大门
		public const int L1_Ship = 41;					// L1船体坠毁处（人类大飞船）
        public const int ServantDeadPlace = 43;         //仆从死亡地点
		public const int RandomDungeonEntrance = 44;    //随机地下城入口 
        //lz-2016.08.01 怪物信标
        public const int MonsterBeacon=45;
        //lz-2016.09.22 人类，paja，puja被摧毁的建筑图标
        public const int HumanBrokenBase = 46;          
        public const int PajaBrokenBase = 47;
        public const int PujaBrokenBase = 48;


        // 没有图标的
        public const int Alien = Monster;				// 	第3方外星人	
		public const int unknownCamp = 26;  			// 未知的传送营地


        public int id;
        public string iconName;
        public EMapIcon iconType;

        public class Mgr : Pathea.MonoLikeSingleton<Mgr>
        {
            protected override void OnInit()
            {
                base.OnInit();
                Load();
            }

            public List<MapIcon> iconList;

            public void Load()
            {
                iconList = new List<MapIcon>();
                SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Icon");
                while (reader.Read())
                {
                    MapIcon fIcon = new MapIcon();
                    fIcon.id = Convert.ToInt32(reader.GetString(0));
                    fIcon.iconName = reader.GetString(1);
                    fIcon.iconType = (EMapIcon)Convert.ToInt32(reader.GetString(2));
                    iconList.Add(fIcon);
                }
            }
        }
    }
}