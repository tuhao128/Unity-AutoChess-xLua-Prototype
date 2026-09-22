using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirZone : GameZone
{
    protected override int MaxCount => 0;

    protected override bool HasMaxCount => false;

    protected override GameZoneType _type => GameZoneType.Air;

    protected override bool keepWorldPos => true;

    protected override void AfterAdd(Card card, Vector3 calculate)
    {
        card.CanPlay = false;
    }

    protected override void AfterInMax(Card card)
    {
        throw new System.NotImplementedException();
    }

    protected override void AfterRemove(Card card)
    {
        card.CanPlay = true;
    }

    protected override Vector3 CalculatePos(Card card)
    {
        return Vector3.zero;
    }
}
