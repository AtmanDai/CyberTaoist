using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class HandSignReceiver : MonoBehaviour
{
    private Thread receiveThread;
    private UdpClient client;
    public int port = 5005;

    // 线程安全队列
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    [Header("Debounce Settings")]
    [Tooltip("连续多少帧相同输入才视为稳定")]
    public int stabilityFrames = 5; 
    
    // 公开给外部的状态
    public string latestSignName = "";
    public int latestSignID = -1;
    public bool hasNewInput = false;

    // 内部去抖动状态变量
    private int currentBufferCount = 0;
    private int lastCandidateID = -1;
    private string lastCandidateName = "";

    void Start()
    {
        InitializeUDP();
    }

    private void InitializeUDP()
    {
        Debug.Log("UDP Receiver starting...");
        try
        {
            client = new UdpClient(port);
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Start Failed: " + e.Message);
        }
    }

    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                messageQueue.Enqueue(text);
            }
            catch (Exception)
            {
                // 忽略超时或中断错误
                Thread.Sleep(10);
            }
        }
    }

    void Update()
    {
        // 1. 获取最新的一条原始消息（每帧只处理最新的一条，丢弃积压的旧消息）
        string rawMessage = null;
        while (messageQueue.TryDequeue(out string result))
        {
            rawMessage = result;
        }

        // 2. 如果这一帧没收到任何数据（或者摄像头丢帧了），直接视为“无输入”，中断连续性
        if (string.IsNullOrEmpty(rawMessage))
        {
            // 可选：如果你希望短暂丢帧不打断结印，可以把这一行注释掉
            ResetDebounce(); 
            return;
        }

        // 3. 解析输入信息
        int incomingID = -1;
        string incomingName = "";
        if (ParseMessage(rawMessage, out incomingID, out incomingName))
        {
            // 4. 执行去抖动逻辑
            ProcessDebounce(incomingID, incomingName);
        }
    }

    // 去抖动核心算法
    void ProcessDebounce(int id, string name)
    {
        // 如果当前收到的 ID 和上一帧的候选者一样
        if (id == lastCandidateID)
        {
            currentBufferCount++;
        }
        else
        {
            // 发生了变化（或者是新的开始），重置计数器，把当前这个设为新候选者
            lastCandidateID = id;
            lastCandidateName = name;
            currentBufferCount = 1;
        }

        // 判定条件：连续帧数达标
        if (currentBufferCount >= stabilityFrames)
        {
            // 这是一个稳定的手势！
            
            // 为了防止同一个手势一直触发（比如一直举着手），我们需要加一个锁
            // 只有当这次确认的 ID 和上次最终输出的 ID 不一样时，才通知外部
            // 如果是单次技能，加上 if (id != latestSignID)
            
            if (lastCandidateID != latestSignID)
            {
                // 新的稳定输入，通知外部
                latestSignID = lastCandidateID;
                latestSignName = lastCandidateName;
                hasNewInput = true;
            }

            
            // 防止溢出，但也保持计数器在阈值以上，以保持“稳定状态”
            currentBufferCount = stabilityFrames; 
        }
        else
        {
            // 还没稳定，不要通知外部有新输入
            // hasNewInput = false; // 注意：通常不需要每帧设为false，因为消费方会设为false
        }
    }

    void ResetDebounce()
    {
        currentBufferCount = 0;
        lastCandidateID = -1;
        // 注意：不重置 latestSignID，保持“最后一次有效输入”的记忆，除非你想让它归零
    }

    bool ParseMessage(string msg, out int id, out string name)
    {
        id = -1;
        name = "";
        try
        {
            string[] parts = msg.Split(':');
            if (parts.Length >= 2)
            {
                int.TryParse(parts[0], out id);
                name = parts[1];
                return true;
            }
        }
        catch {}
        return false;
    }

    void OnDestroy()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}