using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject g = new GameObject("KKK");
        DontDestroyOnLoad(g);

        SceneManager.LoadScene("test_scene", LoadSceneMode.Additive);

        RemoteOperateClient.GetInstance().Sync();
    }
}
