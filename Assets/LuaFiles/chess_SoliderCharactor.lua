local table = {
    name = "Unnamed", -- 名称
    movingSpeed = 2, -- 移动速度

    material = "Piece Black", -- 材质
    model = "Pawn", -- 直接打包模型，决定是哪个模型

    x = 0,
    y = 0,
    z = 0,

    skill = function (msg)
        print("skill attack!")
        return false
    end,
    attack = function (msg)
        print("attack!")
        return false
    end,
    collect = function (msg)
        DataGenerator.LuaAPIs.MoveTo(msg, LuaAPIs.PosTowards(msg, 0, 1, 1))
        return false
    end,

}

return table