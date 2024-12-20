using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is keeping trcak of what is target, and whether the target has been  hit yet

enum Target { Front, Center, Side, Back}



public static class Control 
{
    public Target[] target_seq =  [Front, Center, Front, Center, Side, Center, Side, Center]; // maybe this should represnt tags of intended collsiion objects
    bool hitTarget = False;
    Target currentTarget; // we want to notify interested parties when currentTarget gets changed
    int target_ix = 0;

    // Start is called before the first frame update
    void Start()
    {
        // TO DO: subscribe hitTarget to get changed when some other script does something
        currentTarget = target_seq[target_ix];
    }

    // Update is called once per frame
    void Update()
    {
        if (hitTarget)
        {
            target_ix ++;
            if (target_ix >= target_seq.Length) { target_ix = 0; }
        }
        currentTarget = target_seq[target_ix];
    }


    void ReactToCollision(){
        // here we will have a code that receives notification about collisions
        // it then cehcks if collision is with target
        // if yes, hitTarget is set to True


    }
}
