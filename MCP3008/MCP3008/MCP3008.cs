﻿using Windows.Devices.Enumeration;
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
        const int SPI_CHIP_SELECT_LINE = 0;  // SPI0 CS0 pin 24
        const byte MCP3008_SingleEnded = 0x08;
        const byte MCP3008_Differential = 0x00;

        public async void Initialize()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 3600000;                              // 3.6MHz is the rated speed of the MCP3008 at 5v
                settings.Mode = SpiMode.Mode1;                               
                                                                               
                                                                               

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

            await Task.Delay(5);

            Init = true;
        }

        public async Task<int> ReadADC(byte whichChannel)
        {
            // To line everything up for ease of reading back (on byte boundarys) we 
            // will pad the command start bit with 7 leading "0" bits

            // Write 0000 000S GDDD xxxx xxxx xxxx
            // Read  ???? ???? ???? ?N98 7654 3210
            // S = start bit
            // G = Single / Differentail
            // D = chanel data 
            // ? = undefined, ignore
            // N = 0 "Null bit"
            // 9-0 = 10 data bits

            if (!Init) await begin();

            byte command = whichChannel;
            command |= MCP3008_SingleEnded;
            command <<= 4;

            byte[] commandBuf = new byte[] { 0x01, command, 0x00};

            byte[] readBuf = new byte[] { 0x00, 0x00, 0x00};

            mcp3008.TransferFullDuplex(commandBuf, readBuf);

            int sample = readBuf[2] + ((readBuf[1] & 0x03) << 8);
            int s2 = sample & 0x3FF;
            Debug.Assert(sample == s2);

            Debug.WriteLine("MCP3008::ReadADC C:{0} {1}", whichChannel, sample.ToString());

            return sample;
        }
    }
}
