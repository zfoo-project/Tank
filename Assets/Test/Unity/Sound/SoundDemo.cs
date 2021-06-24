using System;
using System.Collections;
using System.Collections.Generic;
using Spring.Logger;
using UnityEngine;

public class SoundDemo : MonoBehaviour
{
    
    private  AudioSource audioSource; 
    // Start is called before the first frame update
    void Start()
    {
        // audioSource = GetComponent<AudioSource>();
        // audioSource.PlayDelayed(10);
        // audioSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Log.Info(pauseStatus);
    }
}
