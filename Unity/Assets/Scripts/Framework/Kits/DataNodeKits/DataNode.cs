using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.DataNodeKits
{
	public sealed class DataNode : IReference
	{
		private static readonly DataNode[] _emptyDataNodeArray = { };
		private static readonly string[] _pathSplitSeparator = { ".", "/", "\\" };
		private List<DataNode> _children;

		private Var _data;
		public string Name { get; private set; }
		public string FullName => Parent == null ? Name : $"{Parent.FullName}{_pathSplitSeparator[0]}{Name}";
		public DataNode Parent { get; private set; }
		public int ChildCount => _children?.Count ?? 0;

		public void Clear() {
			if (_data != null) {
				ReferencePool.Release(_data);
				_data = null;
			}
			if (_children == null) return;
			foreach (DataNode child in _children) {
				ReferencePool.Release(child);
			}
			_children.Clear();
		}
		private static bool IsValidName(string name) {
			if (string.IsNullOrEmpty(name)) return false;
			return _pathSplitSeparator.All(separator => !name.Contains(separator));
		}
		public static DataNode Create(string name, DataNode parent) {
			if (!IsValidName(name)) {
				throw new ArgumentException("Name of data node is invalid.");
			}
			DataNode node = ReferencePool.Get<DataNode>();
			node.Name = name;
			node.Parent = parent;
			return node;
		}

		public bool TryGetData<T>(out T data) {
			return _data.TryGetValue(out data);
		}

		public void SetData<T>(T data) {
			if (_data != null) {
				if (_data.TrySetValue(data)) return;
				ReferencePool.Release(_data);
			}
			_data = new Var<T>(data);
		}

		public bool HasChild(int index) {
			return index >= 0 && index < ChildCount;
		}

		public bool HasChild(string name) {
			if (!IsValidName(name)) throw new InvalidDataException("Name is invalid.");
			return _children != null && _children.Any(child => child.Name == name);
		}

		public DataNode GetChild(int index) {
			return index >= 0 && index < ChildCount ? _children[index] : null;
		}

		public DataNode GetChild(string name) {
			if (!IsValidName(name)) throw new InvalidDataException("Name is invalid.");
			return _children?.FirstOrDefault(child => child.Name == name);
		}

		public DataNode GetOrAddChild(string name) {
			DataNode node = GetChild(name);
			if (node != null) {
				return node;
			}
			node = Create(name, this);
			_children ??= new List<DataNode>();
			_children.Add(node);
			return node;
		}

		public DataNode[] GetAllChild() {
			return _children == null ? _emptyDataNodeArray : _children.ToArray();
		}

		public void GetAllChild(List<DataNode> results) {
			results.Clear();
			if (_children == null) return;
			results.AddRange(_children);
		}

		public void RemoveChild(int index) {
			DataNode node = GetChild(index);
			if (node == null) return;
			_children.Remove(node);
			ReferencePool.Release(node);
		}

		public void RemoveChild(string name) {
			DataNode node = GetChild(name);
			if (node == null) return;
			_children.Remove(node);
			ReferencePool.Release(node);
		}

		public override string ToString() {
			return $"{FullName}: {ToDataString()}";
		}

		public string ToDataString() {
			return _data == null ? "<Null>" : $"[{_data.Type.Name}] {_data}";
		}
	}
}