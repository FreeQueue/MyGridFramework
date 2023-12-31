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



namespace cfg.entity
{ 

public sealed partial class TbEntityGroup
{
    private readonly Dictionary<entity.EntityGroupId, entity.EntityGroupData> _dataMap;
    private readonly List<entity.EntityGroupData> _dataList;
    
    public TbEntityGroup(JSONNode _json)
    {
        _dataMap = new Dictionary<entity.EntityGroupId, entity.EntityGroupData>();
        _dataList = new List<entity.EntityGroupData>();
        
        foreach(JSONNode _row in _json.Children)
        {
            var _v = entity.EntityGroupData.DeserializeEntityGroupData(_row);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<entity.EntityGroupId, entity.EntityGroupData> DataMap => _dataMap;
    public List<entity.EntityGroupData> DataList => _dataList;

    public entity.EntityGroupData GetOrDefault(entity.EntityGroupId key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public entity.EntityGroupData Get(entity.EntityGroupId key) => _dataMap[key];
    public entity.EntityGroupData this[entity.EntityGroupId key] => _dataMap[key];

    public void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var v in _dataList)
        {
            v.Resolve(_tables);
        }
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var v in _dataList)
        {
            v.TranslateText(translator);
        }
    }
    
    
    partial void PostInit();
    partial void PostResolve();
}

}