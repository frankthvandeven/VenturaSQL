namespace VenturaSQLStudio.Progress
{
	public class ProgressDialogSettings
	{
		public static ProgressDialogSettings WithLabelOnly = new ProgressDialogSettings(false, false, true);
		public static ProgressDialogSettings WithSubLabel = new ProgressDialogSettings(true, false, true);
		public static ProgressDialogSettings WithSubLabelAndCancel = new ProgressDialogSettings(true, true, true);

		public bool ShowSubLabel { get; set; }
		public bool ShowCancelButton { get; set; }
		public bool ShowProgressBarIndeterminate { get; set; }

		public ProgressDialogSettings()
		{
			ShowSubLabel = false;
			ShowCancelButton = false;
			ShowProgressBarIndeterminate = true;
		}

		public ProgressDialogSettings(bool showSubLabel, bool showCancelButton, bool showProgressBarIndeterminate)
		{
			ShowSubLabel = showSubLabel;
			ShowCancelButton = showCancelButton;
			ShowProgressBarIndeterminate = showProgressBarIndeterminate;
		}
	}
}
