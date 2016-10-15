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
            //oknoWyboruPliku.InitialDirectory = System.IO.Directory.GetCurrentDirectory();

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
                /*obrazek.CzytajNaglowek(czytajPlik);
                String[] txt = obrazek.WypiszNaglowek();
                richTextBox1.Text = txt[0];
                richTextBox2.Text = txt[1];*/
                obrazek.CzytajObraz(plik);
                pictureBox1.Image = obrazek.Img;
                plik.Close();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (obrazek == null)
                return;
            pictureBox1.Image = obrazek.filtr();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
