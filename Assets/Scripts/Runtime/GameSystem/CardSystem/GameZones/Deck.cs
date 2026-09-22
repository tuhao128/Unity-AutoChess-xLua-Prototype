using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckCards
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
public class Deck : GameZone, IPhasePackage, IPointerDownHandler
{
    private GameZoneType _aimZone = GameZoneType.Hand;

    protected override int MaxCount => 0;

    protected override bool HasMaxCount => false;

    protected override GameZoneType _type => GameZoneType.Deck;

    protected override bool keepWorldPos => false;

    protected DeckCards deck = new DeckCards();

    protected override void AfterAdd(Card card, Vector3 calculate)
    {
        card.transform.DOLocalMove(calculate, 0.4f);
        card.gameObject.SetActive(false);
    }

    protected override void AfterInMax(Card card)
    {

    }

    protected override void AfterRemove(Card card)
    {
        card.gameObject.SetActive(true);
    }

    protected override Vector3 CalculatePos(Card card)
    {
        return Vector3.zero;
    }

    public void DrawCardTo(GameZone gameZone, PlayerController owner)
    {
        Card card = null;
        if (_cardIn.Count != 0)
        {
            card = _cardIn[_cardIn.Count - 1];
            Remove(card);
        }
        if (card == null) return;
        gameZone.AddCard(card, owner);
    }

    public void InitCards(params Card[] cards)
    {
        deck.InitCards(cards);
        IReadOnlyList<Card> cards1 = deck.Cards;
        for (int i = 0; i < cards1.Count; i++)
        {
            AddCard(cards1[i], PlayerController.Instance);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerController.Instance.CollectDataForEvent(this);
    }

    public void Act()
    {
        DrawCardTo(PlayerController.Instance.GameZones.ReadZone(_aimZone), PlayerController.Instance);
    }
}
