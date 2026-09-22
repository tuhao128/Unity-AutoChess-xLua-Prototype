using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public class LuaEnvManager
{
    private static LuaEnv _luaEnv;

    /// <summary>
    /// 会在第一次调用时自己初始化
    /// </summary>
    public static LuaEnv EnvInstance
    {
        get
        {
            if (_luaEnv == null)
            {
                _luaEnv = new LuaEnv();
                _luaEnv.AddLoader(Instance.CustomLoader);
            }
            return _luaEnv;
        }
    }

    private static LuaEnvManager _instance;

    public static LuaEnvManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LuaEnvManager();
            }
            return _instance;
        }
    }

    private byte[] CustomLoader(ref string filename)
    {
        string path = Application.dataPath + "/LuaFiles/" + filename + ".lua";
        return File.ReadAllBytes(path);
    }

    public void RunFile(string filename)
    {
        EnvInstance.DoString($"require '{filename}'");
    }
}
