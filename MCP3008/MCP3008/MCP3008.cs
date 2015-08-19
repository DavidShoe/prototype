using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MCP3008
{
    class MCP3008
    {
        private SpiDevice mcp3008;
        const int SPI_CHIP_SELECT_LINE = 24;
        public async void Initialize()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 3600000;                              // 3.6MHz is the rated speed of the MCP3008 at 5v
                settings.Mode = SpiMode.Mode3;                                  /* The chip expects an idle-high clock polarity, we use Mode3    
                                                                                * to set the clock polarity and phase to: CPOL = 1, CPHA = 1         
                                                                            */

                string aqs = SpiDevice.GetDeviceSelector();                     /* Get a selector string that will return all SPI controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the SPI bus controller devices with our selector string             */
                mcp3008 = await SpiDevice.FromIdAsync(dis[0].Id, settings);    /* Create an SpiDevice with our bus controller and SPI settings             */
                if (mcp3008 == null)
                {
                    Debug.WriteLine(
                        "SPI Controller {0} is currently in use by " +
                        "another application. Please ensure that no other applications are using SPI.",
                        dis[0].Id);
                    return;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }


        bool Init = false;
        private async Task begin()
        {
            Debug.WriteLine("TCS34725::begin");
            byte[] WriteBuffer = new byte[] { TCS34725_ID | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            // Check the device signature
            mcp3008.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("TCS34725 Signature: " + ReadBuffer[0].ToString());

            if (ReadBuffer[0] != 0x44)
            {
                Debug.WriteLine("TCS34725::begin Signature MISMATCH!!!!!!!!!!!!!!!!!");
                return;
            }

            Init = true;

        }



        public int ReadADC(int whichChannel)
        {

            return 0;
        }
    }
}
