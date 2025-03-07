﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace LangTest
{
    internal partial class MainViewModel : ObservableObject
    {
        private bool _isGerman = true;

        [ObservableProperty]
        private decimal _decimalValue = 32145412.333m;
        
        [RelayCommand]
        private void ChangeLanguage()
        {
            if (this._isGerman)
            {
                LanguageProvider.Instance.Language = XmlLanguage.GetLanguage("en-US");
                this._isGerman = false;
            }
            else
            {
                LanguageProvider.Instance.Language = XmlLanguage.GetLanguage("de-DE");
                this._isGerman = true;
            }
        }
    }
}
