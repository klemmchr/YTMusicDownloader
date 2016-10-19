/*
    Copyright 2016 Christian Klemm

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using MahApps.Metro.Controls.Dialogs;

namespace YTMusicDownloader.ViewModel.Messages
{
    internal class ShowMessageDialogMessage
    {
        public ShowMessageDialogMessage(string title, string content,
            MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            Title = title;
            Content = content;
            Style = style;
        }

        public string Title { get; }
        public string Content { get; }
        public MessageDialogStyle Style { get; }
    }
}