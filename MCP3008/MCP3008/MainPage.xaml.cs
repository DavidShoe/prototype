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

namespace MCP3008
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MCP3008 mcp3008 = new MCP3008();
        public MainPage()
        {
            this.InitializeComponent();

            mcp3008.Initialize();
        }

        private async void OnReadClick(object sender, RoutedEventArgs e)
        {
            for (byte adc = 0; adc < 8; adc++)
            {
                for (int i = 0; i < 5; i++)
                {
                    int readVal = await mcp3008.ReadADC(adc);
                    Debug.WriteLine("Read value: " + readVal.ToString());
                    await Task.Delay(100);
                }
            }
        }
    }
}
