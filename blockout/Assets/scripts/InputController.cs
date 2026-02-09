//
// Ben Albers
// February 2026
// InputController.cs
//

using UnityEngine;
using UnityEngine.InputSystem;

//IMPORTANT:
//Make sure to enable both old and new input handling in Unity:
//Edit > Project Settings > Player > Active Input Handling > Both

//Input Controller:
// ----------------
// Left and Right are keyed to the Left and Right Arrow Keys, and to "A" and "D".
// Up and Down are keyed to the Up and Down Arrow Keys, and to "W" and "S".
// Interact is keyed to Left Shift, Right Shift, and "E".
// Fire1, Fire2, Fire3, and Fire4 are keyed to 1, 2, 3, and 4 on both number pads,
// as well as to "V", "B", "N", and "M", and to the Controller buttons: "A", "B", "RT", and "LT".
// ----------------
public class InputController : MonoBehaviour
{
    //oppositeZero determines if holding opposite directions cancels out movement on that axis.
    public bool oppositeZero = false;

    //use booleans determine which inputs the player can use
    public bool useArrowKeys = true;
    public bool useWASD = true;
    public bool useEInteract = true;
    public bool useShiftInteract = true;
    public bool use1234Fire = true;
    public bool useVBNMFire = true;
    public bool holdFire1 = true;
    public bool holdFire2 = true;
    public bool holdFire3 = true;
    public bool holdFire4 = true;
    public bool useMouse = true;
    public bool interactOnClick = true;
    public bool fireOnClick = true;
    public bool fireOnClickRelease = false;
    public bool useController = true;
    public float controllerDeadZone = 0.1f;

    //A player controller script can reference these booleans to move according to player input.
    public bool moveLeft = false;
    public bool moveRight = false;
    public bool moveUp = false;
    public bool moveDown = false;
    public bool leftClick = false;
    public bool rightClick = false;
    public bool middleClick = false;
    public bool interact = false;
    public bool fire1 = false;
    public bool fire2 = false;
    public bool fire3 = false;
    public bool fire4 = false;
    public bool controllerConnected = false;

    //Update function reads current player inputs and translates them to boolean variables.
    public void Update()
    {
        //check if a controller or joystick is plugged in
        //if we're using a controller (true by default)
        if (useController)
        {
            //look for all joysticks
            var allJoysticks = Joystick.all;
            //if our total number of joysticks is more than zero
            if (allJoysticks.Count > 0)
            {
                //set controllerConnected to true for later checks
                if (!controllerConnected)
                {
                    controllerConnected = true;
                }
            }
            //otherwise set controllerConnected to false for later checks
            //this is important because a connected but unused controller stick sitting at a neutral position can cause problems with Input.GetAxis.
            else if (controllerConnected)
            { 
                controllerConnected = false;
            }
        }

        //Press left
        //Input.GetKeyDown references the frame the key is first tapped.
        if (useArrowKeys && Input.GetKeyDown(KeyCode.LeftArrow) || useWASD && Input.GetKeyDown(KeyCode.A) ||
            useController && controllerConnected && Input.GetAxis("Horizontal") < -controllerDeadZone)
        {
            moveLeft = true;
            moveRight = false;
            //Debug.Log("LEFT");
        }

        //Press right
        if (useArrowKeys && Input.GetKeyDown(KeyCode.RightArrow) || useWASD && Input.GetKeyDown(KeyCode.D) ||
            useController && controllerConnected && Input.GetAxis("Horizontal") > controllerDeadZone)
        {
            moveRight = true;
            moveLeft = false;
            //Debug.Log("RIGHT");
        }

        //Release left
        //Input.GetKeyUp references the frame the key is released.
        if (useArrowKeys && Input.GetKeyUp(KeyCode.LeftArrow) || useWASD && Input.GetKeyUp(KeyCode.A) ||
            useController && controllerConnected && ((Input.GetAxis("Horizontal") > -controllerDeadZone && Input.GetAxis("Horizontal") < controllerDeadZone)))
        {
            moveLeft = false;
            //Check if we're still holding the opposite direction,
            //Input.GetKey checks if the key is being held down.
            if (useArrowKeys && Input.GetKey(KeyCode.RightArrow) || useWASD && Input.GetKey(KeyCode.D))
            {
                //and if we are, start moving that direction again.
                if (moveRight == false)
                {
                    moveRight = true;
                    //Debug.Log("RIGHT");
                }
            }
            else
            {
                //Debug.Log("STOP LEFT");
            }
        }

        //Release right
        if (useArrowKeys && Input.GetKeyUp(KeyCode.RightArrow) || useWASD && Input.GetKeyUp(KeyCode.D) ||
            useController && controllerConnected && ((Input.GetAxis("Horizontal") > -controllerDeadZone && Input.GetAxis("Horizontal") < controllerDeadZone)))
        {
            moveRight = false;
            //Check if we're still holding the opposite direction,
            if (useArrowKeys && Input.GetKey(KeyCode.LeftArrow) || useWASD && Input.GetKey(KeyCode.A))
            {
                //and if we are, start moving that direction again.
                if (moveLeft == false)
                {
                    moveLeft = true;
                    //Debug.Log("LEFT");
                }
            }
            else
            {
                //Debug.Log("STOP RIGHT");
            }
        }

        //apply oppositeZero bool if applicable to neutralize left/right directional inputs
        if (oppositeZero)
        {
            if (useArrowKeys && Input.GetKey(KeyCode.RightArrow) || useWASD && Input.GetKey(KeyCode.D))
            {
                if (useArrowKeys && Input.GetKey(KeyCode.LeftArrow) || useWASD && Input.GetKey(KeyCode.A))
                {
                    moveRight = false;
                    moveLeft = false;
                }
            }
        }

        //move up and down

        //press up
        if (useArrowKeys && Input.GetKeyDown(KeyCode.UpArrow) || useWASD && Input.GetKeyDown(KeyCode.W) ||
            useController && controllerConnected && Input.GetAxis("Vertical") > controllerDeadZone)
        {
            moveUp = true;
            moveDown = false;
            //Debug.Log("UP");
        }

        //press down
        if (useArrowKeys && Input.GetKeyDown(KeyCode.DownArrow) || useWASD && Input.GetKeyDown(KeyCode.S) ||
            useController && controllerConnected && Input.GetAxis("Vertical") < -controllerDeadZone)
        {
            moveDown = true;
            moveUp = false;
            //Debug.Log("DOWN");
        }

        //release up
        if (useArrowKeys && Input.GetKeyUp(KeyCode.UpArrow) || useWASD && Input.GetKeyUp(KeyCode.W) ||
            useController && controllerConnected && ((Input.GetAxis("Vertical") > -controllerDeadZone && Input.GetAxis("Vertical") < controllerDeadZone)))
        {
            moveUp = false;
            //check if we're still holding the opposite direction
            if (useArrowKeys && Input.GetKey(KeyCode.DownArrow) || useWASD && Input.GetKey(KeyCode.S))
            {
                //and if we are, start moving that direction again.
                if (moveDown == false)
                {
                    moveDown = true;
                    //Debug.Log("DOWN");
                }
            }
            else
            {
                //Debug.Log("STOP UP");
            }
        }

        //release down
        if (useArrowKeys && Input.GetKeyUp(KeyCode.DownArrow) || useWASD && Input.GetKeyUp(KeyCode.S) ||
            useController && controllerConnected && ((Input.GetAxis("Vertical") > -controllerDeadZone && Input.GetAxis("Horizontal") < controllerDeadZone)))
        {
            moveDown = false;
            //check if we're still holding the opposite direction
            if (useArrowKeys && Input.GetKey(KeyCode.UpArrow) || useWASD && Input.GetKey(KeyCode.W))
            {
                //and if we are, start moving that direction again.
                if (moveUp == false)
                {
                    moveUp = true;
                    //Debug.Log("UP");
                }
            }
            else
            {
                //Debug.Log("STOP DOWN");
            }
        }

        //apply oppositeZero bool if applicable to neutralize up/down directional inputs
        if (oppositeZero)
        {
            if (useArrowKeys && Input.GetKey(KeyCode.DownArrow) || useWASD && Input.GetKey(KeyCode.S))
            {
                if (useArrowKeys && Input.GetKey(KeyCode.UpArrow) || useWASD && Input.GetKey(KeyCode.W))
                {
                    moveDown = false;
                    moveUp = false;
                }
            }
        }

        //interact key

        if (useShiftInteract && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) || useEInteract && Input.GetKeyDown(KeyCode.E) || useController && controllerConnected && Input.GetKeyDown(KeyCode.JoystickButton0)) //A on XBox
        {
            interact = true;
            //Debug.Log("INTERACT");
        }
        else
        {
            if (interact == true)
            {
                interact = false;
            }
        }

        //fire 1-4

        if (use1234Fire && (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) || useVBNMFire && Input.GetKeyDown(KeyCode.V) || useController && controllerConnected && Input.GetKeyDown(KeyCode.JoystickButton1)) //B on XBox
        {
            fire1 = true;
            //Debug.Log("FIRE 1");
        }
        else
        {
            if (fire1 == true)
            {
                fire1 = false;
            }
        }

        if (use1234Fire && (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) || useVBNMFire && Input.GetKeyDown(KeyCode.B) || useController && controllerConnected && (Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton3))) //right trigger or button3
        {
            fire2 = true;
            //Debug.Log("FIRE 2");
        }
        else
        {
            if (fire2 == true)
            {
                fire2 = false;
            }
        }

        if (use1234Fire && (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) || useVBNMFire && Input.GetKeyDown(KeyCode.N) || useController && controllerConnected && Input.GetKeyDown(KeyCode.JoystickButton4)) //left trigger
        {
            fire3 = true;
            //Debug.Log("FIRE 3");
        }
        else
        {
            if (fire3 == true)
            {
                fire3 = false;
            }
        }

        if (use1234Fire && (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) || useVBNMFire && Input.GetKeyDown(KeyCode.M) || useController && controllerConnected && (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.JoystickButton6))) //X on XBox or button6
        {
            fire4 = true;
            //Debug.Log("FIRE 4");
        }
        else
        {
            if (fire4 == true)
            {
                fire4 = false;
            }
        }

        //holdFire 1-4
        if (holdFire1)
        {
            if (use1234Fire && (Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Alpha1)) || useVBNMFire && Input.GetKey(KeyCode.V) || useController && controllerConnected && Input.GetKey(KeyCode.JoystickButton1)) //B on XBox
            {
                fire1 = true;
                //Debug.Log("FIRE 1");
            }
            else
            {
                if (fire1 == true)
                {
                    fire1 = false;
                }
            }
        }
        if (holdFire2)
        {
            if (use1234Fire && (Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Alpha2)) || useVBNMFire && Input.GetKey(KeyCode.B) || useController && controllerConnected && (Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.JoystickButton3))) //right trigger or button3
            {
                fire2 = true;
                //Debug.Log("FIRE 2");
            }
            else
            {
                if (fire2 == true)
                {
                    fire2 = false;
                }
            }
        }
        if (holdFire3)
        {
            if (use1234Fire && (Input.GetKey(KeyCode.Keypad3) || Input.GetKey(KeyCode.Alpha3)) || useVBNMFire && Input.GetKey(KeyCode.N) || useController && controllerConnected && (Input.GetKey(KeyCode.JoystickButton4))) //left trigger 
            {
                fire3 = true;
                //Debug.Log("FIRE 3");
            }
            else
            {
                if (fire3 == true)
                {
                    fire3 = false;
                }
            }
        }
        if (holdFire4)
        {
            if (use1234Fire && (Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.Alpha4)) || useVBNMFire && Input.GetKey(KeyCode.M) || useController && controllerConnected && (Input.GetKey(KeyCode.JoystickButton2) || Input.GetKey(KeyCode.JoystickButton6))) //X on XBox or button6
            {
                fire4 = true;
                //Debug.Log("FIRE 4");
            }
            else
            {
                if (fire4 == true)
                {
                    fire4 = false;
                }
            }
        }

        //mouse input
        if (useMouse)
        {
            //LMB
            //left click down

            if (Input.GetMouseButtonDown(0))
            {
                leftClick = true;
                if (interactOnClick)
                {
                    interact = true;
                }
                if (fireOnClick)
                {
                    fire1 = true;
                }
            }
            else if (leftClick == true)
            {
                leftClick = false;
                interact = false;
                fire1 = false;
            }

            //left click and hold
            if (Input.GetMouseButton(0))
            {
                leftClick = true;
                if (holdFire1)
                {
                    fire1 = true;
                }
            }

            //left click released
            if (Input.GetMouseButtonUp(0))
            {
                if (fireOnClickRelease)
                {
                    leftClick = true;
                    fire1 = true;
                }
                else if(leftClick == true)
                {
                    leftClick = false;
                    fire1 = false;
                }
            }


            //RMB
            //right click down
            if (Input.GetMouseButtonDown(1))
            {
                rightClick = true;
                if (fireOnClick)
                {
                    fire2 = true;
                }
            }
            else if (rightClick == true)
            {
                rightClick = false;
                fire2 = false;
            }

            //right click and hold
            if (Input.GetMouseButton(1))
            {
                rightClick = true;
                if (holdFire2)
                {
                    fire2 = true;
                }
            }

            //right click released
            if (Input.GetMouseButtonUp(0))
            {
                if (fireOnClickRelease)
                {
                    rightClick = true;
                    fire2 = true;
                }
                else if (rightClick == true)
                {
                    rightClick = false;
                    fire2 = false;
                }
            }


            //MMB
            //middle click down
            if (Input.GetMouseButtonDown(2))
            {
                middleClick = true;

                if (fireOnClick)
                {
                    fire3 = true;
                }
            }
            else if (middleClick == true)
            {
                middleClick = false;
                fire3 = false;
            }

            //middle click and hold
            if (Input.GetMouseButton(2))
            {
                middleClick = true;
                if (holdFire3)
                {
                    fire3 = true;
                }
            }

            //middle click released
            if (Input.GetMouseButtonUp(2))
            {
                if (fireOnClickRelease)
                {
                    middleClick = true;
                    fire3 = true;
                }
                else if (middleClick == true)
                {
                    middleClick = false;
                    fire3 = false;
                }
            }
        }
    }
}
