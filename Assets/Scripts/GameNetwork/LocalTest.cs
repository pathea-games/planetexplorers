
#if LOCALTEST
using UnityEngine;
using System.IO;

public class ClientLocalTest
{
    [RuntimeInitializeOnLoadMethod]
    public static void OnLoad()
    {
        GameObject go = new GameObject("LocalTest");
        go.AddComponent<LocalTest>();
    }
}

[DisallowMultipleComponent]
public class LocalTest : MonoBehaviour
{
	public ulong steamId;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        uLobby.Lobby.AddListener(this);
        uLobby.Lobby.OnConnected += Lobby_OnConnected;
		string idPath = Path.Combine("./", "idtxt.txt");
		if (File.Exists(idPath))
		{
			string idStr = File.ReadAllText(idPath, System.Text.Encoding.UTF8);
			steamId = ulong.Parse(idStr);
		}
	}

    void Lobby_OnConnected()
    {
        RequestTestLogin(steamId);
    }

    void RequestTestLogin(ulong id)
    {
		LobbyInterface.LobbyRPC(ELobbyMsgType.TestLogin, id);
    }
}

#endif