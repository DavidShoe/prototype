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

namespace MCP3208
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MCP3208 mcp3008 = new MCP3208();
        public MainPage()
        {
            this.InitializeComponent();

            mcp3008.Initialize();
        }


        const float SourceVoltage = 3.3F;

        const float MaxADCValue = 0xFFF; // 12 bit resolution

        // The part has a .5v floor.  So any value is going to be offset by this much.
        const float VoltageOffset = 0.5F;
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            byte adc = 0;
            for (int i = 0; i < 50; i++)
            {
                int readVal = await mcp3008.ReadADC(adc);

                // The raw voltage read
                float voltage = readVal * SourceVoltage;

                voltage /= MaxADCValue;

                // Remove the TMP36 .5v offset
                voltage -= VoltageOffset;


                Debug.WriteLine("Read value: " + readVal.ToString());
                var temperatureInC = voltage * 100;
                var temperatureInF = temperatureInC * 9 / 5 + 32;
                Debug.WriteLine(string.Format("V: {0} C: {1} F: {2}", voltage + VoltageOffset, temperatureInC, temperatureInF));
                await Task.Delay(100);
            }
        }

        private void OnReadClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
