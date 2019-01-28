using System;

namespace Labrys.Editor.FeatureEditor
{
	/// <summary>
	/// For use with Tools and selectable grid objects to build a property list
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class PropertyField : Attribute
	{
		public string Name { get; set; }
	}
}
