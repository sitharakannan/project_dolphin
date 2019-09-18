using System;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;
using TMPro;


//Receiving Data from Client
public class NetMqListener
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private void ListenerWork(string ip,string port)
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect($"tcp://{ip}:{port}");
            subSocket.Subscribe("");
            Debug.Log($"Proctor has successfully connected to client1 via tcp://{ip}:{port}");
            while (!_listenerCancelled)
            {
                string frameString;
                
                if (!subSocket.TryReceiveFrameString(out frameString)) continue;
                var value = Int32.Parse(frameString);
                Debug.Log($"value is {value}");
                _messageQueue.Enqueue(frameString);
            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message;
            if (_messageQueue.TryDequeue(out message))
            {
                _messageDelegate(message);
            }
            else
            {
                break;
            }
        }
    }

    public NetMqListener(MessageDelegate messageDelegate,string ip,string port)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(() => ListenerWork(ip, port));
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}

public class MessageSubscriber : MonoBehaviour
{
    
    private NetMqListener _netMqListener;
    public GameObject spinnerController;
    private TextMeshPro myText;
    private string ip = "10.25.174.235";
    private string port = "12346";
    //public AngleManager am;
    
    private void HandleMessage(string message)
    {
        var voltage = Int32.Parse(message);
            var angleInDegrees = (360f * ( voltage / 1024f));
            spinnerController.transform.rotation = Quaternion.identity * Quaternion.AngleAxis(angleInDegrees, Vector3.forward);
            myText.text = angleInDegrees.ToString("F2");
    }


    private void Start()
    {
        _netMqListener = new NetMqListener(HandleMessage, ip, port);
        _netMqListener.Start();
        myText = GameObject.Find("AngleText").GetComponent<TextMeshPro>();
        
    }

    private void Update()
    {
        _netMqListener.Update();
    }

    private void OnDestroy()
    {
        _netMqListener.Stop();
    }
}
