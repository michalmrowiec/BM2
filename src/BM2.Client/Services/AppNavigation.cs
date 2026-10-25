using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BM2.Client.Services;

/// <summary>Knows whether the user got here by navigating inside the app, so "back" stays in the app.</summary>
public sealed class AppNavigation : IDisposable
{
    private readonly NavigationManager _navigation;

    public AppNavigation(NavigationManager navigation)
    {
        _navigation = navigation;
        _navigation.LocationChanged += OnLocationChanged;
    }

    public bool CanGoBack { get; private set; }

    public void Dispose() => _navigation.LocationChanged -= OnLocationChanged;

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => CanGoBack = true;
}
