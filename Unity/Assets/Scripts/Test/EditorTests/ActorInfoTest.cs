#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using Framework.Kits.MiniYamlKits;
using NUnit.Framework;
using OpenRA;
using OpenRA.Traits;

namespace Test.EditorTests
{
	interface IMock : ITraitInfoInterface { }
	class MockTraitInfo : TraitInfo { public override object Create(ActorInitializer init) { return null; } }
	class MockInheritInfo : MockTraitInfo { }

	class MockAInfo : MockInheritInfo, IMock { }
	class MockBInfo : MockTraitInfo, IRequires<IMock>, IRequires<MockInheritInfo> { }
	class MockCInfo : MockTraitInfo, IRequires<MockBInfo> { }

	class MockDInfo : MockTraitInfo, IRequires<MockEInfo> { }
	class MockEInfo : MockTraitInfo, IRequires<MockFInfo> { }
	class MockFInfo : MockTraitInfo, IRequires<MockDInfo> { }

	class MockGInfo : MockInheritInfo, IMock, INotBefore<MockAInfo> { }
	class MockHInfo : MockTraitInfo, INotBefore<IMock>, INotBefore<MockInheritInfo>, INotBefore<MockBInfo> { }
	class MockIInfo : MockTraitInfo, INotBefore<MockHInfo>, INotBefore<MockCInfo> { }

	class MockJInfo : MockTraitInfo, INotBefore<MockKInfo> { }
	class MockKInfo : MockTraitInfo, INotBefore<MockLInfo> { }
	class MockLInfo : MockTraitInfo, INotBefore<MockJInfo> { }

	[TestFixture]
	public class ActorInfoTest
	{
		[TestCase(TestName = "Trait ordering sorts in dependency order correctly")]
		public void TraitOrderingSortsCorrectly()
		{
			var unorderedTraits = new TraitInfo[] { new MockBInfo(), new MockCInfo(), new MockAInfo(), new MockBInfo() };
			var actorInfo = new ActorInfo("test", unorderedTraits);
			var orderedTraits = actorInfo.TraitsInConstructOrder().ToArray();

			CollectionAssert.AreEquivalent(unorderedTraits, orderedTraits);

			for (var i = 0; i < orderedTraits.Length; i++)
			{
				var traitTypesThatMustOccurBeforeThisTrait =
					ActorInfo.PrerequisitesOf(orderedTraits[i]).Concat(ActorInfo.OptionalPrerequisitesOf(orderedTraits[i]));
				var traitTypesThatOccurAfterThisTrait = orderedTraits.Skip(i + 1).Select(ti => ti.GetType());
				var traitTypesThatShouldOccurEarlier = traitTypesThatOccurAfterThisTrait.Intersect(traitTypesThatMustOccurBeforeThisTrait);
				CollectionAssert.IsEmpty(traitTypesThatShouldOccurEarlier, "Dependency order has not been satisfied.");
			}
		}

		[TestCase(TestName = "Trait ordering sorts in optional dependency order correctly")]
		public void OptionalTraitOrderingSortsCorrectly()
		{
			var unorderedTraits = new TraitInfo[] { new MockHInfo(), new MockIInfo(), new MockGInfo(), new MockHInfo() };
			var actorInfo = new ActorInfo("test", unorderedTraits);
			var orderedTraits = actorInfo.TraitsInConstructOrder().ToArray();

			CollectionAssert.AreEquivalent(unorderedTraits, orderedTraits);

			for (var i = 0; i < orderedTraits.Length; i++)
			{
				var traitTypesThatMustOccurBeforeThisTrait =
					ActorInfo.PrerequisitesOf(orderedTraits[i]).Concat(ActorInfo.OptionalPrerequisitesOf(orderedTraits[i]));
				var traitTypesThatOccurAfterThisTrait = orderedTraits.Skip(i + 1).Select(ti => ti.GetType());
				var traitTypesThatShouldOccurEarlier = traitTypesThatOccurAfterThisTrait.Intersect(traitTypesThatMustOccurBeforeThisTrait);
				CollectionAssert.IsEmpty(traitTypesThatShouldOccurEarlier, "Dependency order has not been satisfied.");
			}
		}

		[TestCase(TestName = "Trait ordering exception reports missing dependencies")]
		public void TraitOrderingReportsMissingDependencies()
		{
			var actorInfo = new ActorInfo("test", new MockBInfo(), new MockCInfo());
			var ex = Assert.Throws<YamlException>(() => actorInfo.TraitsInConstructOrder());

			StringAssert.Contains(nameof(MockBInfo), ex.Message, "Exception message did not report a missing dependency.");
			StringAssert.Contains(nameof(MockCInfo), ex.Message, "Exception message did not report a missing dependency.");
			StringAssert.Contains(nameof(MockInheritInfo), ex.Message, "Exception message did not report a missing dependency (from a base class).");
			StringAssert.Contains(nameof(IMock), ex.Message, "Exception message did not report a missing dependency (from an interface).");
		}

		[TestCase(TestName = "Trait ordering allows optional dependencies to be missing")]
		public void TraitOrderingAllowsMissingOptionalDependencies()
		{
			var unorderedTraits = new TraitInfo[] { new MockHInfo(), new MockIInfo() };
			var actorInfo = new ActorInfo("test", unorderedTraits);
			var orderedTraits = actorInfo.TraitsInConstructOrder().ToArray();

			CollectionAssert.AreEquivalent(unorderedTraits, orderedTraits);
		}

		[TestCase(TestName = "Trait ordering exception reports cyclic dependencies")]
		public void TraitOrderingReportsCyclicDependencies()
		{
			var actorInfo = new ActorInfo("test", new MockDInfo(), new MockEInfo(), new MockFInfo());
			var ex = Assert.Throws<YamlException>(() => actorInfo.TraitsInConstructOrder());

			StringAssert.Contains(nameof(MockDInfo), ex.Message, "Exception message should report all cyclic dependencies.");
			StringAssert.Contains(nameof(MockEInfo), ex.Message, "Exception message should report all cyclic dependencies.");
			StringAssert.Contains(nameof(MockFInfo), ex.Message, "Exception message should report all cyclic dependencies.");
		}

		[TestCase(TestName = "Trait ordering exception reports cyclic optional dependencies")]
		public void TraitOrderingReportsCyclicOptionalDependencies()
		{
			var actorInfo = new ActorInfo("test", new MockJInfo(), new MockKInfo(), new MockLInfo());
			var ex = Assert.Throws<YamlException>(() => actorInfo.TraitsInConstructOrder());

			StringAssert.Contains(nameof(MockJInfo), ex.Message, "Exception message should report all cyclic dependencies.");
			StringAssert.Contains(nameof(MockKInfo), ex.Message, "Exception message should report all cyclic dependencies.");
			StringAssert.Contains(nameof(MockLInfo), ex.Message, "Exception message should report all cyclic dependencies.");
		}
	}
}
