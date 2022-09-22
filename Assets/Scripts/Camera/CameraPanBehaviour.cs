using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraPanBehaviour : MonoBehaviour
{
    public static Dictionary<string, CameraPanBehaviour> Instances { get; private set; }

    private GameObject _mainCharacter;
    private Transform _mainCharacterTransform;
    private Stopwatch _stopwatch;
    
    // Start is called before the first frame update
    void Start()
    {
        // singleton per scene
        Instances ??= new Dictionary<string, CameraPanBehaviour>();

        if (Instances.ContainsKey(SceneManager.GetActiveScene().name) &&
            Instances[SceneManager.GetActiveScene().name] != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instances[SceneManager.GetActiveScene().name] = this;
        }

        transform.position = new Vector3(0, 0, transform.position.z);
        _mainCharacter = GameObject.Find("Main Character");
        _mainCharacterTransform = _mainCharacter.transform;

        _stopwatch = new Stopwatch();
        _stopwatch.Reset();
        _stopwatch.Start();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mainCharacterPosition = _mainCharacterTransform.position;
        transform.position = new Vector3(Math.Clamp(mainCharacterPosition.x, 0f, float.PositiveInfinity),
            transform.position.y, transform.position.z);
    }
}
