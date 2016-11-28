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
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using MahApps.Metro.IconPacks;
using YTMusicDownloaderLib.Helpers;
using YTMusicDownloaderLib.Properties;

namespace YTMusicDownloader.ViewModel
{
    internal class SettingViewModel : ViewModelBase
    {
        #region Construction        

        /// <summary>
        ///     Initializes a new instance of the <see cref="SettingViewModel" /> class.
        /// </summary>
        /// <param name="settingSource">The setting source.</param>
        /// <param name="property">The property name of the setting.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <exception cref="ArgumentException">
        ///     maxValue has to be higher or equal than minValue
        ///     or
        ///     minValue has to be lower or equal than minValue
        /// </exception>
        public SettingViewModel(object settingSource, string property, string title, string description,
            PackIconMaterialKind icon, object defaultValue, int minValue = 0, int maxValue = 0)
        {
            if (maxValue < minValue)
                throw new ArgumentException($"{nameof(maxValue)} has to be higher or equal than {nameof(minValue)}");

            if (minValue > maxValue)
                throw new ArgumentException($"{nameof(minValue)} has to be lower or equal than {nameof(maxValue)}");

            if (settingSource.GetType().GetProperty(property).GetValue(settingSource, null).GetType() !=
                defaultValue.GetType())
                throw new ArgumentException("Default type and settings type have to be equal");

            _defaultValue = defaultValue;
            _settingSource = settingSource;
            _property = property;
            Title = title;
            Description = description;
            Icon = icon;

            _minValue = minValue;
            _maxValue = maxValue;

            Options = new Dictionary<Enum, string>();

            SetupEnum();
            SetDefaultValue();
        }

        #endregion

        #region Fields        

        private readonly object _settingSource;
        private readonly string _property;
        private readonly int _minValue;
        private readonly int _maxValue;
        private Enum _selectedOption;
        private readonly object _defaultValue;

        #endregion

        #region Properties        

        /// <summary>
        ///     Gets or sets the setting.
        ///     This is the property holding the refernce to the setting itself vía reflection.
        /// </summary>
        /// <value>
        ///     The setting.
        /// </value>
        public object Setting
        {
            get { return _settingSource.GetType().GetProperty(_property).GetValue(_settingSource, null); }
            set
            {
                if (IsInt)
                {
                    int convertedValue;
                    if (!int.TryParse(value.ToString(), out convertedValue))
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

        /// <summary>
        ///     Gets a value indicating whether the setting type is of type int.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the setting is of type int; otherwise, <c>false</c>.
        /// </value>
        public bool IsInt => Setting is int;

        /// <summary>
        ///     Gets a value indicating whether the setting is of type bool.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the setting is of type bool; otherwise, <c>false</c>.
        /// </value>
        public bool IsBool => Setting is bool;

        /// <summary>
        ///     Gets a value indicating whether the setting is of type enum.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is of type enum ; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnum => Setting is Enum;

        /// <summary>
        ///     Gets the title of the setting.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title { get; }

        /// <summary>
        ///     Gets the description of the setting.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        ///     Gets the default value of the setting.
        /// </summary>
        /// <value>
        ///     The default value.
        /// </value>
        public string DefaultValue { get; private set; }

        /// <summary>
        ///     Gets the icon.
        /// </summary>
        /// <value>
        ///     The icon.
        /// </value>
        public PackIconMaterialKind Icon { get; }

        /// <summary>
        ///     Gets the options (just for enum settings).
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        public Dictionary<Enum, string> Options { get; }

        /// <summary>
        ///     Gets or sets the selected option (just for enum settings).
        /// </summary>
        /// <value>
        ///     The selected option.
        /// </value>
        public Enum SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                _selectedOption = value;
                RaisePropertyChanged(nameof(SelectedOption));
                Setting = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Setups the options for enum types.
        /// </summary>
        private void SetupEnum()
        {
            if (!IsEnum)
                return;

            try
            {
                foreach (Enum value in Enum.GetValues(Setting.GetType()))
                    Options.Add(value, Enumerations.GetDescription(value));

                SelectedOption = Setting as Enum;
            }
            catch
            {
                // ignored
            }
        }

        private void SetDefaultValue()
        {
            if (IsInt)
                DefaultValue = _defaultValue.ToString();
            else if (IsBool)
                DefaultValue = (bool) _defaultValue ? Resources.Activated : Resources.Deactivated;
            else if (IsEnum)
                try
                {
                    DefaultValue = Enumerations.GetDescription((Enum) _defaultValue);
                }
                catch
                {
                    // ignored
                }
        }

        #endregion
    }
}