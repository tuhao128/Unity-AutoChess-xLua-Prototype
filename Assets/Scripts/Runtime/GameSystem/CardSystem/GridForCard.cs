using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class GameZone : MonoBehaviour
{
    /// <summary>
    /// 会被用于填入SetParent里的第二个参数
    /// </summary>
    protected abstract bool keepWorldPos { get; }

    protected abstract int MaxCount { get; }

    protected abstract bool HasMaxCount { get; }

    protected List<Card> _cardIn = new List<Card>();

    public IReadOnlyList<Card> Cards
    {
        get
        {
            return _cardIn.AsReadOnly();
        }
    }

    public int ReadIndex(Card card)
    {
        return _cardIn.IndexOf(card);
    }

    protected abstract GameZoneType _type { get; }

    /// <summary>
    /// 添加卡牌到指定类型的区域
    /// </summary>
    /// <param name="card"></param>
    /// <param name="owner"></param>
    /// <returns>如果判断成功添加就返回true</returns>
    public bool AddCard(Card card, PlayerController owner)
    {
        if (HasMaxCount)
        {
            if (_cardIn.Count < MaxCount)
            {
                _cardIn.Add(card);
                card.GameZoneType = _type;
                card.Owner = owner;
                card.transform.SetParent(this.transform, keepWorldPos);
                AfterAdd(card, CalculatePos(card));
                return true;
            }
            else
            {
                AfterInMax(card);
                return false;
            }
        }
        else
        {
            _cardIn.Add(card);
            card.GameZoneType = _type;
            card.Owner = owner;
            card.transform.SetParent(this.transform, keepWorldPos);
            AfterAdd(card, CalculatePos(card));
            return true;
        }
    }

    /// <summary>
    /// 在发现卡牌数量超过最大值时调用它
    /// </summary>
    /// <param name="card"></param>
    protected abstract void AfterInMax(Card card);

    public void Remove(Card card)
    {
        if (_cardIn.Remove(card))
        {
            card.GameZoneType = GameZoneType.None;
            card.Owner = null;
            AfterRemove(card);
        }
    }

    /// <summary>
    /// AfterAdd传参，计算一个位置
    /// </summary>
    /// <returns>直接作为世界坐标系坐标点使用</returns>
    protected abstract Vector3 CalculatePos(Card card);

    /// <summary>
    /// 在添加了卡牌之后执行
    /// </summary>
    /// <param name="card"></param>
    protected abstract void AfterAdd(Card card, Vector3 calculate);

    protected abstract void AfterRemove(Card card);
}

public enum GameZoneType
{
    None,
    Hand,
    Deck,
    Discard,
    Air
}
/// <summary>
/// 划分出手牌区，抽牌区和弃牌区
/// </summary>
public class GridForCard : MonoBehaviour
{
    [SerializeField]
    private GameObject _testCardPrefab;

    [SerializeField]
    private bool _isDebug;

    [Header("Hand Area")]
    [SerializeField]
    private float _minPoint;

    [SerializeField]
    private float _maxPoint;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private int maxCount;

    [SerializeField]
    private HandZone _handZone;

    [Header("Deck Zone")]
    [SerializeField]
    private Deck _deck;

    [SerializeField]
    private Card[] _cards;

    [Header("Discard Pile")]
    [SerializeField]
    private GameZone _pile;
    private List<Card> _discardPile;

    [Header("Air Zone")]
    [SerializeField]
    private AirZone _airZone;

    private void Awake() 
    {
        _handZone.Init(_minPoint, _maxPoint, _distance, maxCount, true);
        //抽卡区的初始化
        _deck.InitCards(_cards);
    }

    public GameZone ReadZone(GameZoneType type)
    {
        GameZone zone = null;
        switch (type)
        {
            case GameZoneType.Hand:
                zone = _handZone;
                break;
            case GameZoneType.Air:
                zone = _airZone;
                break;
            case GameZoneType.Deck:
                zone = _deck;
                break;
            default:
                break;
        }
        return zone;
    }

    public bool AddCard(Card card, GameZoneType type, PlayerController owner)
    {
        if (ReadZone(type) != null)
        {
            return ReadZone(type).AddCard(card, owner);
        }
        return false;
    }

    public bool RemoveCardTo(Card card, GameZoneType type)
    {
        if (card.GameZoneType != type)
        {
            switch (type)
            {
                case GameZoneType.Hand:
                    card.GameZone.Remove(card);
                    _handZone.AddCard(card, PlayerController.Instance);
                    break;
                case GameZoneType.Air:
                    card.GameZone.Remove(card);
                    _airZone.AddCard(card, PlayerController.Instance);
                    break;
                default:
                    card.GameZone.Remove(card);
                    break;
            }
            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        // if (_isDebug)
        // {
        //     GameObject card = GameObject.Instantiate(_testCardPrefab);
        //     _isDebug = AddCard(card.GetComponent<Card>(), GameZoneType.Hand, PlayerController.Instance);
        // }
        if (_isDebug)
        {
            DrawCard(GameZoneType.Hand, PlayerController.Instance);
            _isDebug = false;
        }
    }

    public void DrawCard(GameZoneType zoneType, PlayerController owner)
    {
        _deck.DrawCardTo(ReadZone(zoneType), owner);
    }
}
