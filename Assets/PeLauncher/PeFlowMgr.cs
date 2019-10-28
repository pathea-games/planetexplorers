using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Pathea
{
    public class PeFlowMgr : PeSingleton<PeFlowMgr>
    {
        const string GameSceneName = "PeGame";
        const string IntroSceneName = "Intro";

        bool LoadUnityScene(string unitySceneName)
        {
            Application.LoadLevel(unitySceneName);
            return true;
        }

        public enum EPeScene
        {
            StartScene = 0,
            RoleScene,
            LobbyScene,
            MainMenuScene,
            ClientScene,
            CreationScene, 
            MultiRoleScene,
			GameScene,
            Intro,
            Max
        }

        string[] mSceneMap = new string[(int)EPeScene.Max]{
            GameConfig.StartSceneName
            ,GameConfig.RoleSceneName
            ,GameConfig.LobbySceneName
            ,GameConfig.MainMenuSceneName
            ,GameConfig.ClientSceneName
            ,GameConfig.CreationSceneName
            ,GameConfig.MultiRoleSceneName
            ,GameSceneName
            ,IntroSceneName
        };

        public EPeScene curScene
        {
            get;
            private set;
        }
        public void LoadScene(EPeScene ePeScene, bool save = true)
        {
            //lz-2016.06.23 加载场景的时候暂停声音
            AudioListener.pause=true;
            if (curScene == EPeScene.GameScene && save){
                AutoArchiveRunner.QuitSave();
            }

            curScene = ePeScene;
			SystemSettingData.Instance.ResetVSync ();

			Resources.UnloadUnusedAssets();
			GC.Collect();

            UILoadScenceEffect.Instance.EnableProgress(false);

			bool bNeedProgress = ePeScene == EPeScene.GameScene;
            UILoadScenceEffect.Instance.EndScence(delegate() {
				LoadUnityScene(mSceneMap[(int)ePeScene]);
				UILoadScenceEffect.Instance.BeginScence(null, bNeedProgress);
			},	bNeedProgress);	// Always enable progress UI for GameScene
		}
	}
}