using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace LangTest.WpfLibrary;

/// <summary>
/// Markup-Extension für lokalisierte Ressourcen mit effizientem Caching und Design-Support
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class DynamicResourceExtension : MarkupExtension
{
    private static readonly DynamicResourceObserver Observer = new();

    public DynamicResourceExtension() { }

    public DynamicResourceExtension(Type resourceType, string propertyName)
    {
        this.ResourceType = resourceType;
        this.PropertyName = propertyName;
    }

    /// <summary>
    /// Der Ressourcen-Typ, der die lokalisierte Ressource enthält
    /// </summary>
    [ConstructorArgument("resourceType")]
    public Type? ResourceType { get; set; }

    /// <summary>
    /// Der Name der Ressource/Property
    /// </summary>
    [ConstructorArgument("propertyName")]
    public string? PropertyName { get; set; }

    /// <summary>
    /// Zeigt Design-Zeit-Meldungen im Visual Studio Designer an
    /// </summary>
    [UsedImplicitly]
    public bool ShowDesignTimeMessages { get; set; } = true;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
#if DEBUG
        // Design-Zeit prüfen
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()) && this.ShowDesignTimeMessages)
        {
            if (this.ResourceType == null)
            {
                return "[ResourceType fehlt]";
            }

            if (string.IsNullOrEmpty(this.PropertyName))
            {
                return "[PropertyName fehlt]";
            }

            return $"[{this.ResourceType.Name}.{this.PropertyName}]";
        }
#endif

        if (this.ResourceType == null || string.IsNullOrEmpty(this.PropertyName))
        {
            return string.Empty;
        }

        // MultiBinding für direkten Typ-Zugriff
        var multiBinding = new MultiBinding
        {
            Converter = new ResourceValueConverter(),
            ConverterParameter = Observer,
            Mode = BindingMode.OneWay
        };

        // ResourceType und PropertyName binden
        multiBinding.Bindings.Add(new Binding
        {
            Source = this.ResourceType,
            Mode = BindingMode.OneWay
        });

        multiBinding.Bindings.Add(new Binding
        {
            Source = this.PropertyName,
            Mode = BindingMode.OneWay
        });

        return multiBinding.ProvideValue(serviceProvider);
    }

    /// <summary>
    /// Konverter für MultiBinding, der Type und PropertyName in eine lokalisierte Ressource umwandelt
    /// </summary>
    private class ResourceValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || parameter is not DynamicResourceObserver observer)
            {
                return string.Empty;
            }

            // Type und PropertyName extrahieren
            if (values[0] is Type resourceType && values[1] is string propertyName)
            {
                return observer.GetValue(resourceType, propertyName);
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
