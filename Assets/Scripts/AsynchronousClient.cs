using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

public enum AsynchronousClientStatus {
    NEW,
    CONNECTED,
    DISCONNECTED,
}

public class AsynchronousClient {
    public static readonly int DEFAULT_PORT = 14641;
    public static readonly int BUFFER_SIZE = 256;
    public static readonly string MESSAGE_SEPARATOR = "||";

    public readonly IPEndPoint serverEndpoint;
    public readonly string username;
    private readonly Action<string> MessageCallback;
    private readonly Socket socket;
    private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
    private byte[] buffer = new byte[BUFFER_SIZE];

    private float realtimeSinceStartupAtMomentOfConnectionEstablished;
    public float RealtimeSinceConnectionEstablished {
        get {
            return Time.realtimeSinceStartup - realtimeSinceStartupAtMomentOfConnectionEstablished;
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

    public AsynchronousClient(string connectionString, string username, Action<string> MessageCallback) {
        Debug.Log(string.Format("Start of setting up connection to {0} as {1}", connectionString, username));
        serverEndpoint = ParseConnectionString(connectionString);
        this.username = username;
        this.MessageCallback = MessageCallback;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        var connectingThread = new Thread(() => {
            Debug.Log(string.Format("Connecting to server at {0}", serverEndpoint));
            socket.BeginConnect(serverEndpoint, new AsyncCallback(ConnectCallback), null);

            connectDone.WaitOne();

            if (socket.Connected) {
                Debug.Log("Succesfully connected to the server");
                Status = AsynchronousClientStatus.CONNECTED;
                realtimeSinceStartupAtMomentOfConnectionEstablished = Time.realtimeSinceStartup;
                BeginReceive(new StringBuilder());
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
                Debug.LogError("Client cannot handle connect callback when it is not new");
                return;
            }
            socket.EndConnect(ar);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        } finally {
            connectDone.Set();
        }
    }

    public void Send(string message) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot send when it is not connected");
            return;
        }

        Debug.Log(string.Format("Sending message to the server: {0}", message));
        byte[] bytes = Encoding.ASCII.GetBytes(string.Format("{0}{1}", message, MESSAGE_SEPARATOR));
        socket.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), null);
    }

    private void SendCallback(IAsyncResult ar) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot handle send callback when it is not connected");
            return;
        }

        try {
            int bytesSent = socket.EndSend(ar);
            Debug.Log(string.Format("Successfully sent {0} byte(s) to the server.", bytesSent));
        } catch (Exception e) {
            Debug.LogError(string.Format("An error occurred in the send callback: {0}", e.ToString()));
        }
    }

    private void BeginReceive(StringBuilder messageBuilder) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot begin receive when it is not connected");
            return;
        }

        socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), messageBuilder);
    }

    private void ReceiveCallback(IAsyncResult ar) {
        if (Status != AsynchronousClientStatus.CONNECTED) {
            Debug.LogError("Client cannot handle receive callback when it is not connected");
            return;
        }

        try {
            StringBuilder messageBuilder = (StringBuilder)ar.AsyncState;
            int bytesRead = socket.EndReceive(ar);

            if (bytesRead > 0) {
                string messagePart = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log(string.Format("Received part of a message from the server: {0}", messagePart));
                messageBuilder.Append(messagePart);
                BeginReceive(messageBuilder);
            } else {
                Debug.Log(string.Format("Full message received: {0}", messageBuilder));
                MessageCallback(messageBuilder.ToString());
                // BeginReceive(new StringBuilder());
            }
        } catch (Exception e) {
            Debug.LogError(string.Format("An error occurred in the receive callback: {0}", e.ToString()));
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