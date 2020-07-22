using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class RemoteOperateClientNetwork
{
    byte[] _bytes = new byte[64 * 1024];
    int _byte_index = 0;

    TcpClient _client = null;
    NetworkStream _stream;
    Queue<byte[]> _receive_list = new Queue<byte[]>();

    public RemoteOperateClientNetwork(TcpClient client)
    {
        _client = client;
        _stream = _client.GetStream();
    }

    public virtual void Close()
    {
        _client?.Close();
        _client = null;
    }

    public void SendMsg(byte[] bytes)
    {
        _stream.Write(bytes, 0, bytes.Length);
    }

    public async Task StartReceiving()
    {
        Debug.Log("begin Receive" + " threadid: " + Thread.CurrentThread.ManagedThreadId);

        try
        {
            while (_client != null && _client.Connected)
            {
                int read = await _stream.ReadAsync(_bytes, _byte_index, _bytes.Length - _byte_index);
                //Debug.LogError("server read " + read);

                if (read > 0)
                {
                    _byte_index += read;
                    ProcessMsg();
                }
                else
                {
                    break; // disconnect
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            Debug.Log("close client");
            _client?.Close();
            _client = null;
        }
    }

    private void ProcessMsg()
    {
        while (true)
        {
            byte[] bytes = GenMsg(_bytes, _byte_index);
            if (bytes != null)
            {
                Array.Copy(_bytes, bytes.Length, _bytes, 0, _byte_index - bytes.Length);
                _byte_index -= bytes.Length;

                lock (_receive_list)
                {
                    _receive_list.Enqueue(bytes);
                }
            }
            else
            {
                break;
            }
        }
    }

    private byte[] GenMsg(byte[] bytes, int index)
    {
        int msg_head = BitConverter.ToInt32(bytes, 0);
        if (msg_head <= index)
        {
            byte[] msg = new byte[msg_head];
            Array.Copy(bytes, 0, msg, 0, msg_head);
            return msg;
        }

        return null;
    }


    public byte[] DequeueOne()
    {
        if (_receive_list.Count > 0)
        {
            return _receive_list.Dequeue();
        }

        return null;
    }
}
