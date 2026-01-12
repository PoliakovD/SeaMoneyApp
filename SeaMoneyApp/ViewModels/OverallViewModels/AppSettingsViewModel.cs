using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReactiveUI;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.ViewModels.OverallViewModels;

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
        get => _currentCulture;
        set => this.RaiseAndSetIfChanged(ref _currentCulture, value);
    }

    private string _selectedLanguage;

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }
    public AppSettingsViewModel()
    {
        CurrentCulture = Localization.Localization.Culture;
        SelectedLanguage = Languages.First(x => x.Value == CurrentCulture!.Name).Key;
        this.WhenAnyValue(x => x.SelectedLanguage).Subscribe(new Action<string>(async lang =>
        {
            LogHost.Default.Debug("Selected language Start Event: " + lang);

            if (Languages[lang] == Localization.Localization.Culture.Name)
            {
                LogHost.Default.Debug("Selected language Same: " + lang);
                return;
            }

            Localization.Localization.Culture = CultureInfo.GetCultureInfo(Languages[lang]);
            LogHost.Default.Debug("Selected language Finish Event: " + lang);
            LogHost.Default.Debug("Selected language Selected Event: " + lang + " / " +
                                  Localization.Localization.Culture.Name);
            var appsession = Locator.Current.GetService<AppSession>();
            appsession.Culture = Localization.Localization.Culture.Name;
            await appsession.SaveSessionAsync();
            
            Locator.Current.GetService<IScreen>().Router.NavigateAndCache<OverallViewModel>();
            
        }));
    }
}