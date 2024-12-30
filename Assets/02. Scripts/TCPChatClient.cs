using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class TCPChatClient : MonoBehaviour
{
    private Socket clientSocket;
    //private NetworkStream stream;
    private Thread receiveThread;
    
    [SerializeField] private ChatManager chatManager;
    [SerializeField] private TMP_InputField serverIpField;
    [SerializeField] private TMP_InputField nameField;

    [SerializeField] private string serverIP = "127.0.0.1"; // 서버 IP 주소
    [SerializeField] private int serverPort = 9000;         // 서버 포트 번호
    public string clientName = "Client";  // 클라이언트 이름
    [SerializeField] private const int BUFFER_SIZE = 1024;
    
    // 메시지 큐
    private readonly Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object();

    private void Update()
    {
        // 메인 스레드에서 메시지 처리
        lock (queueLock)
        {
            while (messageQueue.Count > 0)
            {
                string message = messageQueue.Dequeue();
                chatManager.DisplayMessage(message);
                Debug.Log($"[SERVER] {message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    public void ConnectToServer()
    {
        try
        {
            // 서버 IP, 이름 입력란에서 받아오기 
            serverIP = serverIpField.text;
            clientName = nameField.text;
            
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverIP, serverPort);

            Debug.Log("[CLIENT] 서버에 연결되었습니다.");

            // 서버에서 메시지를 받기 위한 스레드 시작
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            // 연결 메시지 전송
            SendMessageToServer($"{clientName} is connected.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CLIENT] 서버 연결 실패: {e.Message}");
        }
    }

    private void DisconnectFromServer()
    {
        try
        {
            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Join();

            if (clientSocket != null)
                clientSocket.Close();

            Debug.Log("[CLIENT] 서버 연결이 종료되었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CLIENT] 연결 종료 실패: {e.Message}");
        }
    }

    private void ReceiveMessages()
    {
        int retval;
        byte[] buffer = new byte[BUFFER_SIZE];
        while (true)
        {
            try
            {
                if (!clientSocket.Connected)
                {
                    Debug.Log("[CLIENT] 서버 연결이 끊어졌습니다.");
                    break;
                }
                
                retval = clientSocket.Receive(buffer, 0, buffer.Length, 0);
                if (retval == 0)
                {
                    Debug.Log("[CLIENT] 서버 연결이 끊어졌습니다.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, retval);
                
                // 메시지를 큐에 추가
                lock (queueLock)
                {
                    messageQueue.Enqueue(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CLIENT] 메시지 수신 오류: {e.Message}");
                break;
            }
        }
    }
    
    public void SendMessageToServer(string message)
    {
        if (clientSocket == null || !clientSocket.Connected)
        {
            Debug.LogWarning("[CLIENT] 서버와 연결되어 있지 않습니다.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(data, 0, data.Length, 0);
            Debug.Log($"[CLIENT] 보낸 메시지: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CLIENT] 메시지 전송 실패: {e.Message}");
        }
    }
}