using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using XLua.CSObjectWrap;

public class Chess : MonoBehaviour
{
    private IGameScene _gridOn;
    private Vector3Int _pos;

    private float _movingSpeed;

    public float movingSpeed
    {
        get
        {
            return _movingSpeed;
        }
        set
        {
            _movingSpeed = value;
        }
    }

    public void PutChessOn(Vector3Int pos)
    {
        PutChessOn(_gridOn, pos);
    }

    /// <summary>
    /// 把棋子放在一个可能是新棋盘的地方
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="pos"></param>
    public bool PutChessOn(IGameScene grid, Vector3Int pos)
    {
        if (grid.TryReadBlock(pos.x, pos.z, out GridBlock block) && block.Chess == null)
        {
            if (_gridOn != null && _gridOn == grid)
            {
                grid.ReadBlock(pos.x, pos.z).Chess = null;
            }
            _gridOn = grid;
            _pos = pos;
            grid.ReadBlock(pos.x, pos.z).Chess = this;
            _cathedTransform.DOMove(_gridOn.Grid.CellToWorld(_pos), _movingSpeed);
            return true;
        }
        return false;
    }

    private AskedAction _moving;

    public AskedAction Moving
    {
        get
        {
            return _moving;
        }
        set
        {
            _moving = value;
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

    private AskedAction _collectedAction;

    public AskedAction CollectedAction
    {
        get
        {
            return _collectedAction;
        }
        set
        {
            _collectedAction = value;
        }
    }

    private Dictionary<string, Chess> _aims = new Dictionary<string, Chess>();

    private int _speed;

    public int Speed
    {
        get
        {
            return _speed;
        }
    }

    public void InitActions(AskedAction _attackAction, AskedAction _skilledAction, AskedAction _collectedAction, Vector3Int firstPos)
    {
        this._attackAction = _attackAction;
        this._skilledAction = _skilledAction;
        this._collectedAction = _collectedAction;
        this._moving = Move;
        this._pos = firstPos;
    }

    private bool Move(ActionMessage message)
    {
        PutChessOn(GameSceneController.Instance, message.Vector3IntNum);
        return false;
    }

    public Vector3Int Pos
    {
        get
        {
            return GetPos(_gridOn);
        }
    }

    public Vector3Int GetPos(IGameScene gameScene)
    {
        return gameScene.Grid.WorldToCell(this.transform.position);
    }

    public void AddAction(AskedAction action, Vector3Int pos)
    {
        GameSceneController.Instance.AddAction(new SceneAsk(action, GameSceneController.Instance.CreateMessage(this, pos)));
    }

    // void Update()
    // {
    //     if (_isMoving)
    //     {
    //         if ((_cathedTransform.position - _gridOn.CellToWorld(_pos)).magnitude <= 0.01)
    //         {
    //             _isMoving = false;
    //         }
    //         else
    //         {
    //             _cathedTransform.position += (_gridOn.CellToWorld(_pos) - _cathedTransform.position) * Time.deltaTime * 3;
    //         }
    //     }
    // }

    private Transform _cathedTransform;
    void Awake()
    {
        _cathedTransform = transform;
    }

    public void CollectAction(IGameScene scene)
    {
        scene.AddAction(new SceneAsk(CollectedAction, scene.CreateMessage(this, _pos)));
    }
}
