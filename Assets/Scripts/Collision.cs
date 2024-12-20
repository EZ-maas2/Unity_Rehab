using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Collision : MonoBehaviour
{
    public static event Action<string> OnPlateTriger;
    // Start is called before the first frame update
    
    // Update is called once per frame


   private void OnTriggerEnter(Collider colliderObj)
    {
        Debug.Log($"We have detected a collision with {gameObject.tag}");
        OnPlateTriger?.Invoke(gameObject.tag);
    }
}
