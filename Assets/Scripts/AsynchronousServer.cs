using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// State object for reading client data asynchronously
public class ServerStateObject {
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
    // Client socket.
    public Socket workSocket = null;
}

public class AsynchronousServer : MonoBehaviour {
    private Thread thread;

    protected void Start() {
        thread = new Thread(() => StartServer());
        thread.Start();
    }

    protected void OnDestroy() {
        thread.Abort();
    }

    public static ManualResetEvent connectionEstablished = new ManualResetEvent(false);
    public static void StartServer() {
        Debug.Log("Starting server");
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 14641);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            socket.Bind(localEndPoint);
            socket.Listen(100);

            while (true) {
                connectionEstablished.Reset();

                // Start an asynchronous socket to listen for connections.
                Debug.Log(string.Format("Waiting for a connection on {0}", localEndPoint));
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                connectionEstablished.WaitOne();
            }
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }

    public static void AcceptCallback(IAsyncResult ar) {
        // Get the socket that handles the client request.
        Socket socket = (Socket)ar.AsyncState;
        Socket handler = socket.EndAccept(ar);
        Debug.Log(string.Format("Accepting incoming connection from {0}", handler.RemoteEndPoint));
        connectionEstablished.Set();

        // Create the state object.
        ServerStateObject state = new ServerStateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, ServerStateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
    }

    public static void ReceiveCallback(IAsyncResult ar) {
        string content = string.Empty;

        // Retrieve the state object and the handler socket from the asynchronous state object.
        ServerStateObject state = (ServerStateObject)ar.AsyncState;
        Socket handler = state.workSocket;
        Debug.Log(string.Format("Receiving data from {0}", handler.RemoteEndPoint));

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0) {
            // There might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read more data.
            content = state.sb.ToString();
            if (content.IndexOf("||") > -1) {
                // All the data has been read from the client. Display it on the console.
                Debug.Log(string.Format("Read {0} bytes from {1}.\nData: {2}", content.Length, handler.RemoteEndPoint, content));
                // Echo the data back to the client.
                Send(handler, string.Format("Client said: {0}", content));
            } else {
                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, ServerStateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
        }
    }

    private static void Send(Socket handler, string data) {
        Debug.Log(string.Format("Sending \"{0}\" to {1}", data, handler.RemoteEndPoint));

        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar) {
        try {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Debug.Log(string.Format("Sent {0} bytes to client at {1}", bytesSent, handler.RemoteEndPoint));
            
            /*
            string remoteEndpointName = handler.RemoteEndPoint.ToString();
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            Debug.Log(string.Format("Closed the connection with {0}", remoteEndpointName));
            */

        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }
}