using UnityEngine;
using System.Collections;

public enum VCEMsgBoxType : int
{
	CLOSE_QUERY,		// The query msg box when closeing
	CLOSE_QUERY_NOSAVE,	// The query msg box when closeing with no save action
	SWITCH_QUERY,		// The query msg box when switching the tab
	LOAD_QUERY,      	// The query msg box when loading
	MATERIAL_NOT_SAVED, // The error msg box when material not saved 
	MATERIAL_DEL_QUERY, // The query msg box when delete material
	DECAL_NOT_SAVED,    // The error msg box when decal not saved 
	DECAL_DEL_QUERY,    // The query msg box when decal material
	DELETE_ISO,			// The query msg box when deleteing the ISO
	MISSING_ISO,        // The error msg box when loading a missing ISO
	CORRUPT_ISO,        // The error msg box when loading a corrupt ISO
	CANNOT_SAVE_NONAME,	// The error msg box that script didn't assigned a name
	REPLACE_QUERY,		// The query msg box when overwrite an exsisting iso
	SAVE_OK,			// The hint msg box of save ok
	SAVE_FAILED,		// The hint msg box of save failed
	ISO_INCOMPLETE, 	// The error msg box that the ISO is incomplete
	ISO_INVALID,		// The error msg box that the ISO is invalid
	EXPORT_OK,			// The hint msg box of export ok
	EXPORT_NETWORK,		// The hint msg box of network mode exporting
	EXPORT_NETWORK_FAILED,
	EXPORT_EMPTY_NAME,
	EXPORT_EXISTNAME,
	EXPORT_FAILED,		// The hint msg box of export failed
	EXPORT_NOTSAVED,	// The error msg box of export need the script saved first
	EXPORT_FULL,		// The error msg box of there isn't any empty grid
	CANNOT_EXPORT_NOW,	// The error msg box of can not export now because an ISO is during exporting
	CANNOT_EXTRUDE,    	// The error msg box of editor can't do extract operation
}

public enum VCEMsgBoxButton{L,R,C}

public struct VCEMsgBoxDesc
{
	public string Title;
	public string Message;
	public string Icon;
	public string ButtonL;
	public string ButtonC;
	public string ButtonR;
	
	public VCEMsgBoxDesc(VCEMsgBoxType type)
	{
		Title = "";
		Message = "";
		Icon = "";
		ButtonL = "";
		ButtonC = "";
		ButtonR = "";
		switch (type)
		{
		case VCEMsgBoxType.CLOSE_QUERY:
			Title = "Quit and save".ToLocalizationString();
			Message = "Do you want to save the ISO before closing ?".ToLocalizationString();
			ButtonL = "Save".ToLocalizationString();
			ButtonC = "Don't Save".ToLocalizationString();
			ButtonR = "Cancel".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.CLOSE_QUERY_NOSAVE:
			Title = "Quit".ToLocalizationString();
			Message = "Quit the Voxel Creation Editor ?".ToLocalizationString();
			ButtonL = "Quit".ToLocalizationString();
			ButtonR = "Don't Quit".ToLocalizationString();
			Icon = "msgbox_alert";
			break;	
		case VCEMsgBoxType.SWITCH_QUERY:
			Title = "Switch scene".ToLocalizationString();
			Message = "Swiching scene will reset the current scene !".ToLocalizationString() + "\r\n" + "Do you want to save the current scene as an ISO ?".ToLocalizationString();
			ButtonL = "Save".ToLocalizationString();
			ButtonR = "Don't Save".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.LOAD_QUERY:
			Title = "Save ISO".ToLocalizationString();
			Message = "Do you want to save the current ISO before loading another ?".ToLocalizationString();
			ButtonL = "Save".ToLocalizationString();
			ButtonR = "Don't Save".ToLocalizationString();
			ButtonC = "Cancel".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.MATERIAL_NOT_SAVED:
			Title = "Material not saved".ToLocalizationString();
			Message = "Save material list failed ! (Corrupt file or no permission)".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.MATERIAL_DEL_QUERY:
			Title = "Delete material".ToLocalizationString();
			Message = "Do you really want to delete this material ?".ToLocalizationString() + "\r\n " + "You still can restore it from the ISO which contains it.".ToLocalizationString();
			ButtonL = "DELETE".ToLocalizationString();
			ButtonR = "Cancel".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.DECAL_NOT_SAVED:
			Title = "Decal not saved".ToLocalizationString();
			Message = "Save decal list failed ! (Corrupt file or no permission)".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.DECAL_DEL_QUERY:
			Title = "Delete decal".ToLocalizationString();
			Message = "Do you really want to delete this decal ?".ToLocalizationString() + "\r\n" + "You still can restore it from the ISO which contains it.".ToLocalizationString();
			ButtonL = "DELETE".ToLocalizationString();
			ButtonR = "Cancel".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.DELETE_ISO:
			Title = "!! Delete ISO !!".ToLocalizationString();
			Message = "Do you really want to delete this ISO ?".ToLocalizationString() + "\r\n" + "This action cannot be undone !".ToLocalizationString();
			ButtonL = "DELETE".ToLocalizationString();
			ButtonR = "Cancel".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.MISSING_ISO:
			Title = "Loading failed".ToLocalizationString();
			Message = "The source file of this ISO is missing !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.CORRUPT_ISO:
			Title = "Loading failed".ToLocalizationString();
			Message = "The source file of this ISO is corrupt !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.CANNOT_SAVE_NONAME:
			Title = "Cannot save".ToLocalizationString();
			Message = "An ISO name must be assigned !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_deny";
			break;
		case VCEMsgBoxType.REPLACE_QUERY:
			Title = "!! WARNING !!".ToLocalizationString();
			Message = "Do you want to overwrite the old ISO ?".ToLocalizationString();
			ButtonL = "REPLACE".ToLocalizationString();
			ButtonR = "Cancel".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.SAVE_OK:
			Title = "Save OK".ToLocalizationString();
			Message = "The current ISO saved successfully !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_ok";
			break;
		case VCEMsgBoxType.SAVE_FAILED:
			Title = "Error".ToLocalizationString();
			Message = "Failed to save the current ISO !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.ISO_INCOMPLETE:
			Title = "ISO incomplete".ToLocalizationString();
			Message = "The current ISO is incomplete, cannot save or export !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_deny";
			break;
		case VCEMsgBoxType.ISO_INVALID:
			Title = "ISO invalid".ToLocalizationString();
			Message = "The current ISO is invalid !".ToLocalizationString() + "\r\n" + "Please correct your templates !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_deny";
			break;
		case VCEMsgBoxType.EXPORT_OK:
			Title = "Export OK".ToLocalizationString();
			Message = "The creation exported successfully. Enjoy !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_ok";
			break;
		case VCEMsgBoxType.EXPORT_NETWORK_FAILED:
			Title = "Error".ToLocalizationString();
			Message = "Creation uploading failed !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.EXPORT_EMPTY_NAME:
			Title = "Error".ToLocalizationString();
			Message = "Creation name cannot be empty !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.EXPORT_NETWORK:
			Title = "!! WARNING !!".ToLocalizationString();
			Message = "NOTE : Your creation is uploading to the workshop, you will receive it later!".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
			case VCEMsgBoxType.EXPORT_EXISTNAME:
			Title = "Error".ToLocalizationString();
			Message = "Failed to export due to an existing iso name with different data! Please rename it.".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.EXPORT_FAILED:
			Title = "Error".ToLocalizationString();
			Message = "Failed to export the item due to saving resource failure !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_error";
			break;
		case VCEMsgBoxType.EXPORT_NOTSAVED:
			Title = "Hint".ToLocalizationString();
			Message = "You need to save the current ISO first !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.EXPORT_FULL:
			Title = "Hint".ToLocalizationString();
			Message = "Your bag is full , please sort out your bag first !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.CANNOT_EXPORT_NOW:
			Title = "Cannot export now".ToLocalizationString();
			Message = "You cannot export now, because something is now exporting !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_alert";
			break;
		case VCEMsgBoxType.CANNOT_EXTRUDE:
			Title = "Extrude".ToLocalizationString();
			Message = "The editor cannot EXTRUDE more than 8000 voxels at a time !".ToLocalizationString();
			ButtonC = "OK".ToLocalizationString();
			Icon = "msgbox_deny";
			break;
		default:
			Title = "";
			Message = "Undefined Message Box".ToLocalizationString();
			Icon = "msgbox_error";
			ButtonL = "";
			ButtonC = "OK".ToLocalizationString();
			ButtonR = "";
			break;
		}
	}
}