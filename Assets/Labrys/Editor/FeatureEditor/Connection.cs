using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection
	{
		private Tile[] members;

		public Connection(Tile self, Tile other, LinkDirection direction)
		{
			members = new Tile[2];
			members[0] = self;
			members[1] = other;

			self.SetLink(
		}

		public void Draw()
		{

		}

		public bool HandleEvent(Event e)
		{

		}
	}
}
