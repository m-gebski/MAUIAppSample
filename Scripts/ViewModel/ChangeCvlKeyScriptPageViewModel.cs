using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scripts.ScriptLogic;
using Services.AlertService;
using Shared;

namespace Scripts.ViewModels
{
    public partial class ChangeCvlKeyScriptPageViewModel : ObservableObject
    {
        private readonly IPreferences _preferences;
        private readonly IAlertService _alertSvc;

        [ObservableProperty]
        string _environmentSelect;
        [ObservableProperty]
        string _cvlId;
        [ObservableProperty]
        string _cvlKeyOld;
        [ObservableProperty]
        string _cvlKeyNew;
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

        public ChangeCvlKeyScriptPageViewModel(IPreferences preferences, IAlertService alertSvc)
        {
            _preferences = preferences;
            _alertSvc = alertSvc;
            _environmentSelect = _preferences.Get(Config.ChangeCvlKeySettings.Environment, "DEV");
            _cvlId = _preferences.Get(Config.ChangeCvlKeySettings.CvlId, string.Empty);
            _cvlKeyOld = _preferences.Get(Config.ChangeCvlKeySettings.CvlKeyOld, string.Empty);
            _cvlKeyNew = _preferences.Get(Config.ChangeCvlKeySettings.CvlKeyNew, string.Empty);
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

            _preferences.Set(Config.ChangeCvlKeySettings.Environment, EnvironmentSelect);
            _preferences.Set(Config.ChangeCvlKeySettings.CvlId, CvlId);
            _preferences.Set(Config.ChangeCvlKeySettings.CvlKeyOld, CvlKeyOld);
            _preferences.Set(Config.ChangeCvlKeySettings.CvlKeyNew, CvlKeyNew);

            if(string.IsNullOrWhiteSpace(CvlId) || string.IsNullOrWhiteSpace(CvlKeyOld) || string.IsNullOrWhiteSpace(CvlKeyNew))
            {
                _alertSvc.ShowAlert("Incorrect data", "CVL data is not correct. Check settings and try again.", "Ok");
                return;
            }

            var progress = new Progress<double>(val => ProgressValue = val);
            var task = Task.Run(async () =>
                await ChangeCvlKeyScriptLogic.ChangeCvlKey(
                    CvlId,
                    CvlKeyOld,
                    CvlKeyNew,
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
