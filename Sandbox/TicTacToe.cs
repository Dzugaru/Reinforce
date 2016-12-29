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

            public override String ToString()
            {
                return $"({X}, {Y})";
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

            public override String ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < H; i++)
                {
                    for (int j = 0; j < W; j++)
                    {
                        switch(this.Board[i,j])
                        {
                            case Symbol.None: sb.Append('.'); break;
                            case Symbol.O: sb.Append('O'); break;
                            case Symbol.X: sb.Append('X'); break;
                        }
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
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

            public Symbol? WhoWon()
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

                List<IntPoint> freePos = GetFreePositions();
                if (freePos.Count > 0) return null;
                else return Symbol.None;
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
            public Symbol Symb;
            public double LR;
            public double Eps;
            public Dictionary<State, double> V = new Dictionary<State, double>();

            Random rng;

            public Agent(Symbol s, double lr, double eps, int rngSeed)
            {
                this.Symb = s;
                this.LR = lr;
                this.Eps = eps;
                this.rng = new Random(rngSeed);
            }

            public void TrainOneEpisode(State s, IAdversary adversary)
            {
                while (true)
                {
                    State nextState = adversary.MakeMove(s);
                    Symbol? won = nextState.WhoWon();
                    if (!won.HasValue)
                    {
                        List<IntPoint> freePos = nextState.GetFreePositions();
                        IntPoint p = new IntPoint(-1, -1);
                        if (rng.NextDouble() < this.Eps)                        
                            p = freePos[rng.Next(freePos.Count)];                        
                        else
                            p = GetBestMove(nextState);                        

                        nextState = nextState.Clone();
                        nextState.Board[p.Y, p.X] = Symb;
                    }                    

                    double v = GetValue(s);
                    double nv = GetValue(nextState);                    
                    double updV = v + (nv - v) * LR;
                    V[s] = updV;

                    won = nextState.WhoWon();
                    if (won.HasValue) break;

                    s = nextState;
                }
            }

            public Symbol Play(State start, IAdversary adversary)
            {
                State s = start;
                while (true)
                {
                    s = adversary.MakeMove(s);
                    Symbol? won = s.WhoWon();
                    if (won.HasValue) return won.Value;

                    IntPoint move = GetBestMove(s);
                    s = s.Clone();
                    s.Board[move.Y, move.X] = Symb;
                    won = s.WhoWon();
                    if (won.HasValue) return won.Value;
                }
            }

            double GetValue(State s)
            {               
                double v;
                if(!V.TryGetValue(s, out v))
                {
                    Symbol? won = s.WhoWon();
                    if (!won.HasValue) v = 0.5;
                    else if (won == Symb) v = 1.0;
                    else v = 0.0;
                    
                    V[s] = v;
                }               

                return v;
            }

            public IntPoint GetBestMove(State s)
            {
                IntPoint move = new IntPoint(-1, -1);
                List<IntPoint> freePos = s.GetFreePositions();
                double bestValue = double.MinValue;
                foreach (IntPoint cp in freePos)
                {
                    State ns = s.Clone();
                    ns.Board[cp.Y, cp.X] = Symb;
                    double cv = GetValue(ns);
                    if (cv > bestValue)
                    {
                        bestValue = cv;
                        move = cp;
                    }
                }
                return move;
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

            public RandomAdversary(Symbol symbol, int rngSeed)
            {
                this.symbol = symbol;
                rng = new Random(rngSeed);
            }

            public State MakeMove(State s)
            {
                List<IntPoint> freePositions = s.GetFreePositions();
                if (freePositions.Count == 0) return s;

                State n = s.Clone();
                IntPoint p = freePositions[rng.Next(freePositions.Count)];
                n.Board[p.Y, p.X] = symbol;
                return n;
            }
        }

        public class FirstFreeAdversary : IAdversary
        {           
            Symbol symbol;

            public FirstFreeAdversary(Symbol symbol)
            {
                this.symbol = symbol;                
            }

            public State MakeMove(State s)
            {
                List<IntPoint> freePositions = s.GetFreePositions();
                if (freePositions.Count == 0) return s;

                State n = s.Clone();
                IntPoint p = freePositions[0];
                n.Board[p.Y, p.X] = symbol;
                return n;
            }
        }

        public class SelfAdversary : IAdversary
        {
            Symbol symb;
            Agent agent;

            public SelfAdversary(Symbol symb, Agent agent)
            {
                this.symb = symb;
                this.agent = agent;
            }

            public State MakeMove(State s)
            {
                List<IntPoint> freePositions = s.GetFreePositions();
                if (freePositions.Count == 0) return s;
                
                IntPoint p = agent.GetBestMove(s);
                State n = s.Clone();
                n.Board[p.Y, p.X] = symb;
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
            Debug.Assert(st.WhoWon() == null);

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
            Debug.Assert(st.WhoWon() == null);

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
            Debug.Assert(st.WhoWon() == null);
        }
    }
}
