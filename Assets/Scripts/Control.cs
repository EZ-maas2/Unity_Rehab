using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// This script is keeping trcak of what is target, and whether the target has been  hit yet


public class Control : MonoBehaviour
{
    public string[] target_seq =  {"Front", "Center", "Side", "Center"}; // maybe this should represnt tags of intended collsiion objects
    bool hitTarget = false;
    string currentTarget; // we want to notify interested parties when currentTarget gets changed
    int target_ix = 0;

    // Start is called before the first frame update
    void Start()
    {
        // TO DO: subscribe hitTarget to get changed when some other script does something
        Collision.OnPlateTriger += ReactToCollision;
        currentTarget = target_seq[target_ix];
    }

    // Update is called once per frame
    void Update()
    {
        if (hitTarget)
        {
            hitTarget = false;
            target_ix ++;
            if (target_ix >= target_seq.Length) 
            { target_ix = 0; } // if we are outside of range of target sequence, restart it
        }
        currentTarget = target_seq[target_ix];
    }


    void ReactToCollision(string tag){
        // here we will have a code that receives notification about collisions
        // it then cehcks if collision is with target
        // if yes, hitTarget is set to True
        if (tag == currentTarget)
        {
            Debug.Log("Hit current target, let's set new target!");
            hitTarget = true;
        }
        else
        {
            Debug.Log($"Current target is {currentTarget}, but we are detecting {tag}");
        }


    }
}
