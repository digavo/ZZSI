﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Threading; 

namespace ZZ
{
    class IMGFormat
    {
        public Bitmap Img;
        public Bitmap KoncowyImg1; // obraz wynikowy 1
        public Bitmap KoncowyImg2; // obraz wynikowy 2
        //public Bitmap KoncowyImg3; // obraz wynikowy 3
        //public Bitmap KoncowyImg4; // obraz wynikowy 4
        public void CzytajObraz(Stream str)
        {
            Img = new Bitmap(str);
        }
        public Bitmap ZnajdzKolor () 
        {
            Bitmap newImg = new Bitmap(Img);
            Color c;
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    c = newImg.GetPixel(i, j);
                    float hue = c.GetHue();
                    if (hue<290 && hue>170) //niebieski 270-170
                    {
                        int pixelR = 255;
                        int pixelG = 255;
                        int pixelB = 255;
                        newImg.SetPixel(i, j, Color.FromArgb((byte)pixelR,(byte)pixelG, (byte)pixelB));
                    }
                }
            }
            return newImg;
        }
        public Bitmap Filtr()
        {
            Bitmap newImg = new Bitmap(Img);
            Color c;
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    c = newImg.GetPixel(i, j);
                    int pixelR = c.R;
                    int pixelG = c.G - 127;
                    int pixelB = c.B - 255;
                    pixelR = Math.Max(pixelR, 0);
                    pixelR = Math.Min(255, pixelR);
                    pixelG = Math.Max(pixelG, 0);
                    pixelG = Math.Min(255, pixelG);
                    pixelB = Math.Max(pixelB, 0);
                    pixelB = Math.Min(255, pixelB);

                    newImg.SetPixel(i, j, Color.FromArgb((byte)pixelR, (byte)pixelG, (byte)pixelB));
                }
            }
            return newImg;
        }
        public Bitmap Kontrast () 
        {
            double contrast = 50; //-100 - 100
            Bitmap newImg = new Bitmap(Img);
            if (contrast < -100) contrast = -100;
            if (contrast > 100) contrast = 100;
            contrast = (100.0 + contrast) / 100.0;
            contrast *= contrast;
            Color c;
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    c = newImg.GetPixel(i, j);
                    double pR = c.R / 255.0;
                    pR -= 0.5;
                    pR *= contrast;
                    pR += 0.5;
                    pR *= 255;
                    if (pR < 0) pR = 0;
                    if (pR > 255) pR = 255;

                    double pG = c.G / 255.0;
                    pG -= 0.5;
                    pG *= contrast;
                    pG += 0.5;
                    pG *= 255;
                    if (pG < 0) pG = 0;
                    if (pG > 255) pG = 255;

                    double pB = c.B / 255.0;
                    pB -= 0.5;
                    pB *= contrast;
                    pB += 0.5;
                    pB *= 255;
                    if (pB < 0) pB = 0;
                    if (pB > 255) pB = 255;

                    newImg.SetPixel(i, j, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
                }
            }
            return newImg;
        }
        public Bitmap ZnajdzNiebieski() 
        {
            Bitmap kopiaImg = new Bitmap(Img);
            Color c;
            //zmieniamy kopie na pomaranczowy
            for (int i = 0; i < kopiaImg.Width; i++)
            {
                for (int j = 0; j < kopiaImg.Height; j++)
                {
                    c = kopiaImg.GetPixel(i, j);
                    int pixelR, pixelG, pixelB;
                    float hue = c.GetHue();
                    float sat = c.GetSaturation();
                    float bri = c.GetBrightness();
                    if ((160 < hue && hue < 330 && bri > 0.30 && bri < 0.75 && sat > 0.35))
                    // 160-300 jasny niebieski,niebieski, niebiesko-fioletowy
                    {
                        pixelR = 0;
                        pixelG = 0;
                        pixelB = 255;// wyostrzamy niebieski
                    }
                    else
                    {
                        pixelR = c.R;
                        pixelG = c.G - 127;
                        pixelB = c.B - 255;
                        pixelR = Math.Max(pixelR, 0);
                        pixelR = Math.Min(255, pixelR);
                        pixelG = Math.Max(pixelG, 0);
                        pixelG = Math.Min(255, pixelG);
                        pixelB = Math.Max(pixelB, 0);
                        pixelB = Math.Min(255, pixelB);
                    }
                    kopiaImg.SetPixel(i, j, Color.FromArgb((byte)pixelR, (byte)pixelG, (byte)pixelB));
                }
            }
            return kopiaImg;// i zwracamy pomaranczowy obraz z niebieskim wyostrzonym
        }

        public Color zmienpikselek(Color c) //  sprawdzenie piksela i zmiana jego koloru na niebieski lub odcien pomaranczowego 
        {
            int pixelR, pixelG, pixelB;
            float hue = c.GetHue();
            float sat = c.GetSaturation();
            float bri = c.GetBrightness();
            if ((160 < hue && hue < 330 && bri > 0.30 && bri < 0.75 && sat > 0.35))
            // 160-300 jasny niebieski,niebieski, niebiesko-fioletowy
            {
                pixelR = 0;
                pixelG = 0;
                pixelB = 255;// wyostrzamy niebieski
            }
            else
            {
                pixelR = c.R;
                pixelG = c.G - 127;
                pixelB = c.B - 255;
                pixelR = Math.Max(pixelR, 0);
                pixelR = Math.Min(255, pixelR);
                pixelG = Math.Max(pixelG, 0);
                pixelG = Math.Min(255, pixelG);
                pixelB = Math.Max(pixelB, 0);
                pixelB = Math.Min(255, pixelB);
            }
            return  Color.FromArgb((byte)pixelR, (byte)pixelG, (byte)pixelB);
        }
 
        public void watek1()
        {
            Bitmap kopiaImg = new Bitmap(Img);
            Color c;
            //zmieniamy kopie na pomaranczowy
            for (int i = 0; i < kopiaImg.Width/2; i++)
            {
                for (int j = 0; j < kopiaImg.Height; j++)
                {
                    c = kopiaImg.GetPixel(i, j);
                    kopiaImg.SetPixel(i, j, zmienpikselek(c));
                }
            }
            KoncowyImg1 = kopiaImg;
        }
        public void watek2()
        {
            Bitmap kopiaImg = new Bitmap(Img);
            Color c;
            //zmieniamy kopie na pomaranczowy
            for (int i = kopiaImg.Width/2; i < kopiaImg.Width; i++)
            {
                for (int j = 0; j < kopiaImg.Height; j++)
                {
                    c = kopiaImg.GetPixel(i, j);
                    kopiaImg.SetPixel(i, j, zmienpikselek(c));
                }
            }
            KoncowyImg2 = kopiaImg;
        }
       

        public Bitmap ZnajdzNiebieskiWielowatkowo()
        {
            Thread thread1 = new Thread(watek1);
            Thread thread2 = new Thread(watek2);
            //Thread thread3 = new Thread(watek3);
            //Thread thread4 = new Thread(watek4);
            thread1.Start();
            thread2.Start();
            //thread3.Start();
            //thread4.Start();
            thread1.Join(); //czekanie na zakonczenie watku 1
            thread2.Join();  //czekanie na zakonczenie watku 2

            //laczenie obrazow wynikowych z watkow w jedna calosc
            Bitmap kopiaImg = new Bitmap(Img);
            for (int i = 0; i < kopiaImg.Width / 2; i++)
            {
                for (int j = 0; j < kopiaImg.Height; j++)
                {
                    kopiaImg.SetPixel(i, j, KoncowyImg1.GetPixel(i, j));                                            //laczenie obrazow z watku 1
                    kopiaImg.SetPixel(kopiaImg.Width / 2+i, j, KoncowyImg2.GetPixel(kopiaImg.Width / 2 + i, j));    //laczenie obrazow z watku 2
                }
            }
            
            return kopiaImg;// i zwracamy pomaranczowy obraz z niebieskim wyostrzonym
        }
    }
}
            