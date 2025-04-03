using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LangTest;

[MarkupExtensionReturnType(typeof(string))]
public class DynamicResourceExtension : MarkupExtension, INotifyPropertyChanged
{
    private static readonly DynamicResourceObserver Observer = new DynamicResourceObserver();

    public event PropertyChangedEventHandler PropertyChanged;

    public DynamicResourceExtension() { }

    public DynamicResourceExtension(Type resourceType, string propertyName)
    {
        ResourceType = resourceType;
        PropertyName = propertyName;
    }

    [ConstructorArgument("resourceType")]
    public Type ResourceType { get; set; }

    [ConstructorArgument("propertyName")]
    public string PropertyName { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (ResourceType == null || string.IsNullOrEmpty(PropertyName))
        {
            return string.Empty;
        }

        string resourceKey = $"{ResourceType.FullName}|{PropertyName}";

        var binding = new Binding("Value")
        {
            Source = Observer,
            Path = new PropertyPath($"[{resourceKey}]"),
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}