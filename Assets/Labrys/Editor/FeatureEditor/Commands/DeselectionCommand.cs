namespace Labrys.Editor.FeatureEditor.Commands
{
	public class DeselectionCommand : SelectionCommand
	{
		public override void Do()
		{
			base.Undo();
		}

		public override void Undo()
		{
			base.Do();
		}
	}
}

