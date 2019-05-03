using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError("Duplicate GameManager exists in scene!");
            UnityEditor.EditorGUIUtility.PingObject(gameObject);
        }
        //GeneratorUtils.GenerateTileSet();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
