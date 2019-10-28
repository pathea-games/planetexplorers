using UnityEngine;
using System.Collections;

public class SceneMediator : MonoBehaviour
{
    //private static string s_LastScene;
    //private static string s_TargetScene;
    //public delegate void SceneLoadDelegate(string last_scene, string next_scene);
    //public static event SceneLoadDelegate OnSceneLoad;
    //public int m_Frame = 0;

    //public static void LoadScene (string scene_name)
    //{
    //    s_LastScene = Application.loadedLevelName;
    //    s_TargetScene = scene_name;
    //    //GlobalBehaviour.ExitGame(s_LastScene);
    //    if ( s_TargetScene == GameConfig.MainSceneName 
    //        || s_TargetScene == GameConfig.AdventureSceneName
    //        || s_TargetScene == GameConfig.BuildSceneName
    //        || s_TargetScene == GameConfig.ClientSceneName )
    //    {
    //        Application.LoadLevel("GameSceneMediator");
    //    }
    //    else
    //    {
    //        Application.LoadLevel(scene_name);
    //        s_LastScene = "";
    //        s_TargetScene = "";
    //    }
    //}

    //void Start ()
    //{
    //    m_Frame = 0;
    //}
    //void Update ()
    //{
    //    if ( m_Frame == 0 )
    //    {
    //        //GlobalBehaviour.BeforeReadRecord(s_TargetScene);
    //        GameTime.Timer.Reset();
    //        GameTime.PlayTime.Reset();

    //        bool IsNewGame = false;
    //        if (!GameConfig.IsMultiMode)
    //        {
    //            IsNewGame = GameConfig.IsNewGame;
    //            if (GameConfig.IsNewGame)
    //            {
    //                if (LocalDatabase.m_BaseInfo != null && PlayerFactory.self != null && PlayerFactory.self.NewBieFlag == 1)
    //                    LocalDatabase.m_BaseInfo.ResetRecord();

    //                Record.ReadRecord(LocalDatabase.m_BaseInfo, -100);
    //            }
    //            else
    //            {
    //                if (LocalDatabase.m_bLoadRecord && LocalDatabase.m_BaseInfo != null)
    //                    LocalDatabase.m_BaseInfo.Destroy();

    //                NpcMissionDataRepository.Reset();

    //                Record.ReadRecord(LocalDatabase.m_BaseInfo, LocalDatabase.m_RecordIdx);
    //            }
    //        }

    //        //GlobalBehaviour.InitGame(s_TargetScene, IsNewGame);
    //        if ( OnSceneLoad != null )
    //            OnSceneLoad(s_LastScene, s_TargetScene);
    //    }
    //    else if ( m_Frame == 1 )
    //    {
    //        if ( s_TargetScene.Length > 0 )
    //            Application.LoadLevel(s_TargetScene);
    //        s_LastScene = "";
    //        s_TargetScene = "";
    //    }
    //    m_Frame++;
    //}
}
