using System;
using System.Collections.Generic;
using System.Linq;

namespace GoLibrary
{
    //public enum Stone { Black, White, Empty };

    public class Player
    {
        public Stone Color;
        private int _score;

        public Player(Stone color)
        {
            Color = color;
            _score = 0;
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }
    }

    public delegate void OnMoveEventHandler(List<Tuple<int, int>> toRemove, int y, int x, Player player);
    public delegate void OnPlayerChangedEventHandler(Stone color);
    public delegate void OnGameEndEventHandler(int blackPlayerScore, int whitePlayerScore);

    public class GoGame
    {
        Player _blackPlayer;
        Player _whitePlayer;
        Player _currentPlayer;

        Board board;
        Board previousBoard;

        bool _passed;

        public event OnMoveEventHandler OnMoveEvent;
        public event OnPlayerChangedEventHandler OnPlayerChangedEvent;
        public event OnGameEndEventHandler OnGameEndEvent;

        public Stone CurrentPlayer { get { return _currentPlayer.Color; } }
        public int CurrentPlayerScore { get { return _currentPlayer.Score; } }
        public int BlackPlayerScore { get { return _blackPlayer.Score; } }
        public int WhitePlayerScore { get { return _whitePlayer.Score; } }

        public GoGame(int size)
        {
            board = new Board(size);
            previousBoard = new Board(size);
            _blackPlayer = new Player(Stone.Black);
            _whitePlayer = new Player(Stone.White);
            _currentPlayer = _blackPlayer;
            _passed = false;
        }

        public void Reset()
        {
            board.Reset();
            previousBoard.Reset();
            _blackPlayer.Score = 0;
            _whitePlayer.Score = 0;
            _passed = false;
        }

        public void GameEnd()
        {
            int black = 0;
            int white = 0;
            bool[,] visited = new bool[board.Size, board.Size];

            for (int i = 0; i < board.Size; ++i)
            {
                for (int j = 0; j < board.Size; ++j)
                {
                    visited[i, j] = false;
                }
            }

            for (int i = 0; i < board.Size; ++i)
            {
                for (int j = 0; j < board.Size; ++j)
                {
                    if (!visited[i, j] && board.IsEmpty(i, j))
                    {
                        GetPoints(i, j, visited, ref white, ref black);
                    }
                }
            }
            _blackPlayer.Score += black;
            _whitePlayer.Score += white;
            if (OnGameEndEvent != null)
                OnGameEndEvent(BlackPlayerScore, WhitePlayerScore);
        }

        public void GetPoints(int y, int x, bool[,] visited, ref int white, ref int black)
        {
            int score = 0;
            Stone color = Stone.Empty;
            bool onePlayer = true;

            Queue<Tuple<int, int>> toVisit = new Queue<Tuple<int, int>>();
            toVisit.Enqueue(new Tuple<int, int>(y, x));

            visited[y, x] = true;

            while (toVisit.Count > 0)
            {
                var curr = toVisit.Dequeue();

                var currColor = board.board[curr.Item1, curr.Item2];
                if (currColor != Stone.Empty && color == Stone.Empty)
                {
                    color = currColor;
                    visited[curr.Item1, curr.Item2] = false;
                    continue;
                }
                else if (currColor == Stone.Empty)
                {
                    ++score;
                }
                else if (currColor != color)
                {
                    onePlayer = false;
                    visited[curr.Item1, curr.Item2] = false;
                    continue;
                }
                else
                {
                    visited[curr.Item1, curr.Item2] = false;
                    continue;
                }

                if (board.IsWithinBounds(curr.Item1, curr.Item2 + 1) && !visited[curr.Item1, curr.Item2 + 1])
                {
                    visited[curr.Item1, curr.Item2 + 1] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1, curr.Item2 + 1));
                }

                if (board.IsWithinBounds(curr.Item1 + 1, curr.Item2) && !visited[curr.Item1 + 1, curr.Item2])
                {
                    visited[curr.Item1 + 1, curr.Item2] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1 + 1, curr.Item2));
                }

                if (board.IsWithinBounds(curr.Item1, curr.Item2 - 1) && !visited[curr.Item1, curr.Item2 - 1])
                {
                    visited[curr.Item1, curr.Item2 - 1] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1, curr.Item2 - 1));
                }

                if (board.IsWithinBounds(curr.Item1 - 1, curr.Item2) && !visited[curr.Item1 - 1, curr.Item2])
                {
                    visited[curr.Item1 - 1, curr.Item2] = true;
                    toVisit.Enqueue(new Tuple<int, int>(curr.Item1 - 1, curr.Item2));
                }
            }
            if (onePlayer && color != Stone.Empty)
            {
                if (color == Stone.Black) black += score;
                else white += score;
            }
        }

        public List<Tuple<int, int>> TryMove(int y, int x, Stone player)
        {
            if (board.IsWithinBounds(y, x) && board.IsEmpty(y, x))
            {
                Board newBoard = board.Clone();
                newBoard.board[y, x] = player;

                if (newBoard.IsSurrounded(y, x, player))
                {
                    if (newBoard.Dead(y + 1, x, Other(player)) || newBoard.Dead(y, x + 1, Other(player)) ||
                        newBoard.Dead(y - 1, x, Other(player)) || newBoard.Dead(y, x - 1, Other(player)))
                    {
                        var toRemove = newBoard.CaptureSurrounding(y, x, Other(player));
                        if (!Ko(newBoard))
                        {
                            previousBoard = board;
                            board = newBoard;
                            return toRemove;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (newBoard.Dead(y, x, player))
                {
                    if ((newBoard.board[y + 1, x] == Other(player) && newBoard.Dead(y + 1, x, Other(player))) ||
                        (newBoard.board[y, x + 1] == Other(player) && newBoard.Dead(y, x + 1, Other(player))) ||
                        (newBoard.board[y - 1, x] == Other(player) && newBoard.Dead(y - 1, x, Other(player))) ||
                        (newBoard.board[y, x - 1] == Other(player) && newBoard.Dead(y, x - 1, Other(player))))
                    {
                        var toRemove = newBoard.CaptureSurrounding(y, x, Other(player));
                        if (!Ko(newBoard))
                        {
                            previousBoard = board;
                            board = newBoard;
                            return toRemove;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    var toRemove = newBoard.CaptureSurrounding(y, x, Other(player));
                    if (!Ko(newBoard))
                    {

                        previousBoard = board;
                        board = newBoard;
                        return toRemove;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public bool Move(int y, int x)
        {
            var list = TryMove(y, x, _currentPlayer.Color);
            if (list != null)
            {
                _currentPlayer.Score += list.Count;
                if (OnMoveEvent != null)
                    OnMoveEvent(list, y, x, _currentPlayer);
                ChangePlayer();
                _passed = false;
                return true;
            }
            return false;
        }

        public void Pass()
        {
            if (_passed)
            {
                GameEnd();
            }
            else
            {
                _passed = true;
                ChangePlayer();
            }
        }

        public bool Ko(Board newBoard)
        {
            return Enumerable.Range(0, previousBoard.board.Rank).All(dimension => previousBoard.board.GetLength(dimension) == newBoard.board.GetLength(dimension)) &&
                    previousBoard.board.Cast<Stone>().SequenceEqual(newBoard.board.Cast<Stone>());
        }

        public void ChangePlayer()
        {
            if (_currentPlayer.Color == Stone.Black)
            {
                _currentPlayer = _whitePlayer;
            }
            else
            {
                _currentPlayer = _blackPlayer;
            }

            if (OnPlayerChangedEvent != null)
                OnPlayerChangedEvent(_currentPlayer.Color);
        }

        public Stone Other(Stone player)
        {
            if (player == Stone.Empty) return player;
            else if (player == Stone.Black) return Stone.White;
            else return Stone.Black;
        }
    }
}
