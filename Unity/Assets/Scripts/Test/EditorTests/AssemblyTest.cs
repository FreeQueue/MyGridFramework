using Framework;
using Framework.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Test.EditorTests
{
	public class AssemblyTest
	{
		[Test]
		public void EditorAssemblyNameLog() {
			Debug.Log(nameof(EditorAssemblyNameLog));
			Util.Assembly.AllAssemblies.ForEach(Debug.Log);
		}
	}

}