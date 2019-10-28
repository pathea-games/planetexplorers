using System;
using System.Collections.Generic;
using UnityEngine;

//log:lz-2016.05.09 create it.
public class RoomInfo_N:MonoBehaviour
{
    public UILabel GameModeLabel;
    public UILabel GameTypeLabel;
    public UILabel MapNameLabel;
    public UILabel MapSizeLabel;
    public UILabel PlayerNumLabel;
    public UILabel MapInfoLabel;
    public UISlider IsoProcessSlider;
    public UILabel IsoSpeedLabel;
    public UILabel IsoCountLabel;

    #region public methods

    public void UpdateInfo()
    {
        this.UpdateGameMode();
        this.UpdateGameType();
        this.UpdateMapName();
        this.UpdateMapSize();
        this.UpdatePlayerCount();
        this.UpdateMapInfo("");
    }

    public void UpdateMapInfo(string info)
    {
        if (null != info)
            this.MapInfoLabel.text = info;
    }

    public void UpdateIsoProcess(float processVal)
    {
        if (processVal > 0)
            this.IsoProcessSlider.sliderValue = processVal;
    }

    public void UpdateIsoSpeed(string speedStr)
    {
        if (null != speedStr)
            this.IsoSpeedLabel.text = speedStr;
    }

    public void UpdateIsoCount(string countStr)
    {
        if (null != countStr)
            this.IsoCountLabel.text = countStr;
    }
    #endregion

    #region private methods

    private void UpdateGameMode()
    {
        switch (Pathea.PeGameMgr.sceneMode)
        {
            case Pathea.PeGameMgr.ESceneMode.Adventure:
            case Pathea.PeGameMgr.ESceneMode.Build:
            case Pathea.PeGameMgr.ESceneMode.Custom:
            case Pathea.PeGameMgr.ESceneMode.Story:
                GameModeLabel.text = Pathea.PeGameMgr.sceneMode.ToString();
                break;
        }
    }

    private void UpdateGameType()
    {
        switch (Pathea.PeGameMgr.gameType)
        {
            case Pathea.PeGameMgr.EGameType.Cooperation:
            case Pathea.PeGameMgr.EGameType.VS:
            case Pathea.PeGameMgr.EGameType.Survive:
                GameTypeLabel.text = Pathea.PeGameMgr.gameType.ToString();
                break;
        }
    }

    private void UpdateMapName()
    {
        this.MapNameLabel.text = GameClientNetwork.ServerName;
    }

    private void UpdateMapSize()
    {
        string mapSizeText = "";
        switch (RandomMapConfig.mapSize)
        {
            case 0:
                {
                    mapSizeText = "40km * 40km";
                    break;
                }
            case 1:
                {
                    mapSizeText = "20km * 20km";
                    break;
                }
            case 2:
                {
                    mapSizeText = "8km * 8km";
                    break;
                }
            case 3:
                {
                    mapSizeText = "4km * 4km";
                    break;
                }
            case 4:
                {
                    mapSizeText = "2km * 2km";
                    break;
                }
            default:
                break;
        }
        this.MapSizeLabel.text = mapSizeText;
    }

    private void UpdatePlayerCount()
    {
        this.PlayerNumLabel.text = (BattleManager.teamNum * BattleManager.numberTeam).ToString();
    }

    #endregion
}                  
