using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoStaticInstanceCreater<GameManager>
{
    [SerializeField]
    private bool _debug;

    [SerializeField]
    private bool _usingPhase;

    void Start()
    {
        LuaDataFactory.Instance.GenerateData();
        if (_usingPhase)
        {
            PhaseManager.Instance.InitEnterPhase(
                new Phase(
                    "EnterPhase",
                    false,
                    (self) =>
                    {
                        return PlayerState.None;
                    },
                    (self) =>
                    {
                        Debug.Log("New Trun.");
                        Log();
                        return false;
                    },

                    new Phase(
                        "PreparePhase",
                        false,
                        (self) =>
                        {
                            return PlayerState.None;
                        },
                        (self) =>
                        {
                            Debug.Log("Prepare");
                            Log();
                            return false;
                        },

                        new Phase(
                            "EnterPrepare",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("EnterPrepare");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "InPrepare",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("InPrepare");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "ExitPrepare",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("ExitPrepare");
                                Log();
                                return false;
                            }
                        )
                    ),

                    new Phase(
                        "DrawPhase",
                        false,
                        (self) =>
                        {
                            return PlayerState.None;
                        },
                        (self) =>
                        {
                            Debug.Log("DrawPhase");
                            Log();
                            return false;
                        },

                        new Phase(
                            "EnterDraw",
                            false,
                            (self) =>
                            {
                                return PlayerState.Thinking;
                            },
                            (self) =>
                            {
                                Debug.Log("EnterDraw");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "InDraw",
                            true,
                            (self) =>
                            {
                                Debug.Log("Draw a Card.");
                                return PlayerState.Draw;
                            },
                            (self) =>
                            {
                                if (self.CollectedPkg.TryGetValue("InDraw", out IPhasePackage package))
                                {
                                    package.Act();
                                }
                                Debug.Log("inDraw");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "ExitDraw",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("ExitDraw");
                                Log();
                                return false;
                            }
                        )
                    ),
                    new Phase(
                        "PlayPhase",
                        false,
                        (self) =>
                        {
                            return PlayerState.None;
                        },
                        (self) =>
                        {
                            Debug.Log("PlayPhase");
                            Log();
                            return false;
                        },

                        new Phase(
                            "EnterPlay",
                            false,
                            (self) =>
                            {
                                return PlayerState.Thinking;
                            },
                            (self) =>
                            {
                                Debug.Log("EnterPlay");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "InPlay",
                            true,
                            (self) =>
                            {
                                Debug.Log("Play a Card.");
                                return PlayerState.PlayingCard;
                            },
                            (self) =>
                            {
                                if (self.CollectedPkg.TryGetValue("InPlay", out IPhasePackage package))
                                {
                                    package.Act();
                                }
                                Debug.Log("inPlay");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "ExitPlay",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("ExitPlay");
                                Log();
                                return false;
                            }
                        )
                    ),
                    new Phase(
                        "BattlePhase",
                        false,
                        (self) =>
                        {
                            return PlayerState.None;
                        },
                        (self) =>
                        {
                            Debug.Log("BattlePhase");
                            Log();
                            return false;
                        },

                        new Phase(
                            "EnterBattle",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                GameSceneController.Instance.CollectDataForPhase();
                                Debug.Log("EnterBattle");
                                Log();
                                return false;
                            }
                        ),

                        new Phase(
                            "InBattle",
                            false,
                            (self) =>
                            {
                                Debug.Log("Now Battle.");
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("InBattle");
                                Log();
                                return GameSceneController.Instance.Act();
                            }
                        ),

                        new Phase(
                            "ExitBattle",
                            false,
                            (self) =>
                            {
                                return PlayerState.None;
                            },
                            (self) =>
                            {
                                Debug.Log("ExitBattle");
                                Log();
                                return false;
                            }
                        )
                    )
                )
            );
        }
        // PhaseManager.Instance.InitEnterPhase(
        //     new Phase(
        //     "Enter",
        //     false,
        //     (self) =>
        //     {
        //         return PlayerState.None;
        //     },

        //     (self) =>
        //     {
        //         IPhasePackage package;
        //         if (self.CollectedPkg.TryGetValue("Card", out package))
        //             Debug.Log(self.CollectedPkg["Card"]);
        //         return false;
        //     },
        //     new Phase(
        //         "Card",
        //         true,
        //         (self) =>
        //         {
        //             return PlayerState.None;
        //         },
        //         (self) =>
        //         {
        //             PhaseManager.Instance.PushPhases(new Phase(
        //             "Draw2",
        //             true,
        //             (self) =>
        //             {
        //                 return PlayerState.None;
        //             },
        //             (self) =>
        //             {
        //                 Debug.Log(self.CollectedPkg["Draw2"]);
        //                 return false;
        //             }
        //         ));
        //             Debug.Log(self.CollectedPkg["Card"] + " " + self.CollectedPkg["Draw1"] + " " + self.CollectedPkg["Draw"]);
        //             return false;
        //         },
        //         new Phase(
        //             "Draw",
        //             true,
        //             (self) =>
        //             {
        //                 return PlayerState.None;
        //             },
        //             (self) =>
        //             {
        //                 Debug.Log(self.CollectedPkg["Draw"]);
        //                 return false;
        //             }
        //         ),
        //         new Phase(
        //             "Draw1",
        //             true,
        //             (self) =>
        //             {
        //                 return PlayerState.None;
        //             },
        //             (self) =>
        //             {
        //                 Debug.Log(self.CollectedPkg["Draw1"]);
        //                 return false;
        //             }
        //         )
        //     )
        // ));
    }

    void FixedUpdate()
    {
        PhaseManager.Instance.FixedUpdate();
    }

    public void Log()
    {
        foreach (var item in PhaseManager.Instance.phases)
        {
            Debug.LogWarning(item.Name);
        }
    }

}
