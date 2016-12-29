using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            TestsTicTacToe.Test1();

            TicTacToe.Agent agent = new TicTacToe.Agent(TicTacToe.Symbol.X, 0.1, 0.1, rngSeed:1);
            //TicTacToe.IAdversary adversary = new TicTacToe.RandomAdversary(TicTacToe.Symbol.O, rngSeed:2);
            //TicTacToe.IAdversary adversary = new TicTacToe.FirstFreeAdversary(TicTacToe.Symbol.O);
            TicTacToe.IAdversary adversary = new TicTacToe.SelfAdversary(TicTacToe.Symbol.O, agent);
            TicTacToe.State startState = new TicTacToe.State(3, 3, 3);

            for (;;)
            {
                int numEpisodes = 10;
                for (int i = 0; i < numEpisodes; i++)
                    agent.TrainOneEpisode(startState, adversary);

                int numPlays = 1000;
                int numWons = 0;
                int numDraws = 0;
                for (int i = 0; i < numPlays; i++)
                {
                    TicTacToe.Symbol won = agent.Play(startState, adversary);
                    if (won == TicTacToe.Symbol.X)
                        numWons++;
                    else if (won == TicTacToe.Symbol.None)
                        numDraws++;
                }

                Console.WriteLine($"{numWons}/{numDraws}/{numPlays}");
                //Console.ReadLine();
            }
        }
    }
}
