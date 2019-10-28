using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System.IO;

namespace Pathea
{
	public class RequestCmpt : PeCmpt, IPeMsg
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        //BehaveCmpt m_Behave;
        ViewCmpt m_View;


		Request[] m_Request = new Request[(int)EReqType.MAX];

        NetworkInterface _net;
        public NetworkInterface Net
        {
            get
            {
                if (Entity != null && PeGameMgr.IsMulti && _net == null)
                {
                    _net = NetworkInterface.Get(Entity.Id);
                }
                return _net;
            }
        }

        Request Create(EReqType type)
        {
            switch (type)
            {
                case EReqType.Idle:             return new RQIdle();
                case EReqType.Animation:        return new RQAnimation();
                case EReqType.MoveToPoint:      return new RQMoveToPoint();
			    case EReqType.TalkMove:         return new RQTalkMove();
                case EReqType.FollowPath:       return new RQFollowPath();
                case EReqType.FollowTarget:     return new RQFollowTarget();
                case EReqType.Salvation:        return new RQSalvation();
                case EReqType.Dialogue:         return new RQDialogue();
                case EReqType.Translate:        return new RQTranslate();
                case EReqType.Rotate:           return new RQRotate();
                case EReqType.Attack:           return new RQAttack();
                case EReqType.PauseAll:         return new RQPauseAll();
                case EReqType.UseSkill:         return new RQUseSkill();
                case EReqType.MAX:              return null;
                default:                        return null;
            }
        }

        public Request Register(EReqType type, params object[] objs)
        {
            Request request = null;
            switch (type)
            {
                case EReqType.Dialogue:
					request = new RQDialogue(objs);
                    break;
                case EReqType.Idle :
                    request = new RQIdle(objs);
                    break;
                case EReqType.Animation:
                    request = new RQAnimation(objs);
                    break;
                case EReqType.MoveToPoint :
                    request = new RQMoveToPoint(objs);
                    break;
			    case EReqType.TalkMove :
				    request = new RQTalkMove(objs);
				    break;
                case EReqType.FollowPath :
                    request = new RQFollowPath(objs);
                    break;
                case EReqType.FollowTarget :
                    request = new RQFollowTarget(objs);
                    break;
                case EReqType.Salvation:
                    request = new RQSalvation(objs);
                    break;
                case EReqType.Translate:
                    request = new RQTranslate(objs);
                    break;
                case EReqType.Rotate:
                    request = new RQRotate(objs);
                    break;
                case EReqType.Attack:
                    request = new RQAttack();
				    break;
			    case EReqType.PauseAll:
				    request = new RQPauseAll();
                    break;
				case EReqType.UseSkill:
					request = new RQUseSkill();
					break;
            }

            if (!CalculateRelation(request))
                return null;

            AddRequest(request);
            return request;
        }

        //public Request RequestLie(EReqType type, params object[] objs)
        //{
        //    Request request = null;
        //    switch (type)
        //    {
        //        case EReqType.Idle:
        //            request = new RQIdle(objs);
        //            break;
        //        default:
        //            return null;
        //    }

        //    if (!CalculateRelation(request))
        //        return null;

        //    AddRequestMul(request);
        //    return request;
        //}

        public override void Start()
        {
            base.Start();

            //m_Behave = GetComponent<BehaveCmpt>();
            m_View = GetComponent<ViewCmpt>();
        }

        public override void Serialize(BinaryWriter w)
        {
            base.Serialize(w);

            w.Write(Version_Current);

            w.Write(m_Request.Length);

            for (int i = 0; i < m_Request.Length; i++)
            {
                if (m_Request[i] == null)
                {
                    w.Write(-1);
                    continue;
                }

                w.Write((int)m_Request[i].Type);
                m_Request[i].Serialize(w);
            }
        }

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);

            r.ReadInt32();

            int length = r.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                int type = r.ReadInt32();

                if (type == -1) continue;

                Request req = Create((EReqType)type);
                req.Deserialize(r);

				if(req.Type != EReqType.Dialogue)
                  AddRequest(req);
            }
        }

        public Request GetRequest(EReqType type)
        {
			return m_Request [(int)type];
        }

        public void AddRequest(Request request)
        {
            if(m_Request[(int)request.Type] == null)
				m_Request[(int)request.Type] = request;

            if (PeGameMgr.IsSingle && Entity.BehaveCmpt != null && HasRequest())
				Entity.BehaveCmpt.Excute();
        }

        //public void AddRequestMul(Request request)
        //{
        //    if (m_Request[(int)request.Type] == null)
        //        m_Request[(int)request.Type] = request;

        //    if (Entity.BehaveCmpt != null && HasRequest())
        //        Entity.BehaveCmpt.Excute();
        //}

        public void RemoveRequest(EReqType type)
        {
            if (type == EReqType.MoveToPoint && StroyManager.Instance != null)
			{
				RQMoveToPoint R_move = m_Request[(int)type] as RQMoveToPoint;
				if(R_move != null)
				{
					bool isreached = PETools.PEUtil.MagnitudeH(Entity.position, R_move.position) < R_move.stopRadius;
					StroyManager.Instance.EntityReach(Entity, isreached);
				}
			}


            if (PeGameMgr.IsMulti && Entity != null && Entity.netCmpt != null && type == EReqType.FollowPath)
            {
                if (Entity.netCmpt.network.hasOwnerAuth)
                    Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Remove, (int)EReqType.FollowPath, (m_Request[(int)type] as RQFollowPath).VerifPos);
            }

            if (PeGameMgr.IsMulti && Entity != null && Entity.netCmpt != null && type == EReqType.FollowTarget && !PeGameMgr.IsStory)
            {
                if (Entity.netCmpt.network.hasOwnerAuth)
                    Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Remove, (int)EReqType.FollowTarget);
            }

            m_Request[(int)type] = null;

            if (PeGameMgr.IsSingle && Entity.BehaveCmpt != null && m_View != null && !m_View.hasView && !HasRequest())
				Entity.BehaveCmpt.Stopbehave();
           
        }

        public void RemoveRequest(Request request)
        {
            if (request == null)
                return;


            if (request.Type == EReqType.MoveToPoint && StroyManager.Instance != null)
			{
				RQMoveToPoint R_move = request as RQMoveToPoint;
				if(R_move != null)
				{
					bool isreached = PETools.PEUtil.MagnitudeH(Entity.position, R_move.position) < R_move.stopRadius;
					StroyManager.Instance.EntityReach(Entity, isreached);
				}
			}


			if (m_Request[(int)request.Type] != null && m_Request[(int)request.Type].Equals(request))
				m_Request[(int)request.Type] = null;

			if (PeGameMgr.IsSingle && Entity.BehaveCmpt != null && m_View != null && !m_View.hasView && !HasRequest())
				Entity.BehaveCmpt.Stopbehave();
            if(PeGameMgr.IsMulti && Entity != null && Entity.netCmpt != null && request.Type == EReqType.FollowPath)
            {
                if(Entity.netCmpt.network.hasOwnerAuth)
                    Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Remove,(int)EReqType.FollowPath, (request as RQFollowPath).VerifPos);
            }
            if (PeGameMgr.IsMulti && Entity != null && Entity.netCmpt != null && request.Type == EReqType.FollowTarget && !PeGameMgr.IsStory)
            {
                if (Entity.netCmpt.network.hasOwnerAuth)
                    Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Remove, (int)EReqType.FollowTarget);
            }
        }

        public bool Contains(EReqType type)
        {
			return m_Request[(int)type] != null;
        }

        public bool HasRequest()
        {
			int n = (int)EReqType.MAX;
			for (int i = 0; i < n; i++) {
				if(m_Request[i] != null && m_Request[i].CanRun())
					return true;
			}
            return false;
        }

        public bool HasAnyRequest()
        {
			int n = (int)EReqType.MAX;
			for (int i = 0; i < n; i++) {
				if(m_Request[i] != null)
					return true;
			}
            return false;
        }

        public int GetFollowID()
        {
			if(null != m_Request[(int)EReqType.FollowTarget])
            {
				return (m_Request[(int)EReqType.FollowTarget] as RQFollowTarget).id;
            }

            return -1;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

			int n = (int)EReqType.MAX;
			for (int i = 0; i < n; i++) {
				if(null == m_Request[i]) continue;

				m_Request[i].RemoveMask(EReqMask.Blocking);

				for (int j = 0; j < n; j++) {
					if(null == m_Request[i] || null == m_Request[j]) continue;

					EReqRelation relation = m_Request[i].GetRelation(m_Request[j]);					
					if (relation == EReqRelation.Block)
						m_Request[j].Addmask(EReqMask.Blocking);

					if (relation == EReqRelation.Blocked)
						m_Request[i].Addmask(EReqMask.Blocking);

                    //if (relation == EReqRelation.Delete)
                    //    m_Request[j] = null;

                    //if (relation == EReqRelation.Deleted)
                    //    m_Request[i] = null;
				}
			}
        }

        bool CalculateRelation(Request request)
        {
            if (request == null)
                return false;

			int n = (int)EReqType.MAX;
			for (int i = 0; i < n; i++) {
				if(null == m_Request[i]) continue;

				if(request.GetRelation(m_Request[i]) == EReqRelation.Deleted){
					//Debug.Log("Ingore request : " + request.Name);
					return false;
				}
			}
			for (int i = 0; i < n; i++) {
				if(null == m_Request[i]) continue;
				
				if(request.GetRelation(m_Request[i]) == EReqRelation.Delete){
					RemoveRequest(m_Request[i]);
					//m_Request[i] = null;
				}
			}
			return true;
        }

		public void OnMsg(EMsg msg, params object[] args)
		{
			switch (msg)
			{
			case EMsg.Net_Proxy:
//				RequsetProtect();
				break;
			}
		}

		//在NPC有MoveToPointrequest时候，玩家传送时多人处理：controler丢失直接让NPC传送到需求点
		public void RequsetProtect()
		{
			if(Contains(EReqType.MoveToPoint))
			{
				if(Entity!= null && Entity.NpcCmpt!=null)
				{
					RQMoveToPoint request = GetRequest(EReqType.MoveToPoint) as RQMoveToPoint;
					if(request != null)
					{
						Vector3 pos = request.position;
                        StroyManager.Instance.EntityReach(Entity, true);
                        Entity.NpcCmpt.Req_Remove(EReqType.MoveToPoint);                        
                        Entity.NpcCmpt.Req_Translate(pos);
					}
				}
			}

			if(Contains(EReqType.TalkMove))
			{
				if(Entity!= null && Entity.NpcCmpt!=null)
				{
					RQTalkMove request = GetRequest(EReqType.TalkMove) as RQTalkMove;
					if(request != null)
					{
						Vector3 pos = request.position;
						Entity.NpcCmpt.Req_Remove(EReqType.TalkMove);
						Entity.NpcCmpt.Req_Translate(pos);
					}
				}
			}

			if(Contains(EReqType.FollowTarget))
			{
				if(Entity!= null && Entity.NpcCmpt!=null)
				{
					RQFollowTarget request = GetRequest(EReqType.FollowTarget) as RQFollowTarget;
					if(request != null)
					{
						PeEntity target = EntityMgr.Instance.Get(request.id);
						if(target != null)
						{
							Vector3 pos = target.position;
							Entity.NpcCmpt.Req_Remove(EReqType.FollowTarget);
                            Entity.NpcCmpt.Req_Translate(pos, true, false);
                            Entity.NpcCmpt.Req_FollowTarget(target.Id, request.targetPos, request.dirTargetID, request.tgtRadiu);
						}
					}
				}
			}
		}

        #region public interface
        public Request Req_UseSkill()
		{
			if (Net != null && !Net.hasOwnerAuth)
            {
                Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.UseSkill);
            }

            return Register(EReqType.UseSkill);
        }

        public Request Req_Translate(Vector3 position)
        {
            if (Net != null && !Net.hasAuth)
            {
                Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Translate, position);
            }

            return Register(EReqType.Translate, position);
        }

        public Request Req_PlayAnimation(string name, float time)
        {
            if (Net != null && !Net.hasOwnerAuth)
            {
                Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Animation, name, time);
            }

            return Register(EReqType.Animation, name, time);
        }

        public Request Req_MoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
        {
			if (Net != null && !Net.hasOwnerAuth && Entity != null && Entity.viewCmpt != null && Entity.viewCmpt.hasView)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp,(int)EReqType.MoveToPoint,position,stopRadius,isForce,(int)state);
			}
			else if(Net != null && !Net.hasAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.MoveToPoint, position, stopRadius, isForce, (int)state);
			}

            return Register(EReqType.MoveToPoint, position, stopRadius, isForce, state);
        }

		public Request Req_TalkMoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
		{
			if (Net != null && !Net.hasOwnerAuth && Entity != null && Entity.viewCmpt != null && Entity.viewCmpt.hasView)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp,(int)EReqType.TalkMove,position,stopRadius,isForce,(int)state);
			}
			else if(Net != null && !Net.hasAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.TalkMove, position, stopRadius, isForce, (int)state);
			}
			
			return Register(EReqType.TalkMove, position, stopRadius, isForce, state);
		}

        public Request Req_FollowTarget(int targetId,bool bNet = false, bool lostController = true)
		{
			if(Net != null && !bNet)
			{
			    Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.FollowTarget, targetId);					
			}

            PeEntity entity = EntityMgr.Instance.Get(targetId);
            if (entity == null || Contains(EReqType.FollowTarget))
                return null;

            return Register(EReqType.FollowTarget, targetId);
		}

        public Request Req_FollowPath(Vector3[] path, bool isLoop,SpeedState state = SpeedState.Run)
        {
			if (Net != null && !Net.hasOwnerAuth && Entity != null && Entity.viewCmpt != null && Entity.viewCmpt.hasView)
            {
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.FollowPath, path, isLoop,state);
            }

            return Register(EReqType.FollowPath, path, isLoop,state);
        }

        public Request Req_Rotation(Quaternion rotation)
        {
            return Register(EReqType.Rotate, rotation);
        }

        public Request Req_SetIdle(string name)
        {
            return Register(EReqType.Idle, name);
        }

        public Request Req_Dialogue(params object[] objs)
		{
            return Register(EReqType.Dialogue, objs);
		}

        public Request Req_PauseAll()
        {
            return Register(EReqType.PauseAll);
        }
        #endregion
    }

    public enum EReqType
    {
        Idle,
        Animation,
        MoveToPoint,
        FollowPath,
        FollowTarget,
        Salvation,
        Dialogue,
        Translate,
        Rotate,
        Attack,
        PauseAll,
		UseSkill,
		TalkMove,
		Hand,
        Remove,
		MAX,
    }

    public enum EReqRelation
    {
        None        = 0,
        Block       = 1,
        Blocked     = -1,
        Delete      = 2,
        Deleted     = -2
    }

    public enum EReqMask
    {
        None                = 0,
        Blocking            = 1,
        Stucking            = 2
    }

    public class RequestRelation
    {
        public int id;
        public string name;

        public int[] relations;

        static List<RequestRelation> m_RelationList = new List<RequestRelation>();

        public static void LoadData()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AI_ActionRelation");

            int fieldCount = reader.FieldCount - 2;

            while (reader.Read())
            {
                RequestRelation relation = new RequestRelation();
                relation.relations = new int[fieldCount];
                relation.id = reader.GetInt32(0);
                relation.name = reader.GetString(1);

                for (int i = 0; i < relation.relations.Length; i++)
                {
                    relation.relations[i] = reader.GetInt32(i + 2);
                }

                RequestRelation exist = m_RelationList.Find(ret => ret.id == relation.id || ret.name == relation.name);
                if (exist != null)
                {
                    Debug.LogError("The same relation ID = " + relation.id + " is exist!");
                }

                m_RelationList.Add(relation);
            }
        }

        public static int GetRelationValue(int src, int dst)
        {
            RequestRelation relation = m_RelationList.Find(ret => ret.id == src);
            if (relation == null)
            {
                Debug.LogError("Can't find relation data: " + src);
                return 0;
            }

            if (dst < 0 || dst >= relation.relations.Length)
            {
                Debug.LogError("Can't find relation : " + dst);
                return 0;
            }

            return relation.relations[dst];
        }

        public static int GetRelationValue(string srcName, string dstName)
        {
            RequestRelation relation = m_RelationList.Find(ret => ret.name == srcName);
            if (relation == null)
            {
                Debug.LogError("Can't find relation data: " + srcName);
                return 0;
            }

            int dst = Name2ID(dstName);
            if (dst < 0 || dst >= relation.relations.Length)
            {
                Debug.LogError("Can't find relation : " + dst);
                return 0;
            }

            return relation.relations[dst];
        }

        public static int Name2ID(string name)
        {
            RequestRelation relation = m_RelationList.Find(ret => ret.name == name);
            return relation != null ? relation.id : -1;
        }

        public static string ID2Name(int id)
        {
            RequestRelation relation = m_RelationList.Find(ret => ret.id == id);
            return relation != null ? relation.name : "";
        }

        public static bool Contains(string name)
        {
            return m_RelationList.Find(ret => ret.name == name) != null;
        }

        public static bool Contains(int id)
        {
            return m_RelationList.Find(ret => ret.id == id) != null;
        }
    }

    public abstract class Request
    {
        int m_ID;
        string m_Name;

        EReqMask m_Mask;

        Dictionary<EReqType, EReqRelation> m_Relations = new Dictionary<EReqType, EReqRelation>();

        public int ID { get { return m_ID; } }
        public string Name { get { return m_Name; } }
        public EReqMask Mask { get { return m_Mask; } }

        public abstract EReqType Type { get; }
        public abstract void Serialize(BinaryWriter w);
        public abstract void Deserialize(BinaryReader r);

        public Request()
        {
            m_Mask = EReqMask.None;

            m_Name = Type.ToString();
            m_ID = RequestRelation.Name2ID(m_Name);
        }

        public void Addmask(EReqMask mask)
        {
            m_Mask |= mask;
        }

        public void RemoveMask(EReqMask mask)
        {
            m_Mask &= ~mask;
        }

        public void AddRelation(EReqType type, EReqRelation relation)
        {
            if (m_Relations.ContainsKey(type))
                m_Relations[type] = relation;
            else
                m_Relations.Add(type, relation);
        }

        public void RemoveRelation(EReqType type)
        {
            if (m_Relations.ContainsKey(type))
                m_Relations.Remove(type);
        }

        public EReqRelation GetRelation(Request request)
        {
            EReqRelation relation;
            if (request.GetRelation(Type, out relation))
            {
                return relation;
            }

            return (EReqRelation)RequestRelation.GetRelationValue((int)Type, (int)request.Type);
        }

        public bool CanRun()
        {
            return (m_Mask & (EReqMask.Blocking)) == 0 && Type != EReqType.Attack;
        }

		public bool CanWait()
		{
			return (m_Mask & (EReqMask.Stucking)) == 0;
		}

        bool GetRelation(EReqType type, out EReqRelation relation)
        {
            if (m_Relations.ContainsKey(type))
            {
                relation = m_Relations[type];
                return true;
            }

            relation = EReqRelation.None;
            return false;
        }
    }

    public class RQTranslate : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        public Vector3 position = Vector3.zero;
		public bool    adjust = true;

        public RQTranslate() : base()
        {

        }

        public RQTranslate(params object[] objs) : base()
        {
            position = (Vector3)objs[0];
			adjust = (bool)objs[1];
        }

        public override EReqType Type { get { return EReqType.Translate; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(position.x);
            w.Write(position.y);
            w.Write(position.z);

			w.Write(adjust);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();

            position.x = r.ReadSingle();
            position.y = r.ReadSingle();
            position.z = r.ReadSingle();

			adjust = r.ReadBoolean();
        }
    }

    public class RQRotate : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        public Quaternion rotation = Quaternion.identity;

        public RQRotate() : base()
        {

        }

        public RQRotate(params object[] objs) : base()
        {
            rotation = (Quaternion)objs[0];
        }

        public override EReqType Type { get { return EReqType.Rotate; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(rotation.x);
            w.Write(rotation.y);
            w.Write(rotation.z);
            w.Write(rotation.w);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();

            rotation.x = r.ReadSingle();
            rotation.y = r.ReadSingle();
            rotation.z = r.ReadSingle();
            rotation.w = r.ReadSingle();
        }
    }
	
    public class RQIdle : Request
    {
		public enum RQidleType
		{
			Idle = 0,
			InjuredSit,
			InjuredRest,
			BeCarry,
			InjuredSitEX,
            Lie,
			Max
		}

		const int Version_001 = 0;
		const int Version_Current = Version_001;

        public string state = "";

        public RQIdle() : base()
        {

        }

        public RQIdle(params object[] objs) : base()
        {
            state = (string)objs[0];
        }

        public override EReqType Type { get { return EReqType.Idle; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(state);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();

            state = r.ReadString();
        }
    }

    public class RQAnimation : Request
    {
        const int Version_001 = 0;
		const int Version_002 = 1;
		const int Version_Current = Version_002;

        public string animName = "";
        public float animTime = 0.0f;
		public bool  play = true;

        public RQAnimation() : base()
        {

        }

        public RQAnimation(params object[] objs) : base()
        {
            animName = (string) objs[0];
            animTime = (float)  objs[1];

			if(objs.Length >= 3)
			   play  = (bool)objs[2]; 
        }

        public override EReqType Type { get { return EReqType.Animation; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(animName);
            w.Write(animTime);

			if(Version_Current >= Version_002)
			  w.Write(play);
        }

        public override void Deserialize(BinaryReader r)
        {
            int version = r.ReadInt32();

            animName = r.ReadString();
            animTime = r.ReadSingle();

			if(version >= Version_Current)
			   play = r.ReadBoolean();
        }
    }

    public class RQState : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        public string animName = "";

        public RQState() : base()
        {

        }

        public RQState(params object[] objs) : base()
        {
            animName = (string)objs[0];
        }

        public override EReqType Type { get { return EReqType.Animation; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(animName);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();

            animName = r.ReadString();
        }
    }

    public class RQMoveToPoint : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        public Vector3 position = Vector3.zero;
        public float stopRadius = 0.0f;
        public bool isForce = false;
        public SpeedState speedState;

        public RQMoveToPoint() : base()
        {

        }

        public RQMoveToPoint(params object[] objs) : base()
        {
            position        = (Vector3)     objs[0];
            stopRadius      = (float)       objs[1];
            isForce         = (bool)        objs[2];
            speedState      = (SpeedState)  objs[3];
        }

        public void ReachPoint(PeEntity npc)
        {
            if (StroyManager.Instance != null)
                StroyManager.Instance.EntityReach(npc, true);
        }

        public override EReqType Type { get { return EReqType.MoveToPoint; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(position.x);
            w.Write(position.y);
            w.Write(position.z);

            w.Write(stopRadius);
            w.Write(isForce);
            w.Write((int)speedState);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();

            position.x = r.ReadSingle();
            position.y = r.ReadSingle();
            position.z = r.ReadSingle();

            stopRadius = r.ReadSingle();
            isForce = r.ReadBoolean();
            speedState = (SpeedState)r.ReadInt32();
        }
    }

	public class RQTalkMove : Request
	{
		const int Version_001 = 0;
		const int Version_Current = Version_001;

		public Vector3 position = Vector3.zero;
		public float stopRadius = 0.0f;
		public bool isForce = false;
		public SpeedState speedState;

		public RQTalkMove() : base()
		{
			
		}

		public RQTalkMove(params object[] objs) : base()
		{
			position        = (Vector3)     objs[0];
			stopRadius      = (float)       objs[1];
			isForce         = (bool)        objs[2];
			speedState      = (SpeedState)  objs[3];
		}

		public override EReqType Type { get { return EReqType.TalkMove; } }

		public void ReachPoint(PeEntity npc)
		{
			if (GameUI.Instance != null && GameUI.Instance.mNPCTalk != null)
				GameUI.Instance.mNPCTalk.NpcReachToTalk(npc);
		}

		public override void Serialize(BinaryWriter w)
		{
			w.Write(Version_Current);
			
			w.Write(position.x);
			w.Write(position.y);
			w.Write(position.z);
			
			w.Write(stopRadius);
			w.Write(isForce);
			w.Write((int)speedState);
		}
		
		public override void Deserialize(BinaryReader r)
		{
			r.ReadInt32();
			
			position.x = r.ReadSingle();
			position.y = r.ReadSingle();
			position.z = r.ReadSingle();
			
			stopRadius = r.ReadSingle();
			isForce = r.ReadBoolean();
			speedState = (SpeedState)r.ReadInt32();
		}
	}

    public class RQFollowPath : Request
    {
        const int Version_001 = 0;
		const int Version_002 = 1;
		const int Version_Current = Version_002;
        Vector3[] verifPos = new Vector3[2];
        public Vector3[] VerifPos
        {
            get
            {
                return verifPos;
            }
        }

        public Vector3[] path = new Vector3[0];
        public bool isLoop = false;
		public SpeedState speedState = SpeedState.Run;

        public RQFollowPath() : base()
        {

        }

        public RQFollowPath(params object[] objs) : base()
        {
            path    = (Vector3[])   objs[0];
            isLoop  = (bool)        objs[1];
			speedState = (SpeedState)  objs[2];
            if(path.Length != 0)
            {
                verifPos[0] = path[0];
                verifPos[1] = path[path.Length - 1];
            }   
        }

        public override EReqType Type { get { return EReqType.FollowPath; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(path.Length);

            for (int i = 0; i < path.Length; i++)
            {
                w.Write(path[i].x);
                w.Write(path[i].y);
                w.Write(path[i].z);
            }

            w.Write(isLoop);

			if(Version_Current >= Version_002)
				w.Write((int)speedState);
        }

        public override void Deserialize(BinaryReader r)
        {
            int version = r.ReadInt32();

            int length = r.ReadInt32();

            path = new Vector3[length];

            for (int i = 0; i < length; i++)
            {
                path[i].x = r.ReadSingle();
                path[i].y = r.ReadSingle();
                path[i].z = r.ReadSingle();
            }

            isLoop = r.ReadBoolean();

			if(version >= Version_002)
			   speedState = (SpeedState)r.ReadInt32();
        }
        public bool Equal( Vector3[] pos)
        {
            if (pos.Length != 2)
                return false;
            if (Mathf.Abs(pos[0].x - verifPos[0].x) > 2f)
                return false;
            if (Mathf.Abs(pos[0].y - verifPos[0].y) > 2f)
                return false;
            if (Mathf.Abs(pos[0].z - verifPos[0].z) > 2f)
                return false;
            if (Mathf.Abs(pos[1].x - verifPos[1].x) > 2f)
                return false;
            if (Mathf.Abs(pos[1].y - verifPos[1].y) > 2f)
                return false;
            if (Mathf.Abs(pos[1].z - verifPos[1].z) > 2f)
                return false;
            return true;
        }
    }

    public class RQFollowTarget : Request
    {
        const int Version_001 = 0;
        const int Version_002 = 1;
        const int Version_Current = Version_002;

        public int id;

        public float tgtRadiu;
        public int dirTargetID;
        public Vector3 targetPos;
        public RQFollowTarget() : base()
        {

        }

        public RQFollowTarget(params object[] objs) : base()
        {
            id = (int)objs[0];
            targetPos = (Vector3)objs[1];
            dirTargetID = (int)objs[2];
            tgtRadiu = (float)objs[3];
            
        }

        public override EReqType Type { get { return EReqType.FollowTarget; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(id);
            w.Write(targetPos.x);
            w.Write(targetPos.y);
            w.Write(targetPos.z);

            w.Write(dirTargetID);
            w.Write(tgtRadiu);
            
        }

        public override void Deserialize(BinaryReader r)
        {
            int version = r.ReadInt32();

            id = r.ReadInt32();
            if(version >= Version_002)
            {
                targetPos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                dirTargetID = r.ReadInt32();
                tgtRadiu = r.ReadSingle();
            }
        }
    }

    public class RQSalvation : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        public int id = 0;
        public bool carry;

        public Vector3 m_Direction;

        public RQSalvation() : base()
        {

        }

        public RQSalvation(params object[] objs) : base()
        {
            id      = (int)     objs[0];
            carry   = (bool)    objs[1];
        }

        public override EReqType Type { get { return EReqType.Salvation; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);

            w.Write(id);
            w.Write(carry);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();

            id = r.ReadInt32();
            carry = r.ReadBoolean();
        }
    }

    public class RQDialogue : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

		public string RqAction;
		public bool hasDone;
		object Obj;
		Vector3 m_RqRatePos;

		public Vector3 RqRatePos
		{
			get
			{
				if(Obj != null)
				{
					if(Obj is Vector3)
					{
						return (Vector3)Obj;
					}
					else if(Obj is PeTrans)
					{
						return ((PeTrans)Obj).position;
					}
				}

				if(PeCreature.Instance.mainPlayer != null && PeCreature.Instance.mainPlayer.peTrans != null)
					return PeCreature.Instance.mainPlayer.peTrans.position;

				return Vector3.zero;
			}
		}

        public RQDialogue() : base()
        {

        }

		public RQDialogue(params object[] objs)
            : base()
        {
			RqAction = (string)objs[0];
			hasDone = false;
			if(objs.Length >1)
			{
				if(objs[1] == null && PeCreature.Instance.mainPlayer != null)  
					Obj = PeCreature.Instance.mainPlayer.peTrans;

				Obj = objs[1];
			}
        }

		public bool CanDoAction()
		{
			if(RqAction != "" && RqAction != "0" && !hasDone)
			{
				hasDone = true;
				return true;
			}
			return false;
		}

		public bool CanEndAction()
		{
			if(RqAction != "" && hasDone)
			{
				hasDone = false;
				return true;
			}
			return false;
		}

        public override EReqType Type { get { return EReqType.Dialogue; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();
        }
    }

    public class RQAttack : Request
    {
        const int Version_001 = 0;
        const int Version_Current = Version_001;

        public RQAttack() : base() { }

        public override EReqType Type { get { return EReqType.Attack; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();
        }
    }

	public class RQPauseAll : Request
	{
		const int Version_001 = 0;
        const int Version_Current = Version_001;

		public RQPauseAll(): base() { }

		public override EReqType Type { get { return EReqType.PauseAll; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();
        }
	}

	public class RQUseSkill : Request
	{
		const int Version_001 = 0;
        const int Version_Current = Version_001;

		public RQUseSkill(): base() { }
		
		public override EReqType Type { get { return EReqType.UseSkill; } }

        public override void Serialize(BinaryWriter w)
        {
            w.Write(Version_Current);
        }

        public override void Deserialize(BinaryReader r)
        {
            r.ReadInt32();
        }
	}
}

