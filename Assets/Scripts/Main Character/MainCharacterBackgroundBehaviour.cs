using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCharacterBackgroundBehaviour : MonoBehaviour
{
    public static Dictionary<string, float> SceneSizeDictionary { get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        SceneSizeDictionary = new Dictionary<string, float>
        {
            {"Town", 0.2f},
            {"Main Character's House", 0.35f}
        };

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Transform transform1 = transform;
        transform1.localScale = new Vector3(SceneSizeDictionary[scene.name], SceneSizeDictionary[scene.name],
            transform1.localScale.z);
    }
}