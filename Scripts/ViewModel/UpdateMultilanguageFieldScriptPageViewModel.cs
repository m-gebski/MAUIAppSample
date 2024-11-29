using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scripts.ScriptLogic;
using Services.AlertService;
using Shared;
using Shared.RestModel.Model;
using System.Collections.ObjectModel;

namespace Scripts.ViewModels
{
    public partial class UpdateMultilanguageFieldScriptPageViewModel : ObservableObject
    {
        private readonly IPreferences _preferences;
        private readonly IAlertService _alertSvc;

        [ObservableProperty]
        string _environmentSelect;
        [ObservableProperty]
        ObservableCollection<string> _allEntityTypeIds;
        [ObservableProperty]
        ObservableCollection<FieldTypeModelV2> _entityFieldTypeIds;
        [ObservableProperty]
        string _selectedEntityTypeId;
        [ObservableProperty]
        FieldTypeModelV2 _selectedFieldType;
        [ObservableProperty]
        string _newFieldDescription;
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

        public UpdateMultilanguageFieldScriptPageViewModel(IPreferences preferences, IAlertService alertSvc)
        {
            _preferences = preferences;
            _alertSvc = alertSvc;
            _environmentSelect = _preferences.Get(Config.UpdateMultilanguageFieldSettings.Environment, "DEV");
            _selectedEntityTypeId = string.Empty;
            _selectedFieldType = null;
            _newFieldDescription = string.Empty;
            _isNotProcessing = true;
            _entityFieldTypeIds = [];
            _allEntityTypeIds = [];

            Task.Run(async()=> AllEntityTypeIds = new ObservableCollection<string>(await UpdateMultilanguageFieldScriptLogic.GetEntityTypesAsync(_preferences.Get(Consts.Settings.InRiverUrl, string.Empty), Helpers.GetKeyFromPreferences(_preferences, EnvironmentSelect))));
        }

        [RelayCommand]
        async Task EntityTypePickerSelectedIndexChanged(int selectedIndex)
        {
            if (selectedIndex != -1)
            {
                EntityFieldTypeIds = new ObservableCollection<FieldTypeModelV2>(
                                            await UpdateMultilanguageFieldScriptLogic.GetFieldTypesAsync(
                                                AllEntityTypeIds[selectedIndex],
                                                _preferences.Get(Consts.Settings.InRiverUrl, string.Empty),
                                                Helpers.GetKeyFromPreferences(_preferences, EnvironmentSelect)));
            }
        }

        [RelayCommand]
        async Task EnvironmentRadioButtonCheckedChanged(CheckedChangedEventArgs e)
        {
            if (e != null && e.Value)
            {
                EntityFieldTypeIds = [];
                SelectedFieldType = null;
                AllEntityTypeIds = new ObservableCollection<string>(
                                            await UpdateMultilanguageFieldScriptLogic.GetEntityTypesAsync(
                                                    _preferences.Get(Consts.Settings.InRiverUrl, string.Empty),
                                                    Helpers.GetKeyFromPreferences(_preferences, EnvironmentSelect)));
            }
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

            _preferences.Set(Config.UpdateMultilanguageFieldSettings.Environment, EnvironmentSelect);

            if (string.IsNullOrWhiteSpace(SelectedEntityTypeId) || SelectedFieldType == null)
            {
                _alertSvc.ShowAlert("Incorrect data", "Some fields are not filled correctly. Check settings and try again.", "Ok");
                return;
            }

            var progress = new Progress<double>(val => ProgressValue = val);
            var task = Task.Run(async () =>
                await UpdateMultilanguageFieldScriptLogic.UpdateMultilanguageField(
                    SelectedEntityTypeId,
                    SelectedFieldType,
                    NewFieldDescription,
                    _preferences.Get(Consts.Settings.InRiverUrl, string.Empty),
                    Helpers.GetKeyFromPreferences(_preferences, EnvironmentSelect),
                    progress)
            );
            _ = task.ContinueWith(failedTask =>
            {
                IsNotProcessing = true;
                _alertSvc.ShowAlert("Updating field Description failed", "Exception occured during FieldType change. Check settings and try again.", "Ok");
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
