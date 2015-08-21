using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace BME280_I2C
{
    public class BME280_CalibrationData
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

       public byte dig_H1 { get; set; }
       public Int16 dig_H2 { get; set; }
       public byte dig_H3 { get; set; }
       public Int16 dig_H4 { get; set; }
       public Int16 dig_H5 { get; set; }
       public SByte dig_H6 { get; set; }

    }

    public class BME280_Data
    {
        public float Temperature { get; set; }
    }

    class BME280
    {
        const string I2CControllerName = "I2C1";
        const byte BME280_Address = 0x77;
        const byte BME280_Signature = 0x60;

        enum eRegisters : byte
         {
            BME280_REGISTER_DIG_T1 = 0x88,
            BME280_REGISTER_DIG_T2 = 0x8A,
            BME280_REGISTER_DIG_T3 = 0x8C,

            BME280_REGISTER_DIG_P1 = 0x8E,
            BME280_REGISTER_DIG_P2 = 0x90,
            BME280_REGISTER_DIG_P3 = 0x92,
            BME280_REGISTER_DIG_P4 = 0x94,
            BME280_REGISTER_DIG_P5 = 0x96,
            BME280_REGISTER_DIG_P6 = 0x98,
            BME280_REGISTER_DIG_P7 = 0x9A,
            BME280_REGISTER_DIG_P8 = 0x9C,
            BME280_REGISTER_DIG_P9 = 0x9E,

            BME280_REGISTER_DIG_H1 = 0xA1,
            BME280_REGISTER_DIG_H2 = 0xE1,
            BME280_REGISTER_DIG_H3 = 0xE3,
            BME280_REGISTER_DIG_H4 = 0xE4,
            BME280_REGISTER_DIG_H5 = 0xE5,
            BME280_REGISTER_DIG_H6 = 0xE7,

            BME280_REGISTER_CHIPID = 0xD0,
            BME280_REGISTER_VERSION = 0xD1,
            BME280_REGISTER_SOFTRESET = 0xE0,

            BME280_REGISTER_CAL26 = 0xE1,  // R calibration stored in 0xE1-0xF0

            BME280_REGISTER_CONTROLHUMID = 0xF2,
            BME280_REGISTER_CONTROL = 0xF4,
            BME280_REGISTER_CONFIG = 0xF5,

            BME280_REGISTER_PRESSUREDATA_MSB = 0xF7,
            BME280_REGISTER_PRESSUREDATA_LSB = 0xF8,
            BME280_REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

            BME280_REGISTER_TEMPDATA_MSB = 0xFA,
            BME280_REGISTER_TEMPDATA_LSB = 0xFB,
            BME280_REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>

            BME280_REGISTER_HUMIDDATA_MSB = 0xFD,
            BME280_REGISTER_HUMIDDATA_LSB = 0xFE,
        };

        private I2cDevice bme280 = null;

        BME280_CalibrationData CalibrationData;

        public BME280()
        {
            Debug.WriteLine("New BME280");
        }


        public async Task Initialize()
        {
            Debug.WriteLine("BME280::Initialize");

            try
            {
                var settings = new I2cConnectionSettings(BME280_Address);
                settings.BusSpeed = I2cBusSpeed.FastMode;

                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                var dis = await DeviceInformation.FindAllAsync(aqs);
                bme280 = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                if (bme280 == null)
                {
                    Debug.WriteLine("Device not found");
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
            Debug.WriteLine("BME280::Begin");
            byte[] WriteBuffer = new byte[] { (byte) eRegisters.BME280_REGISTER_CHIPID};
            byte[] ReadBuffer = new byte[] { 0xFF };

            // Check the device signature
            bme280.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("BME280 Signature: " + ReadBuffer[0].ToString());

            if (ReadBuffer[0] != BME280_Signature)
            {
                Debug.WriteLine("BME280::begin Signature MISMATCH!!!!!!!!!!!!!!!!!");
                return;
            }

            Init = true;

            // read the coefficients table
            CalibrationData = await ReadCoefficeints();

            // Write control register
            await WriteControlRegister(0x3F);

            // write humidity control register
            await WriteControlRegisterHumidity(0x03);
        }

        private async Task WriteControlRegisterHumidity(int v)
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CONTROLHUMID, 0x03 };

            bme280.Write(WriteBuffer);

            await Task.Delay(1);
            return;
        }

        private async Task WriteControlRegister(int v)
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CONTROL, 0x3F };
            bme280.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        UInt16 ReadUInt16(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bme280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[0] << 8;
            int l = readBuffer[1];
            value = (UInt16)(h + l);
            return value;
        }

        UInt16 ReadUInt16_LittleEndian(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bme280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);
            return value;
        }

        Int16 ReadInt16(byte register)
        {
            Int16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bme280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[0] << 8;
            int l = readBuffer[1];
            value = (Int16)(h + l);
            return value;
        }

        Int16 ReadInt16_LittleEndian(byte register)
        {
            Int16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bme280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (Int16)(h + l);
            return value;
        }

        byte ReadByte(byte register)
        {
            byte value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00 };

            writeBuffer[0] = register;

            bme280.WriteRead(writeBuffer, readBuffer);
            value = readBuffer[0];
            return value;
        }

        SByte ReadSByte(byte register)
        {
            SByte value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00 };

            writeBuffer[0] = register;

            bme280.WriteRead(writeBuffer, readBuffer);
            value = (SByte)readBuffer[0];
            return value;
        }


        private async Task<BME280_CalibrationData> ReadCoefficeints()
        {
            // 16 bit calibration data is stored as Little Endian, the helper method will do the byte swap.
            CalibrationData = new BME280_CalibrationData();

            // Read temperature calibration data
            CalibrationData.dig_T1 = ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_T1);
            CalibrationData.dig_T2 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_T2);
            CalibrationData.dig_T3 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_T3);

            // Read presure calibration data
            CalibrationData.dig_P1 = ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P1);
            CalibrationData.dig_P2 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P2);
            CalibrationData.dig_P3 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P3);
            CalibrationData.dig_P4 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P4);
            CalibrationData.dig_P5 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P5);
            CalibrationData.dig_P6 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P6);
            CalibrationData.dig_P7 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P7);
            CalibrationData.dig_P8 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P8);
            CalibrationData.dig_P9 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P9);

            // Read humidity calibration data
            CalibrationData.dig_H1 = ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H1);
            CalibrationData.dig_H2 = ReadInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_H2);
            CalibrationData.dig_H3 = ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H3);
            CalibrationData.dig_H4 = (Int16)((ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H4) << 4) | (ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H4 + 1) & 0xF));
            CalibrationData.dig_H5 = (Int16)((ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H5 + 1) << 4) | (ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H5) >> 4));
            CalibrationData.dig_H6 = ReadSByte((byte)eRegisters.BME280_REGISTER_DIG_H6);

            //T1: 28376
            //T2: 26237
            //T3: 50
            //P1: 36893
            //P2: -10856
            //P3: 3024
            //P4: 8448
            //P5: 135
            //P6: -7
            //P7: 9900
            //P8: -10230
            //P9: 4285
            //H1: 75
            //H2: 346
            //H3: 0
            //H4: 357
            //H5: 0            //H6: 30

            await Task.Delay(1);
            return CalibrationData;
        }

        // Returns temperature in DegC, resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
        // t_fine carries fine temperature as global value
        Int32 t_fine = Int32.MinValue;
        double BME280_compensate_T_double(Int32 adc_T)
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
        Int64 BME280_compensate_P_Int64(Int32 adc_P)
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
                Debug.WriteLine("BME280_compensate_P_Int64 Jump out to avoid / 0");
                return 0; // avoid exception caused by division by zero
            }
            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((Int64)CalibrationData.dig_P9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((Int64)CalibrationData.dig_P8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((Int64)CalibrationData.dig_P7 << 4);
            return p;
        }


        // Returns humidity in %RH as unsigned 32 bit integer in Q22.10 format (22 integer and 10 fractional bits).
        // Output value of “47445” represents 47445/1024 = 46.333 %RH
        UInt32 bme280_compensate_H_int32(Int32 adc_H)
        {
            Int32 v_x1_u32r;
            v_x1_u32r = (t_fine - ((Int32)76800));
            v_x1_u32r = (((((adc_H << 14) - (((Int32)CalibrationData.dig_H4) << 20) - (((Int32)CalibrationData.dig_H5) * v_x1_u32r)) +
            ((Int32)16384)) >> 15) *(((((((v_x1_u32r * ((Int32)CalibrationData.dig_H6)) >> 10) * (((v_x1_u32r *
                ((Int32)CalibrationData.dig_H3)) >> 11) + ((Int32)32768))) >> 10) + ((Int32)2097152)) *
            ((Int32)CalibrationData.dig_H2) + 8192) >> 14));
            v_x1_u32r = (v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * ((Int32)CalibrationData.dig_H1)) >> 4));
            v_x1_u32r = (v_x1_u32r < 0 ? 0 : v_x1_u32r);
            v_x1_u32r = (v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r);
            return (UInt32)(v_x1_u32r >> 12);
        }

        public async Task<float> ReadTemperature()
        {
            if (!Init) await Begin();

            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_XLSB); // bits 7:4
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);
            double foo = BME280_compensate_T_double(t);

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

            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_XLSB); // bits 7:4
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);
            Int64 foo = BME280_compensate_P_Int64(t);

            return ((float)foo) / 256;
        }

        public async Task<Int32> ReadHumidity()
        {
            if (!Init) await Begin();

            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_HUMIDDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_HUMIDDATA_LSB);
            Int32 t = (tmsb << 8) + tlsb;
            UInt32 foo = bme280_compensate_H_int32(t);

            return (Int32)foo;
        }


        public async Task<BME280_Data> Read()
        {
            if (!Init) await Begin();

            BME280_Data data = new BME280_Data();

            return data;
        }
    }
}
