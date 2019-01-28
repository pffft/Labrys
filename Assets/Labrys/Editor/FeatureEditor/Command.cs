namespace Labrys.Editor.FeatureEditor
{
	public abstract class Command
	{
		public string Description { get; }

		public abstract void Do();
		public abstract void Undo();
	}
}
