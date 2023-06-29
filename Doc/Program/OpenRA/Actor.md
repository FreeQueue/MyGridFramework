## Actor

1. Actor 对象
   World
   create
   
2. ActorReference
   负责从数据文件建立Actor初始化信息（懒加载），不会实际创建实例，但可以根据这些信息创建实例，或者实例的预览。
3. ActorInfo 
   对象配置信息
   TypeDictionary 将Traits计入类型字典，可通过任意接口类型获得对应Trait
   根据IRequire和INotBefore信息对Trait的初始化进行排序。
4. ActorInit
   动态的运行时信息的初始化数据
5. ActorInitializer
   传递给TraitInfo，包含Actor，专用于获取Actor的各种ActorInit（也是一类），用于初始化
6. ActorInfoDictionary
    普通泛型字典包壳，键为字符串，值为ActorInfo，可以以特定的枚举值为键，单纯提供方便。