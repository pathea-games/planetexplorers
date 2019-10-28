using UnityEditor;
using UnityEngine;

namespace WhiteCat
{
	public class AttributeDecoratorDrawer<Attr> : DecoratorDrawer where Attr : PropertyAttribute
	{
		public new Attr attribute { get { return base.attribute as Attr; } }
	}
}
