using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Node
{
    public string name;
    public bool active;
    public List<Node> list;

    public void AddBytes(BinaryWriter writer)
    {
        writer.Write(name);
        writer.Write(active);

        int list_count = list != null ? list.Count : 0;
        writer.Write(list_count);
        for (int i = 0; i < list_count; i++)
        {
            list[i].AddBytes(writer);
        }
    }

    public static Node GenFromBytes(BinaryReader reader)
    {
        Node node = new Node();
        node.name = reader.ReadString();
        node.active = reader.ReadBoolean();

        int list_count = reader.ReadInt32();
        node.list = new List<Node>();
        for (int i = 0; i < list_count; i++)
        {
            node.list.Add(Node.GenFromBytes(reader));
        }

        return node;
    }
}

public class Msg
{
    public short msg_type;

    public string sceneName;
    public List<Node> list;

    public byte[] ToBytes()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(0); // size
        writer.Write(msg_type);
        writer.Write(sceneName);

        int list_count = list != null ? list.Count : 0;
        writer.Write(list_count);
        for (int i = 0; i < list_count; i++)
        {
            list[i].AddBytes(writer);
        }

        writer.Seek(0, SeekOrigin.Begin);
        writer.Write((short)stream.Length);

        return stream.ToArray();
    }

    public static Msg GenFromBytes(byte[] bytes)
    {
        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);

        Msg msg = new Msg();
        reader.ReadInt32();
        msg.msg_type = reader.ReadInt16();
        msg.sceneName = reader.ReadString();


        int list_count = reader.ReadInt32();
        msg.list = new List<Node>();
        for (int i = 0; i < list_count; i++)
        {
            msg.list.Add(Node.GenFromBytes(reader));
        }

        return msg;
    }
}


