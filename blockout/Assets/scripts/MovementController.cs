//
// Ben Albers
// January 2026
// MovementController.cs
//

using UnityEngine;

//MovementController lets players move an object based on Position, Velocity, or Acceleration.
public class MovementController : MonoBehaviour
{
    //Movement types: 1 = position, 2 = velocity, 3 = acceleration
    public int movementType = 1;

    //XY lets the player move along the X and Y axes,
    public bool XY = true;
    //XZ lets the player move along the X and Z axes.
    public bool XZ = false;

    //How far the object steps in movement type 1
    public float movePosition = 0.01f;
    //How fast the object moves in movement type 2
    public float moveVelocity = 200.0f;
    //How quickly the object accelerates in movement type 3
    public float moveAcceleration = 40.0f;
    //How much drag the object has in movement type 3
    public float moveDeceleration = 8.0f;
    
    //inputController interprets keyboard inputs as booleans
    public InputController inputController;
    //Rigidbody rb is our object's rigidbody to influence with movement types 2 and 3
    public Rigidbody rb;

    //Start initializes the object's inputController and Rigidbody.
    public void Start()
    {
        //if we don't already have an inputController assigned,
        if (inputController == null)
        {
            //if the gameObject we're attached to needs an inputController,
            if (gameObject.GetComponent<InputController>() == null)
            {
                //add one.
                gameObject.AddComponent<InputController>();
            }
            //assign the inputController to the movementController.
            inputController = gameObject.GetComponent<InputController>();
        }
        //if we don't already have a Rigidbody assigned,
        if (rb == null)
        {
            //if the gameObject we're attached to lacks a Rigidbody,
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                //add one.
                gameObject.AddComponent<Rigidbody>();
            }
            //assign the Rigidbody to the movementController.
            rb = gameObject.GetComponent<Rigidbody>();
        }
        //turn off gravity,
        rb.useGravity = false;
        //set angular damping to zero,
        rb.angularDamping = 0.0f;
        //set linear damping to our moveDeceleration to represent friction.
        rb.linearDamping = moveDeceleration;
    }

    //Update lets us define our movement type, and reposition our object according to movement type 1
    public void Update()
    {
        //Use the 1, 2, and 3 keys to switch movement types between Position, Velocity, and Acceleration.
        if (inputController.fire1)
        {
            //Position
            movementType = 1;
        }
        if (inputController.fire2)
        {
            //Velocity
            movementType = 2;
        }
        if (inputController.fire3)
        {
            //Acceleration
            movementType = 3;
        }

        //Move based on Position
        if (movementType == 1)
        {
            //if we have an active inputController,
            if (inputController != null)
            {
                //on input, set transform.position as a new Vector3 with the old Vector3 components plus or minus your movePosition.
                if (inputController.moveLeft)
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x - movePosition, gameObject.transform.position.y, gameObject.transform.position.z);
                }
                if (inputController.moveRight)
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x + movePosition, gameObject.transform.position.y, gameObject.transform.position.z);
                }
                if (XY)
                {
                    if (inputController.moveUp)
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + movePosition, gameObject.transform.position.z);
                    }
                    if (inputController.moveDown)
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - movePosition, gameObject.transform.position.z);
                    }
                }
                if (XZ)
                {
                    if (inputController.moveUp)
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + movePosition);
                    }
                    if (inputController.moveDown)
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - movePosition);
                    }
                }
            }
        }
    }

    //FixedUpdate is similar to the Update function. It's used when dealing with Unity's physics system.
    public void FixedUpdate()
    {
        //Move based on Velocity
        if (movementType == 2)
        {
            //if we have an active inputController,
            if (inputController != null)
            {
                //Base case: don't move.
                gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

                //on input, set RigidBody.linearVelocity based on MoveVelocity.
                if (inputController.moveLeft)
                {
                    gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.left * moveVelocity * Time.deltaTime;
                }
                if (inputController.moveRight)
                {
                    gameObject.GetComponent<Rigidbody>().linearVelocity = -Vector3.left * moveVelocity * Time.deltaTime;
                }
                if (XY)
                {
                    if (inputController.moveUp)
                    {
                        gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.up * moveVelocity * Time.deltaTime;
                    }
                    if (inputController.moveDown)
                    {
                        gameObject.GetComponent<Rigidbody>().linearVelocity = -Vector3.up * moveVelocity * Time.deltaTime;
                    }
                }
                if (XZ)
                {
                    if (inputController.moveUp)
                    {
                        gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.forward * moveVelocity * Time.deltaTime;
                    }
                    if (inputController.moveDown)
                    {
                        gameObject.GetComponent<Rigidbody>().linearVelocity = -Vector3.forward * moveVelocity * Time.deltaTime;
                    }
                }
            }
        }

        //Move based on Acceleration
        if (movementType == 3)
        {
            //if we have an active inputController,
            if (inputController != null)
            {
                //on input, set RigidBody.AddForce based on moveAcceleration.
                if (inputController.moveLeft)
                {
                    rb.AddForce(Vector3.left * moveAcceleration);
                }
                if (inputController.moveRight)
                {
                    rb.AddForce(-Vector3.left * moveAcceleration);
                }
                if (XY)
                {
                    if (inputController.moveUp)
                    {
                        rb.AddForce(Vector3.up * moveAcceleration);
                    }
                    if (inputController.moveDown)
                    {
                        rb.AddForce(-Vector3.up * moveAcceleration);
                    }
                }
                if (XZ)
                {
                    if (inputController.moveUp)
                    {
                        rb.AddForce(Vector3.forward * moveAcceleration);
                    }
                    if (inputController.moveDown)
                    {
                        rb.AddForce(-Vector3.forward * moveAcceleration);
                    }
                }
            }
        }
    }
}