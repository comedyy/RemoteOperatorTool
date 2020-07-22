using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    NetWorkServer _server;

    string OBJ_NAME = "____DONT_DELETE_____";

    // Start is called before the first frame update
    void Start()
    {
        _server = new NetWorkServer(9999);

        gameObject.name = OBJ_NAME;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        byte[] bytes = _server?.DequeueOne();
        if (bytes != null)
        {
            Msg msg = Msg.GenFromBytes(bytes);

            //Debug.Log(msg.sceneName);
            //Debug.Log(msg.msg_type);
            //Debug.Log(msg.list.Count);
            //Debug.Log(msg.list[0].name);
            //Debug.Log(msg.list[0].list.Count);
            //Debug.Log(msg.list[0].active);

            //Debug.Log(msg.list[2].list[0].name);
            //Debug.Log(msg.list[2].list[0].active);

            //Debug.Log(msg.list[1].name);
            //Debug.Log(msg.list[1].active);

            //Debug.Log(msg.list[2].name);
            //Debug.Log(msg.list[2].active);

            // 1. delete all objs;
            Scene[] scenes =  SceneManager.GetAllScenes();
            foreach (var item in scenes)
            {
                foreach (var obj in item.GetRootGameObjects())
                {
                    GameObject.Destroy(obj);
                }
            }

            Transform parent = null;
            foreach (var item in msg.list)
            {
                CreateNode(parent, item);
            }
        }
    }

    void CreateNode(Transform t, Node msg)
    {
        GameObject o = new GameObject(msg.name);
        o.transform.SetParent(t);
        o.SetActive(msg.active);
        o.AddComponent<DumpObj>().Init(msg.active, OnStateChange);

        foreach (var item in msg.list)
        {
            CreateNode(o.transform, item);
        }
    }

    private void OnDestroy()
    {
        _server?.Close();
    }

    void OnStateChange(GameObject obj)
    {
        bool active_self = obj.activeSelf;
        List<Node> list_node = new List<Node>() {
            new Node(){
                name = obj.name,
                active = active_self
            }
        };

        Transform parent = obj.transform.parent;
        while (parent)
        {
            list_node.Add(new Node() {
                name = parent.name,
                active = parent.gameObject.activeSelf,
                list = new List<Node>() {
                    list_node[list_node.Count - 1]
                }
            });

            parent = parent.parent;
        }

        Msg msg = new Msg();
        msg.sceneName = "";
        msg.list = new List<Node>() { list_node[list_node.Count - 1] };

        _server.SendMsg(msg.ToBytes());
        Debug.Log("send msg; node = " + GetFullPath(msg.list[0]));
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
}
