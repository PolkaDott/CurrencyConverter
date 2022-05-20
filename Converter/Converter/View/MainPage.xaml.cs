using Converter.ViewModel;
using Xamarin.Forms;

namespace Converter
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
            //uglyFrame.Padding = new Thickness(0, 0, 20, 0);
        }
    }
}
