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

namespace Lesson_202
{
    /// <summary>
    /// The application main page.  Because we are running headless we will not see anything
    /// even though it is begin generated at runtime.  This acts as the main entry point for the 
    /// application functionality.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // The class which wraps our ADC chip.
        MCP3008 mcp3008;

        // The voltage we are using as the ADC reference voltage
        const float ADCReferenceVoltage = 3.3F;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // This method will be called by the application framework when the page is first loaded.
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            await LessonWebAPI.MakeLessonWebApiCall();

            try
            {
                // Create a new InternetLed object
                mcp3008 = new MCP3008(ADCReferenceVoltage);

                // Initialize it for use
                mcp3008.Initialize();

                byte whichChannel = 0;

                for (int i = 0; i < 10; i++)
                {
                    // Read the first adc channel, this will get back an ADC value between MCP3008.MIN (0) and MCP3008.MAX (1023)
                    int adcValue = await mcp3008.ReadADC(whichChannel);

                    // The convert that to a voltage
                    float voltage = mcp3008.ADCToVoltage(adcValue);

                    Debug.WriteLine(String.Format("Channel: {0}, ADC Value: {1}, Voltage: {2}", whichChannel, adcValue, voltage));

                    // Wait 1 second between loops.
                    await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }
}
