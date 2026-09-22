using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCard : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer & LayerMask.GetMask("Card")) != 0)
        {
            Card card = other.gameObject.GetComponent<Card>();
            if (card.CanPlay)
            {
                card.Play();
            }
        }
    }
}
