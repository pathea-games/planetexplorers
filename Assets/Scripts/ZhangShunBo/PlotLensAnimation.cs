using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine.Events;

public class PlotLensAnimation
{
    static int plotClipNum;
    static bool isPlaying = false;

    public static bool IsPlaying
    {
        get { return PlotLensAnimation.isPlaying; }
        set { PlotLensAnimation.isPlaying = value; }
    }

    static List<CameraInfo> plotClips;
    static Dictionary<int, CutsceneClip> talkId_cut;
    static Dictionary<int, CutsceneClip> TalkId_cut
    {
        get
        {
            if (talkId_cut == null)
                talkId_cut = new Dictionary<int, CutsceneClip>();
            return talkId_cut;
        }
        set { talkId_cut = value; }
    }
    public static void PlotPlay(List<CameraInfo> clipsID)
    {
        if (isPlaying == false)
        {
            plotClips = clipsID;
            plotClipNum = 0;
            CutsceneClip first = Cutscene.PlayClip(plotClips[plotClipNum].cameraId);
			TalkId_cut[plotClips[plotClipNum].talkId] = first;	//  TalkId_cut.Add(plotClips[plotClipNum].talkId, first);
            first.onArriveAtEnding.AddListener(PlotNextPlay);
            isPlaying = true;

            if (Pathea.PeCreature.Instance.mainPlayer != null)
            {
                Pathea.PeCreature.Instance.mainPlayer.motionMgr.DoAction(Pathea.PEActionType.Cutscene);
                //Pathea.PeEntityExt.PeEntityExt.SetInvincible(Pathea.PeCreature.Instance.mainPlayer, true);
                Pathea.PESkEntity skentity = Pathea.PeCreature.Instance.mainPlayer.peSkEntity;
                skentity.SetAttribute(Pathea.AttribType.CampID, 28);
                skentity.SetAttribute(Pathea.AttribType.DamageID, 28);
            }
        }
        else
            plotClips.AddRange(clipsID);
    }

	public static bool TooFar(List<int> clipsID)
	{
		foreach(var iter in clipsID)
		{
			if(Cutscene.TooFar(iter))
				return true;
		}
		return false;
	}

    private static void PlotNextPlay() 
    {
        plotClipNum++;
        if (plotClipNum < plotClips.Count)
        {
            CutsceneClip notFirst = Cutscene.PlayClip(plotClips[plotClipNum].cameraId);
			TalkId_cut[plotClips[plotClipNum].talkId] = notFirst;	//TalkId_cut.Add(plotClips[plotClipNum].talkId, notFirst);
            notFirst.onArriveAtEnding.AddListener(PlotNextPlay);
        }
        else
        {
            if (Pathea.PeCreature.Instance.mainPlayer != null)
            {
                Pathea.PeCreature.Instance.mainPlayer.motionMgr.EndAction(Pathea.PEActionType.Cutscene);
                //Pathea.PeEntityExt.PeEntityExt.SetInvincible(Pathea.PeCreature.Instance.mainPlayer, false);
                Pathea.PESkEntity skentity = Pathea.PeCreature.Instance.mainPlayer.peSkEntity;
                skentity.SetAttribute(Pathea.AttribType.CampID, 1);
                skentity.SetAttribute(Pathea.AttribType.DamageID, 1);
            }
            plotClipNum = 0;
            TalkId_cut.Clear();
            isPlaying = false;
        }
    }

    public static void CheckIsStopCamera(int talkId) 
    {
        if (talkId == 0)
            return;
        if (!TalkId_cut.ContainsKey(talkId))
            return;
        GameObject.Destroy(TalkId_cut[talkId].gameObject);
        TalkId_cut.Remove(talkId);
    }
}
