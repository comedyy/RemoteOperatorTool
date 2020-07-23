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

            Scene[] current_scenes = SceneManager.GetAllScenes();
            Scene temp_scene = SceneManager.LoadScene("SWITCH_SCENE", new LoadSceneParameters(){
               loadSceneMode = LoadSceneMode.Single
            } );

            foreach (var item in current_scenes)
            {
                SceneManager.UnloadScene(item);
            }

            GameObject[] dont_destroy_objects = gameObject.scene.GetRootGameObjects();
            foreach (var item in dont_destroy_objects)
            {
                if (item != gameObject)
                {
                    DestroyImmediate(item);
                }
            }

            StartCoroutine(CreateSceness(msg, temp_scene));
        }
    }

    IEnumerator CreateSceness(Msg msg, Scene temp_scene)
    {
        foreach (var item in msg.list)
        {
            yield return CreateSceneNode(item);
        }

        SceneManager.UnloadScene(temp_scene);
    }

    IEnumerator CreateSceneNode(Node msg)
    {
        LoadSceneParameters param = new LoadSceneParameters()
        {
            loadSceneMode = LoadSceneMode.Additive
        };

        bool is_dont_destroy_onload = msg.name == "DontDestroyOnLoad";
        if (!is_dont_destroy_onload)
        {
            Scene scene = SceneManager.CreateScene(msg.name, new CreateSceneParameters()
            {
                localPhysicsMode = LocalPhysicsMode.None
            });

            yield return null;
            SceneManager.SetActiveScene(scene);
        }

        Transform parent = null;
        foreach (var item in msg.list)
        {
            CreateNode(parent, item, is_dont_destroy_onload);
        }
    }

    void CreateNode(Transform t, Node msg, bool is_dont_destroy_onload)
    {
        GameObject o = new GameObject(msg.name);
        o.transform.SetParent(t);
        o.SetActive(msg.active);
        o.AddComponent<DumpObj>().Init(msg.active, OnStateChange);

        if (is_dont_destroy_onload)
        {
            DontDestroyOnLoad(o);
        }

        foreach (var item in msg.list)
        {
            CreateNode(o.transform, item, false);
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
        msg.list = new List<Node>() {
            new Node(){
                name = obj.scene.name, 
                list = new List<Node>(){ list_node[list_node.Count - 1] }
            }
        };

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
