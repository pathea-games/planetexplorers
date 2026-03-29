// Stub implementations for uLobby lobby/matchmaking library.
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;

namespace uLobby
{
    // PublicKey: RSA public key for secure lobby connections.
    public class PublicKey
    {
        public PublicKey() { }
        public PublicKey(string xmlKey) { }
    }

    public enum LobbyConnectionError
    {
        None,
        AlreadyConnectedToAnotherLobby,
        ConnectionFailed,
        RSAPublicKeyMismatch,
        CreateSocketOrThreadFailure,
        ConnectionTimeout
    }

    public enum LobbyConnectionStatus { Disconnected, Connecting, Connected }

    public class LobbyMessageInfo
    {
        public string sender;
    }

    public class ServerInfo
    {
        public string ip;
        public int port;
        public string name;
        public string host;
        public byte[] data;
    }

    public class ServerRegistry
    {
        public static void Subscribe(object listener) { }
        public static void Unsubscribe(object listener) { }
        public static System.Collections.Generic.List<ServerInfo> GetServers() => new System.Collections.Generic.List<ServerInfo>();
    }

    public class Lobby
    {
        public static void Connect(string ip, int port) { }
        public static void ConnectAsClient(string ip, int port) { }
        public static void Disconnect() { }
        public static bool isConnected => false;
        public static bool IsConnected => false;
        public static LobbyConnectionStatus connectionStatus => LobbyConnectionStatus.Disconnected;
        public static PublicKey publicKey { get; set; }
        public static object lobby { get; set; }
        public static void CallRPC(string name, params object[] args) { }
        public static void RPC(string name, params object[] args) { }
        public static void AddListener(object listener) { }
        public static event Action OnConnected;
        public static event Action OnDisconnected;
        public static event Action<LobbyConnectionError> OnFailedToConnect;
    }
}
