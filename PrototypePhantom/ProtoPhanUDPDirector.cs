using System.Collections.Generic;
using System.Text;
using MonoMod.Utils;
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

    public string currentSceneName = "";
    public string previousSceneName = "";

    public int port = 23913;

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
    }

    public void Update()
    {
        if (CheckSceneChanged())
        {
            HandleSceneChanged();
        }

        if (receivedStrings != null && receivedStrings.Count > 0)
        {
            try
            {
                foreach (var str in receivedStrings)
                {
                    Debug.Log("Received UDP message: " + str);
                    if (str.Contains("@UpPl"))
                    {
                        HandleReceivePlayerUpdate(str);
                    }
                }
                receivedStrings.Clear();
            }
            catch
            {
                
            }
        }
    }

    void ReceiveData(object args)
    {
        var arrArgs = (object[])args;
        Thread mainThread = (Thread)arrArgs[0];
        List<string> receivedStringsLocal = (List<string>)arrArgs[1];
        udpClient = new UdpClient(port);
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
        udpClient = new UdpClient(port);
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
            Debug.Log("Received Phantom Player Update");
            Debug.Log(txt);
            PhantomStatus updatedStatus = JsonUtility.FromJson<PhantomStatus>(txt);
            LivePhantom.UpdatePlayer(updatedStatus);
        }
        catch
        {
            
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
        client.Send(data, data.Length, "127.0.0.1", port);
    }
    
    public void TestSendData()
    {
        var data = Encoding.UTF8.GetBytes("Hello world.");
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, "127.0.0.1", port);
    }
    
    public void TestSendCommandToLobbyServer()
    {
        PhantomServerCommand.TestAddPlayer();
        var data = Encoding.UTF8.GetBytes(PhantomServerCommand.AddPlayer());
        UdpClient client = new UdpClient();
        client.Send(data, data.Length, "127.0.0.1", port);
        client.Send(data, data.Length, "127.0.0.1", 20232); //Lobby Server
        
        data = Encoding.UTF8.GetBytes(PhantomServerCommand.RemovePlayer());
        client.Send(data, data.Length, "127.0.0.1", port);
        client.Send(data, data.Length, "127.0.0.1", 20232); //Lobby Server
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

    public static void Instantiate()
    {
        var go = new GameObject("ProtoPhanUDPDirector");
        go.AddComponent<ProtoPhanUDPDirector>();
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
    }
}