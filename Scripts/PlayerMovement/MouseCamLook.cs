using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCamLook : MonoBehaviour
{
    [SerializeField]
    public float sensitivity = 5.0f;
    [SerializeField]
    public float smoothing = 2.0f;
    // Capsule is the Player //
    public GameObject player;
    // Get incremental value of mouse moving //
    private Vector2 mouseLook;
    // Smooth mouse moving //
    private Vector2 smoothV;
    
    // Initialization
    void Start() {
    
        player = this.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update() {
    // md is Mouse Delta // 
        var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        // the interpolated float result between two float values //
        smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
        // incrementally add to the camera look // 
        mouseLook += smoothV;

        // Vector3.right is the X axis //
        transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        player.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, player.transform.up);
    }
}