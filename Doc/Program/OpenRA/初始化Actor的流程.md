### 初始化Actor的流程
1. 使用MiniYaml（如map，包含初始化信息）初始化
    1. 创建ActorReference，包含一个类型字典，存储了各类ActorInit信息，某些初始化信息可以针对某个实例名的Trait，某些Trait是单例（在一个Actor中只有一份，使用ISingleInstanceTrait标识），可直接获取，实际是一个有类型和ID两个键的表。ActorInit包含的是运行时信息，可被修改。
    2. LoadInit，Yaml中键值模式为：
        ```Yaml
        XX@InstanceName：
            Key1:Value1
            Key2:Value2
        ```
        根据XX反序列化XXInit对象，InstanceName可选
    3. 每个Trait都持有有各自的TraitInfo引用，对于一种类型的Actor配置，每个TraitInfo加载后全局只存在一份，不论有多少个Actor的实例对象。但对每个Actor实例，Trait都是各自持有，包含运行时信息，如Transform。

2. 从lua脚本上下文中创建ActorGlobal
   可在lua通过字典实现复杂的初始化信息
   其余流程相同

### 总结
创建Actor需要的数据分为配置信息和运行时信息。
从数据文件到实例
