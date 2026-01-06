using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Localization;
using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class AppSettingsViewModel : RoutableViewModel
{
    public Dictionary<string, string> Languages { get; } = new()
    {
        { "Русский", "ru-RU" },
        { "English", "en-US" }
    };

    private CultureInfo _currentCulture;

    public CultureInfo? CurrentCulture
    {
        get { return _currentCulture; }
        set { this.RaiseAndSetIfChanged(ref _currentCulture, value); }
    }

    private string _selectedLanguage;

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }

    public ReactiveCommand<SelectionChangedEventArgs, Unit> SelectedLanguageCommand { get; private set; }

    public AppSettingsViewModel()
    {
        CurrentCulture = Localization.Localization.Culture;
        SelectedLanguage = Languages.First(x => x.Value == CurrentCulture!.Name).Key;
        this.WhenAnyValue(x => x.SelectedLanguage).Subscribe(new Action<string>(async _ =>
        {
            LogHost.Default.Debug("Selected language Start Event: " + _);

            if (Languages[_] == Localization.Localization.Culture.Name)
            {
                LogHost.Default.Debug("Selected language Same: " + _);
                return;
            }

            Localization.Localization.Culture = CultureInfo.GetCultureInfo(Languages[_]);
            LogHost.Default.Debug("Selected language Finish Event: " + _);
            LogHost.Default.Debug("Selected language Selected Event: " + _ + " / " +
                                  Localization.Localization.Culture.Name);
            var appsession = Locator.Current.GetService<AppSession>();
            appsession.Culture = Localization.Localization.Culture.Name;
            await appsession.SaveSessionAsync();
            
            Locator.Current.GetService<IScreen>().Router.NavigateAndCache<OverallViewModel>();
            
        }));
    }
}