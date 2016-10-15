using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZZ
{
    public partial class MainForm : Form
    {
        string sciezkaPliku = "";
        IMGFormat obrazek;
        BinaryReader czytajPlik;
        Stream plik;
        public MainForm()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog oknoWyboruPliku = new OpenFileDialog();
            oknoWyboruPliku.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            oknoWyboruPliku.Filter = "Wszystkie obrazy|*.bmp;*.jpg;*.jpeg;*.png|"
                                   + "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png";
            oknoWyboruPliku.Title = "Wczytaj obraz";
            oknoWyboruPliku.RestoreDirectory = true;
            if (oknoWyboruPliku.ShowDialog() == DialogResult.OK)
            {
                if (plik != null) plik.Close();
                obrazek = new IMGFormat();
                pictureBox1.Image = null;
                sciezkaPliku = oknoWyboruPliku.FileName;
                plik = File.Open(sciezkaPliku, FileMode.Open);
                czytajPlik = new BinaryReader(plik);
                obrazek.CzytajObraz(plik);
                pictureBox1.Image = obrazek.Img;
                plik.Close();
            }
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog oknoZapisuPliku = new SaveFileDialog();
            oknoZapisuPliku.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            oknoZapisuPliku.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png";
            oknoZapisuPliku.Title = "Zapisz obraz";
            oknoZapisuPliku.FileName = "";
            oknoZapisuPliku.RestoreDirectory = true;
            if (oknoZapisuPliku.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    switch (oknoZapisuPliku.FilterIndex)
                    {
                        case 1:
                            pictureBox1.Image.Save(oknoZapisuPliku.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        case 2:
                            pictureBox1.Image.Save(oknoZapisuPliku.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                            break;
                        case 3:
                            pictureBox1.Image.Save(oknoZapisuPliku.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case 4:
                            pictureBox1.Image.Save(oknoZapisuPliku.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                    }
                }
                catch { System.Windows.Forms.MessageBox.Show("Błąd. brak obrazka do wczytania!"); }
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = obrazek.Filtr();
            }
            catch { System.Windows.Forms.MessageBox.Show("Błąd. brak obrazka do wczytania!"); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = obrazek.Kontrast();
            }
            catch { System.Windows.Forms.MessageBox.Show("Błąd. brak obrazka do wczytania!"); }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = obrazek.ZnajdzNiebieski();
            }
            catch { System.Windows.Forms.MessageBox.Show("Błąd. brak obrazka do wczytania!"); }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = obrazek.ZnajdzKolor();
            }
            catch { System.Windows.Forms.MessageBox.Show("Błąd. brak obrazka do wczytania!"); }
        }

    }
}
