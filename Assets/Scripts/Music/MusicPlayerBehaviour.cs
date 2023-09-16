using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayerBehaviour : MonoBehaviour
{
    public float playSecondsFromStart;
    private AudioSource _audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.PlayDelayed(playSecondsFromStart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
