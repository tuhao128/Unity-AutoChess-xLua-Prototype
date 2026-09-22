LuaAPIs = CS.LuaAPIs

--所有的生成物体的lua脚本，必须要返回一个数据表，不然生成不出来
chessRegisters = {
    "SoliderCharactor"
}

cardRegisters = {
    "FireballCard"
}

metaGenerateTable = {
    __index = {
        name = "Unnamed", -- 名称
        HP = 10, -- 最大生命值
        range = 1, -- 攻击距离

        material = "Piece Black", -- 材质
        model = "Pawn", -- 直接打包模型，决定是哪个模型

        skill = function (msg)
        end,
        attack = function (msg)
        end,
    }
}

function LoadData(luaName)
    if type(luaName) == "string" then
        local datas = require(luaName)
        LuaAPIs.RegisterChessBy(datas)
    end
end

for v, name in ipairs(chessRegisters) do
    LoadData(name)
end

for v, name in ipairs(cardRegisters) do
    LoadData(name)
end