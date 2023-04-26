using Framework.Kits.MiniYamlKits;
using Framework.Kits.YamlSerializeKits;

namespace OpenRA
{
	/*
	编写 ActorInits 时需要注意的事项： 
	- ActorReference 和 ActorGlobal 可以动态创建对象，而无需调用构造函数。
		对象将被直接分配，然后调用最匹配的 Initialize()方法来设置有效状态。
	- ActorReference 将始终尝试调用 Initialize(MiniYaml)。
		ActorGlobal 将使用它首先找到的参数类型与给定的 LuaValue 匹配的参数类型。
		大多数 ActorInits 都希望继承隐藏低级管道的 ValueActorInit 或 CompositeActorInit。
	- 引用Actor的初始化应<T>使用ActorInitActorReference，它允许在map.yaml中按名称引用Actor
	- 只应在执行组件上定义单个实例的初始化应实现 ISingleInstanceInit 以允许直接查询和运行时强制实施。
	- 不是 ISingleInstanceInit 的 Init 应公开接受 TraitInfo 以允许 按特征定位。
	*/
	public abstract class ActorInit
	{
		[FieldLoader.Ignore] public readonly string InstanceName;
		protected ActorInit(string instanceName) {
			InstanceName = instanceName;
		}
		protected ActorInit() {
		}
		public abstract MiniYaml Save();
	}
}