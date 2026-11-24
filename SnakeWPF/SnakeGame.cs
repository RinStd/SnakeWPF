using System;
using System.Collections.Generic;

namespace SnakeWPF
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class SnakeGame
    {
        public const int DefaultRows = 20;
        public const int DefaultColumns = 20;

        private readonly List<Position> snake;
        private readonly Random random;
        private Direction nextDirection;
        private bool hasPendingDirection;

        public int Rows { get; }

        public int Columns { get; }

        public IReadOnlyList<Position> Snake => snake;

        public Position? Food { get; private set; }

        public Direction CurrentDirection { get; private set; }

        public int Score { get; private set; }

        public bool GameOver { get; private set; }

        public bool Won { get; private set; }

        public SnakeGame(int rows = DefaultRows, int columns = DefaultColumns, int? seed = null)
        {
            if (rows < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), "The board must have at least 3 rows.");
            }

            if (columns < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), "The board must have at least 3 columns.");
            }

            Rows = rows;
            Columns = columns;
            random = seed.HasValue ? new Random(seed.Value) : new Random();

            int middleRow = rows / 2;
            int headColumn = Math.Max(2, columns / 2);
            snake = new List<Position>
            {
                new Position(middleRow, headColumn),
                new Position(middleRow, headColumn - 1),
                new Position(middleRow, headColumn - 2)
            };

            CurrentDirection = Direction.Right;
            nextDirection = CurrentDirection;
            PlaceFood();
        }

        public bool ChangeDirection(Direction direction)
        {
            if (GameOver || hasPendingDirection || direction == CurrentDirection
                || IsOpposite(direction, CurrentDirection))
            {
                return false;
            }

            nextDirection = direction;
            hasPendingDirection = true;
            return true;
        }

        public void Move()
        {
            if (GameOver)
            {
                return;
            }

            CurrentDirection = nextDirection;
            hasPendingDirection = false;

            Position head = snake[0];
            Position nextHead = CurrentDirection switch
            {
                Direction.Up => new Position(head.Row - 1, head.Column),
                Direction.Down => new Position(head.Row + 1, head.Column),
                Direction.Left => new Position(head.Row, head.Column - 1),
                Direction.Right => new Position(head.Row, head.Column + 1),
                _ => head
            };

            if (!IsInside(nextHead))
            {
                GameOver = true;
                return;
            }

            bool eatsFood = Food == nextHead;
            int occupiedSegments = snake.Count - (eatsFood ? 0 : 1);
            for (int i = 0; i < occupiedSegments; i++)
            {
                if (snake[i] == nextHead)
                {
                    GameOver = true;
                    return;
                }
            }

            snake.Insert(0, nextHead);
            if (eatsFood)
            {
                Score += 10;
                if (snake.Count == Rows * Columns)
                {
                    Won = true;
                    GameOver = true;
                    Food = null;
                }
                else
                {
                    PlaceFood();
                }
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private bool IsInside(Position position)
        {
            return position.Row >= 0 && position.Row < Rows
                && position.Column >= 0 && position.Column < Columns;
        }

        private void PlaceFood()
        {
            var emptyCells = new List<Position>();
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    var position = new Position(row, column);
                    if (!snake.Contains(position))
                    {
                        emptyCells.Add(position);
                    }
                }
            }

            Food = emptyCells[random.Next(emptyCells.Count)];
        }

        private static bool IsOpposite(Direction first, Direction second)
        {
            return first switch
            {
                Direction.Up => second == Direction.Down,
                Direction.Down => second == Direction.Up,
                Direction.Left => second == Direction.Right,
                Direction.Right => second == Direction.Left,
                _ => false
            };
        }
    }
}