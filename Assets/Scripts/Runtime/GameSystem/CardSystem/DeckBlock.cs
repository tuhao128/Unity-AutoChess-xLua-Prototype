using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBlock : MonoBehaviour
{
    private List<Card> _cards = new List<Card>();

    public IReadOnlyList<Card> Cards
    {
        get => _cards.AsReadOnly();
    }
    public void AddCards(params Card[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            _cards.Add(cards[i]);
        }
    }
    public void InitCards(params Card[] cards)
    {
        _cards.Clear();
        _cards.AddRange(cards);
    }
}
