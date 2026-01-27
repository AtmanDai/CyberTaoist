using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

/// <summary>
/// MediaPipe data structure for JSON parsing
/// </summary>
[System.Serializable]
public class MediaPipeData
{
    public float left_index_x;
    public float left_index_y;
    public int gesture_id;
    public string gesture_name;
    public double timestamp;
}

public class HandSignReceiver : MonoBehaviour
{
    private Thread receiveThread;
    private UdpClient client;
    public int port = 5005;

    // Thread-safe queue
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    [Header("Debounce Settings")]
    [Tooltip("Number of consecutive frames with same input to consider stable")]
    public int stabilityFrames = 5;
    
    // Public state - Hand gesture
    public string latestSignName = "";
    public int latestSignID = -1;
    public bool hasNewInput = false;

    // Public state - Left index finger position for player movement
    [Header("Movement Data")]
    public float leftIndexX = -1f;
    public float leftIndexY = -1f;
    public bool hasValidMovementData = false;

    // Internal debounce state variables
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
                // Ignore timeout or interrupt errors
                Thread.Sleep(10);
            }
        }
    }

    void Update()
    {
        // 1. Get the latest raw message (process only the latest, discard old ones)
        string rawMessage = null;
        while (messageQueue.TryDequeue(out string result))
        {
            rawMessage = result;
        }

        // 2. If no data received this frame, treat as "no input", break continuity
        if (string.IsNullOrEmpty(rawMessage))
        {
            ResetDebounce();
            hasValidMovementData = false;
            return;
        }

        // 3. Parse input - Try new MediaPipe JSON format first
        MediaPipeData mpData = ParseMediaPipeMessage(rawMessage);
        if (mpData != null)
        {
            // Update movement data from left index finger
            leftIndexX = mpData.left_index_x;
            leftIndexY = mpData.left_index_y;
            hasValidMovementData = (leftIndexX >= 0 && leftIndexY >= 0);

            // Process gesture for spells (right hand)
            if (mpData.gesture_id > 0)
            {
                ProcessDebounce(mpData.gesture_id, mpData.gesture_name);
            }
            else
            {
                ResetDebounce();
            }
        }
        else
        {
            // Fallback to old format for backward compatibility
            int incomingID = -1;
            string incomingName = "";
            if (ParseLegacyMessage(rawMessage, out incomingID, out incomingName))
            {
                ProcessDebounce(incomingID, incomingName);
            }
        }
    }

    /// <summary>
    /// Parse MediaPipe JSON message format
    /// </summary>
    private MediaPipeData ParseMediaPipeMessage(string msg)
    {
        try
        {
            // Check if it's JSON format
            if (msg.StartsWith("{"))
            {
                return JsonUtility.FromJson<MediaPipeData>(msg);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to parse MediaPipe message: " + e.Message);
        }
        return null;
    }

    /// <summary>
    /// Parse legacy message format (ID:name)
    /// </summary>
    private bool ParseLegacyMessage(string msg, out int id, out string name)
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

    // Debounce core algorithm
    void ProcessDebounce(int id, string name)
    {
        // If current ID matches previous candidate
        if (id == lastCandidateID)
        {
            currentBufferCount++;
        }
        else
        {
            // Change occurred, reset counter, set current as new candidate
            lastCandidateID = id;
            lastCandidateName = name;
            currentBufferCount = 1;
        }

        // Check if stable (consecutive frames threshold reached)
        if (currentBufferCount >= stabilityFrames)
        {
            // Stable gesture detected!
            
            // Only notify if different from last confirmed gesture
            if (lastCandidateID != latestSignID)
            {
                // New stable input, notify external systems
                latestSignID = lastCandidateID;
                latestSignName = lastCandidateName;
                hasNewInput = true;
            }

            // Prevent overflow, maintain counter at threshold
            currentBufferCount = stabilityFrames;
        }
    }

    void ResetDebounce()
    {
        currentBufferCount = 0;
        lastCandidateID = -1;
        // Don't reset latestSignID, keep memory of last valid input
    }

    void OnDestroy()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}
