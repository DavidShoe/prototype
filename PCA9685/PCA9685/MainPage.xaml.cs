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

namespace PCA9685
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PCA9685 pca9685 = new PCA9685();
        public MainPage()
        {
            this.InitializeComponent();
            _ServoSlider.Minimum = PCA9685.SERVOMIN;
            _ServoSlider.Maximum = PCA9685.SERVOMAX;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("OnNavigatedTo");
            await pca9685.Initialize();

            byte servonum = 15;

            for (UInt16 pulselen = PCA9685.SERVOMIN; pulselen < PCA9685.SERVOMAX; pulselen+=10)
            {
                pca9685.SetPWM(servonum, 0, pulselen);
                _CurVal.Text = pca9685.CurOff.ToString();
            }
            await Task.Delay(500);

            for (UInt16 pulselen = PCA9685.SERVOMAX; pulselen > PCA9685.SERVOMIN; pulselen-=10)
            {
                pca9685.SetPWM(servonum, 0, pulselen);
                _CurVal.Text = pca9685.CurOff.ToString();
            }

            await Task.Delay(500);

            pca9685.SetPWM(servonum, 0, PCA9685.SERVOMID);
            _CurVal.Text = pca9685.CurOff.ToString();

        }

        byte servonum = 15;
        private void OnSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Debug.WriteLine("OnSliderChanged");
            if ((pca9685 != null) && (pca9685.IsInit))
            {
                pca9685.SetPWM(servonum, 0, (ushort)e.NewValue);
            }
        }

        private void OnCCWClick(object sender, RoutedEventArgs e)
        {
            pca9685.SetPWM(servonum, 0, (UInt16)(pca9685.CurOff + 1));
            _CurVal.Text = pca9685.CurOff.ToString();
        }

        private void OnCWClick(object sender, RoutedEventArgs e)
        {
            pca9685.SetPWM(servonum, 0, (UInt16)(pca9685.CurOff - 1));
            _CurVal.Text = pca9685.CurOff.ToString();
        }
    }
}
