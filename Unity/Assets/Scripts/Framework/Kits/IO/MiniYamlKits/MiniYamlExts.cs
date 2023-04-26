using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Extensions;

namespace Framework.Kits.MiniYamlKits
{
	public static class MiniYamlExts
	{
		public static void WriteToFile(this List<MiniYamlNode> @this, string filename)
		{
			File.WriteAllLines(filename, @this.ToLines().Select(x => x.TrimEnd()).ToArray());
		}

		public static string WriteToString(this List<MiniYamlNode> @this)
		{
			// Remove all trailing newlines and restore the final EOF newline
			return @this.ToLines().JoinWith("\n").TrimEnd('\n') + "\n";
		}
		
		public static List<MiniYamlNode> NodesOrEmpty(this MiniYaml @this, string s) {
			var nd = @this.ToDictionary();
			return nd.ContainsKey(s) ? nd[s].Nodes : new List<MiniYamlNode>();
		}
		
		public static IEnumerable<string> ToLines(this List<MiniYamlNode> @this)
		{
			foreach (MiniYamlNode kv in @this)
				foreach (var line in kv.Value.ToLines(kv.Key, kv.Comment))
					yield return line;
		}
	}
}