using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class NetWorkClient
{
    byte[] _bytes = new byte[64 * 1024];
    int _byte_index = 0;

    TcpClient _client = null;
    NetworkStream _stream;
    Queue<byte[]> _receive_list = new Queue<byte[]>();

    public NetWorkClient(TcpClient client)
    {
        _client = client;
        _stream = _client.GetStream();
    }

    public virtual void Close() {
        _client?.Close();
        _client = null;
    }

    public void SendMsg(byte[] bytes)
    {
        _stream.Write(bytes, 0, bytes.Length);
    }

    public async Task StartReceiving()
    {
        Debug.Log("begin Receive");

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
        byte[] bytes = GenMsg(_bytes, _byte_index);
        if (bytes != null)
        {
            _byte_index -= bytes.Length;
            lock (_receive_list)
            {
                _receive_list.Enqueue(bytes);
            }
        }
    }

    private byte[] GenMsg(byte[] bytes, int index)
    {
        short msg_head = BitConverter.ToInt16(bytes, 0);
        if (msg_head <= index)
        {
            byte[] msg = new byte[msg_head];
            Array.Copy(bytes, msg, msg_head);
            Array.Copy(bytes, msg_head, bytes, 0, bytes.Length - msg_head);
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
