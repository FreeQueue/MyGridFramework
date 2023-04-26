1. SerializeAttribute
   1. GetLoader 根据Loader获取字段的加载函数
   2. 
2. FieldLoader
   1. BuildTypeLoadInfo
      建立类型字段加载信息
   2. Load
        
   3. GetValue
      判断字段类型，用表中解析方法加载字段值。分泛型和非泛型
      泛型类型支持
      1. HashSet
      2. List
      3. BitSet
      4. Dictionary
      5. Nullable。
        非泛型会检测是否是数组并加载
        最后检测是否能转化为string

