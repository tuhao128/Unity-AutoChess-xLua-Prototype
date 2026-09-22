using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[CSharpCallLua]
public delegate bool AskedAction(ActionMessage askMessage);

/// <summary>
/// 代表一个游戏场景，自带执行器并且存储许多信息
/// </summary>
public interface IGameScene
{
    /// <summary>
    /// 代表了游戏场景的网格信息
    /// </summary>
    Grid Grid { get; }

    /// <summary>
    /// 获取ij位置对应的那个棋盘格子信息
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    GridBlock ReadBlock(int i, int j);

    bool TryReadBlock(int i, int j, out GridBlock block);

    /// <summary>
    /// 把一个棋子放在棋盘的指定位置
    /// </summary>
    /// <param name="chess"></param>
    /// <param name="vector3Int"></param>
    void PutChessOn(Chess chess, Vector3Int vector3Int);

    /// <summary>
    /// 代表棋盘运行一次，会自动初始化和判定是否中途继续，返回true代表可以继续执行
    /// </summary>
    bool Act();

    /// <summary>
    /// 添加棋盘指令
    /// </summary>
    /// <param name="ask"></param>
    void AddAction(SceneAsk ask);

    /// <summary>
    /// 棋盘必须提供创建行动信息的api，负责管理棋盘格子的信息回调
    /// </summary>
    /// <param name="chess"></param>
    /// <param name="vector3Int"></param>
    /// <returns></returns>
    ActionMessage CreateMessage(Chess chess, Vector3Int vector3Int);

    /// <summary>
    /// 代表当前棋盘是否在执行操作
    /// </summary>
    bool ShouldAct { get; }

    /// <summary>
    /// 代表了当前棋盘指令的最大速度
    /// </summary>
    int MaxSpeed { get; }
}

/// <summary>
/// 作为参数存储传给执行函数
/// </summary>
public struct ActionMessage
{
    private Chess askFrom;

    public Chess AskFrom => askFrom;

    public int Speed
    {
        get
        {
            return askFrom.Speed;
        }
    }

    private Vector3Int _vector3IntNum;

    public Vector3Int Vector3IntNum
    {
        get
        {
            return _vector3IntNum;
        }
    }

    private IGameScene _scene;

    public IGameScene GameScene
    {
        get
        {
            return _scene;
        }
        set
        {
            _scene = value;
        }
    }

    public ActionMessage(Chess chess, IGameScene gameScene, Vector3Int vector3Int)
    {
        this.askFrom = chess;
        this._vector3IntNum = vector3Int;
        this._scene = gameScene;
    }
}

public struct SceneAsk
{
    private ActionMessage askMessage;

    public IGameScene MsgGameScene
    {
        get
        {
            return askMessage.GameScene;
        }
        set
        {
            askMessage.GameScene = value;
        }
    }

    private AskedAction askAction;

    public SceneAsk(AskedAction askAction, ActionMessage askMessage)
    {
        this.askAction = askAction;
        this.askMessage = askMessage;
    }

    public static SceneAsk CreateAsk(IGameScene gameScene, AskedAction askAction, Chess chess, Vector3Int vector3Int)
    {
        return new SceneAsk(askAction, gameScene.CreateMessage(chess, vector3Int));
    }

    public bool Act()
    {
        return askAction.Invoke(askMessage);
    }

    public int Speed
    {
        get
        {
            return askMessage.Speed;
        }
    }
}

public class GridBlock
{
    private Chess _chessOn;

    public Chess Chess
    {
        get
        {
            return _chessOn;
        }
        set
        {
            _chessOn = value;
        }
    }

    public void CollectAction(IGameScene scene)
    {
        if (_chessOn != null)
        {
            _chessOn.CollectAction(scene);
        }
    }
}

public class GameSceneController : MonoStaticInstanceCreater<GameSceneController>, IGameScene
{
    [SerializeField]
    private Grid _grid;

    [SerializeField]
    private int _width;

    [SerializeField]
    private int _height;

    private GridBlock[,] _blocks;

    public GridBlock ReadBlock(int i, int j)
    {
        return _blocks[i, j];
    }

    public Grid Grid
    {
        get
        {
            return _grid;
        }
    }

    public bool ShouldAct { get => _act; }

    public int MaxSpeed => maxSpeed;

    private bool _act;

    private bool _finished = true;

    private IEnumerator enumerator;

    private int maxSpeed;

    private Dictionary<int, Stack<SceneAsk>> _speed_Actions = new Dictionary<int, Stack<SceneAsk>>();

    protected override void Awake()
    {
        base.Awake();
        _blocks = new GridBlock[_width, _height];
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                _blocks[i, j] = new GridBlock();
            }
        }
    }

    public ActionMessage CreateMessage(Chess chess, Vector3Int vector3Int)
    {
        return new ActionMessage(chess, this, vector3Int);
    }

    public void AddAction(SceneAsk ask)
    {
        int speed = ask.Speed;
        if (maxSpeed < speed)
        {
            maxSpeed = speed;
        }
        if (!_speed_Actions.ContainsKey(speed))
        {
            _speed_Actions.Add(speed, new Stack<SceneAsk>());
        }
        _speed_Actions[speed].Push(ask);
    }

    IEnumerable ActActions()
    {
        _finished = false;
        for (int i = 0; i <= maxSpeed; i++)
        {
            if (_speed_Actions.TryGetValue(i, out Stack<SceneAsk> stack))
            {
                while (stack.TryPop(out SceneAsk result))
                {
                    bool @continue = true;
                    while (@continue)
                    {
                        @continue = result.Act();
                        yield return @continue;
                    }
                }
            }
        }
        _finished = true;
        _act = false;
    }

    /// <summary>
    /// 执行下一项棋盘操作，如果发现还有棋盘指令就返回true
    /// </summary>
    /// <returns></returns>
    private bool MoveNext()
    {
        if (_act)
        {
            return enumerator.MoveNext();
        }
        return false;
    }

    private void ActPhase()
    {
        _act = true;
        if (_finished)
        {
            enumerator = ActActions().GetEnumerator();
        }
    }

    public bool Act()
    {
        ActPhase();
        return MoveNext();
    }

    public void Pose()
    {
        _act = false;
    }

    public void PutChessOn(Chess chess, Vector3Int vector3Int)
    {
        chess.PutChessOn(this, vector3Int);
    }

    public void CollectDataForPhase()
    {
        maxSpeed = 0;
        _speed_Actions.Clear();
        foreach (var block in _blocks)
        {
            block.CollectAction(this);
        }
    }

    public bool TryReadBlock(int i, int j, out GridBlock block)
    {
        if (i >= _width || j >= _height)
        {
            block = null;
            return false;
        }
        else
        {
            block = ReadBlock(i, j);
            return true;
        }
    }
}
