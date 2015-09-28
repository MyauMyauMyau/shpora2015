using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
namespace ShporaTetris
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Stopwatch();
            t.Start();
            //var tetris = new Tetris(@"tests/cubes-w1000-h1000-c1000000.json");
            var tetris = new Tetris(@"tests/clever-w20-h25-c100000.json");
            
            tetris.Play();
            Console.WriteLine(t.Elapsed);
            t.Stop();
        }
    }


    public sealed class Tetris
    {
        public GameStream GameStream { get; }
        private ImmutableDictionary<char, Func<Piece, Piece>> Actions { get; }
        public Tetris(string jsonFilePath)
        {
            GameStream = new GameStream(jsonFilePath);
            Actions = new Dictionary<char, Func<Piece, Piece>>()
            {
                {'A', currentPiece => currentPiece.Move(DirectionOfMovement.Left)},
                {'D', currentPiece => currentPiece.Move(DirectionOfMovement.Right)},
                {'S', currentPiece => currentPiece.Move(DirectionOfMovement.Down)},
                {'W', currentPiece => currentPiece.Move(DirectionOfMovement.Up)},
                {'Q', currentPiece => currentPiece.Rotate(DirectionOfRotation.Anticlockwise)},
                {'E', currentPiece => currentPiece.Rotate(DirectionOfRotation.Clockwise)}
            }
            .ToImmutableDictionary();
        }

        public void Play()
        {
            var commandCount = -1;
            var pieceCounter = 0;
            var currentPiece = GameStream.Pieces[0];
            var gameField = ImmutableHashSet<Point>.Empty;
            currentPiece = currentPiece.Drop(GameStream.Width);
            var totalPoints = 0;
            int bonusPoints = 0;
            if (CheckCollisions(currentPiece, gameField, GameStream.Width, GameStream.Height))
            {
                totalPoints -= 10;
            }
            foreach (var command in GameStream.Commands)
            {
                commandCount++;
                if (command == 'P')
                    PrintField(currentPiece, gameField, GameStream.Width, GameStream.Height);
                else
                {
                    currentPiece = Actions[command](currentPiece);
                        
                    if (CheckCollisions(currentPiece, gameField, GameStream.Width, GameStream.Height))
                    {
                        currentPiece = Actions[GameStream.ReverseCommands[command]](currentPiece);
                        gameField = PlacePiece(currentPiece, gameField);
                        gameField = CheckFilledLines(gameField, GameStream.Width, out bonusPoints);
                        totalPoints += bonusPoints;
                        pieceCounter++;
                        currentPiece = GameStream.Pieces[pieceCounter % GameStream.Pieces.Count()];
                        currentPiece = currentPiece.Drop(GameStream.Width);

                        if (CheckCollisions(currentPiece, gameField, GameStream.Width, GameStream.Height))
                        {
                            gameField = ImmutableHashSet<Point>.Empty;
                            totalPoints -= 10;
                        }
                        Console.WriteLine(commandCount + " " + totalPoints);
                    }
                }
            }
        }

        private void PrintField(Piece currentPiece, ImmutableHashSet<Point> gameField, int width, int height)
        {
            var formattedField = new string[width, height];
            foreach (var cell in gameField)
                formattedField[cell.AbsX, cell.AbsY] = "#";
            foreach (var cell in currentPiece.Coordinates)
                formattedField[cell.AbsX, cell.AbsY] = "*";
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    Console.Write(formattedField[j, i] ?? ".");
                Console.WriteLine();
            }
        }
        public ImmutableHashSet<Point> CheckFilledLines(ImmutableHashSet<Point> gameField, int width, out int bonusPoints)
        {
            var fallingHeight = 0;
            gameField = gameField
                .GroupBy(c => c.AbsY)
                .OrderByDescending(g => g.Key)
                .SelectMany(g =>
                {
                    if (g.Count() == width)
                    {
                        fallingHeight++;
                        return Enumerable.Empty<Point>();
                    }
                    else
                        return g.Select(x => x.Move(0, fallingHeight));
                })
                .ToImmutableHashSet();
            bonusPoints = fallingHeight;
            return gameField;
        }

        public ImmutableHashSet<Point> PlacePiece(Piece piece, ImmutableHashSet<Point> gameField)
        {
            return
                gameField.Concat(piece.Coordinates).ToImmutableHashSet();
        }

        public bool CheckCollisions(Piece piece, ImmutableHashSet<Point> gameField, int width, int height)
        {
            foreach (var cell in piece.Coordinates)
            {
                if (cell.AbsX < 0 || cell.AbsY < 0 
                    || cell.AbsX >= width || cell.AbsY >= height)
                    return true;
            }
            return
                gameField.Overlaps(piece.Coordinates);
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
        public ImmutableArray<Point> Coordinates{ get; }
        static public explicit operator Piece(ImmutableArray<Point> cells)
        {
            return new Piece(cells);
        }

        public Piece(ImmutableArray<Point> cells)
        {
            Coordinates = cells;
        }

        public Piece Move(DirectionOfMovement directionOfMovement)
        {
            var moveDictionary = new Dictionary<DirectionOfMovement, Func<Piece>>()
            {
                {DirectionOfMovement.Down, () =>
                        new Piece(Coordinates.Select(x => x.Move(0,1)).ToImmutableArray())},
                {DirectionOfMovement.Up, () =>
                    new Piece(Coordinates.Select(x => x.Move(0,-1)).ToImmutableArray())},
                {DirectionOfMovement.Left, () =>
                    new Piece(Coordinates.Select(x => x.Move(-1,0)).ToImmutableArray())},
                {DirectionOfMovement.Right, () =>
                    new Piece(Coordinates.Select(x => x.Move(1,0)).ToImmutableArray())}
            };

            return moveDictionary[directionOfMovement]();
        }

        public Piece Rotate(DirectionOfRotation directionOfRotation)
        {
            return new Piece(Coordinates
                                    .Select(x => x.Rotate(directionOfRotation))
                                    .ToImmutableArray());
        }

        public Piece Drop(int width)
        {
            var minX = Coordinates.Min(x=> x.RelX);
            var minY = Coordinates.Min(x => x.RelY);
            var figureWidth =  Coordinates.Max(x => x.RelX) - minX + 1;
            return new Piece(Coordinates
                .Select(c => new Point(c.RelX, c.RelY, c.RelX + (width - figureWidth)/2- minX, c.RelY - minY))
                .ToImmutableArray());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Piece)) return false;
            for (int i = 0; i < Coordinates.Count(); i++)
            {
                if (!Coordinates[i].Equals((obj as Piece).Coordinates[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Coordinates
                .Aggregate(0, (total, next) => total + next.AbsX.GetHashCode() + next.AbsY.GetHashCode());
        }
    }
    public struct Point
    {
        public int RelX { get; }
        public int RelY { get; }
        public int AbsX { get; }
        public int AbsY { get; }

        public Point(int relX, int relY, int absX = 0, int absY = 0)
        {
            RelX = relX;
            RelY = relY;
            AbsX = absX;
            AbsY = absY;
        }

        public Point Move(int xShift, int yShift)
        {
            return new Point(RelX, RelY, AbsX + xShift, AbsY + yShift);
        }

        public Point Rotate(DirectionOfRotation dirOfRotation)
        {
            var x0 = AbsX - RelX;
            var y0 = AbsY - RelY;
            return dirOfRotation == DirectionOfRotation.Clockwise ?
                new Point(-RelY, RelX,x0 - AbsY + y0,y0 + AbsX - x0) :
                new Point(RelY, -RelX, x0 + AbsY - y0, y0 - AbsX + x0);
        }

        public override bool Equals(object obj)
        {
            return obj is Point && AbsX == ((Point)obj).AbsX && AbsY == ((Point)obj).AbsY;
        }

        public override int GetHashCode()
        {
            return AbsX.GetHashCode() + AbsY.GetHashCode();
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

    //public static class IEnumerableExtensions
    //{
    //    public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
    //    {
    //        foreach (T item in value)
    //        {
    //            action(item);
    //        }
    //    }

    //    public static IEnumerable<T> Process<T>(this IEnumerable<T> value, Action<T> action)
    //    {
    //        foreach (T item in value)
    //        {
    //            action(item);
    //            yield return item;
    //        }
    //    }
    //}
}
