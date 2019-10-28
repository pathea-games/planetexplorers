using UnityEditor;
using UnityEngine;

namespace WhiteCat
{
	public class AttributePropertyDrawer<Attr> : BasePropertyDrawer where Attr : PropertyAttribute
	{
		public new Attr attribute { get { return base.attribute as Attr; } }
	}
}
