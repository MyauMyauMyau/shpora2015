using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace ShporaTetris
{
    class Program
    {
        static void Main(string[] args)
        {
            var tetris = new Tetris(@"tests/smallest.json");
            Console.Write(String.Join(",",tetris.GameStream.Commands));
            Console.Write(tetris.GameStream.Pieces[1].RelativeCoordinates[1].Y);
            Console.ReadKey();
        }
    }


    public sealed class Tetris
    {
        public GameStream GameStream { get; }
        private ImmutableDictionary<char, Action<Piece>> Actions { get; }
        public Tetris(string jsonFilePath)
        {
            GameStream = new GameStream(jsonFilePath);
            Actions = new Dictionary<char, Action<Piece>>()
            {
                {'A', currentPiece => Piece.Move(currentPiece, DirectionOfMovement.Left)},
                {'D', currentPiece => Piece.Move(currentPiece, DirectionOfMovement.Right)},
                {'S', currentPiece => Piece.Move(currentPiece, DirectionOfMovement.Down)},
                {'W', currentPiece => Piece.Move(currentPiece, DirectionOfMovement.Up)},
                {'Q', currentPiece => Piece.Rotate(currentPiece, DirectionOfRotation.Clockwise)},
                {'E', currentPiece => Piece.Rotate(currentPiece, DirectionOfRotation.Anticlockwise)}
            }
            .ToImmutableDictionary();
        }

        public void Play()
        {
            var pieceCounter = 0;
            var currentPiece = GameStream.Pieces[0];
            var gameField = ImmutableHashSet<Point>.Empty;
            Piece.Drop(currentPiece, GameStream.Width);
            var points = 0;
            if (CheckCollisions(currentPiece,gameField))
            {
                points -= 10;
            }
            GameStream.Commands
                .ForEach(command =>
                {
                    if (command == 'P')
                        PrintField(currentPiece, gameField, GameStream.Width, GameStream.Height);
                    else
                    {
                        Actions[command](currentPiece);
                        if (CheckCollisions(currentPiece, gameField))
                        {
                            Actions[GameStream.ReverseCommands[command]](currentPiece);
                            gameField = PlacePiece(currentPiece, gameField);
                            gameField = CheckFilledLines(gameField, GameStream.Width, GameStream.Height, ref points);
                            pieceCounter++;
                            currentPiece = GameStream.Pieces[pieceCounter % GameStream.Pieces.Count()];
                            Piece.Drop(currentPiece, GameStream.Width);
                            if (CheckCollisions(currentPiece,gameField))
                            {
                                gameField = ImmutableHashSet<Point>.Empty;
                                points -= 10;
                            }
                        }
                    }
                });
        }

        private void PrintField(Piece currentPiece, ImmutableHashSet<Point> gameField, int width, int height)
        {
            var formattedField = new string[width,height];
            foreach (var cell in gameField)
                formattedField[cell.X, cell.Y] = "#";
            foreach (var cell in currentPiece.AbsoluteCoordinates)
                formattedField[cell.X, cell.Y] = "*";
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                    Console.WriteLine(formattedField[i, j] ?? ".");
                Console.WriteLine();
            }
        }


        private ImmutableHashSet<Point> CheckFilledLines(ImmutableHashSet<Point> gameField, int width, int height, ref int points)
        {
            var tempPoints = points;
            Enumerable.Range(0, height)
                .Where(index => gameField
                                        .Where(cell => cell.Y == index)
                                        .Count() == width)
                .Process(index =>
                {
                    gameField = gameField.Where(cell => cell.Y != index).ToImmutableHashSet();
                    tempPoints += 1;
                })
                .ForEach(index => gameField = gameField
                                        .Select(cell => index > cell.Y ? cell.WithY(cell.Y - 1) : cell)
                                        .ToImmutableHashSet<Point>());
            points = tempPoints;
            return gameField;
        }

        private ImmutableHashSet<Point> PlacePiece(Piece piece, ImmutableHashSet<Point> gameField)
        {
            return 
                gameField.Concat(piece.AbsoluteCoordinates).ToImmutableHashSet();
        }
        private bool CheckCollisions(Piece piece, ImmutableHashSet<Point> gameField)
        {
            return
                !gameField.Intersect(piece.AbsoluteCoordinates).IsEmpty;
        }
    }


    public sealed class GameStream
    {
        public int Width { get; }
        public int Height { get; }
        public ImmutableArray<Piece> Pieces { get; }
        public ImmutableArray<char> Commands { get; }
        public ImmutableDictionary<char, char> ReverseCommands { get; } = new Dictionary<char, char>()
        {
            {'A', 'D'},
            {'S', 'W'},
            {'D', 'A'},
            {'W', 'S'},
            {'Q', 'E'},
            {'E', 'Q'}
        }
        .ToImmutableDictionary();
        public GameStream(string jsonFilePath)
        {
            using (StreamReader jsonReader = File.OpenText(jsonFilePath))
            {
                JObject jObject = JObject.Parse(jsonReader.ReadToEnd());
                Width = (int)jObject["Width"];
                Height = (int)jObject["Height"];
                Commands = ((string)jObject["Commands"])
                    .ToImmutableArray();
                Pieces = jObject["Pieces"]
                    .Select(x => (Piece)x["Cells"]
                            .Select(cell => new Point((int)cell["X"], (int)cell["Y"]))
                            .ToImmutableArray())
                    .ToImmutableArray();
            }
        }
    }
    public sealed class Piece
    {
        public ImmutableArray<Point> RelativeCoordinates{ get; }
        public ImmutableArray<Point> AbsoluteCoordinates { get; }
        static public explicit operator Piece(ImmutableArray<Point> cells)
        {
            return new Piece(cells);
        }
        public Piece(ImmutableArray<Point> cells)
        {
            RelativeCoordinates = cells;
        }
        public static void Move(Piece piece, DirectionOfMovement directionOfMovement)
        {
            throw new NotImplementedException();
        }
        public static void Rotate(Piece piece, DirectionOfRotation directionOfRotation)
        {
            throw new NotImplementedException();
        }
        public static void Drop(Piece piece, int width)
        {
            throw new NotImplementedException();
        }
    }
    public struct Point
    {
        public int X { get;}
        public int Y { get;}
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Point WithY(int y)
        {
            return new Point(X, y);
        }
    }
    public enum DirectionOfMovement
    {
        Left,
        Right,
        Down,
        Up
    }
    public enum DirectionOfRotation
    {
        Clockwise,
        Anticlockwise
    }
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (T item in value)
            {
                action(item);
            }
        }
        public static IEnumerable<T> Process<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (T item in value)
            {
                action(item);
                yield return item;
            }
        }
    }
}
