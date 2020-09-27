using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public enum AsynchronousClientStatus {
    NEW,
    CONNECTED,
    DISCONNECTED,
}

public class AsynchronousClient {
    public static readonly int DEFAULT_PORT = 14641;
    public static readonly int RECEIVING_BUFFER_SIZE = 256;
    public static readonly int MAX_FRAME_SIZE = 256;

    public readonly IPEndPoint serverEndpoint;
    private readonly Socket socket;
    private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
    private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);
    private readonly ByteFramer byteFramer;
    private byte[] receiveBuffer = new byte[RECEIVING_BUFFER_SIZE];

    private float connectionEstablishedTimestamp;
    public float RealtimeSinceConnectionEstablished {
        get {
            return Time.realtimeSinceStartup - connectionEstablishedTimestamp;
        }
    }
    public AsynchronousClientStatus Status { get; private set; } = AsynchronousClientStatus.NEW;

    private static IPEndPoint ParseConnectionString(string connectionString) {
        try {
            string[] parts = connectionString.Split(':');
            if (parts[0] == "localhost") {
                parts[0] = "127.0.0.1";
            }
            if (!IPAddress.TryParse(parts[0], out IPAddress serverIpAddress)) {
                serverIpAddress = Dns.GetHostEntry(parts[0]).AddressList[0];
            }
            int port = parts.Length == 1 ? DEFAULT_PORT : int.Parse(parts[1]);
            return new IPEndPoint(serverIpAddress, port);
        } catch (Exception) {
            throw new InvalidOperationException("Invalid connection string provided");
        }
    }

    public AsynchronousClient(string connectionString, Action<byte[]> OnFrameReceived) {
        Debug.Log(string.Format("Start of setting up connection to {0}", connectionString));
        ThreadManager.Activate();
        serverEndpoint = ParseConnectionString(connectionString);
        byteFramer = new ByteFramer(MAX_FRAME_SIZE, OnFrameReceived);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        var connectingThread = new Thread(() => {
            Debug.Log(string.Format("Connecting to server at {0}", serverEndpoint));
            socket.BeginConnect(serverEndpoint, new AsyncCallback(ConnectCallback), null);

            connectDone.WaitOne();

            if (socket.Connected) {
                Debug.Log("Succesfully connected to the server");
                Status = AsynchronousClientStatus.CONNECTED;
                ThreadManager.ExecuteOnMainThread(() => {
                    connectionEstablishedTimestamp = Time.realtimeSinceStartup;
                });
                InitiateReceiveLoop();
            } else {
                Debug.LogWarning("Failed to connect to the server");
                Status = AsynchronousClientStatus.DISCONNECTED;
            }
        });
        connectingThread.Start();
    }

    private void ConnectCallback(IAsyncResult ar) {
        try {
            if (Status != AsynchronousClientStatus.NEW) {
                Debug.LogError("Client cannot handle a connect callback when it is not new");
                return;
            }
            socket.EndConnect(ar);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        } finally {
            connectDone.Set();
        }
    }

    public void Disconnect() {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot disconnect when it is not connected");
            return;
        }
        if (!socket.Connected) {
            Status = AsynchronousClientStatus.DISCONNECTED;
            return;
        }
        Debug.Log("Disconnecting from server");
        Status = AsynchronousClientStatus.DISCONNECTED;
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        Debug.Log("Successfully disconnected from the server");
    }

    public void Send(byte[] bytes) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot send when it is not connected");
            return;
        }

        byte[] frame = byteFramer.Frame(bytes);
        Debug.Log(string.Format(
            "Sending a frame of {0} byte(s) to the server: {1}{2}",
            frame.Length,
            BitConverter.ToString(frame, 0, Math.Min(16, frame.Length)),
            frame.Length > 16 ? "-.." : string.Empty
        ));

        socket.BeginSend(frame, 0, frame.Length, 0, new AsyncCallback(SendCallback), null);
    }

    private void SendCallback(IAsyncResult ar) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot handle a send callback when it is not connected");
            return;
        }

        try {
            int bytesSent = socket.EndSend(ar);
            Debug.Log(string.Format("Successfully sent {0} byte(s) to the server.", bytesSent));
        } catch (Exception e) {
            Debug.LogError(string.Format("An error occurred in the send callback: {0}", e.ToString()));
        }
    }

    private void InitiateReceiveLoop() {
        Debug.Log("Initiated receive loop, client is ready for incoming frames from the server");
        while (true) {
            receiveDone.Reset();
            if (Status != AsynchronousClientStatus.CONNECTED) {
                Debug.Log("Breaking out of receive loop since client is no longer connected");
                break;
            }

            socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
            receiveDone.WaitOne();
        }
    }

    private void ReceiveCallback(IAsyncResult ar) {
        try {
            int bytesRead = socket.Connected ? socket.EndReceive(ar) : 0;
            if (bytesRead == 0) {
                if (Status == AsynchronousClientStatus.CONNECTED) {
                    Debug.Log("Handling a receive callback containing 0 bytes or the socket is no longer connected, this means the connection should be disconnected");
                    Disconnect();
                }
                return;
            }

            Debug.Log(string.Format("Received {0} bytes from the server.", bytesRead));
            byte[] bytes = new byte[bytesRead];
            Buffer.BlockCopy(receiveBuffer, 0, bytes, 0, bytesRead);
            byteFramer.Append(bytes);

        } catch (Exception e) {
            Debug.LogError(string.Format("An error occurred in the receive callback: {0}", e.ToString()));
        } finally {
            receiveDone.Set();
        }
    }
}