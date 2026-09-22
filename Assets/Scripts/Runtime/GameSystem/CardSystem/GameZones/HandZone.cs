using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandZone : GameZone
{
    private Vector3[] _poss;
    private float _minPoint;
    private float _maxPoint;

    private int _maxCount;

    private bool _hasMax;

    private float _distance;
    public void Init(float _minPoint, float _maxPoint, float distance, int maxCount, bool hasMax)
    {
        this._minPoint = _minPoint;
        this._maxPoint = _maxPoint;
        this._distance = distance;
        this._maxCount = maxCount;
        _hasMax = hasMax;
    }

    protected override GameZoneType _type => GameZoneType.Hand;

    protected override int MaxCount => _maxCount;

    protected override bool HasMaxCount => _hasMax;

    protected override bool keepWorldPos => true;

    protected override void AfterAdd(Card card, Vector3 calculate)
    {
        //card.transform.localPosition = calculate;
        //card.transform.DOLocalMove(calculate, 1);
    }

    protected override void AfterInMax(Card card)
    {
        //Debug.Log(card);
    }

    protected override void AfterRemove(Card card)
    {
        CalculatePosAndMove(card);
    }

    protected override Vector3 CalculatePos(Card card)
    {
        if (card.transform.localEulerAngles != new Vector3(0, 180, 0))
        {
            card.transform.DOLocalRotate(new Vector3(0, 180, 0), 0.4f);
        }
        return CalculatePosAndMove(card);
    }

    private Vector3 CalculatePosAndMove(Card card)
    {
        bool hasCenter;
        int countOneSide;
        int currentCount;
        if (_cardIn.Count % 2 == 0)
        {
            hasCenter = false;
            countOneSide = _cardIn.Count / 2;
        }
        else
        {
            hasCenter = true;
            countOneSide = (_cardIn.Count - 1) / 2;
        }

        Vector3 vector3 = Vector3.zero;
        bool calculateNew = true;
        if (card.UseOld)
        {
            _cardIn.Remove(card);
            _cardIn.Insert(card.OldIndex, card);
            calculateNew = false;
        }

        Vector3 pos = Vector3.zero;
        currentCount = -countOneSide;
        foreach (var cardV in _cardIn)
        {
            pos = GetPos(currentCount, hasCenter);
            if (!calculateNew && cardV == card)
            {
                vector3 = pos;
            }
            cardV.transform.DOLocalMove(pos, 1);
            currentCount++;
        }
        if (calculateNew)
        {
            vector3 = pos;
        }
        return vector3;
    }
    
    private Vector3 GetPos(int count, bool hasCenter)
    {
        float slide = 0;
        if (!hasCenter)
        {
            slide = _distance / 2;
        }
        // if (hasCenter)
        // {
        //     if (count > 0)
        //     {
        //         return new Vector3((count + 1) * _distance, 0, 0);
        //     }
        //     else if (count == 0)
        //     {
        //         return Vector3.zero;
        //     }
        //     else
        //     {
        //         return new Vector3((count - 1) * _distance, 0, 0);
        //     }
        // }
        // else
        // {
        //     return new Vector3(count * _distance, 0, 0);
        // }
        return new Vector3(count * _distance + slide, 0, 0);
    }
}
