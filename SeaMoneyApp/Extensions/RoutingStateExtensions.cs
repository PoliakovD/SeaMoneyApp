using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ReactiveUI;
using SeaMoneyApp.ViewModels;

namespace SeaMoneyApp.Extensions;

public static class RoutingStateExtensions
{
    // Кэш для хранения ViewModel по типу
    private static readonly ConcurrentDictionary<Type, RoutableViewModel> ViewModelCache = new();
    
    public static void NavigateAndCache<T>(this RoutingState router, Func<T>? factory = null) 
        where T : RoutableViewModel, new()
    {
        var type = typeof(T);
        var vm = (T)ViewModelCache.GetOrAdd(type, _ => factory?.Invoke() ?? new T());
        router.Navigate.Execute(vm);
    }
    
    public static void NavigateAndCache<T>(this RoutingState router, RoutableViewModel routableViewModel) 
    {
        var type = typeof(T);
        var vm = ViewModelCache.GetOrAdd(type, routableViewModel);
        router.Navigate.Execute(vm);
    }
    
    public static void ClearCache(this RoutingState router)
    {
        ViewModelCache.Clear();
    }
    
    public static void ClearCache<T>(this RoutingState router) where T : RoutableViewModel
    {
        ViewModelCache.TryRemove(typeof(T), out _);
    }
}

public interface IScreenBackCommand : IScreen
{
    
}