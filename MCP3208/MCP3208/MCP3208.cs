using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MCP3208
{
    class MCP3208
    {
        private SpiDevice mcp3208;
        const int SPI_CHIP_SELECT_LINE = 0;  // SPI0 CS0 pin 24
        const byte MCP3208_SingleEnded = 0x08;
        const byte MCP3208_Differential = 0x00;

        public async void Initialize()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 1000000;                              // 3.6MHz is the rated speed of the MCP3208 at 5v
                settings.Mode = SpiMode.Mode0;                               
                                                                               
                                                                               

                string aqs = SpiDevice.GetDeviceSelector();                     /* Get a selector string that will return all SPI controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the SPI bus controller devices with our selector string             */
                mcp3208 = await SpiDevice.FromIdAsync(dis[0].Id, settings);    /* Create an SpiDevice with our bus controller and SPI settings             */
                if (mcp3208 == null)
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
            Debug.WriteLine("MCP3200::begin");

            await Task.Delay(5);

            Init = true;
        }


        const byte MCP3208_StartBit = 0x10;
        public async Task<int> ReadADC(byte whichChannel)
        {
            // To line everything up for ease of reading back (on byte boundary) we 
            // will pad the command start bit with 7 leading "0" bits

            // Write 0000 0SGD DDxx xxxx xxxx xxxx
            // Read  ???? ???? ???N BA98 7654 3210
            // S = start bit
            // G = Single / Differential
            // D = Chanel data 
            // ? = undefined, ignore
            // N = 0 "Null bit"
            // B-0 = 12 data bits

            if (!Init) await begin();

            byte command1 = whichChannel;
            byte command2 = whichChannel;
            command1 |= MCP3208_SingleEnded;
            command1 |= MCP3208_StartBit;
            command1 >>= 2;
            command2 <<= 6;

            byte[] commandBuf = new byte[] { command1, command2, 0x00, 0x00, 0x00 };

            byte[] readBuf = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };

            mcp3208.TransferFullDuplex(commandBuf, readBuf);

            int sample = readBuf[2] + ((readBuf[1] & 0x0F) << 8);
            int s2 = sample & 0x0FFF;
            Debug.Assert(sample == s2);

            Debug.WriteLine("MCP3208::ReadADC C:{0} {1}", whichChannel, sample.ToString());

            return sample;
        }

    }
}
