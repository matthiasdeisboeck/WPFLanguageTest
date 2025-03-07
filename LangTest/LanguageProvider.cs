using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LangTest;

public partial class LanguageProvider : ObservableObject
{
    public static LanguageProvider Instance { get; } = new();

    [ObservableProperty]
    private XmlLanguage _language = XmlLanguage.GetLanguage("de-DE");
}