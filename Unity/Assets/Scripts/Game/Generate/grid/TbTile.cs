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

public sealed partial class TbTile
{
    private readonly Dictionary<grid.TileId, grid.TileData> _dataMap;
    private readonly List<grid.TileData> _dataList;
    
    public TbTile(JSONNode _json)
    {
        _dataMap = new Dictionary<grid.TileId, grid.TileData>();
        _dataList = new List<grid.TileData>();
        
        foreach(JSONNode _row in _json.Children)
        {
            var _v = grid.TileData.DeserializeTileData(_row);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<grid.TileId, grid.TileData> DataMap => _dataMap;
    public List<grid.TileData> DataList => _dataList;

    public grid.TileData GetOrDefault(grid.TileId key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public grid.TileData Get(grid.TileId key) => _dataMap[key];
    public grid.TileData this[grid.TileId key] => _dataMap[key];

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