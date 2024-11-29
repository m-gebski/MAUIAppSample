using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scripts.ScriptLogic;
using Services.AlertService;
using Shared;

namespace Scripts.ViewModels
{
    public partial class ChangeCvlIdScriptPageViewModel : ObservableObject
    {
        private readonly IPreferences _preferences;
        private readonly IAlertService _alertSvc;

        [ObservableProperty]
        string _environmentSelect;
        [ObservableProperty]
        string _cvlIdOld;
        [ObservableProperty]
        string _cvlIdNew;
        [ObservableProperty]
        bool _isNotProcessing;

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            private set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
                OnProgressComplete();
            }
        }

        public ChangeCvlIdScriptPageViewModel(IPreferences preferences, IAlertService alertSvc)
        {
            _preferences = preferences;
            _alertSvc = alertSvc;
            _environmentSelect = _preferences.Get(Config.ChangeCvlIdSettings.Environment, "DEV");
            _cvlIdOld = _preferences.Get(Config.ChangeCvlIdSettings.CvlIdOld, string.Empty);
            _cvlIdNew = _preferences.Get(Config.ChangeCvlIdSettings.CvlIdNew, string.Empty);
            _isNotProcessing = true;
        }

        [RelayCommand]
        async static Task Cancel()
        {

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task RunScript()
        {
            ProgressValue = 0d;
            IsNotProcessing = false;

            _preferences.Set(Config.ChangeCvlIdSettings.Environment, EnvironmentSelect);
            _preferences.Set(Config.ChangeCvlIdSettings.CvlIdOld, CvlIdOld);
            _preferences.Set(Config.ChangeCvlIdSettings.CvlIdNew, CvlIdNew);

            if(string.IsNullOrWhiteSpace(CvlIdOld) || string.IsNullOrWhiteSpace(CvlIdNew) || CvlIdNew.Equals(CvlIdOld))
            {
                _alertSvc.ShowAlert("Incorrect data", "CVL data is not correct. Check settings and try again.", "Ok");
                return;
            }

            var progress = new Progress<double>(val => ProgressValue = val);
            var task = Task.Run(async () =>
                await ChangeCvlIdScriptLogic.ChangeCvlId(
                    CvlIdOld,
                    CvlIdNew,
                    _preferences.Get(Consts.Settings.InRiverUrl, string.Empty),
                    Helpers.GetKeyFromPreferences(_preferences, EnvironmentSelect),
                    progress)
            );
            _ = task.ContinueWith(failedTask =>
            {
                IsNotProcessing = true;
                _alertSvc.ShowAlert("Changing CVL Key failed", "Exception occured during CVL Key change. Check settings and try again.", "Ok");
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        private void OnProgressComplete()
        {
            if(ProgressValue == 1)
            {
                IsNotProcessing = true;
            }
        }
    }
}
