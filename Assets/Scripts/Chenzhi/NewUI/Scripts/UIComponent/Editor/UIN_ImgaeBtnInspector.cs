using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(N_ImageButton),true)]
public class UIN_ImgaeBtnInspector : UIBaseInspector 
{
	N_ImageButton mButton;
//	UISprite mSprite;

//	void OnSelectAtlas (MonoBehaviour obj)
//	{
//		if (mButton.target != null)
//		{
//			NGUIEditorTools.RegisterUndo("Atlas Selection", mButton.target);
//			mButton.target.atlas = obj as UIAtlas;
//			mButton.target.MakePixelPerfect();
//		}
//	}

	public override void OnInspectorGUI_Propertys()
	{
		EditorGUIUtility.labelWidth = 80;
		EditorGUIUtility.fieldWidth = 0;
		mButton = target as N_ImageButton;
//		mSprite = EditorGUILayout.ObjectField("Sprite", mButton.target, typeof(UISprite), true) as UISprite;
//		
//		if (mButton.target != mSprite)
//		{
//			NGUIEditorTools.RegisterUndo("Image Button Change", mButton);
//			mButton.target = mSprite;
//			if (mSprite != null) mSprite.spriteName = mButton.normalSprite;
//		}
//		
//		if (mSprite != null)
//		{
//			//ComponentSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas);
//			
//			if (mSprite.atlas != null)
//			{
//				NGUIEditorTools.SpriteField("Normal", mSprite.atlas, mButton.normalSprite, _OnNormal);
//				NGUIEditorTools.SpriteField("Hover", mSprite.atlas, mButton.hoverSprite, _OnHover);
//				NGUIEditorTools.SpriteField("Pressed", mSprite.atlas, mButton.pressedSprite, _OnPressed);
//				NGUIEditorTools.SpriteField("Disable", mSprite.atlas, mButton.disableSprite, _OnDisable);
//			}
//		}
		//bool value;
		mButton.disable = EditorGUILayout.Toggle("Disable",mButton.disable);
//		mButton.effectTexture = EditorGUILayout.ObjectField("effectTexture", mButton.effectTexture, typeof(UITexture), true) as UITexture;
		mButton.lbAlphaFlag = EditorGUILayout.FloatField("LbAlphaFlag",mButton.lbAlphaFlag);
		mButton.normalItensity = EditorGUILayout.FloatField("Normal",mButton.normalItensity);
		mButton.hoverItensity = EditorGUILayout.FloatField("Hover",mButton.hoverItensity);
		mButton.pressedItensity = EditorGUILayout.FloatField("Pressed",mButton.pressedItensity);
		mButton.disableItensity = EditorGUILayout.FloatField("Disable",mButton.disableItensity);
		
		//		if (mButton.texHandler != null)
//		{

//		}
	}

	public override void OnInspectorGUI_Events()
	{
		FieldInfo[]	infos;
		infos = typeof(N_ImageButton).GetFields();
		
		DrawConpomentEvents(infos,mButton); 
	}

//	void _OnNormal (string spriteName)
//	{
//		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
//		mButton.normalSprite = spriteName;
//		mSprite.spriteName = spriteName;
//		mSprite.MakePixelPerfect();
//		if (mButton.collider == null || (mButton.collider is BoxCollider)) NGUITools.AddWidgetCollider(mButton.gameObject);
//		Repaint();
//	}
//	
//	void _OnHover (string spriteName)
//	{
//		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
//		mButton.hoverSprite = spriteName;
//		Repaint();
//	}
//	
//	void _OnPressed (string spriteName)
//	{
//		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
//		mButton.pressedSprite = spriteName;
//		Repaint();
//	}
//
//	void _OnDisable (string spriteName)
//	{
//		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
//		mButton.disableSprite = spriteName;
//		Repaint();
//	}
}
