using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace PCA9685
{
    class PCA9685
    {
        const byte PCA9685_Address = 0x40;

        const string I2CControllerName = "I2C1";
        private I2cDevice pca9685 = null;

        enum eFlags
        {
            PCA9685_SUBADR1 = 0x2,
            PCA9685_SUBADR2 = 0x3,
            PCA9685_SUBADR3 = 0x4,

            PCA9685_MODE1 = 0x0,
            PCA9685_PRESCALE = 0xFE,

            LED0_ON_L = 0x6,
            LED0_ON_H = 0x7,
            LED0_OFF_L = 0x8,
            LED0_OFF_H = 0x9,

            ALLLED_ON_L = 0xFA,
            ALLLED_ON_H = 0xFB,
            ALLLED_OFF_L = 0xFC,
            ALLLED_OFF_H = 0xFD
        }

        public const UInt16 SERVOMIN = 116;
        public const UInt16 SERVOMAX = 618;
        public const UInt16 SERVOMID = SERVOMIN + (SERVOMAX - SERVOMIN) / 2;


        public PCA9685(int ledControlPin = 12)
        {
            Debug.WriteLine("New PCA9685");
        }

        public async Task Initialize()
        {
            Debug.WriteLine("PCA9685::Initialize");

            try
            {
                var settings = new I2cConnectionSettings(PCA9685_Address);
                settings.BusSpeed = I2cBusSpeed.FastMode;

                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                var dis = await DeviceInformation.FindAllAsync(aqs);
                pca9685 = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                if (pca9685 == null)
                {
                    Debug.WriteLine("PCA9685 failed to initialize");
                }

                await Begin();
                setPWMFreq(60);

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }

        }

        public async Task Reset()
        {
            Debug.WriteLine("PCA9685::Reset");
            if (!Init) await Begin();

            byte[] WriteBuffer = new byte[] { (byte)eFlags.PCA9685_MODE1, 0x00 };
            pca9685.Write(WriteBuffer);

        }

        bool Init = false;
        public bool IsInit { get { return Init; } }
        private async Task Begin()
        {
            Debug.WriteLine("PCA9685::Begin");

            Init = true;

            await Reset();

            // This device doesn't have a signature built in.
        }

        Byte Read8(Byte address)
        {
            byte[] writeBuffer = { address };
            byte[] readBuffer = { 0x00 };
            pca9685.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        void Write8(Byte address, Byte value)
        {
            byte[] writeBuffer = { address, value };
            pca9685.Write(writeBuffer);
        }

        public void setPWMFreq(double freq)
        {
            Debug.WriteLine("PCA9685::setPWMFreq");
            freq *= 0.9;  // Correct for overshoot in the frequency setting (see issue #11).
            double prescaleval = 25000000;
            prescaleval /= 4096;
            prescaleval /= freq;
            prescaleval -= 1;

            Byte prescale = (byte)Math.Floor(prescaleval + 0.5);


            Byte oldmode = Read8((byte)eFlags.PCA9685_MODE1);
            Byte newmode = (byte)((oldmode & 0x7F) | 0x10); // sleep
            Write8((byte)eFlags.PCA9685_MODE1, newmode); // go to sleep
            Write8((byte)eFlags.PCA9685_PRESCALE, prescale); // set the prescaler
            Write8((byte)eFlags.PCA9685_MODE1, oldmode);

            Task.Delay(5);

            Write8((byte)eFlags.PCA9685_MODE1, (byte)(oldmode | 0xa1));
        }

        public async Task Enable()
        {
            Debug.WriteLine("PCA9685::enable");
            if (!Init) await Begin();

            setPWMFreq(60);  // Analog servos run at ~60 Hz updates

            await Task.Delay(3);
        }

        public UInt16 CurOff
        {
            private set;
            get;
        }

        public async void SetPWM(Byte Channel, UInt16 on, UInt16 off)
        {
            Debug.WriteLine(String.Format("PCA9685::SetPWM {0}, on:{1}, off:{2}", Channel, on, off));
            if (!Init) await Begin();
            CurOff = off;

            byte channel = (byte)(eFlags.LED0_ON_L + (4 * Channel));
            byte onHighByte = (byte)(on >> 8);
            byte onLowByte = (byte)(on & 0xFF);
            byte offHighByte = (byte)(off >> 8);
            byte offLowByte = (byte)(off & 0xff);
            byte[] writeBuffer = new byte[] { channel, onLowByte, onHighByte, offLowByte, offHighByte };

            pca9685.Write(writeBuffer);
        }


    }
}
