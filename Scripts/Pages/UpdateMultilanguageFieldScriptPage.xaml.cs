using Scripts.ViewModels;

namespace Scripts.Pages;

public partial class UpdateMultilanguageFieldScriptPage : ContentPage
{
    public UpdateMultilanguageFieldScriptPage(UpdateMultilanguageFieldScriptPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}