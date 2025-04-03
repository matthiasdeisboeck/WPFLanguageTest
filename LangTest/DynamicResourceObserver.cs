using System.ComponentModel;
using System.Reflection;

namespace LangTest;

public class DynamicResourceObserver : INotifyPropertyChanged
{
    // ToDo: Cache mit explizitem Wert statt mit Funktion, es ist immer nur eine Ressource ...
    private readonly Dictionary<string, Func<string>> _cachedResolvers = new();
    public event PropertyChangedEventHandler PropertyChanged;

    public DynamicResourceObserver()
    {
        // Sprachänderungen überwachen
        LanguageProvider.Instance.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LanguageProvider.Language))
            {
                _cachedResolvers.Clear();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            }
        };
    }

    // Indexer zur dynamischen Abfrage von Ressourcen
    public string this[string resourceKey]
    {
        get
        {
            // Resolver aus Cache verwenden oder neu erstellen
            if (!_cachedResolvers.TryGetValue(resourceKey, out var resolver))
            {
                resolver = CreateResolver(resourceKey);
                _cachedResolvers[resourceKey] = resolver;
            }

            return resolver();
        }
    }

    private Func<string> CreateResolver(string resourceKey)
    {
        // Format: "TypeName|PropertyName" auswerten
        if (resourceKey.Contains("|"))
        {
            var parts = resourceKey.Split('|');
            if (parts.Length == 2)
            {
                var typeName = parts[0];
                var propertyName = parts[1];

                // Optimierung für Resources-Klasse
                if (typeName == typeof(Properties.Resources).FullName)
                {
                    return () => {
                        var propertyInfo = typeof(Properties.Resources).GetProperty(propertyName);
                        if (propertyInfo != null)
                        {
                            return propertyInfo.GetValue(null)?.ToString() ?? $"[{resourceKey}]";
                        }

                        return $"[{resourceKey}]";
                    };
                }

                // Typ finden
                var type = Type.GetType(typeName) ?? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch
                        {
                            return Array.Empty<Type>();
                        }
                    })
                    .FirstOrDefault(t => t.FullName == typeName);

                if (type != null)
                {
                    var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                    if (property != null)
                    {
                        return () => property.GetValue(null)?.ToString() ?? $"[{resourceKey}]";
                    }
                }
            }
        }

        // Fallback für direkten Ressourcenschlüssel
        return () => {
            try
            {
                return Properties.Resources.ResourceManager.GetString(resourceKey) ?? $"[{resourceKey}]";
            }
            catch
            {
                return $"[{resourceKey}]";
            }
        };
    }
}