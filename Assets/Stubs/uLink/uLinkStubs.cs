// Stub implementations for uLink networking library.
// uLink has been replaced by a managed C# networking layer (PER-23),
// but some files still reference uLink types directly.
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;
using UnityEngine;

// [RPC] attribute used without `using uLink;` in some game files.
// Defining only RPCAttribute at global scope (not RPC) avoids CS1614 ambiguity.
// Files with `using uLink;` will find uLink.RPCAttribute for [RPC].
[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
public class RPCAttribute : System.Attribute { }

namespace uLink
{
    public class MonoBehaviour : UnityEngine.MonoBehaviour { }

    public class BitStream
    {
        public T Read<T>() => default;
        public bool TryRead<T>(out T value) { value = default; return false; }
        public void Write<T>(T value) { }
        public bool isTypesafe => false;
        public bool ReadBoolean() => false;
        public int ReadInt32() => 0;
        public float ReadSingle() => 0f;
        public double ReadDouble() => 0.0;
        public byte[] ReadBytes() => System.Array.Empty<byte>();
        public void WriteBoolean(bool value) { }
        public void WriteInt32(int value) { }
        public void WriteSingle(float value) { }
        public void WriteDouble(double value) { }
        public void WriteBytes(byte[] bytes) { }
    }

    // Codec delegate types matching the uLink BitStreamCodec API.
    // Serialize: void(BitStream stream, object obj, params object[] options)
    // Deserialize: object(BitStream stream, params object[] options)
    public delegate void BitStreamSerializer(BitStream stream, object obj, params object[] codecOptions);
    public delegate object BitStreamDeserializer(BitStream stream, params object[] codecOptions);

    public class BitStreamCodec
    {
        // Accepts any uLink-style serialize/deserialize method pair.
        public static void AddAndMakeArray<T>(BitStreamDeserializer deserializer, BitStreamSerializer serializer) { }
    }

    public class NetworkMessageInfo
    {
        public double timestamp => 0;
        public NetworkView networkView => null;
        public NetworkPlayer sender => default;
    }

    public class NetworkP2PMessageInfo
    {
        public double timestamp => 0;
        public NetworkPlayer sender => default;
    }

    public struct NetworkPlayer : IEquatable<NetworkPlayer>
    {
        public bool Equals(NetworkPlayer other) => false;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
        public static bool operator ==(NetworkPlayer a, NetworkPlayer b) => false;
        public static bool operator !=(NetworkPlayer a, NetworkPlayer b) => true;
    }

    public struct NetworkViewID : IEquatable<NetworkViewID>
    {
        public int id;
        public bool Equals(NetworkViewID other) => false;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
        public static readonly NetworkViewID unassigned = default;
        public static bool operator ==(NetworkViewID a, NetworkViewID b) => false;
        public static bool operator !=(NetworkViewID a, NetworkViewID b) => true;
    }

    public class NetworkView : UnityEngine.MonoBehaviour
    {
        public NetworkViewID viewID => default;
        public NetworkPlayer owner => default;
        public bool isMine => false;
        public bool isOwner => false;
        public bool isProxy => false;
        public BitStream initialData => null;
        public void RPC(string name, RPCMode mode, params object[] args) { }
        public void RPC(string name, NetworkPlayer target, params object[] args) { }
        public void UnreliableRPC(string name, RPCMode mode, params object[] args) { }
        public void UnreliableRPC(string name, NetworkPlayer target, params object[] args) { }
        public static NetworkView Get(UnityEngine.Component component) => null;
        public static NetworkView Get(UnityEngine.GameObject go) => null;
        public static NetworkView Find(NetworkViewID viewID) => null;
    }

    public enum RPCMode { All, Others, Server, AllBuffered, OthersBuffered, Owner }

    public enum NetworkBufferedRPC { None }

    public enum NetworkPeerType { Disconnected, Server, Client, Connecting }

    public enum NetworkStatus { Disconnected, Connecting, Connected }

    public enum NetworkConnectionError { NoError, RSAPublicKeyMismatch, InvalidPassword, ConnectionFailed, LimitedPlayers, ConnectionBanned }

    public enum NetworkDisconnection { LostConnection, Disconnected }

    public enum MasterServerEvent { HostListReceived, RegistrationSucceeded, RegistrationFailedGameName }

    public enum NetworkLogFlags { None, BadMessage, InformationMessages, Debug, Full, AuthoritativeServer }

    public enum NetworkLogLevel { Off, Informational, Debug, Full }

    // Delegate type for uLink NetworkLog writer callbacks.
    public delegate void NetworkLogWriter(NetworkLogFlags flags, object[] args);

    public class NetworkLog
    {
        public static NetworkLogFlags minLevel = NetworkLogFlags.None;
        // Writers accept (NetworkLogFlags, object[]) delegates
        public static NetworkLogWriter infoWriter { get; set; }
        public static NetworkLogWriter errorWriter { get; set; }
        public static NetworkLogWriter warningWriter { get; set; }
        public static void Info(string message) { }
        public static void Info(NetworkLogFlags flags, params object[] args) { }
        public static void Error(string message) { }
        public static void Error(NetworkLogFlags flags, params object[] args) { }
        public static void Warning(string message) { }
        public static void Warning(NetworkLogFlags flags, params object[] args) { }
        public static void SetLevel(NetworkLogFlags flags) { }
        public static void SetLevel(NetworkLogFlags flags, NetworkLogLevel level) { }
    }

    public class NetworkLogUtility
    {
        public static string ObjectsToString(object[] args) => string.Empty;
    }

    public class NetworkUtility
    {
        public static bool IsPortAvailable(int port) => true;
        public static int FindAvailablePort(int startPort) => startPort;
        // 2-arg overload: MyServerManager.cs passes (startPort, endPort).
        public static int FindAvailablePort(int startPort, int endPort) => startPort;
    }

    // NetworkConfig: runtime configuration object exposed via Network.config
    public class NetworkConfig
    {
        public int timeoutDelay { get; set; }
        public int sendBufferSize { get; set; }
        public int receiveBufferSize { get; set; }
    }

    public class Network
    {
        public static NetworkPeerType peerType => NetworkPeerType.Disconnected;
        public static bool isServer => false;
        public static bool isClient => false;
        // Assignable in GameClientNetwork.cs
        public static bool isAuthoritativeServer { get; set; }
        public static NetworkPlayer player => default;
        public static NetworkStatus status => NetworkStatus.Disconnected;
        public static NetworkConfig config { get; set; } = new NetworkConfig();
        public static void Disconnect() { }
        public static void Disconnect(int timeout) { }
        public static NetworkConnectionError Connect(string IP, int port) => NetworkConnectionError.NoError;
        public static NetworkConnectionError Connect(string IP, int port, string password) => NetworkConnectionError.NoError;
        public static NetworkConnectionError Connect(string IP, int port, string password, params object[] objs) => NetworkConnectionError.NoError;
        public static NetworkConnectionError Connect(HostData host, string password, params object[] objs) => NetworkConnectionError.NoError;
        public static void CloseConnection(NetworkPlayer target, bool sendDisconnectionNotification) { }
        public static void DestroyPlayerObjects(NetworkPlayer playerID) { }
        public static void RemoveRPCs(NetworkPlayer playerID) { }
        public static int connections => 0;
        public static float sendRate { get; set; }
        // Assignable (game sets public key for secure connections)
        public static uLink.PublicKey publicKey { get; set; }
        public static bool requireSecurityForConnecting { get; set; }
        public static int minimumUsedViewIDs { get; set; }
        public static int minimumAllocatableViewIDs { get; set; }
        public static int maxManualViewIDs { get; set; }
    }

    public class NetworkP2P
    {
        // Instance-level methods (some game code uses NetworkP2P as an instance)
        public void Connect(string IP, int port) { }
        public void Disconnect() { }
        public void RPC(string name, NetworkPlayer target, params object[] args) { }
        // P2PManager.cs passes NetworkPeer (not NetworkPlayer) as second arg.
        public void RPC(string name, NetworkPeer target, params object[] args) { }
        // Static variants for code calling without an instance.
        public static void Connect(string IP, int port, bool dummy) { }
        public static void Disconnect(bool dummy) { }
        public static void RPC(string name, NetworkPlayer target, bool dummy, params object[] args) { }
    }

    // NetworkPeer: game code treats NetworkPeer and NetworkPlayer as interchangeable.
    // Provide implicit conversion and equality operators to allow cross-type comparison.
    public class NetworkPeer
    {
        public static implicit operator NetworkPeer(NetworkPlayer player) => new NetworkPeer();
        public static bool operator ==(NetworkPeer a, NetworkPlayer b) => false;
        public static bool operator !=(NetworkPeer a, NetworkPlayer b) => true;
        public static bool operator ==(NetworkPeer a, NetworkPeer b) => ReferenceEquals(a, b);
        public static bool operator !=(NetworkPeer a, NetworkPeer b) => !ReferenceEquals(a, b);
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }

    public class MasterServer
    {
        public static string ipAddress { get; set; }
        public static int port { get; set; }
        public static string password { get; set; }
        public static float updateRate { get; set; }
        public static void RegisterHost(string gameTypeName, string gameName) { }
        public static void UnregisterHost() { }
        public static void RequestHostList(string gameTypeName) { }
        public static HostData[] PollHostList() => Array.Empty<HostData>();
        public static void ClearHostList() { }
        public static void DiscoverLocalHosts(string gameTypeName) { }
        public static void DiscoverLocalHosts(string gameTypeName, int startPort, int endPort) { }
        public static HostData[] PollDiscoveredHosts() => Array.Empty<HostData>();
        public static void ClearDiscoveredHosts() { }
    }

    public class HostData
    {
        public string gameName;
        public string gameType;
        public string ip;
        // ipAddress: game code assigns to string field, so treat as string (not string[]).
        public string ipAddress;
        public int port;
        public bool passwordProtected;
        public int connectedPlayers;
        public int playerLimit;
        public bool useProxy;
        public int ping;
        public string comment;
    }

    public class PublicKey
    {
        public PublicKey() { }
        // Constructor from XML-encoded RSA key string
        public PublicKey(string xmlKey) { }
    }

    public class RegisterPrefabs : UnityEngine.MonoBehaviour { }

    // RPC attribute for marking networked methods. Named RPC (not RPCAttribute) to match
    // how game code uses [RPC] with `using uLink;` — C# attribute lookup strips the
    // "Attribute" suffix, so [RPC] resolves to uLink.RPCAttribute when using uLink is active.
    // Do not also define uLink.RPCAttribute here: that causes CS1614 ambiguity.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RPCAttribute : Attribute { }
}

// Extension method: uLobby.ServerInfo.data.GetRemainingBitStream() used in ServerRegistered.cs
// Defined at global scope (not in uLink namespace) so files without `using uLink;` can use it.
public static class ByteArrayExtensions
{
    public static uLink.BitStream GetRemainingBitStream(this byte[] data) => new uLink.BitStream();
}
