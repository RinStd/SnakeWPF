using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeWPF
{
    public partial class MainWindow : Window
    {
        private const double CellSize = 24;
        private readonly DispatcherTimer gameTimer;
        private SnakeGame game = new SnakeGame();
        private bool isPaused;

        public MainWindow()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(140)
            };
            gameTimer.Tick += GameTimer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGame();
            Focus();
            gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            game.Move();
            DrawGame();
            gameTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(65, 140 - game.Score / 10 * 5));

            if (game.GameOver)
            {
                gameTimer.Stop();
                ShowGameOver();
            }
        }

        private void DrawGame()
        {
            GameCanvas.Children.Clear();
            DrawGrid();

            if (game.Food.HasValue)
            {
                Position food = game.Food.Value;
                var apple = new Ellipse
                {
                    Width = CellSize - 8,
                    Height = CellSize - 8,
                    Fill = new SolidColorBrush(Color.FromRgb(251, 113, 133))
                };
                Canvas.SetLeft(apple, food.Column * CellSize + 4);
                Canvas.SetTop(apple, food.Row * CellSize + 4);
                GameCanvas.Children.Add(apple);
            }

            for (int i = 0; i < game.Snake.Count; i++)
            {
                Position segment = game.Snake[i];
                var rectangle = new Rectangle
                {
                    Width = CellSize - 2,
                    Height = CellSize - 2,
                    RadiusX = 5,
                    RadiusY = 5,
                    Fill = i == 0
                        ? new SolidColorBrush(Color.FromRgb(134, 239, 172))
                        : new SolidColorBrush(Color.FromRgb(34, 197, 94))
                };
                Canvas.SetLeft(rectangle, segment.Column * CellSize + 1);
                Canvas.SetTop(rectangle, segment.Row * CellSize + 1);
                GameCanvas.Children.Add(rectangle);
            }

            ScoreText.Text = game.Score.ToString();
        }

        private void DrawGrid()
        {
            var cellBrush = new SolidColorBrush(Color.FromRgb(17, 28, 46));
            var lineBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59));

            for (int row = 0; row < game.Rows; row++)
            {
                for (int column = 0; column < game.Columns; column++)
                {
                    var cell = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = cellBrush,
                        Stroke = lineBrush,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(cell, column * CellSize);
                    Canvas.SetTop(cell, row * CellSize);
                    GameCanvas.Children.Add(cell);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
            {
                RestartGame();
                e.Handled = true;
                return;
            }

            if (game.GameOver)
            {
                if (e.Key == Key.Enter)
                {
                    RestartGame();
                }
                return;
            }

            if (e.Key == Key.Space)
            {
                TogglePause();
                e.Handled = true;
                return;
            }

            if (isPaused)
            {
                return;
            }

            Direction? direction = e.Key switch
            {
                Key.Up or Key.W => Direction.Up,
                Key.Down or Key.S => Direction.Down,
                Key.Left or Key.A => Direction.Left,
                Key.Right or Key.D => Direction.Right,
                _ => null
            };

            if (direction.HasValue)
            {
                game.ChangeDirection(direction.Value);
                e.Handled = true;
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                gameTimer.Stop();
                OverlayTitle.Text = "Пауза";
                OverlayMessage.Text = "Нажмите пробел, чтобы продолжить";
                OverlayButton.Content = "Продолжить";
                Overlay.Visibility = Visibility.Visible;
            }
            else
            {
                Overlay.Visibility = Visibility.Collapsed;
                gameTimer.Start();
            }
        }

        private void ShowGameOver()
        {
            OverlayTitle.Text = game.Won ? "Победа!" : "Игра окончена";
            OverlayMessage.Text = $"Ваш счёт: {game.Score}";
            OverlayButton.Content = "Играть снова";
            Overlay.Visibility = Visibility.Visible;
        }

        private void RestartGame()
        {
            gameTimer.Stop();
            game = new SnakeGame();
            isPaused = false;
            Overlay.Visibility = Visibility.Collapsed;
            DrawGame();
            gameTimer.Interval = TimeSpan.FromMilliseconds(140);
            gameTimer.Start();
            Focus();
        }

        private void OverlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (game.GameOver)
            {
                RestartGame();
            }
            else
            {
                TogglePause();
            }
        }
    }
}