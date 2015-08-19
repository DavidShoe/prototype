using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TCS34725
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TCS34725 ColorSensor = new TCS34725();
        public MainPage()
        {
            Debug.WriteLine("new MainPage");
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");
            try
            {
                await ColorSensor.Initialize();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void OnLedRadioChanged(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage::OnLedRadioChanged");
            ColorSensor.LedState = (_LedRadioButtonOn.IsChecked == true ? TCS34725.eLedState.On : TCS34725.eLedState.Off);
        }

        private async void OnReadRawClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage::OnReadRawClicked");

            ColorData colorData = await ColorSensor.getRawData();
            Debug.WriteLine("red: {0}, green: {1}, blue: {2}, c: {3}", colorData.Red, colorData.Green, colorData.Blue, colorData.Clear);

            _Clear.Text = colorData.Clear.ToString();
            _Red.Text = colorData.Red.ToString();
            _Green.Text = colorData.Green.ToString();
            _Blue.Text = colorData.Blue.ToString();
        }
    }
}
