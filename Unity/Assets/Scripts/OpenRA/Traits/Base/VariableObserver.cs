using System.Collections.Generic;
using Framework;

namespace OpenRA.Traits
{
	[RequireExplicitImpl]
	public interface IObservesVariablesInfo : ITraitInfoInterface
	{
	}

	public delegate void VariableObserverNotifier(Actor self, IReadOnlyDictionary<string, int> variables);

	public interface IObservesVariables
	{
		IEnumerable<VariableObserver> GetVariableObservers();
	}

	public struct VariableObserver
	{
		public VariableObserverNotifier Notifier;
		public IEnumerable<string> Variables;
		public VariableObserver(VariableObserverNotifier notifier, IEnumerable<string> variables) {
			Notifier = notifier;
			Variables = variables;
		}
	}
}