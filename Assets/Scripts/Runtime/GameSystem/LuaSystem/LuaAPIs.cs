using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaAPIs
{
    public static void RegisterChessBy(LuaTable table)
    {
        LuaDataFactory.Instance.RegisterChess(
            table.Get<string>("name"),
            new ChessConfig(AssetBundleLoader.Instance.CollectPack(table.Get<string>("model"), table.Get<string>("material")), table.Get<AskedAction>("skill"), table.Get<AskedAction>("attack"), table.Get<AskedAction>("collect"), table.Get<float>("movingSpeed"), new Vector3Int(table.Get<int>("x"), table.Get<int>("y"), table.Get<int>("z")))
        );
        table.Dispose();
        LuaDataFactory.Instance.CreateChessAt("Unnamed", new Vector3Int(0, 0, 0), GameSceneController.Instance);
    }

    public static void RegisterCardBy(LuaTable table)
    {
        LuaDataFactory.Instance.RegisterCard(
            table.Get<string>("name"),
            new CardConfig()
            {
                OnUse = table.Get<CardEventHandler>("onUse"),
                OnDrop = table.Get<CardEventHandler>("onDrop")
            }
        );
        table.Dispose();
    }

    public static void MoveTo(ActionMessage message, int x, int y, int z)
    {
        message.AskFrom.PutChessOn(new Vector3Int(x, y, z));
    }

    public static void PosTowards(ActionMessage message, int xPos, int zPos, int distance, out int x, out int y, out int z)
    {
        Vector3Int pos = message.AskFrom.Pos;
        pos.x = pos.x + distance * xPos;
        pos.y = pos.y;
        pos.z = pos.z + distance * zPos;
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }
}
