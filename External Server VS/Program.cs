using Neo4j.Driver;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class DriverIntroductionExample : IDisposable
{

    //For hiding the window
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;
    //

    public string query = "";
    public string uri = "";
    public string user = "";
    public string password = "";



    private bool _disposed = false;
    private IDriver _driver;
    Socket socket;
    string MSG = "";
    ~DriverIntroductionExample() => Dispose(false);
    public async Task FindSpeed()
    {
        var session = _driver.AsyncSession();

        try
        {
            var readResults = await session.ReadTransactionAsync(async tx =>
            {
                var result = await tx.RunAsync(query);
                return (await result.ToListAsync());
            });
            string Data = readResults[0]["p.Speed"].As<String>();
            Console.WriteLine("Data: " + Data);
            MSG = Data;
        }
        // Capture any errors along with the query and data for traceability
        catch (Neo4jException ex)
        {
            Console.WriteLine($"{query} - {ex}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            await session.CloseAsync();
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _driver?.Dispose();
        }

        _disposed = true;
    }   
    public static async Task Main(string[] args)
    {
        //Hiding the console
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
        //

        DriverIntroductionExample example = new DriverIntroductionExample();

        example.socket = null;
        Console.WriteLine("Connecting");
        for (; ; )
        {
            if (example.socket == null || !example.socket.Connected)
            {
                Console.WriteLine("Creating Socket");
                example.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                while (!example.socket.Connected)
                {
                    Console.WriteLine("Connecting Socket");
                    try
                    {
                        example.socket.Connect(new IPEndPoint(IPAddress.Loopback, 11004));
                        Console.WriteLine("Connected");
                        byte[] StartData = new byte[512];
                        example.socket.Receive(StartData);
                        //Unpack
                        string[] StartDataSplit = Encoding.UTF8.GetString(StartData).Split('|');
                        //Set the server up with the data we got from unity.
                        Console.WriteLine(StartDataSplit[0] + "\n" + StartDataSplit[1] + "\n" + StartDataSplit[2]);
                        example._driver = GraphDatabase.Driver(StartDataSplit[0], AuthTokens.Basic(StartDataSplit[1], StartDataSplit[2]));
                        if (StartDataSplit[3] == "1")
                        {
                            ShowWindow(handle, SW_SHOW);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Connection Failed... Reconnecting");
                    }

                }
            }
            try
            {
                byte[] InBuffer = new byte[128];
                byte[] OutBuffer = new byte[8];

                example.socket.Receive(InBuffer);
                example.query = Encoding.UTF8.GetString(InBuffer).Split('|')[0];
                Console.WriteLine(example.query + ";");

                await example.FindSpeed();
                OutBuffer = Encoding.UTF8.GetBytes(example.MSG);
                example.socket.Send(OutBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        Console.ReadLine();
    }
}