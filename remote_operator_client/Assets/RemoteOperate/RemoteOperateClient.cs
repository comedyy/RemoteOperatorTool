using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RemoteOperateClient : MonoBehaviour
{
    RemoteOperateClientNetwork _client;
    static RemoteOperateClient _instance;

    // Start is called before the first frame update
    void Start()
    {
        TcpClient client = new TcpClient("127.0.0.1", 9999);
        _client = new RemoteOperateClientNetwork(client);
        _client.StartReceiving();
    }

    private void Update()
    {
        byte[] bytes = _client?.DequeueOne();
        if (bytes != null)
        {
            Msg msg = Msg.GenFromBytes(bytes);

            //Debug.Log(msg.sceneName);
            //Debug.Log(msg.msg_type);
            //Debug.Log(msg.list.Count);
            //Debug.Log(msg.list[0].name);
            //Debug.Log(msg.list[0].list.Count);
            //Debug.Log(msg.list[0].active);

            //Debug.Log(msg.list[0].list[0].name);
            //Debug.Log(msg.list[0].list[0].active);

            //Debug.Log(msg.list[1].name);
            //Debug.Log(msg.list[1].active);

            //Debug.Log(msg.list[2].name);
            //Debug.Log(msg.list[2].active);

            Scene scene = SceneManager.GetActiveScene();
            foreach (var item in msg.list)
            {
                GameObject obj = scene.GetRootGameObjects().ToList().Where(m => m.name == item.name).FirstOrDefault();
                if (obj)
                {
                    SetNode(obj.transform, item);
                }
                else
                {
                    Debug.LogErrorFormat("obj not exist {0}", item.name);
                }
            }
        }
    }

    void SetNode(Transform t, Node msg)
    {
        t.gameObject.SetActive(msg.active);
        foreach (var item in msg.list)
        {
            Transform trans = t.Find(item.name);
            if (trans)
            {
                SetNode(trans, item);
            }
            else
            {
                Debug.LogErrorFormat("obj not exist {0}", trans.name);
            }
        }
    }

    internal static RemoteOperateClient GetInstance()
    {
        if (_instance != null)
        {
            GameObject.DestroyImmediate(_instance.gameObject);
        }

        _instance = new GameObject().AddComponent<RemoteOperateClient>();
        _instance.name = "_____RemoteOperateClient_____";
        DontDestroyOnLoad(_instance);

        return _instance;
    }

    public void Sync()
    {
        StartCoroutine(SyncIenumerator());
    }

    public IEnumerator SyncIenumerator() { 
        yield return new WaitUntil(()=> { return _client != null; });

        Msg msg = new Msg();
        Scene scene = SceneManager.GetActiveScene();
        msg.sceneName = scene.name;
        msg.list = new List<Node>(scene.rootCount);
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (var item in roots)
        {
            msg.list.Add(GenNode(item.transform));
        }

        _client.SendMsg(msg.ToBytes());
    }

    private Node GenNode(Transform item)
    {
        Node node = new Node()
        {
            name = item.name,
            active = item.gameObject.activeSelf
        };

        node.list = new List<Node>();
        for (int i = 0; i < item.transform.childCount; i++)
        {
            node.list.Add(GenNode(item.transform.GetChild(i)));
        }

        return node;
    }

    private void OnDestroy()
    {
        _client.Close();
    }
}
