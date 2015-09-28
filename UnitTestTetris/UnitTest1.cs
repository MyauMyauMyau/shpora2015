
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShporaTetris;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Immutable;
namespace UnitTestTetris
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestPointRotate()
        {
            Point p = new Point(1, 2, 3, 4);
            var p2 = p.Rotate(DirectionOfRotation.Anticlockwise);
            Point expectedP = new Point(1, 2, 4, 1);
            Assert.AreEqual(p2, expectedP);
            var p3 = p2.Rotate(DirectionOfRotation.Clockwise);
            Assert.AreEqual(p3, p, p3.AbsX + " " + p3.AbsY);

            p = new Point(0, 0, 1, 2);
            p2 = p.Rotate(DirectionOfRotation.Clockwise);
            expectedP = new Point(959, 0, 1, 2);
            Assert.AreEqual(p2, expectedP);

            p = new Point(0, 1, 3, 1);
            p2 = p.Rotate(DirectionOfRotation.Clockwise);
            expectedP = new Point(0, 9999, 2, 0);
            Assert.AreEqual(p2, expectedP, p2.AbsX + " " + p2.AbsY);
        }
        [TestMethod]
        public void TestPointMove()
        {
            var p = new Point(1, 2, 3, 4);
            p = p.Move(1, 5);
            var expected = new Point(1, 2, 4, 9);
            Assert.AreEqual(p, expected);
            p = p.Move(0, -2);
            expected = new Point(0, 2, 4, 7);
            Assert.AreEqual(p, expected);
        }
        [TestMethod]
        public void TestPieceDrop()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0),
                new Point(0,1),
                new Point(0,2),
                new Point(1,2)
            }.ToImmutableArray());
            var p1 = piece.Drop(8);
            var expected = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());
            Assert.AreEqual(p1,expected, p1.Coordinates[1].AbsX +" " + p1.Coordinates[1].AbsY);
        }
        [TestMethod]
        public void TestPieceDrop2()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0),
                new Point(0,-1),
                new Point(0,2),
                new Point(1,2)
            }.ToImmutableArray());
            var p1 = piece.Drop(8);
            var expected = new Piece(new List<Point>()
            {
                new Point(0,0,3,1),
                new Point(0,0,3,0),
                new Point(0,0,3,3),
                new Point(0,0,4,3)
            }.ToImmutableArray());
            Assert.AreEqual(p1, expected, p1.Coordinates[1].AbsX + " " + p1.Coordinates[1].AbsY);
        }
        [TestMethod]
        public void TestPieceDrop3()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0),
                new Point(0,-1),
                new Point(0,2),
                new Point(1,2)
            }.ToImmutableArray());
            var p1 = piece.Drop(9);
            var expected = new Piece(new List<Point>()
            {
                new Point(0,0,3,1),
                new Point(0,0,3,0),
                new Point(0,0,3,3),
                new Point(0,0,4,3)
            }.ToImmutableArray());
            Assert.AreEqual(p1, expected, p1.Coordinates[1].AbsX + " " + p1.Coordinates[1].AbsY);
        }
        [TestMethod]
        public void TestPieceRotate()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());
            var p1 = piece.Rotate(DirectionOfRotation.Anticlockwise);
            var expected = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,4,0),
                new Point(0,2,5,0),
                new Point(1,2,5,-1)
            }.ToImmutableArray());
            Assert.AreEqual(p1, expected, p1.Coordinates[1].AbsX + " " + p1.Coordinates[1].AbsY);
        }
        [TestMethod]
        public void TestPieceRotate2()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());
            var p1 = piece.Rotate(DirectionOfRotation.Clockwise);
            var expected = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,2,0),
                new Point(0,2,1,0),
                new Point(1,2,1,1)
            }.ToImmutableArray());
            Assert.AreEqual(p1, expected, p1.Coordinates[2].AbsX + " " + p1.Coordinates[2].AbsY);
        }
        //[TestMethod]
        //public void TestPieceRotate3()
        //{
        //    var piece = new Piece(new List<Point>()
        //    {
        //        new Point(0,0,0,0),
        //        new Point(1,0,1,0),
        //        new Point(0,1,0,1),
        //        new Point(1,1,1,1)
        //    }.ToImmutableArray());
        //    var p1 = piece.Rotate(DirectionOfRotation.Clockwise);
        //    var expected = new Piece(new List<Point>()
        //    {
        //        new Point(0,0,0,0),
        //        new Point(1,0,0,1),
        //        new Point(0,1,-1,1),
        //        new Point(1,1,0,1)
        //    }.ToImmutableArray());
        //    Assert.AreEqual(p1, expected, p1.Coordinates[2].AbsX + " " + p1.Coordinates[2].AbsY);
        //}
        [TestMethod]
        public void TestPieceMove()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());
            var p1 = piece.Move(DirectionOfMovement.Up);
            var expected = new Piece(new List<Point>()
            {
                new Point(0,0,3,-1),
                new Point(0,1,3,0),
                new Point(0,2,3,1),
                new Point(1,2,4,1)
            }.ToImmutableArray());
            Assert.AreEqual(p1, expected, p1.Coordinates[2].AbsX + " " + p1.Coordinates[2].AbsY);
        }
        [TestMethod]
        public void TestPieceMove2()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());
            var p1 = piece.Move(DirectionOfMovement.Left);
            var expected = new Piece(new List<Point>()
             {
                new Point(0,0,2,0),
                new Point(0,1,2,1),
                new Point(0,2,2,2),
                new Point(1,2,3,2)
            }.ToImmutableArray());
            Assert.AreEqual(p1, expected, p1.Coordinates[2].AbsX + " " + p1.Coordinates[2].AbsY);
        }
        [TestMethod]
        public void TestCheckCollisions()
        {
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());

            var field = new List<Point>()
            {
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,4,2),
                new Point(0,0,3,2)
            }.ToImmutableHashSet();
            var field2 = new List<Point>()
            {
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,4,0),
                new Point(0,0,3,3)
            }.ToImmutableHashSet();
            var t = new Tetris(@"C:\Users\Meow\Desktop\git\ShporaTetris\ShporaTetris\bin\Debug\tests\smallest.json");
            Assert.AreEqual(true, t.CheckCollisions(piece, field, 6, 5));
            Assert.AreEqual(false, t.CheckCollisions(piece, field2,6,5));
        }
        [TestMethod]
        public void TestCheckFilledLines()
        {
            var t = new Tetris(@"C:\Users\Meow\Desktop\git\ShporaTetris\ShporaTetris\bin\Debug\tests\smallest.json");
            var points = 0; 
            var field = new List<Point>()
            {
                new Point(0,0,0,3),
                new Point(0,0,1,3),
                new Point(0,0,1,4),
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,2,3),
                new Point(0,0,3,3),
                new Point(0,0,4,1),
                new Point(0,0,4,3),
                new Point(0,0,5,3),

            }.ToImmutableHashSet();
            var result = t.CheckFilledLines(field, 6, out points);
            var expected = new List<Point>()
            {
                new Point(0,0,1,4),
                new Point(0,0,2,1),
                new Point(0,0,2,2),
                new Point(0,0,4,2)
            }.ToImmutableHashSet();
            var isCorrect = expected.SetEquals(result);
            Assert.AreEqual(true, isCorrect, result.ElementAt(3).AbsX +" " + result.ElementAt(3).AbsY);
            Assert.AreEqual(1, points);
        }
        [TestMethod]
        public void TestCheckFilledLines2()
        {
            var t = new Tetris(@"C:\Users\Meow\Desktop\git\ShporaTetris\ShporaTetris\bin\Debug\tests\smallest.json");
            var points = 10;
            var field = new List<Point>()
            {
                new Point(0,0,0,3),
                new Point(0,0,1,3),
                new Point(0,0,1,4),
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,2,3),
                new Point(0,0,3,3),
                new Point(0,0,4,1),
                new Point(0,0,4,3),
                new Point(0,0,5,4),

            }.ToImmutableHashSet();
            var result = t.CheckFilledLines(field, 6,out points);
            var expected = new List<Point>()
            {
                new Point(0,0,0,3),
                new Point(0,0,1,3),
                new Point(0,0,1,4),
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,2,3),
                new Point(0,0,3,3),
                new Point(0,0,4,1),
                new Point(0,0,4,3),
                new Point(0,0,5,4),
            }.ToImmutableHashSet();
            var isCorrect = expected.SetEquals(result);
            Assert.AreEqual(true, isCorrect, result.ElementAt(3).AbsX + " " + result.ElementAt(3).AbsY);
            Assert.AreEqual(0, points);
        }
        [TestMethod]
        public void TestPlacePiece()
        {
            var field = new List<Point>()
            {
                new Point(0,0,0,3),
                new Point(0,0,1,3),
                new Point(0,0,1,4),
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,2,3),
                new Point(0,0,3,3),
                new Point(0,0,4,1),
                new Point(0,0,4,3),
                new Point(0,0,5,4),

            }.ToImmutableHashSet();
            var piece = new Piece(new List<Point>()
            {
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)
            }.ToImmutableArray());
            var t = new Tetris(@"C:\Users\Meow\Desktop\git\ShporaTetris\ShporaTetris\bin\Debug\tests\smallest.json");
            var expected = new List<Point>()
            {
                new Point(0,0,0,3),
                new Point(0,0,1,3),
                new Point(0,0,1,4),
                new Point(0,0,2,0),
                new Point(0,0,2,1),
                new Point(0,0,2,3),
                new Point(0,0,3,3),
                new Point(0,0,4,1),
                new Point(0,0,4,3),
                new Point(0,0,5,4),
                new Point(0,0,3,0),
                new Point(0,1,3,1),
                new Point(0,2,3,2),
                new Point(1,2,4,2)

            }.ToImmutableHashSet();
            var isTrue = expected.SetEquals(t.PlacePiece(piece, field));
            Assert.AreEqual(true, isTrue);
        }
    }
}
