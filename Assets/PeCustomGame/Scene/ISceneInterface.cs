// Custom game mode:  Scene Interface
// (c) by Wu Yiqiu

using UnityEngine;
using System.IO;

namespace PeCustom
{
	public enum ESceneNoification
	{
		InitAgents,
        CreateAgent,
        RemoveSpawnPoint,
        EnableSpawnPoint,

		CreateMonster,
		CreateNpc,
		CreateDoodad,

		MonsterDead,
        DoodadDead,
		EntityDestroy,

        SceneBegin,
		SceneEnd
	}

	public interface ISceneController
	{
		HashBinder Binder { get; }

		void OnNotification(ESceneNoification msg_type, params object[] data);

	}
	

	public interface IMonoLike
	{
        void OnGUI();
		void Start();
		void Update();
		void OnDestroy();
	}
}