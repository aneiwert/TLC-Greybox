using UnityEngine;

public class alarmboxAnim : MonoBehaviour
{
    //Angel Rennick
    public GameObject player;
    // Update is called once per frame
    public bool AlarmAnim = false;
    public bool istriggered = false;
    void Update()
    {
        if (AlarmAnim == true && istriggered == false )
        {
            player.GetComponent<Animator>().Play("New Animation");
            Debug.Log("Click");
            istriggered = true;
        }

    }
}
