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
    public static readonly int MAX_PACKET_SIZE = 256;

    public readonly IPEndPoint serverEndpoint;
    public readonly string username;
    private readonly Action<byte[]> PacketCallback;
    private readonly Socket socket;
    private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
    private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);
    private readonly PacketFramer packetFramer;
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

    public AsynchronousClient(string connectionString, string username, Action<byte[]> PacketCallback) {
        Debug.Log(string.Format("Start of setting up connection to {0} as {1}", connectionString, username));
        ThreadManager.Activate();
        serverEndpoint = ParseConnectionString(connectionString);
        this.username = username;
        this.PacketCallback = PacketCallback;
        packetFramer = new PacketFramer(MAX_PACKET_SIZE, PacketCallback);
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

    public void Send(byte[] packet) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot send when it is not connected");
            return;
        }

        byte[] bytes = packetFramer.FramePacket(packet);
        Debug.Log(string.Format(
            "Sending {0} byte(s) to the server with the first 16 bytes being: {1}{2}",
            bytes.Length,
            BitConverter.ToString(bytes, 0, Math.Min(16, bytes.Length)),
            bytes.Length > 16 ? "-.." : string.Empty
        ));

        socket.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), null);
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
        Debug.Log("Initiated receive loop, client is ready for incoming packets from the server");
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
                    Debug.Log("Received an empty packet from the server or the socket is no longer connected, this means the connection has closed");
                    Disconnect();
                }
                return;
            }

            Debug.Log(string.Format("Received {0} bytes from the server.", bytesRead));
            packetFramer.Append(receiveBuffer);

        } catch (Exception e) {
            Debug.LogError(string.Format("An error occurred in the receive callback: {0}", e.ToString()));
        } finally {
            receiveDone.Set();
        }
    }

    public void Disconnect() {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot disconnect when it is not connected");
            return;
        }

        Debug.Log("Disconnecting from server");
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        Status = AsynchronousClientStatus.DISCONNECTED;
        Debug.Log("Successfully disconnected from the server");
    }
}