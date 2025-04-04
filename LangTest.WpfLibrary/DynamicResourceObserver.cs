using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace LangTest.WpfLibrary;

/// <summary>
/// Optimierter Observer für lokalisierte Ressourcen mit direkter ResourceManager-Unterstützung
/// </summary>
public class DynamicResourceObserver
{
    // Cache für direkte String-Werte - Optimierung mit Generation-Cache möglich
    private readonly ConcurrentDictionary<string, string> _cachedValues = new();

    // Cache für ResourceManager nach Type
    private readonly ConcurrentDictionary<Type, ResourceManager> _resourceManagers = new();

    public DynamicResourceObserver()
    {
        // Cache vor Sprachwechsel leeren - Wechsel der Sprache löst UI-Update aus.
        LanguageProvider.Instance.LanguageChanging += (_, _) =>
        {
            this._cachedValues.Clear();
        };
    }

    /// <summary>
    /// Holt eine lokalisierte Ressource direkt über Type und PropertyName
    /// </summary>
    public string GetValue(Type? resourceType, string propertyName)
    {
        if (resourceType == null || string.IsNullOrEmpty(propertyName))
        {
            return string.Empty;
        }

        var cacheKey = $"{resourceType.FullName}|{propertyName}";
        return _cachedValues.GetOrAdd(cacheKey, key => LoadResourceValue(resourceType, propertyName, key));
    }

    /// <summary>
    /// Lädt einen Ressourcenwert mit dem optimalen Weg
    /// </summary>
    private string LoadResourceValue(Type resourceType, string propertyName, string cacheKey)
    {
        // Versuche, über ResourceManager zu laden (effizienteste Methode)
        var resourceValue = GetResourceFromManager(resourceType, propertyName);
        if (!string.IsNullOrEmpty(resourceValue))
        {
            return resourceValue;
        }

        // Fallback: Versuche, über Reflection auf statische Property zuzugreifen
        var property = resourceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        if (property != null && property.CanRead)
        {
            var value = property.GetValue(null);
            return value?.ToString() ?? $"[Null-Wert: {cacheKey}]";
        }

        return $"[Nicht gefunden: {cacheKey}]";
    }

    /// <summary>
    /// Holt eine Ressource über einen ResourceManager
    /// </summary>
    private string? GetResourceFromManager(Type resourceType, string propertyName)
    {
        try
        {
            var resourceManager = _resourceManagers.GetOrAdd(resourceType, type => new ResourceManager(type));
            return resourceManager.GetString(propertyName);
        }
        catch
        {
            // Fehler beim Zugriff auf die Ressource ignorieren
            return null;
        }
    }
}