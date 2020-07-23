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

            Debug.Log("receive msg; node = " + GetFullPath(msg.list[0]));

            foreach (var scene in msg.list)
            {
                List<GameObject> root_objs = FindRootObjs(scene);
                foreach (var root_node in scene.list)
                {
                    GameObject obj = root_objs.Where(m => m.name == root_node.name).FirstOrDefault();

                    if (obj)
                    {
                        SetNode(obj.transform, root_node);
                    }
                    else
                    {
                        Debug.LogErrorFormat("obj not exist {0}", GetFullPath(scene));
                    }
                }
            }
        }
    }

    private List<GameObject> FindRootObjs(Node node)
    {
        Scene scene;
        if (node.name == "DontDestroyOnLoad")
        {
            scene = gameObject.scene;
        }
        else
        {
            scene = SceneManager.GetSceneByName(node.name);
        }

        return scene.GetRootGameObjects().ToList();
    }

    static string GetFullPath(Node node)
    {
        string path = node.name;
        if (node.list != null && node.list.Count > 0)
        {
            path = path + "/" + GetFullPath(node.list[0]);
        }

        return path;
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

        List<Scene> lst_scene = new List<Scene>();
        lst_scene.Add(gameObject.scene);
        lst_scene.AddRange(SceneManager.GetAllScenes());

        Msg msg = new Msg();

        msg.list = new List<Node>();
        foreach (var item in lst_scene)
        {
            msg.list.Add(GenSceneNode(item));
        }

        _client.SendMsg(msg.ToBytes());
    }

    private Node GenSceneNode(Scene scene)
    {
        Node node = new Node() {
            list = new List<Node>(),
            name = scene.name
        };

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (var item in roots)
        {
            if (item != gameObject)
            {
                node.list.Add(GenNode(item.transform));
            }
        }

        return node;
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
