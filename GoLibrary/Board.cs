using System;
using System.Collections.Generic;

namespace GoLibrary
{
    public delegate void OnStoneRemoval(int y, int x);
    public delegate void OnStoneAddition(int y, int x, Stone player);

    public enum Stone { Empty, Black, White };

    public struct Point
    {
        public int X;
        public int Y;
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Board
    {
        public Stone[,] board;

        public readonly int Size;

        public Board(int size)
        {
            Size = size;

            board = new Stone[Size, Size];

            Reset();

        }

        public void Reset()
        {
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    board[i, j] = Stone.Empty;
                }
            }
        }

        public Stone Other(Stone stone)
        {
            if (stone == Stone.Empty) return stone;
            else if (stone == Stone.Black) return Stone.White;
            else return Stone.Black;
        }

        public bool IsSurrounded(int y, int x, Stone stone)
        {

            return (!IsWithinBounds(y + 1, x) || board[y + 1, x] == Other(stone)) &&
                   (!IsWithinBounds(y, x + 1) || board[y, x + 1] == Other(stone)) &&
                   (!IsWithinBounds(y - 1, x) || board[y - 1, x] == Other(stone)) &&
                   (!IsWithinBounds(y, x - 1) || board[y, x - 1] == Other(stone));
        }

        public bool Dead(int y, int x, Stone stone)
        {
            if (!IsWithinBounds(y, x)) return false;

            Queue<Tuple<int, int>> toVisit = new Queue<Tuple<int, int>>();
            toVisit.Enqueue(new Tuple<int, int>(y, x));

            bool[,] visited = new bool[Size, Size];
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    visited[i, j] = false;
                }
            }
            visited[y, x] = true;

            while (toVisit.Count > 0)
            {
                var curr = toVisit.Dequeue();

                if (board[curr.Item1, curr.Item2] == Stone.Empty) return false;

                else if (board[curr.Item1, curr.Item2] != stone) continue;

                if (IsWithinBounds(curr.Item1, curr.Item2 + 1) && !visited[curr.Item1, curr.Item2 + 1])
                {
                    visited[curr.Item1, curr.Item2 + 1] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1, curr.Item2 + 1));
                }

                if (IsWithinBounds(curr.Item1 + 1, curr.Item2) && !visited[curr.Item1 + 1, curr.Item2])
                {
                    visited[curr.Item1 + 1, curr.Item2] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1 + 1, curr.Item2));
                }

                if (IsWithinBounds(curr.Item1, curr.Item2 - 1) && !visited[curr.Item1, curr.Item2 - 1])
                {
                    visited[curr.Item1, curr.Item2 - 1] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1, curr.Item2 - 1));
                }

                if (IsWithinBounds(curr.Item1 - 1, curr.Item2) && !visited[curr.Item1 - 1, curr.Item2])
                {
                    visited[curr.Item1 - 1, curr.Item2] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1 - 1, curr.Item2));
                }
            }

            return true;
        }

        public List<Tuple<int, int>> Capture(int y, int x, Stone player)
        {
            Queue<Tuple<int, int>> toVisit = new Queue<Tuple<int, int>>();

            List<Tuple<int, int>> toCapture = new List<Tuple<int, int>>();

            toVisit.Enqueue(new Tuple<int, int>(y, x));

            bool[,] visited = new bool[Size, Size];
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    visited[i, j] = false;
                }
            }
            visited[y, x] = true;

            while (toVisit.Count > 0)
            {
                var curr = toVisit.Dequeue();

                if (board[curr.Item1, curr.Item2] == Stone.Empty)
                {
                    return null;
                }
                else if (board[curr.Item1, curr.Item2] != player) continue;
                else toCapture.Add(curr);

                if (IsWithinBounds(curr.Item1, curr.Item2 + 1) && !visited[curr.Item1, curr.Item2 + 1])
                {
                    visited[curr.Item1, curr.Item2 + 1] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1, curr.Item2 + 1));
                }

                if (IsWithinBounds(curr.Item1 + 1, curr.Item2) && !visited[curr.Item1 + 1, curr.Item2])
                {
                    visited[curr.Item1 + 1, curr.Item2] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1 + 1, curr.Item2));
                }

                if (IsWithinBounds(curr.Item1, curr.Item2 - 1) && !visited[curr.Item1, curr.Item2 - 1])
                {
                    visited[curr.Item1, curr.Item2 - 1] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1, curr.Item2 - 1));
                }

                if (IsWithinBounds(curr.Item1 - 1, curr.Item2) && !visited[curr.Item1 - 1, curr.Item2])
                {
                    visited[curr.Item1 - 1, curr.Item2] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1 - 1, curr.Item2));
                }
            }
            foreach (var stone in toCapture)
            {
                DeleteStone(stone);
            }
            return toCapture;
        }

        public List<Tuple<int, int>> CaptureSurrounding(int y, int x, Stone player)
        {
            List<Tuple<int, int>> toRemove = new List<Tuple<int, int>>();
            if (IsWithinBounds(y + 1, x) && board[y + 1, x] == player)
            {
                var list = Capture(y + 1, x, player);
                if (list != null)
                {
                    toRemove.AddRange(list);
                }
            }
            if (IsWithinBounds(y, x + 1) && board[y, x + 1] == player)
            {
                var list = Capture(y, x + 1, player);
                if (list != null)
                {
                    toRemove.AddRange(list);
                }
            }
            if (IsWithinBounds(y - 1, x) && board[y - 1, x] == player)
            {
                var list = Capture(y - 1, x, player);
                if (list != null)
                {
                    toRemove.AddRange(list);
                }
            }
            if (IsWithinBounds(y, x - 1) && board[y, x - 1] == player)
            {
                var list = Capture(y, x - 1, player);
                if (list != null)
                {
                    toRemove.AddRange(list);
                }
            }
            return toRemove;
        }

        public void DeleteStone(int y, int x)
        {
            board[y, x] = Stone.Empty;
        }

        public void DeleteStone(Tuple<int, int> pos)
        {
            board[pos.Item1, pos.Item2] = Stone.Empty;
        }

        public bool AddStone(int y, int x, Stone player)
        {
            if (IsEmpty(y, x))
            {
                board[y, x] = player;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsWithinBounds(int y, int x)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size;
        }

        public bool IsEmpty(int y, int x)
        {
            if (!IsWithinBounds(y, x)) return false;
            return board[y, x] == Stone.Empty;
        }

        public Board Clone()
        {
            Board newBoard = new Board(Size);
            newBoard.board = (Stone[,])board.Clone();
            return newBoard;
        }

        public void DisplayInConsole()
        {
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    if (board[i, j] == Stone.Black)
                        Console.Write("B ");
                    else if (board[i, j] == Stone.White)
                        Console.Write("W ");
                    else
                        Console.Write("E ");
                }
                Console.WriteLine();
            }
        }
    }
}
