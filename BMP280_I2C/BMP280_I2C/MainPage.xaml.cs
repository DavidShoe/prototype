using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace BMP280_I2C
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BMP280 BMP280 = new BMP280();
        public MainPage()
        {
            this.InitializeComponent();
        }

        BMP280_Data data;
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");
            try
            {
                await BMP280.Initialize();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            data = await BMP280.Read();

            OnReadButtonClick(null, null);
        }

        private async void OnReadButtonClick(object sender, RoutedEventArgs e)
        {
            float temp = 0;
            float preasure = 0;
            for (int i = 0; i < 10; i++)
            {
                temp = await BMP280.ReadTemperature();
                Debug.WriteLine("Temperature: " + temp.ToString());

                preasure = await BMP280.ReadPreasure();
                Debug.WriteLine("Pressure: " + preasure.ToString());

                await Task.Delay(100);
            }

        }
    }
}
