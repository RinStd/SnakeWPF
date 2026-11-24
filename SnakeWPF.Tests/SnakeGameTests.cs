using System;
using System.Linq;
using NUnit.Framework;

namespace SnakeWPF.Tests
{
    [TestFixture]
    public class SnakeGameTests
    {
        [Test]
        public void NewGamePlacesSnakeAndFoodInsideBoard()
        {
            var game = new SnakeGame(3, 3, seed: 1);

            Assert.That(game.Snake, Has.Count.EqualTo(3));
            Assert.That(game.Food.HasValue, Is.True);
            foreach (Position segment in game.Snake)
            {
                Assert.That(segment.Row, Is.InRange(0, game.Rows - 1));
                Assert.That(segment.Column, Is.InRange(0, game.Columns - 1));
            }
            Assert.That(game.Snake, Does.Not.Contain(game.Food!.Value));
        }

        [Test]
        public void DirectionCannotReverseOrQueueMultipleTurns()
        {
            var game = new SnakeGame(seed: 1);

            Assert.That(game.ChangeDirection(Direction.Left), Is.False);
            Assert.That(game.ChangeDirection(Direction.Right), Is.False);
            Assert.That(game.ChangeDirection(Direction.Up), Is.True);
            Assert.That(game.ChangeDirection(Direction.Left), Is.False);

            game.Move();

            Assert.That(game.CurrentDirection, Is.EqualTo(Direction.Up));
            Assert.That(game.Snake[0], Is.EqualTo(new Position(9, 10)));
        }

        [Test]
        public void EatingFoodIncreasesScoreAndGrowsSnake()
        {
            Position nextHead = new Position(10, 11);
            int seed = FindSeedWithFoodAt(nextHead);
            var game = new SnakeGame(seed: seed);

            game.Move();

            Assert.That(game.Score, Is.EqualTo(10));
            Assert.That(game.Snake, Has.Count.EqualTo(4));
            Assert.That(game.Food.HasValue, Is.True);
            Assert.That(game.Snake, Does.Not.Contain(game.Food!.Value));
        }

        [Test]
        public void MovingIntoWallEndsGameAndFurtherMovesDoNothing()
        {
            var game = new SnakeGame(3, 3, seed: 1);
            Position[] originalSnake = game.Snake.ToArray();

            game.Move();
            Position[] afterCollision = game.Snake.ToArray();
            game.Move();

            Assert.That(game.GameOver, Is.True);
            Assert.That(afterCollision, Is.EqualTo(originalSnake));
            Assert.That(game.Snake, Is.EqualTo(originalSnake));
        }

        [Test]
        public void MovingIntoOwnBodyEndsGame()
        {
            int seed = FindSeedForTwoFoodPositions(new Position(2, 3), new Position(2, 4));
            var game = new SnakeGame(5, 5, seed);
            game.Move();
            game.Move();

            Assert.That(game.Score, Is.EqualTo(20));
            Assert.That(game.Snake, Has.Count.EqualTo(5));

            game.ChangeDirection(Direction.Down);
            game.Move();
            game.ChangeDirection(Direction.Left);
            game.Move();
            Position[] snakeBeforeCollision = game.Snake.ToArray();

            game.ChangeDirection(Direction.Up);
            game.Move();

            Assert.That(game.GameOver, Is.True);
            Assert.That(game.Snake, Is.EqualTo(snakeBeforeCollision));
        }

        [Test]
        public void BoardMustBeLargeEnoughForInitialSnake()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SnakeGame(rows: 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SnakeGame(columns: 2));
        }

        private static int FindSeedWithFoodAt(Position position)
        {
            for (int seed = 0; seed < 10000; seed++)
            {
                if (new SnakeGame(seed: seed).Food == position)
                {
                    return seed;
                }
            }

            throw new AssertionException("Could not find a deterministic food position for the test.");
        }

        private static int FindSeedForTwoFoodPositions(Position firstFood, Position secondFood)
        {
            for (int seed = 0; seed < 10000; seed++)
            {
                var game = new SnakeGame(5, 5, seed);
                if (game.Food != firstFood)
                {
                    continue;
                }

                game.Move();
                if (game.Food == secondFood)
                {
                    return seed;
                }
            }

            throw new AssertionException("Could not find deterministic food positions for the test.");
        }
    }
}