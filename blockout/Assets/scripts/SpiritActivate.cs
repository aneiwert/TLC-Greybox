using UnityEngine;
using System.Collections;
using UnityEditor;
public class SpiritActivate : MonoBehaviour
{
    //Angel Rennick
    public GameObject Spirit;
    public Vector3 Offset;
    public bool istriggered = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        InstantianteSpirit();
    }
    public void InstantianteSpirit()
    {
        if (GameObject.FindWithTag("timer").GetComponent<Timer>().Death == true && istriggered == false)
        {
            Instantiate(Spirit, -transform.position ,transform.rotation);
            GameObject.FindWithTag("spirit").GetComponent<spiritAnim>().SpiritAnima = true;
            istriggered = true;
            Debug.Log("Spirit!");
        }
    }
}
