using System.Collections;
using System.Collections.Generic;
using Unity.Android.Types;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

public class CardConfig
{
    private CardEventHandler _onUse;

    public CardEventHandler OnUse
    {
        get => _onUse;
        set => _onUse = value;
    }

    private CardEventHandler _onDrop;

    public CardEventHandler OnDrop
    {
        get => _onDrop;
        set => _onDrop = value;
    }
}

public class ChessConfig
{
    private Vector3Int _firstPos;

    public Vector3Int FirstPos
    {
        get
        {
            return _firstPos;
        }
    }

    private float _movingSpeed;

    public float MovingSpeed
    {
        get
        {
            return _movingSpeed;
        }
    }

    private AskedAction _collectAction;

    public AskedAction Collect
    {
        get
        {
            return _collectAction;
        }
        set
        {
            _collectAction = value;
        }
    }

    private AskedAction _skilledAction;

    public AskedAction Skilled
    {
        get
        {
            return _skilledAction;
        }
        set
        {
            _skilledAction = value;
        }
    }

    private AskedAction _attackAction;

    public AskedAction Attack
    {
        get
        {
            return _attackAction;
        }
        set
        {
            _attackAction = value;
        }
    }

    private ModelPack modelPack;

    public ModelPack ModelPack
    {
        get
        {
            return modelPack;
        }
        set
        {
            modelPack = value;
        }
    }

    public ChessConfig(ModelPack modelPack, AskedAction _skilledAction, AskedAction _attackAction, AskedAction _collectAction, float movingSpeed, Vector3Int firstPos)
    {
        this.modelPack = modelPack;
        this._skilledAction = _skilledAction;
        this._attackAction = _attackAction;
        this._collectAction = _collectAction;
        _firstPos = firstPos;
        _movingSpeed = movingSpeed;
    }
}

public class LuaDataFactory
{
    private static LuaDataFactory _instance;
    public static LuaDataFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LuaDataFactory();
            }
            return _instance;
        }
    }

    private Dictionary<string, ChessConfig> _charaters = new Dictionary<string, ChessConfig>();

    private Dictionary<string, CardConfig> _cards = new Dictionary<string, CardConfig>();

    public void GenerateData()
    {
        LuaEnvManager.Instance.RunFile("DataGenerator");
    }

    public void RegisterCard(string cName, CardConfig config)
    {
        _cards.Add(cName, config);
    }
    
    public Card CreateCard(string name)
    {
        GameObject @object = new GameObject(name);
        if (_cards.TryGetValue(name, out CardConfig config))
        {
            Card card = @object.AddComponent<Card>();
            card.OnUse = config.OnUse;
            card.OnDrop = config.OnDrop;
            return card;
        }
        return null;
    }

    public void RegisterChess(string cName, ChessConfig config)
    {
        _charaters.Add(cName, config);
    }

    public void CreateChessAt(string name, Vector3Int position, IGameScene gameScene)
    {
        if (_charaters.TryGetValue(name, out ChessConfig config))
        {
            GameObject obj = new GameObject(name);

            obj.AddComponent<MeshFilter>().mesh = config.ModelPack.Mesh;
            obj.AddComponent<MeshRenderer>().material = config.ModelPack.Material;
            // obj.AddComponent<CapsuleCollider>();

            // Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
            // rigidbody.freezeRotation = true;
            // rigidbody.drag = 5;

            Chess chess = obj.AddComponent<Chess>();
            chess.movingSpeed = config.MovingSpeed;
            chess.InitActions(config.Attack, config.Skilled, config.Collect, config.FirstPos);
            chess.PutChessOn(GameSceneController.Instance, position);

            //GameSceneController.Instance.ActPhase();

            obj.transform.localScale = new Vector3(10, 10, 10);
        }
    }
}
