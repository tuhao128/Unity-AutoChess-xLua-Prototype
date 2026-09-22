using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

[CSharpCallLua]
public delegate void CardEventHandler(CardMessage msg);


public struct CardMessage
{
    private Card _card;

    public Card AskFrom => _card;

    public CardMessage(Card card)
    {
        _card = card;
    }
}

public struct CardAsk
{
    private CardMessage _cardMsg;

    private CardEventHandler _action;

    public void Act()
    {
        _action?.Invoke(_cardMsg);
    }

    public CardAsk(CardMessage cardMsg, CardEventHandler action)
    {
        _cardMsg = cardMsg;
        _action = action;
    }
}
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler, IPhasePackage
{
    private string _cardName;

    public string CardName
    {
        get => _cardName;
        set => _cardName = value;
    }

    private CardEventHandler _onUse;

    public CardEventHandler OnUse
    {
        set => _onUse = value;
    }

    private CardEventHandler _onDrop;

    public CardEventHandler OnDrop
    {
        set => _onDrop = value;
    }

    private bool _onDrag;

    private int _oldIndex;

    public int OldIndex => _oldIndex;

    private bool _useOld;

    public bool UseOld => _useOld;

    private PlayerController _owner;

    public PlayerController Owner
    {
        get => _owner;
        set
        {
            _owner = value;
        }
    }

    private bool _canPlay;

    private GameZoneType _zoneType;

    public GameZoneType GameZoneType
    {
        get
        {
            return _zoneType;
        }
        set
        {
            _zoneType = value;
        }
    }

    public GameZone GameZone
    {
        get
        {
            return Owner.GameZones.ReadZone(GameZoneType);
        }
    }

    public bool CanPlay
    {
        get
        {
            return _canPlay;
        }
        set
        {
            _canPlay = value;
        }
    }

    public void Play()
    {
        //PlayerController.Instance.CollectDataForEvent(this);
        PlayerController.Instance.CollectDataForEvent(this);
        MoveTo(GameZoneType.None, false, false);
    }

    public void MoveTo(GameZoneType type, bool useOld, bool keepIndex)
    {
        bool flag = type != this.GameZoneType;
        if (flag)
        {
            _useOld = useOld;
            if (keepIndex)
            {
                _oldIndex = this.GameZone.ReadIndex(this);
            }
            Owner.GameZones.RemoveCardTo(this, type);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Owner.CanMoveCard)
        {
            _onDrag = true;
            Vector3 vector3 =
            eventData.pressEventCamera.ScreenToWorldPoint(new Vector3(eventData.position.x,
            eventData.position.y,
            //eventData.pressEventCamera.WorldToScreenPoint(this.transform.position).z
            0.6f
            ));
            MoveTo(GameZoneType.Air, false, true);

            this.transform.DOMove(vector3, 0.3f);
        }
        else
        {
            MoveTo(GameZoneType.Hand, true, false);
            _onDrag = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _onDrag = false;
        MoveTo(GameZoneType.Hand, true, false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.DOScale(Vector3.one * 1.4f, 0.4f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_onDrag)
        {
            this.transform.DOScale(Vector3.one, 0.4f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (this.CanPlay)
        {
            this.Play();
        }
    }

    public void Act()
    {
        new CardAsk(new CardMessage(this), _onUse).Act();
    }

    // void FixedUpdate()
    // {
    //     if (_searchingPath)
    //     {
    //         if (_oldPathPoint != Vector3.zero)
    //         {
    //             Debug.DrawLine(_oldPathPoint, this.transform.eulerAngles, Color.green, 100f);
    //         }
    //         _oldPathPoint = this.transform.eulerAngles;
    //     }
    // }
}
