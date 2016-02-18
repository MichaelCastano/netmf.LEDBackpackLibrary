using System.Threading;
using netmf.LEDBackpackLibrary;

namespace LED_Demo
{
    public class Program
    {
        public static void Main()
        {
            // Create and Initialize Matrix
            HT16K33_8x8_Matrix myMatrix = new HT16K33_8x8_Matrix();
            myMatrix.Init();

            while (true)
            { 
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        myMatrix.SetPixel(i, j, true);
                        myMatrix.WriteBufferToDisplay();
                        Thread.Sleep(100);
                    }
                }

                myMatrix.ClearBuffer(false);
                myMatrix.WriteBufferToDisplay();
            }
        }
    }
}
