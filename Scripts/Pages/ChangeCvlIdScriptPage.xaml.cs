using Scripts.ViewModels;

namespace Scripts.Pages;

public partial class ChangeCvlIdScriptPage : ContentPage
{
    public ChangeCvlIdScriptPage(ChangeCvlIdScriptPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}