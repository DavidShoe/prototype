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

namespace BME280_SPI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BME280 bme280 = new BME280();

        public MainPage()
        {
            this.InitializeComponent();
        }

        BME280_Data data;
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");
            try
            {
                await bme280.Initialize();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            data = await bme280.Read();

            OnReadButtonClick(null, null);
        }

        private async void OnReadButtonClick(object sender, RoutedEventArgs e)
        {
            float temp = 0;
            float preasure = 0;
            Int32 humidity = 0;
            for (int i = 0; i < 10; i++)
            {
                temp = await bme280.ReadTemperature();
                Debug.WriteLine("temperature: " + temp.ToString());

                preasure = await bme280.ReadPreasure();
                Debug.WriteLine("Preasure: " + preasure.ToString());

                humidity = await bme280.ReadHumidity();
                Debug.WriteLine("Humidity: " + humidity.ToString());

                await Task.Delay(100);
            }

        }
    }
}
