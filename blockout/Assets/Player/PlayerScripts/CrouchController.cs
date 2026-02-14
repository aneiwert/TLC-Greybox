using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : MonoBehaviour
{
    public float CrouchHeight = 1.0f;
    public float CrouchSmoothTime = 0.5f;
    public GameObject player; 
    public Vector3 m_StandingPosition;
    public Vector3 m_CrouchingPosition;

    public Vector3 m_CurrentVelocity;

    void Start()
    {
        m_StandingPosition = new Vector3(0.0f, 1.0f, 0.0f);
        m_CrouchingPosition = new Vector3(0.0f, 0.2f, 0.0f);
    }
    
    void Awake()
    {
        //Starting Height is Standard height.//
        m_StandingPosition = transform.transform.localPosition;

        // Move the Position down to Crouching Position 
    //m_CrouchingPosition = transform.localPosition;
      // m_CrouchingPosition = CrouchHeight;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Decides what height the camera moves towards the frame. //

        Vector3 targetPosition;
        //KeyCode
        if(Input.GetKey(KeyCode.C))
        {
            targetPosition = m_CrouchingPosition;
        }
        else
        {
            targetPosition = m_StandingPosition;
        } 

    }
}
