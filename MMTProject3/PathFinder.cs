using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMTProject3
{
    class PathFinder
    {
        private Cell[][] field;
        private Graphics g;
        private float cellSize; //pixels
        private int nrows;
        private int ncolumns;
        private Cell start;
        private Cell end;
        public bool AllowDiagonal { get; set; }
        public bool Euclidean { get; set; }
        public int PadLengte { get; set; }

        public PathFinder(int nrows, int ncolumns, Graphics graphics,float cellSize)
        {
            this.start = null;
            this.end = null;
            this.ncolumns = ncolumns;
            this.nrows = nrows;
            this.cellSize = cellSize;
            field = new Cell[nrows][];
            for (int i = 0; i < field.Length; i++)
            {
                field[i] = new Cell[ncolumns];
                for (int j = 0; j < field[i].Length; j++)
                {
                    field[i][j] = new Cell(i,j,this);
                }
            }

            this.nrows = nrows;
            this.ncolumns = ncolumns;
            this.g = graphics;
            AllowDiagonal = false;
            Euclidean = false;
            PadLengte = 0;
        }

        public void drawField()
        {
            Pen p = new Pen(Brushes.Black);
            p.Width = 1.0f;

            for (int y = 0; y <= nrows; ++y)
            {
                g.DrawLine(p, 0, cellSize * y, ncolumns * cellSize, cellSize * y);
            }

            for (int x = 0; x <= ncolumns; ++x)
            {
                g.DrawLine(p, x * cellSize, 0, x * cellSize, nrows * cellSize);
            }
        }

        public void drawCells()
        {
            for (int i = 0; i < field.Length; i++)
            {
                for (int j = 0; j < field[i].Length; j++)
                {
                    float y = (i * cellSize) + cellSize / 4;
                    float x = (j * cellSize) + cellSize / 4;
                    switch (field[i][j].CellType)
                    {
                        case CellType.obstacle:
                            g.FillRectangle(Brushes.Black,j*cellSize,i*cellSize,cellSize,cellSize);
                            break;
                        case CellType.passthrough:
                            g.FillEllipse(Brushes.Green, x+2, y+2, cellSize / 3, cellSize / 3);
                            break;
                        case CellType.start:
                            g.FillEllipse(Brushes.Red,x,y,cellSize/2,cellSize/2 );
                            break;
                        case CellType.end:
                            g.FillEllipse(Brushes.Blue, x, y, cellSize / 2, cellSize / 2);
                            break;
                        case CellType.empty:
                            break;

                    }
                }
            }
        }

        public void setCelltype(int row, int column, CellType ct,CellType original)
        {
            if (original == CellType.start) start = null;
            if (original == CellType.end) end = null;
            if (ct == CellType.start)
            {
                if (start != null)
                {
                    this.start.CellType = CellType.empty;
                }
                start = field[row][column];
            }
            else if (ct == CellType.end)
            {
                if (end != null)
                {
                    this.end.CellType = CellType.empty;
                }
                end = field[row][column];
            }
            else if (ct == CellType.obstacle && field[row][column].CellType == CellType.obstacle)
            {
                ct = CellType.empty;
            }
            field[row][column].CellType = ct;
        }

        public CellType GetCellType(int row, int column)
        {
            return field[row][column].CellType;
        }

        public bool FindPath()
        {
            if (start == null || end == null) return false;
            PadLengte = 0;
            for (int i = 0; i < nrows; i++)
            {
                for (int j = 0; j < ncolumns; j++)
                {
                    field[i][j].Init(i,j);
                    if (field[i][j].CellType == CellType.passthrough)
                    {
                        field[i][j].CellType = CellType.empty;
                    }
                }
            }

            List<Cell> opened = new List<Cell>();
            List<Cell> closed = new List<Cell>();

            Cell current = null;

            opened.Add(start);

            while (opened.Count != 0)
            {
                int lowestFscore = 0;

                for (int i = 0; i < opened.Count; i++)
                {
                    if (opened[i].F < opened[lowestFscore].F)
                    {
                        lowestFscore = i;
                    }
                }

                current = opened[lowestFscore];

                if (current == end) //eindpunt gevonden
                {
                    return true;
                }

                opened.Remove(current);
                closed.Add(current);

                foreach(Cell c in GetAdjacent(current))
                {
                    if(closed.Contains(c)) continue; //cell is al bezocht

                    if (!opened.Contains(c)) //cell wordt voor de eerste keer bekeken
                    {
                        opened.Add(c);
                        c.Parent = current;
                        c.CalculateScores(current, end);
                    }
                    else //we zijn hier al eens geweest langs een ander pad
                    {
                        //indien G score (pad naar start) kleiner is geworden herbereken dan de score
                        if (c.G > c.GetGScore(current))
                        {
                            c.Parent = current;
                            c.CalculateScores(current,end);
                        }
                    }
                }
            }
            return false; //geen pad gevonden
        }

        public void SetPath()
        {
            Cell current = end.Parent;
            PadLengte = 2;
            while (current!=start)
            {
                current.CellType = CellType.passthrough;
                current = current.Parent;
                PadLengte++;
            }
        }

        //geeft de bereikbare buren van een Cell terug
        public List<Cell> GetAdjacent(Cell c)
        {
            List<Cell> adjacent = new List<Cell>();
            if (Reachable(c.rowY, c.colX - 1)) //links
            {
                adjacent.Add(field[c.rowY][c.colX-1]);
            }
            if (Reachable(c.rowY, c.colX + 1)) //rechts
            {
                adjacent.Add(field[c.rowY][c.colX +1]);
            }
            if (Reachable(c.rowY-1, c.colX)) //boven
            {
                adjacent.Add(field[c.rowY -1][c.colX]);
            }
            if (Reachable(c.rowY+1, c.colX)) //onder
            {
                adjacent.Add(field[c.rowY+1][c.colX]);
            }
            if (AllowDiagonal)
            {
                if (Reachable(c.rowY+1, c.colX + 1)) //rechtsonder
                {
                    adjacent.Add(field[c.rowY+1][c.colX + 1]);
                }
                if (Reachable(c.rowY -1, c.colX -1)) //linksboven
                {
                    adjacent.Add(field[c.rowY - 1][c.colX - 1]);
                }
                if (Reachable(c.rowY - 1, c.colX + 1)) //rechtsboven
                {
                    adjacent.Add(field[c.rowY - 1][c.colX + 1]); 
                }
                if (Reachable(c.rowY + 1, c.colX - 1)) //linksonder
                {
                    adjacent.Add(field[c.rowY + 1][c.colX - 1]);
                }

            }
            return adjacent;
        }

        //een cel is bereikbaar als ze bestaat en geen obstakel is
        public bool Reachable(int row,int column)
        {
            if (row < 0 || row > nrows - 1 || column < 0 || column > ncolumns - 1 ||
                field[row][column].CellType == CellType.obstacle)
            {
                return false;
            }
            return true;
        }

        public int Distance(Cell c1, Cell c2)
        {
            if (Euclidean)
            {
                //return (int)Math.Ceiling(Math.Sqrt(((c1.colX - c2.colX)*(c1.colX - c2.colX)) + ((c1.rowY - c2.rowY)*(c1.rowY - c2.rowY))));
                return (int)(Math.Sqrt(((c1.colX - c2.colX) * (c1.colX - c2.colX)) + ((c1.rowY - c2.rowY) * (c1.rowY - c2.rowY))));
            }
            return Math.Abs(c1.rowY - c2.rowY) + Math.Abs(c1.colX - c2.colX);
        }
    }

    internal enum CellType
    {
        obstacle,
        start,
        end,
        passthrough,
        empty,
    };

    class Cell
    {
        public Cell Parent { get; set; }
        public CellType CellType { get; set; }
        public int rowY { get; private set; }
        public int colX { get; private set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F { get; set; }
        public PathFinder pf;

        public Cell (int row,int col,PathFinder pf)
        {
            Init(row,col);
            this.pf = pf;
            this.CellType = CellType.empty;
        }

        public void CalculateScores(Cell parent, Cell end)
        {
            G = GetGScore(parent);
            H = pf.Distance(this, end);
            F = G + H;
        }

        public int GetGScore(Cell parent)
        {
            if (parent.colX == colX || parent.rowY == rowY)
            {
                return parent.G + 10;
            }
            return parent.G + 14;
        }

        public void Init(int row,int col)
        {
            F = 0;
            G = 0;
            H = 0;
            rowY = row;
            colX = col;
            Parent = null;
        }
    }
}
