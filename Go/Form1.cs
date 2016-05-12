using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Go
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int WSize;
        public int UnitSize;
        GoGame game = new GoGame(9);

        public void DrawBoard()
        {
            WSize = Math.Min(Size.Width, Size.Height) - 30;
            UnitSize = WSize / game.Size;
            using (Graphics g = CreateGraphics())
            {
                g.Clear(Color.Wheat);

                DrawGrid(g);
                foreach (var field in game.Board)
                {
                    DrawField(g, field);
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            for (int i = 1; i < game.Size; i++)
            {
                g.DrawLine(Pens.Black, i * UnitSize, 0, i * UnitSize, WSize);
                g.DrawLine(Pens.Black, 0, i * UnitSize, WSize, i * UnitSize);
            }
        }

        public Point DetermineField(Point p)
        {
            return new Point(p.X / UnitSize, p.Y / UnitSize);
        }

        public void DrawField(Graphics g, Field f)
        {
            if (f.Player == GoGame.Players.None)
                g.FillRectangle(Brushes.Wheat, f.X * UnitSize + 1, f.Y * UnitSize + 1, UnitSize - 2, UnitSize - 2);
            else
                g.FillEllipse(f.Player == GoGame.Players.White ? Brushes.White : Brushes.Black, f.X * UnitSize, f.Y * UnitSize, UnitSize, UnitSize);
        }
        bool d = true;

        private void Form1_Click(object sender, EventArgs e)
        {
            Point fieldSelected = DetermineField(PointToClient(MousePosition));
            if (!ValidateSelectedField(fieldSelected))
                return;
            if (!game.MakeMove(fieldSelected.X, fieldSelected.Y))
                MessageBox.Show("Illegal move");
            else
            {
                DrawBoard();
                AlphaBeta.Perform(game, !d);
            }
            DrawBoard();
            CreateGraphics().FillEllipse(Brushes.Red, game.LastMove.X * UnitSize, game.LastMove.Y * UnitSize, UnitSize, UnitSize);
            Text = "White: " + game.Points[0] + ", Black: " + game.Points[1];
            /*while (true)
            {
                d = !d;

                AlphaBeta.Perform(game, d);
                //DrawField(CreateGraphics(), game.Board[fieldSelected.X - 1, fieldSelected.Y - 1]);
                DrawBoard();
                CreateGraphics().FillEllipse(Brushes.Red, game.LastMove.X * UnitSize, game.LastMove.Y * UnitSize, UnitSize, UnitSize);
                Text = "White: " + game.Points[0] + ", Black: " + game.Points[1];
               
            }*/
        }

        private bool ValidateSelectedField(Point fieldSelected)
        {
            return fieldSelected.X >= 0 && fieldSelected.Y >= 0 &&
                fieldSelected.X < game.Size && fieldSelected.Y < game.Size;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DrawBoard();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            DrawBoard();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == ' ')
            {
                game.SwapTurns();
                AlphaBeta.Perform(game, !d);
                DrawBoard();
                CreateGraphics().FillEllipse(Brushes.Red, game.LastMove.X * UnitSize, game.LastMove.Y * UnitSize, UnitSize, UnitSize);
            }
        }
    }
}
