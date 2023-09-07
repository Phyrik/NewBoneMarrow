using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCharacterBackgroundBehaviour : MonoBehaviour
{
    private Dictionary<string, float> _sceneSizeDictionary;
    
    // Start is called before the first frame update
    void Start()
    {
        _sceneSizeDictionary = new Dictionary<string, float>
        {
            { "Town", 0.2f },
            { "Main Character's House", 0.35f }
        };
        
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Transform transform1 = transform;
        transform1.localScale = new Vector3(_sceneSizeDictionary[scene.name], _sceneSizeDictionary[scene.name],
            transform1.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
