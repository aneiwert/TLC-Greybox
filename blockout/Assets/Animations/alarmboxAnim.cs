using UnityEngine;

public class alarmboxAnim : MonoBehaviour
{
    public GameObject Player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Alarmbox"))
        {
            Player.GetComponent<Animator>().Play("New Animation");
            Debug.Log("Click");
        }

    }
}
