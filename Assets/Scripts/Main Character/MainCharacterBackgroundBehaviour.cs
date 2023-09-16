using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCharacterBackgroundBehaviour : MonoBehaviour
{
    public static Dictionary<string, float> SceneSizeDictionary { get; private set; }
    private Dictionary<string, float> _sceneGravityDictionary;
    private Rigidbody2D _rigidbody;

    // Start is called before the first frame update
    private void Start()
    {
        SceneSizeDictionary = new Dictionary<string, float>
        {
            {"Town", 0.2f},
            {"Main Character's House", 0.35f}
        };
        _sceneGravityDictionary = new Dictionary<string, float>
        {
            {"Town", 1f},
            {"Main Character's House", 2f}
        };

        _rigidbody = GetComponent<Rigidbody2D>();

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        transform.localScale = new Vector3(SceneSizeDictionary[scene.name], SceneSizeDictionary[scene.name],
            transform.localScale.z);
        _rigidbody.gravityScale = _sceneGravityDictionary[scene.name];
    }
}