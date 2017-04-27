using System;
using System.Collections.Generic;
using System.Text;

namespace SI.Mobile.RPMSGViewer.Lib
{
    public class EventsUtils
    {
	    public static void CallActionEvent(Action ae)
	    {
		    if (ae != null)
			    ae();
	    }

		public static void CallActionEvent<T>(Action<T> ae, T obj)
		{
			if (ae != null)
				ae(obj);
		}
	}
}
