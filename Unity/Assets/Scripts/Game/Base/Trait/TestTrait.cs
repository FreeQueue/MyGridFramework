namespace S100
{
	public class TestRequireTraitInfo : TraitInfo<TestTrait>
	{
	}
	
	[RequireTrait(typeof(TestRequireTraitInfo))]
	public class TestTraitInfo : TraitInfo<TestTrait>
	{
		public int testField;
	}

	public class TestTrait
	{

	}
}