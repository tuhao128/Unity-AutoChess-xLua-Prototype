using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//每个阶段的行为是调整玩家状态，然后控制玩家的输入对象
//在玩家使用一个专门的Collectapi之后，接下去执行具体内容，然后调整到下一阶段，同时控制玩家状态为下一阶段的状态
//每个阶段的具体内容就是，传递自身引用给玩家，修改玩家状态，然后执行具体内容，最后把接收到的数据给父节点
//阶段的存储方式为树状结构，但是执行方式为栈
//通过栈回溯节点，高效遍历

//阶段系统的核心思想：发出指令，而指令是全局的，没有特定阶段执行特定指令这种说法
public class PhaseManager : StaticInstanceCreater<PhaseManager>
{
    private int _time = 1;

    private bool _collectingData;

    private Phase _enterNode;

    private IEnumerator<bool> _enumerator;

    private Stack<Phase> _phaseEvents = new Stack<Phase>();

    public Stack<Phase> phases => _phaseEvents;

    IEnumerable<bool> ActAfterCollect(Phase phaseEvent)
    {
        if (_phaseEvents.TryPeek(out Phase result))
        {
            if (phaseEvent == result)
            {
                _phaseEvents.Pop();
                result.Finish(_time);
                bool @continue = true;
                while (@continue)
                {
                    @continue = result.Act();
                    yield return @continue;
                }
                if (_phaseEvents.TryPeek(out Phase father))
                {
                    father.CollectFromSon(result);
                }
                result.CollectedPkg.Clear();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Current Phase isnt same as phase Entered.");
#endif
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Please Push At Least One Phase.");
#endif
        }
    }

    public void FinishedCollect(Phase phaseEvent)
    {
        if (phaseEvent == null)
        {
#if UNITY_EDITOR
            Debug.Log("Please Collect A Phase.");
#endif
            return;
        }
        _enumerator = ActAfterCollect(phaseEvent).GetEnumerator();
        PlayerController.Instance.ExitPhase();
        _collectingData = false;
    }

    //每次FixedUpdate进行遍历
    //如果发现迭代器，也就是阶段结算还在进行，那就进行阶段结算
    //如果发现结算完成，就带着玩家进入下一个阶段
    public void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in _phaseEvents)
            {
                Debug.Log(item.Name);
            }
        }
        if (!_collectingData)
        {
            if (_enterNode != null)
            {
                if (_enumerator == null)
                {
                    PushPhases(_enterNode);
                    if (_phaseEvents.TryPeek(out Phase result))
                    {
                        CollectingData(result, false);
                    }
                }
                if (_enumerator != null && !_enumerator.MoveNext())
                {
                    if (_phaseEvents.TryPeek(out Phase result))
                    {
                        foreach (var phaseEvent in result.Sons)
                        {
                            if (!phaseEvent.Finished(_time))
                            {
                                CollectingData(phaseEvent, true);
                                return;
                            }
                        }
                        CollectingData(result, false);
                    }
                    else
                    {
                        _time += 1;
                        // Debug.LogWarning("New Phase");
                        PushPhases(_enterNode);
                        // GameManager.Instance.Log();
                        if (_phaseEvents.TryPeek(out Phase result1))
                        {
                            CollectingData(result1, false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 执行一个阶段，如果为true就先把它的子阶段入栈再执行，确保执行过程是从最底部开始
    /// </summary>
    /// <param name="phase"></param>
    /// <param name="shouldPush"></param>
    private void CollectingData(Phase phase, bool shouldPush)
    {
        Phase phase1 = phase;
        if (shouldPush)
        {
            PushPhases(phase);
            if (_phaseEvents.TryPeek(out Phase result))
            {
                phase1 = result;
            }
        }
        _collectingData = true;
        if (!phase1.NeedData)
        {
            FinishedCollect(phase1);
        }
        else
        {
            PlayerController.Instance.EnterPhase(phase1);
        }
    }

    public void InitEnterPhase(Phase phase)
    {
        _enterNode = phase;
    }

    /// <summary>
    /// 建议在Act里面调用，因为Act之前能确保Pop出了一个阶段，否则可能会有几个阶段被吞
    /// </summary>
    /// <param name="phase"></param>
    public void PushPhases(Phase phase)
    {
        _phaseEvents.Push(phase);
        foreach (Phase child_phase in phase.Sons)
        {
            if (!IsFinished(child_phase))
            {
                PushPhases(child_phase);
                return;
            }
        }
    }

    private bool IsFinished(Phase phase)
    {
        return phase.Finished(_time);
    }
}
