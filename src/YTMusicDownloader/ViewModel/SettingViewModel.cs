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

using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using YTMusicDownloader.Properties;

namespace YTMusicDownloader.ViewModel
{
    internal class SettingViewModel: ViewModelBase
    {
        #region Fields        
        private readonly object _settingSource;
        private readonly string _property;
        private readonly int _minValue;
        private readonly int _maxValue;
        #endregion

        #region Properties

        public object Setting
        {
            get { return _settingSource.GetType().GetProperty(_property).GetValue(_settingSource, null); }
            set
            {
                if (IsInt)
                {
                    int convertedValue;
                    if(!int.TryParse(value.ToString(), out convertedValue))
                        return;
                    
                    if (convertedValue > _maxValue)
                        convertedValue = _maxValue;
                    else if (convertedValue < _minValue)
                        convertedValue = _minValue;

                    _settingSource.GetType().GetProperty(_property).SetValue(_settingSource, convertedValue);
                }
                else
                {
                    _settingSource.GetType().GetProperty(_property).SetValue(_settingSource, value);
                }
                
                RaisePropertyChanged(nameof(Setting));
            }
        }

        public bool IsInt => IsInDesignMode || _settingSource.GetType().GetProperty(_property).GetValue(_settingSource, null) is int;
        public bool IsBool => IsInDesignMode || _settingSource.GetType().GetProperty(_property).GetValue(_settingSource, null) is bool;
        public bool IsEnum => IsInDesignMode || _settingSource.GetType().GetProperty(_property).GetValue(_settingSource, null) is Enum;

        public string Title { get; }
        public string Description { get; }
        public object DefaultValue { get; }
        public string Icon { get; }
        #endregion

        #region Construction
        public SettingViewModel(object settingSource, string property, string title, string description, object defaultValue, string icon, int minValue, int maxValue)
        {
            if(maxValue < minValue)
                throw new ArgumentException($"Value has to be higher or equal than {nameof(minValue)}", nameof(maxValue));

            if (minValue > maxValue)
                throw new ArgumentException($"Value has to be lower or equal than {nameof(maxValue)}", nameof(minValue));

            _settingSource = settingSource;
            _property = property;
            Title = title;
            Description = description;
            DefaultValue = defaultValue;
            Icon = icon;

            _minValue = minValue;
            _maxValue = maxValue;
        }
        #endregion

        #region Methods
        #endregion
    }
}