using System;
using System.Windows.Forms;

namespace TestGUIApp
{
    public partial class MainForm : Form
    {
        PictureBox[] pictureBoxes = new PictureBox[9];
        private void redrawScene()
        {
            var imageGenerator = new ImageGenerator(100, 100, 20, 20);
            templateBox.Image = imageGenerator.GenerateImage();

            var random = new Random().Next(9);
            for (int i = 0; i < pictureBoxes.Length; i++)
            {
                if(pictureBoxes[i].Image != null)
                    pictureBoxes[i].Image.Dispose();
                if (random == i)
                {
                    pictureBoxes[i].Image = templateBox.Image;
                    pictureBoxes[i].Tag = true;
                }
                else
                {
                    pictureBoxes[i].Image = imageGenerator.GenerateImage();
                    pictureBoxes[i].Tag = false;
                }
            }
        }
        public MainForm()
        {
            InitializeComponent();

            pictureBoxes = new PictureBox[]
            {
                pictureBox1,
                pictureBox2,
                pictureBox3,
                pictureBox4,
                pictureBox5,
                pictureBox6,
                pictureBox7,
                pictureBox8,
                pictureBox9
            };

            redrawScene();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            if ((bool)((PictureBox)sender).Tag == true)
                redrawScene();
        }
    }
}
