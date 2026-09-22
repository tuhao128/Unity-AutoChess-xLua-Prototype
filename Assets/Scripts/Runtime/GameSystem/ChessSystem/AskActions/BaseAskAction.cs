using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAskAction
{
    public abstract bool CanAct(ActionMessage message, Grid grid);

    public abstract void Act(ActionMessage message, Grid grid);
}
