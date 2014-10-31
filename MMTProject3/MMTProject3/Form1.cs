using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMTProject3
{
    public partial class Form1 : Form
    {
        private PathFinder p;
        private Graphics pbGraphics;
        private Graphics bufferGraphics;
        private Bitmap backbuffer;
        private int nrows;
        private int ncolumns;
        private int cellSize;
        private bool hasStart = false;
        private bool hasEnd = false;
        private CellType drawMode;
        private bool mousedown = false;
        private int lastCellX = 0;
        private int lastCellY = 0;

        public Form1()
        {
            InitializeComponent();
            backbuffer = new Bitmap(panel1.Width,panel1.Height);
            nrows = 17;
            ncolumns = 26;
            cellSize = 20;
            pbGraphics = panel1.CreateGraphics();
            bufferGraphics = Graphics.FromImage(backbuffer);
            bufferGraphics.SmoothingMode = SmoothingMode.AntiAlias;

            p = new PathFinder(nrows,ncolumns,bufferGraphics,cellSize);
            drawMode = CellType.obstacle;
            obstacleRadioButton.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            p = new PathFinder(nrows, ncolumns, bufferGraphics, cellSize);
            textBox1.Text = "";
            Redraw();
        }

        private void Redraw()
        {
            bufferGraphics.Clear(Color.MintCream);
            p.drawField();
            p.drawCells();
            pbGraphics.DrawImage(backbuffer, new Rectangle(0, 0, panel1.Width, panel1.Height), new Rectangle(0, 0, panel1.Width, panel1.Height), GraphicsUnit.Pixel);

        }

        private void DoClickAndDrag(MouseEventArgs e)
        {
            Console.WriteLine("x:" + e.X);
            Console.WriteLine("y:" + e.Y);

            if (e.X >= ncolumns * cellSize || e.X < 0 || e.Y >= nrows * cellSize || e.Y < 0) return;

            int column = e.X / cellSize;
            int row = e.Y / cellSize;
            lastCellX = column;
            lastCellY = row;
            CellType original = p.GetCellType(row, column);

            p.setCelltype(row, column, drawMode, original);
            Redraw();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mousedown = true;
            DoClickAndDrag(e);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            int currentCellX = e.X/cellSize;
            int currentCellY = e.Y/cellSize;
            if (mousedown && !(currentCellX == lastCellX && currentCellY == lastCellY) && drawMode == CellType.obstacle)
            {
                DoClickAndDrag(e);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mousedown = false;
        }

        private void startRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = CellType.start;
        }

        private void endRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = CellType.end;
        }

        private void obstacleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            drawMode = CellType.obstacle;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (p.FindPath())
            {
                Console.WriteLine("pad gevonden");
                p.SetPath();
                Redraw();
                textBox1.Text = p.PadLengte.ToString();
            }
            else
            {
                Console.WriteLine("geen pad gevonden");
                textBox1.Text = "geen pad";
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Checked)
            {
                p.AllowDiagonal = true;
            }
            else if (checkBox1.CheckState == CheckState.Unchecked)
            {
                p.AllowDiagonal = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.CheckState == CheckState.Checked)
            {
                p.Euclidean = true;
            }
            else if (checkBox2.CheckState == CheckState.Unchecked)
            {
                p.Euclidean = false;
            }
        }

    }
}
