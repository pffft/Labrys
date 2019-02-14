using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public abstract class Tool
	{
		public virtual void Draw() { }
		public abstract bool HandleEvent(Event e);
	}
}
