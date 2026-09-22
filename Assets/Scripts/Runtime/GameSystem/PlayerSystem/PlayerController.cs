using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerState
{
    None,
    Thinking,
    PlayingCard,
    Draw,
    MovingChess
}

public class PlayerController : MonoStaticInstanceCreater<PlayerController>
{
    [SerializeField]
    private PhysicsRaycaster _physicsRaycaster;

    [SerializeField]
    private GridForCard _gameZones;

    public GridForCard GameZones
    {
        get => _gameZones;
    }

    [SerializeField]
    private PlayerState _testState;

    private Phase _eventNow;

    private PlayerState _currentState = PlayerState.None;

    public PlayerState CurrentState
    {
        get
        {
            return _currentState;
        }
    }

    [SerializeField]
    private AnimationClip[] clips;

    [SerializeField]
    private Animation aim;

    [SerializeField]
    private bool isLegacy;

    private bool lookingAtCard;

    private bool lookingAtBoard;

    private bool _canPlay;

    public bool CanMoveCard
    {
        get
        {
            bool flag = !lookingAtBoard && !lookingAtCard && _canPlay;
            // if (!flag)
            // {
            //     _physicsRaycaster.eventMask &= ~(1 << LayerMask.NameToLayer("Card"));
            // }
            // else
            // {
            //     _physicsRaycaster.eventMask |= 1 << LayerMask.NameToLayer("Card");
            // }
            return flag;
        }
    }

    private void Start()
    {
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].legacy = isLegacy;
        }
        SwitchState(_testState);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (lookingAtCard)
            {
                aim["CameraBack"].speed = -1;
                aim["CameraBack"].time = aim["CameraBack"].length;
                aim.Play("CameraBack");
                lookingAtCard = false;
            }
            else if (!lookingAtBoard)
            {
                aim["CameraMoving"].speed = 1;
                aim.Play("CameraMoving");
                lookingAtBoard = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (lookingAtBoard)
            {
                aim["CameraMoving"].speed = -1;
                aim["CameraMoving"].time = aim["CameraMoving"].length;
                aim.Play("CameraMoving");
                lookingAtBoard = false;
            }
            else if (!lookingAtCard)
            {
                aim["CameraBack"].speed = 1;
                aim.Play("CameraBack");
                lookingAtCard = true;
            }
        }
    }

    private void SwitchState(PlayerState state)
    {
        if (_currentState != state)
        {
            this._currentState = state;
            switch (CurrentState)
            {
                case PlayerState.Thinking:
                    _canPlay = false;
                    SwitchLayer(LayerMask.GetMask("Card", "Chess"));
                    break;
                case PlayerState.Draw:
                    _canPlay = false;
                    SwitchLayer(LayerMask.GetMask("Deck"));
                    break;
                case PlayerState.PlayingCard:
                    _canPlay = true;
                    SwitchLayer(LayerMask.GetMask("Card"));
                    break;
                default:
                    _canPlay = false;
                    SwitchLayer(LayerMask.GetMask("Card", "Chess"));
                    break;
            }
        }
    }

    private void SwitchLayer(LayerMask layer)
    {
        _physicsRaycaster.eventMask = layer;
    }

    public void CollectDataForEvent(IPhasePackage package)
    {
        if (this._eventNow != null)
        {
            this._eventNow.CollectFromSelf(package);
            PhaseManager.Instance.FinishedCollect(_eventNow);
        }
    }

    public void EnterPhase(Phase phase)
    {
        _eventNow = phase;
        SwitchState(phase.SwitchedState);
    }

    public void ExitPhase()
    {
        _eventNow = null;
        SwitchState(PlayerState.None);
    }
}
