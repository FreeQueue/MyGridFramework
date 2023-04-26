using System.Collections.Generic;
using Framework;
using Framework.Kits.ProcedureKits;
using Sirenix.OdinInspector;
using UnityEngine;

namespace S100.Modules
{
	public class ProcedureModuleHelper:ModuleHelper<ProcedureModuleHelper>
	{
		[SerializeField]
		[Required("Need StartProcedure",InfoMessageType.Error)]
		[ValueDropdown(nameof(GetProcedureTypeNames))]
		private string startProcedure;
		public string StartProcedure => startProcedure;

		[SerializeField]
		[ValueDropdown(nameof(GetProcedureTypeNames),ExpandAllMenuItems = true,IsUniqueList = true)]
		private List<string> _procedures;
		public IReadOnlyList<string> Procedures => _procedures;
		private IEnumerable<string> GetProcedureTypeNames() {
			return Util.Implement<ProcedureBase>.GetTypeNames();
		}
	}
}