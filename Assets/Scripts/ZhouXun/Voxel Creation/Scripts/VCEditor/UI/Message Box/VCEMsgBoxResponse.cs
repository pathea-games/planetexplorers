using UnityEngine;
using System.Collections;

public static class VCEMsgBoxResponse
{
	public static void Response (VCEMsgBoxType type, VCEMsgBoxButton button, int frameindex)
	{
		switch (type)
		{
		case VCEMsgBoxType.CLOSE_QUERY:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEditor.Instance.m_UI.OnSaveClick();
			}
			else if ( button == VCEMsgBoxButton.C )
			{
				VCEditor.Quit();
			}
			break;
		}
		case VCEMsgBoxType.CLOSE_QUERY_NOSAVE:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEditor.Quit();
			}
			break;
		}
		case VCEMsgBoxType.SWITCH_QUERY:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEditor.Instance.m_UI.OnSaveClick();
			}
			else if ( button == VCEMsgBoxButton.R )
			{
				VCEUISceneMenuItem.DoCreateSceneFromMsgBox();
			}
			VCEUISceneMenuItem.s_SceneToCreate = null;
			break;
		}
		case VCEMsgBoxType.LOAD_QUERY:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEditor.Instance.m_UI.OnSaveClick();
			}
			else if ( button == VCEMsgBoxButton.R )
			{
				VCEUIIsoItem.DoLoadFromMsgBox();
			}
			VCEUIIsoItem.s_IsoToLoad = "";
			break;
		}
		case VCEMsgBoxType.MATERIAL_DEL_QUERY:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEUIMaterialItem.DoDeleteFromMsgBox();
			}
			VCEUIMaterialItem.s_CurrentDelMat = null;
			break;
		}
		case VCEMsgBoxType.DECAL_DEL_QUERY:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEUIDecalItem.DoDeleteFromMsgBox();
			}
			VCEUIDecalItem.s_CurrentDelDecal = null;
			break;
		}
		case VCEMsgBoxType.DELETE_ISO:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEUIIsoItem.DoDeleteFromMsgBox();
			}
			VCEUIIsoItem.s_IsoToDelete = "";
			break;
		}
		case VCEMsgBoxType.MISSING_ISO:
		{
			break;
		}
		case VCEMsgBoxType.CORRUPT_ISO:
		{
			break;
		}
		case VCEMsgBoxType.CANNOT_SAVE_NONAME:
		{
			break;
		}
		case VCEMsgBoxType.REPLACE_QUERY:
		{
			if ( button == VCEMsgBoxButton.L )
			{
				VCEUISaveWnd.DoSaveForOverwrite();
			}
			VCEUISaveWnd.s_SaveTargetForOverwrite = "";
			break;
		}
		case VCEMsgBoxType.SAVE_OK:
		{
			break;
		}
		case VCEMsgBoxType.SAVE_FAILED:
		{
			break;
		}
		case VCEMsgBoxType.ISO_INCOMPLETE:
		{
			break;
		}
		case VCEMsgBoxType.ISO_INVALID:
		{
			break;
		}
		case VCEMsgBoxType.EXPORT_OK:
		{
			VCEditor.Instance.m_UI.m_ExportWindow.Hide();
			break;
		}
		case VCEMsgBoxType.EXPORT_NETWORK:
		{
			VCEditor.Instance.m_UI.m_ExportWindow.Hide();
			break;
		}
		case VCEMsgBoxType.EXPORT_FAILED:
		{
			break;
		}
		case VCEMsgBoxType.EXPORT_NOTSAVED:
		{
			VCEditor.Instance.m_UI.OnSaveAsClick();
			break;
		}
		case VCEMsgBoxType.EXPORT_FULL:
		{
			break;
		}
		case VCEMsgBoxType.CANNOT_EXPORT_NOW:
		{
			break;
		}
		case VCEMsgBoxType.CANNOT_EXTRUDE:
		{
			break;
		}
		}
	}
}
