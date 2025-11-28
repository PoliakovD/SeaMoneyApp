using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData.Binding;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class RegistrationViewModel : ViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }
    public string UrlPathSegment => "/login";
    public ReactiveCommand<Unit, Unit> RegistrationCommand { get; private set; }

    private Position? _selectedPosition;

    public Position? SelectedPosition
    {
        get => _selectedPosition;
        set => this.RaiseAndSetIfChanged(ref _selectedPosition, value);
    }

    private string? _searchText;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ObservableCollection<Position> Positions { get; set; } = new ObservableCollection<Position>();

    private string? _password = string.Empty;

    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string? _errorMessage = string.Empty;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private string? _username = string.Empty;

    public string? Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private short? _toursInRank;

    public short? ToursInRank
    {
        get => _toursInRank;
        set => this.RaiseAndSetIfChanged(ref _toursInRank, value);
    }

    public RegistrationViewModel(IScreen? screen = null)
    {
        var authService = Locator.Current.GetService<IAuthorizationService>()
                          ?? throw new InvalidOperationException("IAuthorizationService not registered");

        HostScreen = screen ?? Locator.Current.GetService<IScreen>()
            ?? throw new InvalidOperationException("IScreen not registered");

        Positions = new ObservableCollection<Position>();

        var positions = Locator.Current.GetService<DataBaseContext>()!.Positions.ToList();

        foreach (var position in positions)
        {
            Positions!.Add(position);
        }

        this.WhenAnyValue(vm => vm.SearchText)
            .WhereNotNull()
            .Subscribe(t =>
            {
                var positionsByName = Locator.Current.GetService<DataBaseContext>()!.GetPositionsByName(t);

                Positions.Clear();
                foreach (var product in positionsByName)
                {
                    Positions.Add(product);
                }
            });


        RegistrationCommand = ReactiveCommand.Create(() =>
            {
                if (authService.Login(Username!, Password!))
                {
                    HostScreen.Router.Navigate.Execute(new SearchViewModel());
                }
            },
            this
                .WhenAnyValue(x => x.Username, x => x.Password)
                .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2)));

        // Подписываемся на изменения ошибки
        authService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        //  сбрасывать ошибку при изменении полей
        //this.WhenAnyValue(x => x.Username, x => x.Password)
        //     .Subscribe(_ => ErrorMessage = string.Empty);
    }
}