using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class TicTacToe
    {
        public enum Symbol
        {
            None = 0,
            O = 1,
            X = 2
        }

        public struct IntPoint
        {
            public int X, Y;
            public IntPoint(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        public class State : IEquatable<State>
        {
            public int LineLen;
            public int W, H;
            public Symbol[,] Board;

            public State(int w, int h, int lineLen)
            {
                this.W = w;
                this.H = h;
                this.LineLen = lineLen;
                this.Board = new Symbol[h, w];
            }

            public State(int lineLen, int[,] board) : this(board.GetLength(1), board.GetLength(0), lineLen)
            {
                for (int i = 0; i < this.H; i++)
                    for (int j = 0; j < this.W; j++)
                        this.Board[i, j] = (Symbol)board[i, j];
            }

            public State Clone()
            {
                State c = new State(this.W, this.H, this.LineLen);
                Array.Copy(this.Board, c.Board, this.Board.Length);
                return c;
            }

            public override Int32 GetHashCode()
            {
                int hash = 0;
                int pow = 1;
                for (int i = 0; i < H; i++)
                    for (int j = 0; j < W; j++)
                    {
                        hash += ((int)Board[i, j]) * pow;
                        pow *= 3;
                    }   
                return hash;
            }

            public bool Equals(State other)
            {
                for (int i = 0; i < H; i++)
                    for (int j = 0; j < W; j++)
                        if (other.Board[i, j] != this.Board[i, j]) return false;
                return true;
            }

            Symbol IsLineLenExists(int x0, int y0, int dx, int dy)
            {
                int count = 0;
                Symbol s = Symbol.None;
                for (int i = y0, j = x0; i >= 0 && i < H && j >= 0 && j < W; i += dy, j += dx)
                {
                    if (Board[i, j] != s)
                    {
                        count = 1;
                        s = Board[i, j];
                    }
                    else
                    {
                        count++;
                        if (s != Symbol.None && count == LineLen)
                            return s;
                    }
                }
                return Symbol.None;
            }

            public Symbol WhoWon()
            {
                for (int i = 0; i < H; i++)
                {
                    Symbol s = IsLineLenExists(0, i, 1, 0);
                    if (s != Symbol.None) return s;
                }
                for (int j = 0; j < W; j++)
                {
                    Symbol s = IsLineLenExists(j, 0, 0, 1);
                    if (s != Symbol.None) return s;
                }
                for (int i = 0; i < H - LineLen + 1; i++)
                {
                    Symbol s = IsLineLenExists(0, i, 1, 1);
                    if (s != Symbol.None) return s;
                    s = IsLineLenExists(0, H - i - 1, 1, -1);
                    if (s != Symbol.None) return s;
                }
                for (int j = 1; j < W - LineLen + 1; j++)
                {
                    Symbol s = IsLineLenExists(j, 0, 1, 1);
                    if (s != Symbol.None) return s;
                    s = IsLineLenExists(j, H - 1, 1, -1);
                    if (s != Symbol.None) return s;
                }
                return Symbol.None;
            }

            public List<IntPoint> GetFreePositions()
            {
                List<IntPoint> ps = new List<IntPoint>();
                for (int i = 0; i < this.H; i++)
                    for (int j = 0; j < this.W; j++)
                        if (Board[i, j] == Symbol.None)
                            ps.Add(new IntPoint(j, i));
                return ps;                
            }
        }

        public class Agent
        {
            public double LR;
            public double Eps;
            public Dictionary<State, double> V = new Dictionary<State, double>();

            Random rng;

            public Agent(double lr, double eps)
            {
                this.LR = lr;
                this.Eps = eps;
                this.rng = new Random();
            }

            public void TrainOneEpisode(State s, IAdversary adversary)
            {
                while(true)
                {
                    double reward;
                    s = adversary.MakeMove(s);
                    List<IntPoint> freePos = s.GetFreePositions();
                    if (freePos.Count == 0)
                        reward = 0;
                    else
                    {                       
                        if(rng.NextDouble() < this.Eps)
                        {
                            IntPoint p = freePos[rng.Next(freePos.Count)];
                        }
                    }
                }
            }
        }

        public interface IAdversary
        {
            State MakeMove(State s);
        }

        public class RandomAdversary : IAdversary
        {
            Random rng;
            Symbol symbol;

            public RandomAdversary(Symbol symbol)
            {
                this.symbol = symbol;
                rng = new Random();
            }

            public State MakeMove(State s)
            {
                List<IntPoint> freePositions = s.GetFreePositions();
                State n = s.Clone();
                IntPoint p = freePositions[rng.Next(freePositions.Count)];
                n.Board[p.Y, p.X] = symbol;
                return n;
            }
        }
    }

    static class TestsTicTacToe
    {
        public static void Test1()
        {
            var st = new TicTacToe.State(3, new int[,]
                {
                    {0,0,0},
                    {0,0,0},
                    {0,0,0}
                });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.None);

            st = new TicTacToe.State(3, new int[,]
                {
                    {0,0,0},
                    {1,1,1},
                    {2,0,0}
                });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.O);

            st = new TicTacToe.State(3, new int[,]
                {
                    {0,0,2},
                    {1,1,2},
                    {0,0,2}
                });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.X);

            st = new TicTacToe.State(3, new int[,]
                {
                    {1,0,2},
                    {1,1,2},
                    {0,0,1}
                });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.O);

            st = new TicTacToe.State(3, new int[,]
                {
                    {1,0,2},
                    {1,2,2},
                    {0,0,1}
                });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.None);

            st = new TicTacToe.State(3, new int[,]
               {
                    {1,1,2},
                    {1,2,2},
                    {2,1,1}
               });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.X);

            st = new TicTacToe.State(3, new int[,]
               {
                    {1,1,2,0},
                    {1,2,2,0},
                    {2,0,1,0},
                    {2,1,1,0}
               });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.X);

            st = new TicTacToe.State(3, new int[,]
               {
                    {1,1,2,0},
                    {1,2,2,0},
                    {0,0,1,0},
                    {2,1,1,0}
               });
            Debug.Assert(st.WhoWon() == TicTacToe.Symbol.None);
        }
    }
}
