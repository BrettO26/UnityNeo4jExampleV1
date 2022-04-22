using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class PullNeo4j : MonoBehaviour
{
    [Tooltip("Put the full path of the executeble")]
    public string ServerFile = @"Where you put the internal server \Neo4J_CSharp.exe";
    public string query = "MATCH (p:Entity) WHERE p.name = \"TestCube\" RETURN p.Speed";
    public string uri = "The Database URI";
    public string user = "Your Username";
    public string password = "Your Password";
    public bool ShowConsole = false;
    public Process Server;
    public static PullNeo4j instance;
    (bool, bool) CloseRequest = (false, false);
    Thread Neo4jUpdateThread;
    [Header("Readonly")]
    public int DataOut = 0;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Neo4jUpdateThread = new Thread(ThreadedStart) { IsBackground = true };
        Neo4jUpdateThread.Start();
    }
    private void ThreadedStart()
    {
        UnityEngine.Debug.Log("Starting internal server...");
        Server = Process.Start(ServerFile);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11004);
        Socket socket;
        for (; ; )
        {
            try
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(12);
                Socket handler = socket.Accept();
                //Pack start data and send it.
                byte[] StartDataOut = new byte[512];
                StartDataOut = Encoding.UTF8.GetBytes(uri + "|" + user + "|" + password + "|" + (ShowConsole ? 1 : 0).ToString() + "|");
                handler.Send(StartDataOut);
                //
                for (; ; )
                {
                    byte[] dataOut = new byte[128];
                    byte[] dataIn = new byte[8];
                    //Send the query
                    dataOut = Encoding.UTF8.GetBytes(query + "|"); 
                    handler.Send(dataOut);
                    //Receive the data the server got
                    handler.Receive(dataIn);
                    string convertedDatatIn = Encoding.UTF8.GetString(dataIn);
                    UnityEngine.Debug.Log(convertedDatatIn);

                    if (!int.TryParse(convertedDatatIn, out DataOut))
                    {
                        UnityEngine.Debug.Log("A number is required!");
                    } 

                    //CLOSE if requested
                    if (CloseRequest.Item1)
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        while (Neo4jUpdateThread.ThreadState == System.Threading.ThreadState.Running)
                        {
                            Neo4jUpdateThread.Abort();
                        }
                        CloseRequest.Item2 = true;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(e.Message);
            }
        }
    }
    private void OnApplicationQuit()
    {
        ShutDown();
    }
    void ShutDown()
    {
        CloseRequest.Item1 = true;
        if (!Server.HasExited)
        {
            try
            {
                Server.Kill();
            }
            catch
            {
                UnityEngine.Debug.Log("Server Already Closed");
            }
        }
        Neo4jUpdateThread.Abort();
    }
    private void OnDisable()
    {

        ShutDown();
    }
}