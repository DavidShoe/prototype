using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace BMP280_SPI
{
    public class BMP280_CalibrationData
    {
        public UInt16 dig_T1 { get; set; }
        public Int16 dig_T2 { get; set; }
        public Int16 dig_T3 { get; set; }

        public UInt16 dig_P1 { get; set; }
        public Int16 dig_P2 { get; set; }
        public Int16 dig_P3 { get; set; }
        public Int16 dig_P4 { get; set; }
        public Int16 dig_P5 { get; set; }
        public Int16 dig_P6 { get; set; }
        public Int16 dig_P7 { get; set; }
        public Int16 dig_P8 { get; set; }
        public Int16 dig_P9 { get; set; }

    }

    public class BMP280_Data
    {
        public float Temperature { get; set; }
    }

    class BMP280
    {
        private SpiDevice bmp280;
        const int SPI_CHIP_SELECT_LINE = 0; // SPI0 CS0 pin 24
        enum eRegisters : byte
        {
            BMP280_REGISTER_DIG_T1 = 0x88,
            BMP280_REGISTER_DIG_T2 = 0x8A,
            BMP280_REGISTER_DIG_T3 = 0x8C,

            BMP280_REGISTER_DIG_P1 = 0x8E,
            BMP280_REGISTER_DIG_P2 = 0x90,
            BMP280_REGISTER_DIG_P3 = 0x92,
            BMP280_REGISTER_DIG_P4 = 0x94,
            BMP280_REGISTER_DIG_P5 = 0x96,
            BMP280_REGISTER_DIG_P6 = 0x98,
            BMP280_REGISTER_DIG_P7 = 0x9A,
            BMP280_REGISTER_DIG_P8 = 0x9C,
            BMP280_REGISTER_DIG_P9 = 0x9E,

            BMP280_REGISTER_CHIPID = 0xD0,
            BMP280_REGISTER_VERSION = 0xD1,
            BMP280_REGISTER_SOFTRESET = 0xE0,

            BMP280_REGISTER_CAL26 = 0xE1,  // R calibration stored in 0xE1-0xF0

            BMP280_REGISTER_CONTROLHUMID = 0x72, // 0xF2
            BMP280_REGISTER_CONTROL = 0x74, // 0xF4
            BMP280_REGISTER_CONFIG = 0x75, // 0xF5

            BMP280_REGISTER_PRESSUREDATA_MSB = 0xF7,
            BMP280_REGISTER_PRESSUREDATA_LSB = 0xF8,
            BMP280_REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

            BMP280_REGISTER_TEMPDATA_MSB = 0xFA,
            BMP280_REGISTER_TEMPDATA_LSB = 0xFB,
            BMP280_REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>

            BMP280_REGISTER_HUMIDDATA_MSB = 0xFD,
            BMP280_REGISTER_HUMIDDATA_LSB = 0xFE,
        };

        BMP280_CalibrationData CalibrationData;

        public BMP280()
        {
            Debug.WriteLine("New BMP280");
        }

        public async Task Initialize()
        {
            Debug.WriteLine("BMP280::Initialize");

            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 5000000;                              // 10MHz is the max speed of the chip
                settings.Mode = SpiMode.Mode0;                                  // TODO: Figure out why Mode1 doesn't work.

                string aqs = SpiDevice.GetDeviceSelector();                     /* Get a selector string that will return all SPI controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the SPI bus controller devices with our selector string             */
                bmp280 = await SpiDevice.FromIdAsync(dis[0].Id, settings);    /* Create an SpiDevice with our bus controller and SPI settings             */
                if (bmp280 == null)
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
        private async Task Begin()
        {
            Debug.WriteLine("bmp280::begin");

            Init = true;

            // read the coefficients table
            CalibrationData = await ReadCoefficeints();

            // Write control register
            await WriteControlRegister(0x3F);
        }


        private async Task WriteControlRegister(int v)
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CONTROL, 0x3F };
            bmp280.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        private async Task<BMP280_CalibrationData> ReadCoefficeints()
        {
            // 16 bit calibration data is stored as Little Endian, the helper method will do the byte swap.
            CalibrationData = new BMP280_CalibrationData();

            // Read temperature calibration data
            CalibrationData.dig_T1 = ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T1);
            CalibrationData.dig_T2 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T2);
            CalibrationData.dig_T3 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T3);

            // Read presure calibration data
            CalibrationData.dig_P1 = ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P1);
            CalibrationData.dig_P2 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P2);
            CalibrationData.dig_P3 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P3);
            CalibrationData.dig_P4 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P4);
            CalibrationData.dig_P5 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P5);
            CalibrationData.dig_P6 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P6);
            CalibrationData.dig_P7 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P7);
            CalibrationData.dig_P8 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P8);
            CalibrationData.dig_P9 = ReadInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P9);

            await Task.Delay(1);
            return CalibrationData;
        }


        UInt16 ReadUInt16(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00, 0x00, 0x00 }; // Byte 2 & 3 are don't care
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00 };  // will throw away the first byte

            writeBuffer[0] = register;

            bmp280.TransferFullDuplex(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[2];
            value = (UInt16)(h + l);
            return value;
        }

        const byte SPI_CSB_Start = 0x00;
        const byte SPI_CSB_Stop = 0x10;

        UInt16 ReadUInt16_LittleEndian(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00, 0x00, 0x00 }; // Byte 2 & 3 are don't care
            byte[] readBuffer = new byte[]  { 0x00, 0x00, 0x00 };  // will throw away the first byte

            writeBuffer[0] = (byte)(register | 0x80);

            bmp280.TransferFullDuplex(writeBuffer, readBuffer);
            int h = readBuffer[2] << 8;
            int l = readBuffer[1];
            value = (UInt16)(h + l);
            return value;
        }

        Int16 ReadInt16(byte register)
        {
            Int16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00, 0x00, 0x00 }; // Byte 2 & 3 are don't care
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00 };  // will throw away the first byte

            writeBuffer[0] = (byte)(register | 0x80);

            bmp280.TransferFullDuplex(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[2];
            value = (Int16)(h + l);
            return value;
        }

        Int16 ReadInt16_LittleEndian(byte register)
        {
            Int16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00, 0x00, 0x00 }; // Byte 2 & 3 are don't care
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00 };  // will throw away the first byte

            writeBuffer[0] = (byte)(register | 0x80);

            bmp280.TransferFullDuplex(writeBuffer, readBuffer);
            int h = readBuffer[2] << 8;
            int l = readBuffer[1];
            value = (Int16)(h + l);
            return value;
        }

        byte ReadByte(byte register)
        {
            byte value = 0;
            byte[] writeBuffer = new byte[] { 0x00, 0x00 };// Byte 2 don't care
            byte[] readBuffer = new byte[] { 0x00, 0x00 }; // will throw away the first byte

            writeBuffer[0] = (byte)(register | 0x80);

            bmp280.TransferFullDuplex(writeBuffer, readBuffer);
            value = readBuffer[1];
            return value;
        }

        SByte ReadSByte(byte register)
        {
            SByte value = 0;
            byte[] writeBuffer = new byte[] { 0x00, 0x00 };// Byte 2 don't care
            byte[] readBuffer = new byte[] { 0x00, 0x00 }; // will throw away the first byte

            writeBuffer[0] = (byte)(register | 0x80);

            bmp280.TransferFullDuplex(writeBuffer, readBuffer);
            value = (SByte)readBuffer[1];
            return value;

        }
        // Returns temperature in DegC, resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
        // t_fine carries fine temperature as global value
        Int32 t_fine = Int32.MinValue;
        double BMP280_compensate_T_double(Int32 adc_T)
        {
            double var1, var2, T;
            var1 = ((adc_T / 16384.0) - (CalibrationData.dig_T1 / 1024.0)) * CalibrationData.dig_T2;
            var2 = ((adc_T / 131072.0) - (CalibrationData.dig_T1 / 8192.0)) * CalibrationData.dig_T3;

            t_fine = (Int32)(var1 + var2);

            T = (var1 + var2) / 5120.0;
            return T;
        }


        // Returns pressure in Pa as unsigned 32 bit integer in Q24.8 format (24 integer bits and 8 fractional bits).
        // Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        Int64 BMP280_compensate_P_Int64(Int32 adc_P)
        {
            Int64 var1, var2, p;
            var1 = t_fine - 128000;
            var2 = var1 * var1 * (Int64)CalibrationData.dig_P6;
            var2 = var2 + ((var1 * (Int64)CalibrationData.dig_P5) << 17);
            var2 = var2 + ((Int64)CalibrationData.dig_P4 << 35);
            var1 = ((var1 * var1 * (Int64)CalibrationData.dig_P3) >> 8) + ((var1 * (Int64)CalibrationData.dig_P2) << 12);
            var1 = (((((Int64)1 << 47) + var1)) * (Int64)CalibrationData.dig_P1) >> 33;
            if (var1 == 0)
            {
                Debug.WriteLine("BMP280_compensate_P_Int64 Jump out to avoid / 0");
                return 0; // avoid exception caused by division by zero
            }
            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((Int64)CalibrationData.dig_P9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((Int64)CalibrationData.dig_P8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((Int64)CalibrationData.dig_P7 << 4);
            return p;
        }



        public async Task<float> ReadTemperature()
        {
            if (!Init) await Begin();

            byte tmsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_XLSB); // bits 7:4
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);
            double foo = BMP280_compensate_T_double(t);

            return (float)foo;
        }

        public async Task<float> ReadPreasure()
        {
            if (!Init) await Begin();

            if (t_fine == Int32.MinValue)
            {
                // need the temperature first to load the t_fine value
                await ReadTemperature();
            }

            byte tmsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_XLSB); // bits 7:4
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);
            Int64 foo = BMP280_compensate_P_Int64(t);

            return ((float)foo) / 256;
        }

        public async Task<BMP280_Data> Read()
        {
            if (!Init) await Begin();

            BMP280_Data data = new BMP280_Data();

            return data;
        }
    }
}