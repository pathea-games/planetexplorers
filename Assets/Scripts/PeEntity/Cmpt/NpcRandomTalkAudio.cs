using UnityEngine;
using Pathea;
using Pathea.PeEntityExt;
using System.Collections;
using System.Collections.Generic;

public class NpcRandomTalkAudio
{
    //static AudioController mAdController = null;
    static Dictionary<int, AudioController> contrillers = new Dictionary<int, AudioController>();
    static List<int> secnarioIDs = new List<int>();
    public static bool PlaySound(PeEntity npc, int caseId, int secnarioID)
    {
        if (AudioManager.instance == null)
            return false;

        if (secnarioIDs.Contains(caseId))
            return false;

        if (npc.NpcCmpt != null && npc.NpcCmpt.voiceType <= 0)
        {
            RandomNpcDb.Item item = RandomNpcDb.Get(npc.ProtoID);
            if (item != null && npc.commonCmpt != null)
               npc.ExtSetVoiceType(item.voiveMatch.GetRandomVoice(npc.commonCmpt.sex));
        }

         if (npc.NpcCmpt != null && npc.NpcCmpt.voiceType <= 0)
            return false;

        int audioID = NpcVoiceDb.GetVoiceId(secnarioID, npc.NpcCmpt.voiceType);
        if (audioID > 0&& null!=npc.peTrans)
        {
            //lz-2016.12.06 声音要跟着模型动，所以用模型的Trans
            AudioController adc =AudioManager.instance.Create(npc.position, audioID, npc.peTrans.realTrans, true, true);
            adc.DestroyEvent += OnDelete;
            contrillers.Add(caseId, adc);
            secnarioIDs.Add(caseId);
            return true;
        }

        // mAdController.PlayAudio();
        return false;
    }


    static void OnDelete(AudioController audioCtrl)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < secnarioIDs.Count; i++)
        {
            if (contrillers[secnarioIDs[i]] != null && contrillers[secnarioIDs[i]].Equals(audioCtrl)
                || contrillers[secnarioIDs[i]] != null && contrillers[secnarioIDs[i]].time >= contrillers[secnarioIDs[i]].length
                )
            {
                contrillers.Remove(secnarioIDs[i]);
                list.Add(secnarioIDs[i]);
                continue;
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            secnarioIDs.Remove(list[i]);
        }
        audioCtrl.DestroyEvent -= OnDelete;
    }

}
