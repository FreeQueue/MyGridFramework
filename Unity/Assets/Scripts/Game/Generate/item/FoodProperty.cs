//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;
using SimpleJSON;



namespace cfg.item
{ 

public sealed partial class FoodProperty :  item.Property 
{
    public FoodProperty(JSONNode _json)  : base(_json) 
    {
        { if(!_json["food"].IsNumber) { throw new SerializationException(); }  Food = _json["food"]; }
        { if(!_json["water"].IsNumber) { throw new SerializationException(); }  Water = _json["water"]; }
        { var _j = _json["effect"]; if (_j.Tag != JSONNodeType.None && _j.Tag != JSONNodeType.NullValue) { { if(!_j.IsNumber) { throw new SerializationException(); }  Effect = (item.FoodEvent)_j.AsInt; } } else { Effect = null; } }
        { var _j = _json["rot_turn"]; if (_j.Tag != JSONNodeType.None && _j.Tag != JSONNodeType.NullValue) { { if(!_j.IsNumber) { throw new SerializationException(); }  RotTurn = _j; } } else { RotTurn = null; } }
        PostInit();
    }

    public FoodProperty(int food, int water, item.FoodEvent? effect, int? rot_turn )  : base() 
    {
        this.Food = food;
        this.Water = water;
        this.Effect = effect;
        this.RotTurn = rot_turn;
        PostInit();
    }

    public static FoodProperty DeserializeFoodProperty(JSONNode _json)
    {
        return new item.FoodProperty(_json);
    }

    /// <summary>
    /// 食物
    /// </summary>
    public int Food { get; private set; }
    /// <summary>
    /// 水分
    /// </summary>
    public int Water { get; private set; }
    /// <summary>
    /// 效果
    /// </summary>
    public item.FoodEvent? Effect { get; private set; }
    /// <summary>
    /// 腐败回合
    /// </summary>
    public int? RotTurn { get; private set; }

    public const int __ID__ = 256371566;
    public override int GetTypeId() => __ID__;

    public override void Resolve(Dictionary<string, object> _tables)
    {
        base.Resolve(_tables);
        PostResolve();
    }

    public override void TranslateText(System.Func<string, string, string> translator)
    {
        base.TranslateText(translator);
    }

    public override string ToString()
    {
        return "{ "
        + "Food:" + Food + ","
        + "Water:" + Water + ","
        + "Effect:" + Effect + ","
        + "RotTurn:" + RotTurn + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}
}
