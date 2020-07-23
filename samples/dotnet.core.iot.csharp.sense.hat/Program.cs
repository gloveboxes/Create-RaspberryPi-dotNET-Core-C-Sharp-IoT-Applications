using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Iot.Device.CpuTemperature;
using Iot.Device.SenseHat;

namespace dotnet.core.iot
{   
    class Program
    {
        static Color[] pixels = new Color[]{
            Color.FromArgb(255, 0, 0), Color.FromArgb(255, 0, 0), Color.FromArgb(255, 87, 0), Color.FromArgb(255, 196, 0), Color.FromArgb(205, 255, 0), Color.FromArgb(95, 255, 0), Color.FromArgb(0, 255, 13), Color.FromArgb(0, 255, 122),
            Color.FromArgb(255, 0, 0), Color.FromArgb(255, 96, 0), Color.FromArgb(255, 205, 0), Color.FromArgb(196, 255, 0), Color.FromArgb(87, 255, 0), Color.FromArgb(0, 255, 22), Color.FromArgb(0, 255, 131), Color.FromArgb(0, 255, 240),
            Color.FromArgb(255, 105, 0), Color.FromArgb(255, 214, 0), Color.FromArgb(187, 255, 0), Color.FromArgb(78, 255, 0), Color.FromArgb(0, 255, 30), Color.FromArgb(0, 255, 140), Color.FromArgb(0, 255, 248), Color.FromArgb(0, 152, 255),
            Color.FromArgb(255, 223, 0), Color.FromArgb(178, 255, 0), Color.FromArgb(70, 255, 0), Color.FromArgb(0, 255, 40), Color.FromArgb(0, 255, 148), Color.FromArgb(0, 253, 255), Color.FromArgb(0, 144, 255), Color.FromArgb(0, 34, 255),
            Color.FromArgb(170, 255, 0), Color.FromArgb(61, 255, 0), Color.FromArgb(0, 255, 48), Color.FromArgb(0, 255, 157), Color.FromArgb(0, 243, 255), Color.FromArgb(0, 134, 255), Color.FromArgb(0, 26, 255), Color.FromArgb(83, 0, 255),
            Color.FromArgb(52, 255, 0), Color.FromArgb(0, 255, 57), Color.FromArgb(0, 255, 166), Color.FromArgb(0, 235, 255), Color.FromArgb(0, 126, 255), Color.FromArgb(0, 17, 255), Color.FromArgb(92, 0, 255), Color.FromArgb(201, 0, 255),
            Color.FromArgb(0, 255, 66), Color.FromArgb(0, 255, 174), Color.FromArgb(0, 226, 255), Color.FromArgb(0, 117, 255), Color.FromArgb(0, 8, 255), Color.FromArgb(100, 0, 255), Color.FromArgb(210, 0, 255), Color.FromArgb(255, 0, 192),
            Color.FromArgb(0, 255, 183), Color.FromArgb(0, 217, 255), Color.FromArgb(0, 109, 255), Color.FromArgb(0, 0, 255), Color.FromArgb(110, 0, 255), Color.FromArgb(218, 0, 255), Color.FromArgb(255, 0, 183), Color.FromArgb(255, 0, 74)
        };

        static CpuTemperature temperature = new CpuTemperature();

        static SenseHatPressureAndTemperature pt = new SenseHatPressureAndTemperature();

        static Color[] colours = new Color[] { Color.Red, Color.BlueViolet, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Purple, Color.Pink, Color.White, Color.Black };
        static Color currentColour;

        static Thread thread1 = new Thread(Rainbow);
        static void Main(string[] args)
        {
            thread1.Start();

            while (true)
            {
                if (temperature.IsAvailable)
                {
                    Console.WriteLine($"The CPU temperature is {Math.Round(temperature.Temperature.Celsius, 2)}, Room temperature is {Math.Round(pt.Temperature.Celsius, 2)}");
                }
                Thread.Sleep(2000); // sleep for 2000 milliseconds, 2 seconds
            }
        }

        public static void sparkle()
        {
            SenseHatLedMatrixI2c ledMatrix = new SenseHatLedMatrixI2c();
            ledMatrix.Fill(Color.Purple);
            var rand = new Random();

            while (true)
            {
                var c = Color.FromArgb((byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
                var x = rand.Next(0, 8);
                var y = rand.Next(0, 8);
                ledMatrix.SetPixel(x, y, c);
                Thread.Sleep(50);
            }
        }

        // https://github.com/pimoroni/unicorn-hat/blob/master/examples/rainbow.py
        static void Rainbow()
        {
            const double intensity = 0.4;
            const int width = 8;
            const int height = 8;
            Color[] data = new Color[width * height];
            double i = 0.0;
            double offset = 30;            

            SenseHatLedMatrixI2c ledMatrix = new SenseHatLedMatrixI2c();

            while (true)
            {
                i = i + 0.3;
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < height; x++)
                    {
                        var r = (Math.Cos((x + i) / 2.0) + Math.Cos((y + i) / 2.0)) * 96.0 + 32.0;
                        var g = (Math.Sin((x + i) / 1.5) + Math.Sin((y + i) / 2.0)) * 96.0 + 32.0;
                        var b = (Math.Sin((x + i) / 2.0) + Math.Cos((y + i) / 1.5)) * 96.0 + 32.0;

                        r = Math.Max(0, Math.Min(255, r + offset));
                        g = Math.Max(0, Math.Min(255, g + offset));
                        b = Math.Max(0, Math.Min(255, b + offset));

                        data[x + (y * 8)] = Color.FromArgb((byte)r, (byte)g, (byte)b);
                    }
                }
                ledMatrix.Write(data);
            }
        }

        public static void DoWork()
        {
            int cycle = 0;
            SenseHatLedMatrixI2c ledMatrix = new SenseHatLedMatrixI2c();

            Color[] data = new Color[64];

            while (true)
            {
                cycle++;
                for (int y = 0; y < 8; y++)
                {
                    currentColour = colours[(y + cycle) % 8];
                    for (int x = 0; x < 8; x++)
                    {
                        data[x + (y * 8)] = currentColour;
                    }
                }
                ledMatrix.Write(data);
                Thread.Sleep(100);
            }
        }

        static void nextColor(ref Color pix)
        {
            byte r = pix.R;
            byte g = pix.G;
            byte b = pix.B;

            if (r == 255 && g < 255 && b == 0)
            {
                g += 1;
            }

            if (g == 255 && r > 0 && b == 0)
            {
                r -= 1;
            }

            if (g == 255 && b < 255 && r == 0)
            {
                b += 1;
            }

            if (b == 255 && g > 0 && r == 0)
            {
                g -= 1;
            }

            if (b == 255 && r < 255 && g == 0)
            {
                r += 1;
            }

            if (r == 255 && b > 0 && g == 0)
            {
                b -= 1;
            }

            pix = Color.FromArgb(r, g, b);

        }

        public static void Rainbars()
        {
            using (var ledMatrix = new SenseHatLedMatrixI2c())
            {
                while (true)
                {
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        nextColor(ref pixels[i]);
                    }
                    ledMatrix.Write(pixels);
                }
            }
        }

        public static void matrix()
        {
            using (var magnetometer = new SenseHatMagnetometer())
            using (var ledMatrix = new SenseHatLedMatrixI2c())
            {
                Console.WriteLine("Move SenseHat around in every direction until dot on the LED matrix stabilizes when not moving.");
                ledMatrix.Fill();
                Stopwatch sw = Stopwatch.StartNew();
                Vector3 min = magnetometer.MagneticInduction;
                Vector3 max = magnetometer.MagneticInduction;
                while (min == max)
                {
                    Vector3 sample = magnetometer.MagneticInduction;
                    min = Vector3.Min(min, sample);
                    max = Vector3.Max(max, sample);
                    Thread.Sleep(50);
                }

                const int intervals = 8;
                Color[] data = new Color[64];

                while (true)
                {
                    Vector3 sample = magnetometer.MagneticInduction;
                    min = Vector3.Min(min, sample);
                    max = Vector3.Max(max, sample);
                    Vector3 size = max - min;
                    Vector3 pos = Vector3.Divide(Vector3.Multiply((sample - min), intervals - 1), size);
                    int x = Math.Clamp((int)pos.X, 0, intervals - 1);

                    // reverse y to match magnetometer coordinate system
                    int y = intervals - 1 - Math.Clamp((int)pos.Y, 0, intervals - 1);
                    int idx = SenseHatLedMatrix.PositionToIndex(x, y);

                    // fading
                    for (int i = 0; i < 64; i++)
                    {
                        data[i] = Color.FromArgb((byte)Math.Clamp(data[i].R - 1, 0, 255), data[i].G, data[i].B); ;
                    }

                    Color col = data[idx];
                    col = Color.FromArgb(Math.Clamp(col.R + 20, 0, 255), col.G, col.B);
                    Vector2 pos2 = new Vector2(sample.X, sample.Y);
                    Vector2 center2 = Vector2.Multiply(new Vector2(min.X + max.X, min.Y + max.Y), 0.5f);
                    float max2 = Math.Max(size.X, size.Y);
                    float distFromCenter = (pos2 - center2).Length();

                    data[idx] = Color.FromArgb(0, 255, (byte)Math.Clamp(255 * distFromCenter / max2, 0, 255));

                    ledMatrix.Write(data);
                    data[idx] = col;

                    Thread.Sleep(50);
                }
            }
        }
    }
}