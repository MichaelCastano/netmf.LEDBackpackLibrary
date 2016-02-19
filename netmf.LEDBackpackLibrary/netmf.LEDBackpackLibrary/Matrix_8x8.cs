using Microsoft.SPOT.Hardware;

namespace netmf.MC.LEDBackpack.Matrix
{
    /// <summary>
    /// Class to control an 8x8 Matrix driven by an Holtek HT16K33 IC
    /// 
    /// Inspired by the Adafruit-LED-Backpack library.
    /// https://github.com/adafruit/Adafruit-LED-Backpack-Library
    /// </summary>
    public class Matrix_8x8
    {
        // Basic constant adresses
        private static byte HT16K33_OSC_ON = 0x21; // Oscillator ON 
        //private static byte HT16K33_DISPLAY_OFF = 0x80; // Display OFF
        private static byte HT16K33_DISPLAY_ON = 0x81; // Display ON + No Blinking

        //private static byte HT16K33_BLINK_2HZ = 0x83;
        //private static byte HT16K33_BLINK_1HZ = 0x85;
        //aprivate static byte HT16K33_BLINK_HALFHZ = 0x87;

        //private static byte HT16K33_DIM_FULL = 0xEF; // Full Brightness
        //private static byte HT16K33_DIM_LOW = 0xE0; // Lowest Brightness

        private byte HT16K33_ADRESS = 0x70; // 7-bit I2C Adress
        private int HT16K33_CLKRATE = 100; // Clockrate in kHz
        private int Timeout = 1000;

        private I2CDevice Matrix;
        private I2CDevice.Configuration Config;

        private byte[] DisplayBuffer = new byte[8];

        /// <summary>
        /// Basic constructor.
        /// Uses the default Adress 0x70 for the Matrix.
        /// And a standard clockrate of 100 kHz.
        /// </summary>
        public Matrix_8x8()
        {
            // Uses standard configuration
        }

        /// <summary>
        /// Basic constructor.
        /// And a standard clockrate of 100 kHz.
        /// </summary>
        /// <param name="adress">7-bit adress of the Device</param>
        public Matrix_8x8(byte adress)
        {
            HT16K33_ADRESS = adress;
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="adress">7-bit adress of the Device</param>
        /// <param name="clockrate">Clockrate in kHz. Maximum 400</param> 
        public Matrix_8x8(byte adress, int clockrate)
        {
            HT16K33_ADRESS = adress;

            if (clockrate > 0 && clockrate < 400)
                HT16K33_CLKRATE = clockrate;
        }

        /// <summary>
        /// Turns on the Oscillator. Turns on the Display. Turns off Blinking. Sets Brightness to full.
        /// </summary>
        public void Init()
        {
            Config = new I2CDevice.Configuration(HT16K33_ADRESS, HT16K33_CLKRATE);
            Matrix = new I2CDevice(Config);

            byte[] write = new byte[1];
            write[0] = HT16K33_OSC_ON; // IC Oscillator ON

            byte[] write2 = new byte[1];
            write2[0] = HT16K33_DISPLAY_ON; // Display ON

            I2CDevice.I2CTransaction[] i2cTx = new I2CDevice.I2CTransaction[1];
            i2cTx[0] = I2CDevice.CreateWriteTransaction(write);

            I2CDevice.I2CTransaction[] i2cTx2 = new I2CDevice.I2CTransaction[1];
            i2cTx2[0] = I2CDevice.CreateWriteTransaction(write2);

            Matrix.Execute(i2cTx, Timeout);
            Matrix.Execute(i2cTx2, Timeout);

            // initialize DisplayBuffer
            for (int i = 0; i < 8; i++)
            {
                DisplayBuffer[i] = 0x00;
            }
        }

        /// <summary>
        /// Set the Brightness of the LED Matrix
        /// </summary>
        /// <param name="level">Brightness level from 0 (dark) to 15 (bright) </param>
        public void setBrightness(int level)
        {
            if (level < 0)
                level = 0;
            if (level > 15)
                level = 15;

            byte[] write = new byte[1];
            write[0] = (byte)(0xE0 + level);

            I2CDevice.I2CTransaction[] i2cTx = new I2CDevice.I2CTransaction[1];
            i2cTx[0] = I2CDevice.CreateWriteTransaction(write);
            Matrix.Execute(i2cTx, Timeout);
        }

        /// <summary>
        /// Set the Blink rate of the Matrix
        /// </summary>
        /// <param name="rate">0 = off; 1 = 2Hz; 2 = 1Hz; 3 = 0.5Hz</param>
        public void setBlinkRate(int rate)
        {
            if (rate < 0)
                rate = 0;
            if (rate > 3)
                rate = 0;

            byte tmp = 0;

            switch (rate)
            {
                case 0:
                    tmp = 1;
                    break;
                case 1:
                    tmp = 3;
                    break;
                case 2:
                    tmp = 5;
                    break;
                case 3:
                    tmp = 7;
                    break;
                default:
                    break;
            }

            byte[] write = new byte[1];
            write[0] = (byte)(0x80 + tmp);

            I2CDevice.I2CTransaction[] i2cTx = new I2CDevice.I2CTransaction[1];
            i2cTx[0] = I2CDevice.CreateWriteTransaction(write);
            Matrix.Execute(i2cTx, Timeout);
        }

        /// <summary>
        /// Set a specific pixel on or off in the internal Displaybuffer.
        /// </summary>
        /// <param name="x">x-coordinate from 0 to 7</param>
        /// <param name="y">y-coordinate from 0 to 7</param>
        /// <param name="on">set on/off state</param>
        public void SetPixel(int x, int y, bool on)
        {
            if (x > 7 || x < 0 || y > 7 || y < 0)
                return;

            // Shift 1 Pixel left
            x += 7;
            x %= 8;

            if (on)
            {
                DisplayBuffer[y] |= (byte)(1 << x);
            }
            else
            {
                DisplayBuffer[y] &= (byte)~(1 << x);
            }
        }

        /// <summary>
        /// Get the current state of a Pixel in the Displaybuffer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPixelState(int x, int y)
        {
            int state = -1;
            if (x > 7 || x < 0 || y > 7 || y < 0)
                return state;

            byte tmp = (byte)((DisplayBuffer[y]) & (1 << x));

            if (tmp > 0)
                state = 1;
            if (tmp == 0)
                state = 0;

            return state;
        }

        /// <summary>
        /// Set the whole internal Displaybuffer.
        /// </summary>
        /// <param name="Buffer">Bytearray with a lenght on 8. Represents all pixels of the 8x8 matrix</param>
        public void SetMatrix(byte[] Buffer)
        {
            if (Buffer.Length != 8)
                return;

            DisplayBuffer = Buffer;
        }

        /// <summary>
        /// Sets all Pixels On or Off.
        /// </summary>
        /// <param name="AllOff">true = set On; false = set Off</param>
        public void ClearBuffer(bool AllOn)
        {
            byte value = 0x00;

            if (AllOn)
                value = 0xFF;

            for (int i = 0; i < 8; i++)
            {
                DisplayBuffer[i] = value;
            }
        }

        /// <summary>
        /// Writes the current internal Displaybuffer to the Matrix
        /// </summary>
        public void WriteBufferToDisplay()
        {
            byte[] tmp = new byte[17];
            tmp[0] = 0x00;

            for (int i = 0; i < 16; i = i + 2)
            {
                tmp[i + 1] = DisplayBuffer[i / 2];
                tmp[i + 2] = (byte)(DisplayBuffer[i / 2] >> 8);
            }

            I2CDevice.I2CTransaction[] i2cTx = new I2CDevice.I2CTransaction[1];
            i2cTx[0] = I2CDevice.CreateWriteTransaction(tmp);
            Matrix.Execute(i2cTx, Timeout);
        }

        public byte[] GetDisplayBuffer()
        {
            return DisplayBuffer;
        }
    }
}
