using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




// This script is keeping trcak of what is target, and whether the target has been  hit yet


public class Control : MonoBehaviour
{
    public string[] target_seq =  {"Front", "Center", "Side", "Center"}; // maybe this should represnt tags of intended collsiion objects
    bool hitNewTarget = false;
    string currentTarget; // we want to notify interested parties when currentTarget gets changed
   
    GameObject currentTargetObj;
    public static event Action<GameObject> OnCurrTargetChanged;
    public static event Action OnCalEnded;

    int target_ix = 0;

    public Vector3 screen_coordinates;
    public Vector3 screen_rotation;
    private GameObject currentScreen;  // should this be a pointer
    
    public GameObject screen_front;
    public GameObject screen_side;
    public GameObject screen_center;
    public GameObject screen_back;
    private string nextScreenTag; 
    private bool CalibrationOver = false; 
    private bool StartedLogic = false; 

    // Start is called before the first frame update
    void Start()
    {

        // Ensure all screens are instantiated but only one is active
        screen_front = Instantiate(screen_front, screen_coordinates, Quaternion.Euler(screen_rotation));
        screen_side = Instantiate(screen_side, screen_coordinates, Quaternion.Euler(screen_rotation));
        screen_back = Instantiate(screen_back, screen_coordinates, Quaternion.Euler(screen_rotation));
        screen_center = Instantiate(screen_center, screen_coordinates, Quaternion.Euler(screen_rotation));

        // Disable all screens initially
        screen_front.SetActive(false);
        screen_side.SetActive(false);
        screen_back.SetActive(false);
        screen_center.SetActive(false);

        // Set default screen
        currentScreen = screen_center;
        currentScreen.SetActive(true);
        // TO DO: subscribe hitTarget to get changed when some other script does something
        Collision.OnPlateTriger += ReactToCollision;
        TcpReceiver.OnCalibrationReceived += UpdateInstructions;
        TcpReceiver.OnCalibrationOver += ReactToCalibrationOver;
        Control.OnCalEnded += LogicStart;
        //SetCurrTarget(target_ix);
    }

    void ReactToCalibrationOver()
    {
        Debug.Log("React to calibration over is called");
        CalibrationOver = true;
        OnCalEnded?.Invoke();
    }

    void LogicStart(){
        
        Debug.Log("Logic Started");
        SetCurrTarget(target_ix);
    }

    // Update is called once per frame
    void Update()
    {

        if (hitNewTarget)
        {
            hitNewTarget = false;
            target_ix ++;
            if (target_ix >= target_seq.Length) 
                { target_ix = 0; } // if we are outside of range of target sequence, restart it
            SetCurrTarget(target_ix);
        }
        ExecuteUpdateInstructions();
    }

    void SetCurrTarget(int ix)
    {
        // get the current target object
        currentTarget = target_seq[ix];
        UpdateInstructions(currentTarget);
        currentTargetObj = GameObject.FindWithTag(currentTarget);
        OnCurrTargetChanged?.Invoke(currentTargetObj);
    }

    void ReactToCollision(string tag){
        // here we will have a code that receives notification about collisions
        // it then cehcks if collision is with target
        // if yes, hitTarget is set to True
        if (tag == currentTarget)
        {
            Debug.Log("Hit current target, let's set new target!");
            hitNewTarget = true;
            gameObject.GetComponent<AudioSource>().Play();
        }
        else
        {
            Debug.Log($"Current target is {currentTarget}, but we are detecting {tag}");
        }
    }


void UpdateInstructions(string target_tag)
{
    nextScreenTag = target_tag;
}

void ExecuteUpdateInstructions()
{
    if (currentScreen) currentScreen.SetActive(false); // turn off the previous screen

    switch (nextScreenTag)
    {
        case "Front":
            currentScreen = screen_front;
            break;
        case "Side":
            currentScreen = screen_side;
            break;
        case "Back":
            currentScreen = screen_back;
            break;
        default:
            currentScreen = screen_center;
            break;
    }

    if (currentScreen) currentScreen.SetActive(true); // turn on the new screen

}
}






