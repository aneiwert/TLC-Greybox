using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : MonoBehaviour
{
    public float CrouchHeight = 0.5f;
    public float CrouchSmoothTime = 0.5f;

    Vector3 m_StandingPosition;
    Vector3 m_CrouchingPosition;

    Vector3 m_CurrentVelocity;

    void Start()
    {
        m_StandingPosition = new Vector3(0.0f, 0.0f, 0.0f);
        m_CrouchingPosition = new Vector3(0.0f, -1.0f, 0.0f);
    }
    
    void Awake()
    {
        //Starting Height is Standard height.//
        m_StandingPosition = transform.transform.localPosition;

        // Move the Position down to Crouching Position 
    // m_CrouchingPosition = transform.localPosition;
       // m_CrouchingPosition = CrouchHeight;//
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

        // Smoothly Moves camera toward position //
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPosition,
            ref m_CurrentVelocity,
            CrouchSmoothTime
        );
    }
}
