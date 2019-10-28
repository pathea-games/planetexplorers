using System.Collections;
using System;

internal delegate void ActionEventHandler(object sender, EventArgs e);

internal class ActionDelegate
{
	private object _sender;
	private ActionEventHandler _actionEvent;
	private EventArgs _eventArgs;
	
	internal ActionDelegate(object sender, ActionEventHandler action, EventArgs args)
	{
		_sender = sender;
		_actionEvent = action;
		_eventArgs = args;
	}
	
	internal void OnAction()
	{
		if (null != _actionEvent)
			_actionEvent(_sender, _eventArgs);
	}
}
