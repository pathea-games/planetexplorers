using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSSelectedBoxModify : BSModify
{
	
//	IntVector3[] old_indexes;
//	byte[] old_value;
//	IntVector3[] new_indexes;
//	byte[] new_value;

	BSSelectBrush select_brush;

	Dictionary<IntVector3, byte>  old_selection;
	Dictionary<IntVector3, byte>  new_selection;
	
	IBSDataSource data_source = null;

	public BSSelectedBoxModify (Dictionary<IntVector3, byte> old_value, Dictionary<IntVector3, byte> new_value, BSSelectBrush brush)
	{

		old_selection = old_value;
		new_selection = new_value;
		select_brush = brush;
		data_source = brush.dataSource;
	}

	public override bool Redo ()
	{
		if (select_brush != null)
			select_brush.ResetSelection(new_selection);
		return true;
	}

	public override bool Undo ()
	{
		if (select_brush != null)
			select_brush.ResetSelection(old_selection);
		return true;
	}

	public override bool IsNull ()
	{
		return (select_brush == null) || (select_brush.dataSource != data_source);
	}
}
