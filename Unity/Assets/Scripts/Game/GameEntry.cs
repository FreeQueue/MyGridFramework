using System.Linq;
using cfg;
using Framework;
using S100.Modules;
using Procedure;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
	public static BaseModule Base { get; private set; }
	public static EventModule Event{ get; private set; }
	public static EntityModule Entity{ get; private set; }
	public static TimerModule Timer{ get; private set; }
	public static ProcedureModule Procedure{ get; private set; }
	public static ConfigModule Config{ get; private set; }
	public static FsmModule Fsm{ get; private set; }
	public static Tables Tables{ get; private set; }
	public static DefaultControls Controls{ get; private set; }
	private void Awake() {
		Base = FrameworkEntry.RegisterModule<BaseModule>();
		FrameworkEntry.RegisterModule<ObjectPoolModule>();
		FrameworkEntry.RegisterModule<ResourceModule>();
		Config = FrameworkEntry.RegisterModule<ConfigModule>();
		Event = FrameworkEntry.RegisterModule<EventModule>();
		Entity = FrameworkEntry.RegisterModule<EntityModule>();
		Timer = FrameworkEntry.RegisterModule<TimerModule>();
		Procedure = FrameworkEntry.RegisterModule<ProcedureModule>();
		Fsm = FrameworkEntry.RegisterModule<FsmModule>();
		FrameworkEntry.RegisterModule<InputModule>();
		Controls = FrameworkEntry.GetModule<InputModule>().Controls;
		Tables = Config.Tables;
	}

	private void Start() {
		FrameworkEntry.GetModule<ProcedureModule>().StartProcedure<MainMenuProcedure>();
	}
	private void Update() {
		FrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
	}
	private void OnApplicationQuit() {
		FrameworkEntry.Shutdown();
	}

	private void OnDestroy() {
		FrameworkEntry.Shutdown();
	}
}