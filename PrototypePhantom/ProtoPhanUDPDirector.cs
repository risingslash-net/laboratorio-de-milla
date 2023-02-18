using System;
using System.Collections.Generic;
using System.Text;
using MonoMod.Utils;
using Rewired.Demos;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.PrototypePhantom;

using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ProtoPhanUDPDirector : MonoBehaviour
{
    public UdpClient udpClient;
    public Thread udpThread;
    public static ProtoPhanUDPDirector Instance;

    public static List<string> receivedStrings = new List<string>();
    public static HashSet<IPEndPoint> endpointsToUpdate = new HashSet<IPEndPoint>();
    public static List<IPEndPoint> endpointLobbyServers = new List<IPEndPoint>();
    public static IPEndPoint endpointCurrentLobbyServer = null;

    public string currentSceneName = "";
    public string previousSceneName = "";

    public static int currentLobbyID = 0;
    public static int currentRoomID = 0;

    public static int defaultPhantomPort = 23913;
    public static int defaultLobbyPort = 20232;
    
    public static string playerName = "Phantom Chaser";
    public static string playerDiscriminator = "0000";
    
    public static PhantomLobbyResponse phantomLobbyResponse = null;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        //udpThread = new Thread(new ThreadStart(ReceiveData));
        udpThread = new Thread(ReceiveData);
        udpThread.IsBackground = true;      
        udpThread.Start(new object[]{Thread.CurrentThread, receivedStrings});
        
        Debug.Log("----------------------------------------");
        Debug.Log("Example:");
        Debug.Log(PhantomLobbyResponse.GenerateExampleJSON());
        Debug.Log("----------------------------------------");
    }

    public void Update()
    {
        if (CheckSceneChanged())
        {
            HandleSceneChanged();
        }

        if (receivedStrings != null && receivedStrings.Count > 0)
        {
            foreach (var str in receivedStrings)
            {
                try
                {
                    //Debug.Log("Received UDP message: " + str);
                    if (str.Contains("@UpPl"))
                    {
                        HandleReceivePlayerUpdate(str);
                    }
                    else if (str.Contains("player_names") && str.Contains("map_votes_values")) // Probably just just add a more specific key in the lobby response.
                    {
                        HandleReceivedUpdatedRoomState(str);
                    }
                }
                catch
                {
                    Debug.LogError("Something went wrong when handling received UDP messages.");
                    Debug.LogError(str);
                }
            }
            receivedStrings.Clear();
        }
    }

    void ReceiveData(object args)
    {
        var arrArgs = (object[])args;
        Thread mainThread = (Thread)arrArgs[0];
        List<string> receivedStringsLocal = (List<string>)arrArgs[1];
        udpClient = new UdpClient(defaultPhantomPort);
        while (true)
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.Receive(ref anyIP);
            if (data.Length > 4)
            {
                //var txt = data.ToString();
                var txt = Encoding.UTF8.GetString(data);
                //mainThread.
                receivedStringsLocal.Add(txt);
                if (receivedStringsLocal.Count > 50)
                {
                    receivedStringsLocal.RemoveAt(0);
                }

                Debug.Log("Received UDP message: " + txt);
            }
            // process data received here
        }
    }
    
    // Rewrite this to actually be async.
    void ReceiveDataAsync(object args)
    {
        var arrArgs = (object[])args;
        Thread mainThread = (Thread)arrArgs[0];
        udpClient = new UdpClient(defaultPhantomPort);
        while (true)
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.Receive(ref anyIP);
            if (data.Length > 4)
            {
                // process data received here
                var txt = data.ToString();
                //var txt = Encoding.Convert(Encoding.Default, Encoding.UTF8, data);
                //mainThread.
            }
        }
    }

    public void HandleReceivePlayerUpdate(string txt)
    {
        try
        {
            //Debug.Log("Received Phantom Player Update");
            //Debug.Log(txt);
            PhantomStatus updatedStatus = JsonUtility.FromJson<PhantomStatus>(txt);
            LivePhantom.UpdatePlayer(updatedStatus);
        }
        catch
        {
            
        }
    }
    
    public void HandleReceivedUpdatedRoomState(string jsonUpdatedRoomState)
    {
        try
        {
            Debug.Log(jsonUpdatedRoomState);
            Debug.Log("------------------");

            phantomLobbyResponse = JsonUtility.FromJson<PhantomLobbyResponse>(jsonUpdatedRoomState);
            Debug.Log(phantomLobbyResponse);

            if (phantomLobbyResponse.player_names != null && phantomLobbyResponse.player_names.Length > 0)
            {
                for (int i = 0; i < phantomLobbyResponse.player_names.Length; i++)
                {
                    if (phantomLobbyResponse.player_names[0] == null)
                    {
                        Debug.Log("Player object appears to be null.");
                        continue;
                    }
                    else
                    {
                        Debug.Log(phantomLobbyResponse.addressHosts[i]);
                        Debug.Log(phantomLobbyResponse.addressPorts[i]);
                        Debug.Log(phantomLobbyResponse.character_ids[i]);
                        Debug.Log(phantomLobbyResponse.readys[i]);
                    }

                    var playerEndpoint = GetIPEndPointFromHostName(phantomLobbyResponse.addressHosts[i]
                        ,int.Parse(phantomLobbyResponse.addressPorts[i]));
                    if (endpointsToUpdate.Contains(playerEndpoint))
                    {
                        continue;
                    }
                    endpointsToUpdate.Add(playerEndpoint);
                } 
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void SendData(byte[] data, string ipAddress, int remotePort)
    {
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, ipAddress, remotePort);
    }
    
    public void SendData(string text, string ipAddress, int remotePort)
    {
        var data = Encoding.UTF8.GetBytes(text);
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, ipAddress, remotePort);
    }
    
    public void SendData(string text)
    {
        var data = Encoding.UTF8.GetBytes(text);
        UdpClient client = new UdpClient();
        foreach (var endpoint in endpointsToUpdate)
        {
            // Potential gotcha: We're _assuming_ this port is open,
            // but we don't know that the remote _game_ is listening on the port the endpoint is sending from. 
            client.Send(data, data.Length, endpoint.Address.ToString(), defaultPhantomPort);
        }
    }
    
    public void SendDataToLobby(string text)
    {
        if (endpointCurrentLobbyServer == null)
        {
            return;
        }

        var data = Encoding.UTF8.GetBytes(text);
        UdpClient client = udpClient;
        if (udpClient == null)
        {
            client = new UdpClient(defaultPhantomPort);
        }

        client.Send(data, data.Length, endpointCurrentLobbyServer.Address.ToString(), endpointCurrentLobbyServer.Port);
    }
    
    public void SendDataLocal(string text)
    {
        var data = Encoding.UTF8.GetBytes(text);
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, "127.0.0.1", defaultPhantomPort);
    }
    
    public void TestSendData()
    {
        var data = Encoding.UTF8.GetBytes("Hello world.");
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, "127.0.0.1", defaultPhantomPort);
    }
    
    public void TestSendCommandToLobbyServer()
    {
        PhantomServerCommand.TestAddPlayer();
        var data = Encoding.UTF8.GetBytes(PhantomServerCommand.AddPlayer());
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, "127.0.0.1", defaultPhantomPort);
        client.Send(data, data.Length, "127.0.0.1", defaultLobbyPort); //Lobby Server
        
        data = Encoding.UTF8.GetBytes(PhantomServerCommand.RemovePlayer());
        client.Send(data, data.Length, "127.0.0.1", defaultPhantomPort);
        client.Send(data, data.Length, "127.0.0.1", defaultLobbyPort); //Lobby Server
    }
    
    public string AddPlayerToLobbyServer()
    {
        var jsonOut = "";
        var characterID = 0;
        if (FPStage.currentStage != null)
        {
            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpp != null)
            {
                characterID = (int)fpp.characterID;
            }
        }

        jsonOut = PhantomServerCommand.AddPlayer(0, 0
            , playerName, playerDiscriminator, characterID);
        var data = Encoding.UTF8.GetBytes(jsonOut);
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, endpointCurrentLobbyServer.Address.ToString(), endpointCurrentLobbyServer.Port);
        return jsonOut;
    }

    void OnApplicationQuit()
    {
        if (udpThread != null)
        {
            udpThread.Abort();
        }
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }

    public static ProtoPhanUDPDirector Instantiate()
    {
        var go = new GameObject("ProtoPhanUDPDirector");
        var instance = go.AddComponent<ProtoPhanUDPDirector>();
        endpointCurrentLobbyServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), defaultLobbyPort);
        return instance;
    }

    public static void AddConnectionToUpdate(string hostName, int port)
    {
        var endpoint = GetIPEndPointFromHostName(hostName, port);
        if (endpointsToUpdate.Contains(endpoint))
        {
            return;
        }

        endpointsToUpdate.Add(endpoint);
    }
    
    public static void AddConnectionToUpdate(string ipAddress)
    {
        AddConnectionToUpdate(ipAddress, defaultPhantomPort);
    }

    // Snippet from: https://stackoverflow.com/questions/2101777/creating-an-ipendpoint-from-a-hostname
    public static IPEndPoint GetIPEndPointFromHostName(string hostName, int port, bool throwIfMoreThanOneIP = false)
    {
        var addresses = System.Net.Dns.GetHostAddresses(hostName);
        if (addresses.Length == 0)
        {
            throw new ArgumentException(
                "Unable to retrieve address from specified host name.", 
                "hostName"
            );
        }
        else if (throwIfMoreThanOneIP && addresses.Length > 1)
        {
            throw new ArgumentException(
                "There is more that one IP address to the specified host.", 
                "hostName"
            );
        }
        return new IPEndPoint(addresses[0], port); // Port gets validated here.
    }
    
    public static void AddLobbyServer(string hostName, int port)
    {
        var endPoint = GetIPEndPointFromHostName(hostName, port);
        if (endpointLobbyServers.Contains(endPoint))
        {
            return;
        }

        endpointLobbyServers.Add(endPoint);
        SetLobbyServer(0);
    }
    
    public static void AddLobbyServer(string ipAddress)
    {
        AddLobbyServer(ipAddress, defaultLobbyPort);
    }
    
    public static void SetLobbyServer(string hostname, int port)
    {
        endpointCurrentLobbyServer = GetIPEndPointFromHostName(hostname, port);
    }
    public static void SetLobbyServer(int index)
    {
        endpointCurrentLobbyServer = endpointLobbyServers[index];
    }

    public bool CheckSceneChanged()
    {
        bool changed = false;
        previousSceneName = currentSceneName;
        currentSceneName = SceneManager.GetActiveScene().name;
        
        changed = (!currentSceneName.Equals(previousSceneName));

        return changed;
    }

    public void HandleSceneChanged()
    {
        PhantomPlayerTracker.BindToMainPlayer();
        AddPlayerToLobbyServer();
        RequestUpdatedRoomState();
    }

    public string RequestUpdatedRoomState()
    {
        var reqJSON = "";
        try
        {
            var root = new PhantomLobbyRequestRoot();
            root.request.command = "get_room_status";
            root.request.args = new string[] { "0", "0"};
            reqJSON = JsonUtility.ToJson(root.request);
            Debug.Log(reqJSON);
            SendDataToLobby(reqJSON);
        }
        catch
        {
            Debug.LogError("Something blew up when trying to request the updated room state from the lobby, but we're going to ignore it.");
        }

        
        return reqJSON;
    }
    
    public struct Scrunch
    {
        public string command;
        public object[] args;
    }
    
    public struct ScrunchWrapSupreme
    {
        public Scrunch request;
    }

    public static void SetPlayerNameAndDiscriminator(string playerNameValue, string discriminatorValue)
    {
        playerName = playerNameValue;
        playerDiscriminator = discriminatorValue;
    }
}