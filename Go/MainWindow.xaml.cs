using GoLibrary;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Go
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int Padding = 40;
        int CellSize;
        int Height;
        int Width;
        int Size = 19;
        Ellipse ellipse;
        int CursorRadius = 40;
        int CurrX;
        int CurrY;
        TextBlock WhiteScore;
        TextBlock BlackScore;

        private List<List<Ellipse>> stones;
        GoGame game;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, e) => {
                Height = (int)canvas.ActualHeight;
                Width = (int)canvas.ActualWidth;
                CellSize = (int)((Height - Padding * 2) / (Size - 1));

                ellipse = new Ellipse();
                ellipse.Stroke = Brushes.Black;
                ellipse.Width = CursorRadius;
                ellipse.Height = CursorRadius;
                canvas.Children.Add(ellipse);

                stones = new List<List<Ellipse>>();
                for (int i = 0; i < Size; ++i)
                {
                    stones.Add(new List<Ellipse>());
                    for (int j = 0; j < Size; ++j)
                    {
                        stones[i].Add(null);
                    }
                }
                game = new GoGame(Size);

                game.OnMoveEvent += (toRemove, y, x, player) =>
                {
                    int stoneSize = (int)(CellSize * 0.8);
                    var stone = new Ellipse();
                    stone.Width = stoneSize;
                    stone.Height = stoneSize;
                    stone.Fill = stone.Stroke = player.Color == Stone.Black ? Brushes.Black : Brushes.White;
                    Canvas.SetTop(stone, Padding + y * CellSize - stoneSize / 2);
                    Canvas.SetLeft(stone, Padding + x * CellSize - stoneSize / 2);
                    canvas.Children.Add(stone);
                    stones[y][x] = stone;
                    foreach (var rem in toRemove)
                    {
                        RemoveStone(rem);
                    }
                    BlackScore.Text = "" + game.BlackPlayerScore;
                    WhiteScore.Text = "" + game.WhitePlayerScore;
                };

                game.OnGameEndEvent += (black, white) =>
                {
                    string str = "";
                    if (black > white) str += "Black player Won!\n";
                    else if (white > black) str += "White player Won!\n";
                    else str += "Draw!\n";
                    str += "Black player score: " + black;
                    str += "\nWhite player score: " + white;
                    MessageBox.Show(str);
                    game.Reset();
                    BlackScore.Text = "" + 0;
                    WhiteScore.Text = "" + 0;
                    ResetBoard();
                };

                game.OnPlayerChangedEvent += player => ellipse.Stroke = player == Stone.Black ?
                    Brushes.Black : Brushes.White;
                AddGrid(canvas);

                Text(CellSize * Size + Padding + 20, Padding, "Scores", Colors.Black);
                Text(CellSize * Size + Padding + 20, Padding + 40, "White: ", Colors.Black);
                Text(CellSize * Size + Padding + 20, Padding + 80, "Black: ", Colors.Black);
                WhiteScore = Text(CellSize * Size + Padding + 110, Padding + 40, "" + 0, Colors.Black);
                BlackScore = Text(CellSize * Size + Padding + 110, Padding + 80, "" + 0, Colors.Black);

                Button button = new Button();
                var textBlock = new TextBlock();
                textBlock.Text = "Pass";
                textBlock.FontSize = 25;
                button.Content = textBlock;
                Canvas.SetLeft(button, CellSize * Size + Padding + 40);
                Canvas.SetTop(button, Padding + 120);
                button.Click += (clickSender, clickE) => game.Pass();
                canvas.Children.Add(button);
            };

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void ResetBoard()
        {
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    if (stones[i][j] != null)
                    {
                        canvas.Children.Remove(stones[i][j]);
                        stones[i][j] = null;
                    }
                }
            }
        }



        void RemoveStone(Tuple<int, int> stone)
        {
            if (stones[stone.Item1][stone.Item2] != null)
            {
                canvas.Children.Remove(stones[stone.Item1][stone.Item2]);
                stones[stone.Item1][stone.Item2] = null;
            }
        }

        void Move(int y, int x)
        {
            game.Move(y, x);
        }

        void AddGrid(Canvas canvas)
        {
            var lines = new List<Line>();
            int size = Size;
            var h = canvas.ActualHeight;
            var w = canvas.ActualWidth;
            int padding = 40;
            int cellSize = (int)((h - padding * 2) / (size - 1));

            for (int i = 0; i < size; ++i)
            {
                var line = new Line();
                line.X1 = Padding;
                line.X2 = CellSize * (Size - 1) + Padding;
                line.Y1 = Padding + CellSize * i;
                line.Y2 = Padding + CellSize * i;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;
                canvas.Children.Add(line);
            }

            for (int i = 0; i < size; ++i)
            {
                var line = new Line();
                line.X1 = Padding + CellSize * i;
                line.X2 = Padding + CellSize * i;
                line.Y1 = Padding;
                line.Y2 = CellSize * (Size - 1) + Padding;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;
                canvas.Children.Add(line);
            }
        }

        TextBlock Text(double x, double y, string text, Color color)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = 30;
            textBlock.Foreground = new SolidColorBrush(color);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            canvas.Children.Add(textBlock);
            return textBlock;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ResetBoard();
                BlackScore.Text = "" + 0;
                WhiteScore.Text = "" + 0;
                game.Reset();
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(canvas);
            double mx = pos.X - Padding;
            double my = pos.Y - Padding;

            mx /= CellSize;
            my /= CellSize;
            mx = Math.Round(mx);
            my = Math.Round(my);
            CurrX = (int)mx;
            CurrY = (int)my;
            int canvasX = Padding + CurrX * CellSize;
            int canvasY = Padding + CurrY * CellSize;

            if (CurrY >= 0 && CurrY < Size && CurrX >= 0 && CurrX < Size)
            {
                Canvas.SetLeft(ellipse, canvasX - CursorRadius / 2);
                Canvas.SetTop(ellipse, canvasY - CursorRadius / 2);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrY >= 0 && CurrY < Size && CurrX >= 0 && CurrX < Size)
                Move(CurrY, CurrX);
        }
    }
}
