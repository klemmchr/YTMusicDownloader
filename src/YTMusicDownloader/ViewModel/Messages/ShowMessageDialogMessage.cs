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
using Microsoft.Win32.SafeHandles;
using YTMusicDownloader.ViewModel.Messages.Callbacks;

namespace YTMusicDownloader.ViewModel.Messages
{
    internal class ShowMessageDialogMessage
    {
        public string Title { get; }
        public string Content { get; }
        public MessageDialogStyle Style { get; } = MessageDialogStyle.Affirmative;
        public ShowMessageDialogResultCallback Callback { get; }
        public MetroDialogSettings Settings { get; }

        public ShowMessageDialogMessage(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public ShowMessageDialogMessage(string title, string content, MessageDialogStyle style, ShowMessageDialogResultCallback callback, MetroDialogSettings settings = null): this(title, content)
        {
            Style = style;
            Callback = callback;
            Settings = settings;
        }
    }
}