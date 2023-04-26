//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Kits.ReferencePoolKits
{
	public static partial class ReferencePool
	{
		public sealed class ReferenceCollection
		{
			private readonly Queue<IReference> _references;

			public Type ReferenceType { get; }

			public int UnusedReferenceCount => _references.Count;

			public int UsingReferenceCount { get; private set; }

			public int AcquireReferenceCount { get; private set; }

			public int ReleaseReferenceCount { get; private set; }

			public int AddReferenceCount { get; private set; }

			public int RemoveReferenceCount { get; private set; }

			public ReferenceCollection(Type referenceType) {
				_references = new Queue<IReference>();
				ReferenceType = referenceType;
				UsingReferenceCount = 0;
				AcquireReferenceCount = 0;
				ReleaseReferenceCount = 0;
				AddReferenceCount = 0;
				RemoveReferenceCount = 0;
			}

			public T Get<T>() where T : class, IReference, new() {
				Debug.Assert(typeof(T) == ReferenceType, "Type is invalid.");
				UsingReferenceCount++;
				AcquireReferenceCount++;
				lock (_references) {
					if (_references.Count > 0) {
						return (T)_references.Dequeue();
					}
				}

				AddReferenceCount++;
				return new T();
			}

			public IReference Get() {
				UsingReferenceCount++;
				AcquireReferenceCount++;
				lock (_references) {
					if (_references.Count > 0) {
						return _references.Dequeue();
					}
				}

				AddReferenceCount++;
				return (IReference)Activator.CreateInstance(ReferenceType);
			}

			public void Release(IReference reference) {
				reference.Clear();
				lock (_references) {
					if (EnableStrictCheck && _references.Contains(reference)) {
						throw new InvalidOperationException("The reference has been released.");
					}

					_references.Enqueue(reference);
				}

				ReleaseReferenceCount++;
				UsingReferenceCount--;
			}

			public void Add<T>(int count) where T : class, IReference, new() {
				Debug.Assert(typeof(T) == ReferenceType, "Type is invalid.");
				lock (_references) {
					AddReferenceCount += count;
					while (count-- > 0) {
						_references.Enqueue(new T());
					}
				}
			}

			public void Add(int count) {
				lock (_references) {
					AddReferenceCount += count;
					while (count-- > 0) {
						_references.Enqueue((IReference)Activator.CreateInstance(ReferenceType));
					}
				}
			}

			public void Remove(int count) {
				lock (_references) {
					if (count > _references.Count) {
						count = _references.Count;
					}

					RemoveReferenceCount += count;
					while (count-- > 0) {
						_references.Dequeue();
					}
				}
			}

			public void RemoveAll() {
				lock (_references) {
					RemoveReferenceCount += _references.Count;
					_references.Clear();
				}
			}
		}
	}
}