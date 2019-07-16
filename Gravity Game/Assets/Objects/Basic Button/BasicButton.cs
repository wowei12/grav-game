using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicButton : MonoBehaviour
{
    public GameObject connected;

    private GameObject below;
    private bool TurnedOn;

    void Start()
    {
        below = this.transform.GetChild(0).gameObject;
        TurnedOn = false;
    }

    void OnCollisionStay(Collision c)
    {
        if ((c.gameObject.tag == "Player" || c.gameObject.tag == "Interactable") && !TurnedOn)
        {
            if (connected != null)
            {
                connected.GetComponent<Activatable>();
            }
            below.transform.position = below.transform.position - below.transform.up * 0.10f;
            TurnedOn = true;
        }
    }

    void OnCollisionExit(Collision c)
    {
        if ((c.gameObject.tag == "Player" || c.gameObject.tag == "Interactable") && TurnedOn)
        {
            below.transform.position = below.transform.position + below.transform.up * 0.10f;
            TurnedOn = false;
        }
    }
}
