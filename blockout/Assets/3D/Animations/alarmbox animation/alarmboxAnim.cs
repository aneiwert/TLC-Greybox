using UnityEngine;

public class alarmboxAnim : MonoBehaviour
{
    public GameObject player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Alarmbox"))
        {
            player.GetComponent<Animator>().Play("New Animation");
            Debug.Log("Click");
        }

    }
}
