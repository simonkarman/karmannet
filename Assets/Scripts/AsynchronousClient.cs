using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

// State object for receiving data from remote device.
public class ClientStateObject {
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 256;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousClient : MonoBehaviour {
    // The port number for the remote device.
    private const int port = 14641;

    // ManualResetEvent instances signal completion.
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    // The response from the remote device.
    private static string response = string.Empty;
    private Thread thread;

    protected void Start() {
        thread = new Thread(() => StartClient());
        thread.Start();
    }

    protected void OnDestroy() {
        thread.Abort();
    }

    private static void StartClient() {
        Debug.Log("Starting client");
        // Connect to a remote device.
        try {
            // Establish the remote endpoint for the socket.
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            Debug.Log(string.Format("Connecting to server at {0}", remoteEP));

            // Create a TCP/IP socket.  
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
            Debug.Log(string.Format("Conntected to server at {0}", remoteEP));

            // Send test data to the remote device.
            Send(client, "This is a test<EOF>");
            sendDone.WaitOne();

            // Receive the response from the remote device.
            Receive(client);
            receiveDone.WaitOne();

            // Write the response to the console.
            Debug.Log(string.Format("Response received: {0}", response));

            // Release the socket.
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            Debug.Log("Disconnected from server");

        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }

    private static void ConnectCallback(IAsyncResult ar) {
        try {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.
            client.EndConnect(ar);

            Debug.Log(string.Format("Connected to {0}", client.RemoteEndPoint.ToString()));

            // Signal that the connection has been made.
            connectDone.Set();
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }

    private static void Receive(Socket client) {
        try {
            // Create the state object.
            ClientStateObject state = new ClientStateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.
            client.BeginReceive(state.buffer, 0, ClientStateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar) {
        try {
            // Retrieve the state object and the client socket from the asynchronous state object.
            ClientStateObject state = (ClientStateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0) {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Get the rest of the data.  
                client.BeginReceive(state.buffer, 0, ClientStateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            } else {
                // All the data has arrived; put it in response.
                if (state.sb.Length > 1) {
                    response = state.sb.ToString();
                }
                // Signal that all bytes have been received.
                receiveDone.Set();
            }
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }

    private static void Send(Socket client, string data) {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar) {
        try {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
            Debug.Log(string.Format("Sent {0} bytes to server.", bytesSent));

            // Signal that all bytes have been sent.
            sendDone.Set();
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }
}