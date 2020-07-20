using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetWorkServer
{
    TcpListener _server = null;
    NetWorkClient _client;

    public NetWorkServer(int port)
    {
        Listen(port);
    }

    async void Listen(int port)
    {
        TcpClient client = null;
        try
        {
            _server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            _server.Start();

            Debug.Log("server start at " + port);

            while (true)
            {
                client = await _server.AcceptTcpClientAsync();

                Debug.Log("client connect");

                _client = new NetWorkClient(client);
                await _client.StartReceiving();
            }
        }
        catch (Exception ex)
        {
            // display the error message or whatever
            Debug.LogException(ex);
        }
        finally
        {
            Debug.Log("close server");
            _server?.Stop();
        }
    }

    internal void Close()
    {
        _client?.Close();
        _client = null;
    }

    public void SendMsg(byte[] bytes)
    {
        _client.SendMsg(bytes);
    }

    public byte[] DequeueOne()
    {
        return _client?.DequeueOne();
    }
}
