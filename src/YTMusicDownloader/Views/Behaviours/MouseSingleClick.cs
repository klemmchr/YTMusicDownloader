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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YTMusicDownloader.Views.Behaviours
{
    internal class MouseSingleClick
    {
        public static DependencyProperty CommandProperty =
                DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(Behaviours.MouseSingleClick),
                new UIPropertyMetadata(CommandChanged));

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                                                typeof(object),
                                                typeof(Behaviours.MouseSingleClick),
                                                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }
        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }
        private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var control = target as Control;
            if (control != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    control.MouseLeftButtonUp += OnMouseSingleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.MouseDoubleClick -= OnMouseSingleClick;
                }
            }
        }

        private static void OnMouseSingleClick(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            var command = (ICommand)control.GetValue(CommandProperty);
            var commandParameter = control.GetValue(CommandParameterProperty);
            command.Execute(commandParameter);
        }
    }
}