using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 如果后续访问对象，可以做一个对象池，然后把这个结构体里的信息存进去
/// 之后从对象池获取Phase，就不仅仅是获取一份它的存储，还会获取它的信息
/// </summary>
public struct PhaseMessage
{
    private Dictionary<string, IPhasePackage> _collectedData;
    public Dictionary<string, IPhasePackage> CollectedPkg { get => _collectedData; }

    private int finished;
    private int Finished { get => finished; }

    private List<Phase> _phases;
    public List<Phase> Sons { get => _phases; }
}

public interface IPhasePackage
{
    void Act();
}
public class Phase
{
    private bool usingStruct;

    private string _name;
    public string Name
    {
        get => _name;
    }
    private bool _needData;
    public bool NeedData { get => _needData; }

    private Func<Phase, PlayerState> _switchFunc;

    private Func<Phase, bool> _actFunc;
    private Dictionary<string, IPhasePackage> _collectedData = new Dictionary<string, IPhasePackage>();
    //这个参数，会保存，是我不想看见的，或许可以修改一下
    //用位运算，每次判断是否已经完成，就看位计算结果是不是正确，每次重复一次就把位移动一次
    private int finished = 0;
    //public bool Finished { get => finished; set => finished = value; }

    public bool Finished(int num)
    {
        bool flag = finished == num;
        return flag;
    }

    public void Finish(int num)
    {
        finished = num;
    }

    public PlayerState SwitchedState { get => SwitchState(); }
    public Dictionary<string, IPhasePackage> CollectedPkg { get => _collectedData; }

    private List<Phase> _phases = new List<Phase>();
    public List<Phase> Sons { get => _phases; }

    public bool Act()
    {
        return _actFunc(this);
    }

    public void CollectFromSon(Phase sonPhase)
    {
        CollectedPkg.AddRange(sonPhase.CollectedPkg);
    }

    public void CollectFromSelf(IPhasePackage package)
    {
        CollectedPkg.TryAdd(Name, package);
    }

    protected PlayerState SwitchState()
    {
        return _switchFunc(this);
    }

    public void AddChlidPhase(Phase phase, int num)
    {
        switch (num)
        {
            case 0:
                Sons.Add(phase);
                break;
            default:
                Sons.Insert(num, phase);
                break;
        }
    }

    public Phase(string name, bool needData, Func<Phase, PlayerState> _switchFunc, Func<Phase, bool> _actFunc, params Phase[] phases)
    {
        this._name = name;
        this._needData = needData;
        this._switchFunc = _switchFunc;
        this._actFunc = _actFunc;
        Sons.AddRange(phases);
    }
}
