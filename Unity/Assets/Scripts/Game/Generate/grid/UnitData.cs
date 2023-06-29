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



namespace cfg.grid
{ 

public sealed partial class UnitData :  Bright.Config.BeanBase 
{
    public UnitData(JSONNode _json) 
    {
        { if(!_json["id"].IsNumber) { throw new SerializationException(); }  Id = (grid.UnitId)_json["id"].AsInt; }
        { if(!_json["is_collider"].IsBoolean) { throw new SerializationException(); }  IsCollider = _json["is_collider"]; }
        { if(!_json["behaviour"].IsString) { throw new SerializationException(); }  Behaviour = _json["behaviour"]; }
        PostInit();
    }

    public UnitData(grid.UnitId id, bool is_collider, string behaviour ) 
    {
        this.Id = id;
        this.IsCollider = is_collider;
        this.Behaviour = behaviour;
        PostInit();
    }

    public static UnitData DeserializeUnitData(JSONNode _json)
    {
        return new grid.UnitData(_json);
    }

    public grid.UnitId Id { get; private set; }
    /// <summary>
    /// 是否可碰撞
    /// </summary>
    public bool IsCollider { get; private set; }
    /// <summary>
    /// 行为
    /// </summary>
    public string Behaviour { get; private set; }

    public const int __ID__ = 2029909270;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "IsCollider:" + IsCollider + ","
        + "Behaviour:" + Behaviour + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}
}