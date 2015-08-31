using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Lesson_201
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // which GPIO pin do we want to use to control the LED light
        const int GPIOToUse = 27;

        // The class which wraps or LED.
        InternetLed internetLed;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            try
            {
                // Create a new InternetLed object
                internetLed = new InternetLed(GPIOToUse);

                // Initialize it for use
                internetLed.InitalizeLed();

                // Now have it make the web API call and get the led state.
                InternetLed.eLedState ledState = await internetLed.MakeWebApiCall();

                // And finally set the state of the led to that new value.
                internetLed.LedState = ledState;

                // And for fun do that 100 more times so we can see if things change over time.
                for (int i = 0; i < 100; i++)
                {
                    ledState = await internetLed.MakeWebApiCall();
                    internetLed.LedState = ledState;
                    await Task.Delay(100);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }
}
