using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace LangTest.WpfLibrary;

/// <summary>
/// Optimierter Observer für lokalisierte Ressourcen mit direkter ResourceManager-Unterstützung
/// </summary>
public class DynamicResourceObserver : INotifyPropertyChanged
{
    // Cache für direkte String-Werte
    private readonly Dictionary<string, string> _cachedValues = new();

    // Todo: Abklären - Optional auf Generationen basierender Cache für Werte - nur bei großen Datenmengen sinnvoll
    // private readonly Dictionary<string, (string value, int generation)> _cachedValuesWithGeneration = new();

    // Cache für ResourceManager nach Type
    private readonly Dictionary<Type, ResourceManager> _resourceManagers = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public DynamicResourceObserver()
    {
        // Cache vor Sprachwechsel leeren - Wechsel löst UI-Update aus.
        LanguageProvider.Instance.LanguageChanging += (sender, args) =>
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

        string cacheKey = $"{resourceType.FullName}|{propertyName}";

        // Wert aus Cache verwenden oder neu erstellen
        if (!this._cachedValues.TryGetValue(cacheKey, out string? value))
        {
            value = LoadResourceValue(resourceType, propertyName, cacheKey);
            this._cachedValues[cacheKey] = value;
        }

        return value;
    }

    /// <summary>
    /// Lädt einen Ressourcenwert mit dem optimalen Weg
    /// </summary>
    private string LoadResourceValue(Type resourceType, string propertyName, string cacheKey)
    {
        // Versuche, über ResourceManager zu laden (effizienteste Methode)
        string? resourceValue = GetResourceFromManager(resourceType, propertyName);
        if (!string.IsNullOrEmpty(resourceValue))
        {
            return resourceValue;
        }

        // Fallback: Versuche, über Reflection auf statische Property zuzugreifen
        var property = resourceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        if (property != null && property.CanRead)
        {
            object? value = property.GetValue(null);
            return value?.ToString() ?? $"[Null-Wert: {cacheKey}]";
        }

        // Ressource nicht gefunden
        return $"[Nicht gefunden: {cacheKey}]";
    }

    /// <summary>
    /// Holt eine Ressource über einen ResourceManager
    /// </summary>
    private string? GetResourceFromManager(Type resourceType, string propertyName)
    {
        try
        {
            // ResourceManager aus Cache verwenden oder neu erstellen
            if (!this._resourceManagers.TryGetValue(resourceType, out var resourceManager))
            {
                resourceManager = new ResourceManager(resourceType);
                this._resourceManagers[resourceType] = resourceManager;
            }

            // Ressource laden
            return resourceManager.GetString(propertyName);
        }
        catch
        {
            // Fehler beim Zugriff auf die Ressource ignorieren
            return null;
        }
    }
}