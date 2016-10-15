using MahApps.Metro.Controls.Dialogs;

namespace YTMusicDownloader.ViewModel.Messages
{
    class ShowMessageDialogMessage
    {
        public string Title { get; }
        public string Content { get; }
        public MessageDialogStyle Style { get; }

        public ShowMessageDialogMessage(string title, string content, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            Title = title;
            Content = content;
            Style = style;
        }
    }
}
