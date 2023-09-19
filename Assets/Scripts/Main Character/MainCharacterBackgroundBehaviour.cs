using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCharacterBackgroundBehaviour : MonoBehaviour
{
    public static Dictionary<string, float> SceneSizeDictionary { get; private set; } // public get due to being used for speed calculations by movement script
    private Dictionary<string, float> _sceneGravityDictionary;
    private Dictionary<string, MainCharacterMovementBehaviour.PreventMovementCollider[]> _scenePreventMovementCollidersDictionary;
    private Rigidbody2D _rigidbody;
    private MainCharacterMovementBehaviour _mainCharacterMovementBehaviour;

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
        _scenePreventMovementCollidersDictionary =
            new Dictionary<string, MainCharacterMovementBehaviour.PreventMovementCollider[]>
            {
                {
                    "Town",
                    new MainCharacterMovementBehaviour.PreventMovementCollider[]
                        {new MainCharacterMovementBehaviour.PreventMovementCollider("Rocks", true, false)}
                },
                {"Main Character's House", new MainCharacterMovementBehaviour.PreventMovementCollider[] { }}
            };

        _rigidbody = GetComponent<Rigidbody2D>();
        _mainCharacterMovementBehaviour = GetComponent<MainCharacterMovementBehaviour>();

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManagerOnsceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        transform.localScale = new Vector3(SceneSizeDictionary[scene.name], SceneSizeDictionary[scene.name],
            transform.localScale.z);
        _rigidbody.gravityScale = _sceneGravityDictionary[scene.name];
        _mainCharacterMovementBehaviour.preventMovementColliders = _scenePreventMovementCollidersDictionary[scene.name];
    }
}