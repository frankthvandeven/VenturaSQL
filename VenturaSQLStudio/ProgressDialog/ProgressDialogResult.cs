using System;
using System.ComponentModel;

namespace VenturaSQLStudio.Progress
{
	internal class ProgressDialogResult
	{
		public object Result { get; private set; }
		public bool Cancelled { get; private set; }
		public Exception Error { get; private set; }

		public bool OperationFailed
		{
			get { return Error != null; }
		}

		public ProgressDialogResult(RunWorkerCompletedEventArgs e)
		{
			if(e.Cancelled)
				Cancelled = true;
			else if(e.Error != null)
				Error = e.Error;
			else
				Result = e.Result;
		}
	}
}
