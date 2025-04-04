using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LangTest.WpfLibrary;

public class LanguageProvider : ObservableObject
{
    public static LanguageProvider Instance { get; } = new();

    private XmlLanguage _language = XmlLanguage.GetLanguage("de-DE");
    public XmlLanguage Language
    {
        get { return this._language; }
        set
        {
            OnLanguageChanging(value);
            this.SetProperty(ref this._language, value);
        }
    }

    public EventHandler? LanguageChanging;

    protected virtual void OnLanguageChanging(XmlLanguage value)
    {
        this.LanguageChanging?.Invoke(this, EventArgs.Empty);
    }
}