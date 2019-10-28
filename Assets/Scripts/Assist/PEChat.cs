using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PEChat
{
    public class Chat
    {
        int m_Initiator;
        float m_ChatRadius;
        float m_JoinRadius;
        Vector3 m_Position;

        public int Initiator { get { return m_Initiator; } }
        public Vector3 Position { get { return m_Position; } }
        public float ChatRadius { get { return m_ChatRadius; } }
        public float JoinRadius { get { return m_JoinRadius; } }

        List<int> m_ChatIDs;

        public Chat(int initiator, Vector3 pos, float minRadius, float maxRadius)
        {
            m_Initiator = initiator;
            m_Position = pos;
            m_ChatRadius = minRadius;
            m_JoinRadius = maxRadius;

            m_ChatIDs = new List<int>();
            m_ChatIDs.Add(m_Initiator);
        }

        public void Join(int id)
        {
            if (!m_ChatIDs.Contains(id))
                m_ChatIDs.Add(id);
        }

        public void Exit(int id)
        {
            if (m_ChatIDs.Contains(id))
                m_ChatIDs.Remove(id);
        }
    }

    static List<Chat> s_Chats = new List<Chat>();

    public static Chat CreatChat(int initiator, Vector3 pos, float minRadius, float maxRadius)
    {
        Chat chat = new Chat(initiator, pos, minRadius, maxRadius);
        s_Chats.Add(chat);
        return chat;
    }

    public static Chat GetChat(Vector3 pos)
    {
        return s_Chats.Find(ret => PETools.PEUtil.SqrMagnitudeH(ret.Position, pos) < ret.JoinRadius*ret.JoinRadius);
    }
}
