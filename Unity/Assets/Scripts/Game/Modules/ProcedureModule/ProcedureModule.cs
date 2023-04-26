using System;
using Framework;
using Framework.Kits.ProcedureKits;
using Helper = S100.Modules.ProcedureModuleHelper;

namespace S100.Modules
{
	public class ProcedureModule : ProcedureManager, IModule
	{
		public void Init()
		{
			ProcedureBase[] procedures = new ProcedureBase[ProcedureModuleHelper.Instance.Procedures.Count];
			for (int i = 0; i < procedures.Length; i++) {
				Type procedureType
					= Util.Implement<ProcedureBase>.GetByName(ProcedureModuleHelper.Instance.Procedures[i]);
				if (procedureType == null) {
					throw new FormatException(
						$"Can not find procedure type '{ProcedureModuleHelper.Instance.Procedures[i]}'.");
				}
				procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
			}
			Initialize(GameEntry.Fsm, procedures);
			StartProcedure(
				Util.Implement<ProcedureBase>.GetByName(ProcedureModuleHelper.Instance.StartProcedure));
		}
		void IModule.Shutdown()
		{
			Clear();
		}
	}
}