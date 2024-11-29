using Scripts.ViewModels;

namespace Scripts.Pages;

public partial class ChangeCvlKeyScriptPage : ContentPage
{
    public ChangeCvlKeyScriptPage(ChangeCvlKeyScriptPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}