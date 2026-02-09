//
// Ben Albers
// February 2026
// ActionTimer.cs
//

using Unity.VisualScripting;
using UnityEngine;

//ActionTimer lets a user easily assign timers to actions with the actionFired boolean. Instantiating prefabs is also included.
public class ActionTimer : MonoBehaviour
{
    //Action display name for Unity editor, not otherwise referenced in this script
    public string actionName;
    //Time until action occurs
    public float secondsToAction = 1.0f;
    //splashScreen makes hasActionFired take us to the next scene
    public bool nextScene = false;
    //Internal timer float
    private float timer = 0.0f;
    //Prefab to spawn after timer completes
    public GameObject prefabToSpawn;
    //Position to spawn new object
    public Transform spawnPoint;
    //Option to continue repeating the action after the first event is fired
    public bool repeatAction = false;
    //Number of actions possible to fire (or objects possible to spawn)
    public int maxActionCount = 1;
    //Option to keep looping endlessly, bypassing maxActionCount
    public bool infiniteActions = false;
    //Reference this hasActionFired boolean with another script on the same gameObject:
    // if (gameObject.GetComponent<ActionTimer>() && gameObject.GetComponent<ActionTimer>().hasActionFired) { }
    public bool hasActionFired = false;
    //Number of actions fired (or objects spawned if applicable)
    public int actionCount = 0;

    //Start function initializes spawn point
    private void Start()
    {
        //if no other spawn point is defined, 
        if (spawnPoint == null)
        {
            //set the gameObject this script is attached to as the spawn point
            spawnPoint = gameObject.transform;
        }
        if (prefabToSpawn == null)
        {
            //Debug.Log("No prefab assigned.");
        }
    }

    //Update function increments timer until timeToAction is hit, then sets actionFired bool to true and spawns a prefab if applicable
    private void Update()
    {
        //If we've not yet fired our action, or if we chose to reset and continue looping after firing once,
        if (!hasActionFired || repeatAction)
        {
            //and our internal timer is still below our number of seconds to action,
            if (timer < secondsToAction)
            {
                //increment timer by the length of time passed.
                timer += Time.deltaTime;
            }
            //when timer passes our time to action,
            else
            {
                //set hasActionFired bool to true for other scripts to reference
                hasActionFired = true;
                //check to make sure we haven't reached our maximum action count (or if we set infiniteActions to true)
                if (actionCount < maxActionCount || infiniteActions)
                {
                    //increment actionCount by one,
                    actionCount++;
                    //if we're looping this function,
                    if (repeatAction)
                    {
                        //reset our internal timer.
                        timer = 0.0f;
                    }
                    //if we have a prefab set to spawn,
                    if (prefabToSpawn != null)
                    {
                        //spawn that prefab at our assigned spawnPoint Transform (default this gameObject).
                        Instantiate(prefabToSpawn, spawnPoint);
                    }
                }
                //if we're going to the next scene when our event is fired,
                if (nextScene)
                {
                    //look for the SceneController component as sceneController,
                    if (TryGetComponent<SceneController>(out SceneController sceneController))
                    {
                        //if we don't already have a scene manager script on this gameObject,
                        if (!gameObject.GetComponent<SceneController>())
                        {
                            //add it.
                            sceneController = gameObject.AddComponent<SceneController>();
                        }
                        //load the next scene in the scene index (File > Build Profiles).
                        sceneController.LoadNextScene();
                    }
                }
            }
        }
    }
}