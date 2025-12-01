using System;
using ReactiveUI;
using Splat;

namespace SeaMoneyApp.ViewModels;

public abstract partial class RoutableViewModel : ReactiveObject,IRoutableViewModel
{
    public string UrlPathSegment { get; set; }

    public IScreen? HostScreen { get; set; }

    public RoutableViewModel()
    {
        if (this is not IScreen)
        HostScreen ??= Locator.Current.GetService<IScreen>()
                       ?? throw new InvalidOperationException("IScreen not registered");
        
        UrlPathSegment = this.GetType().Name.Replace("ViewModel", "").ToLower();
    }
    
}