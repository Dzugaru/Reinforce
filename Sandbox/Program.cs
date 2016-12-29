using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            TestsTicTacToe.Test1();

            TicTacToe.Agent agent = new TicTacToe.Agent(TicTacToe.Symbol.X, 0.1, 0.1, rngSeed:1);
            TicTacToe.IAdversary adversaryTrain = new TicTacToe.SelfAdversary(TicTacToe.Symbol.O, agent);
            TicTacToe.IAdversary adversaryTest = new TicTacToe.RandomAdversary(TicTacToe.Symbol.O, rngSeed:2);

            //TicTacToe.IAdversary adversaryTrain = new TicTacToe.RandomAdversary(TicTacToe.Symbol.O, rngSeed: 3);
            //TicTacToe.IAdversary adversaryTest = new TicTacToe.RandomAdversary(TicTacToe.Symbol.O, rngSeed:2);

            //TicTacToe.IAdversary adversaryTrain = new TicTacToe.SelfAdversary(TicTacToe.Symbol.O, agent);
            //TicTacToe.IAdversary adversaryTest = new TicTacToe.SelfAdversary(TicTacToe.Symbol.O, agent);

            //TicTacToe.IAdversary adversary = new TicTacToe.FirstFreeAdversary(TicTacToe.Symbol.O);            
            TicTacToe.State startState = new TicTacToe.State(3, 3, 3);


            var series = new OxyPlot.Series.LineSeries();
            var plotView = new OxyPlot.WindowsForms.PlotView();
            plotView.Model = new PlotModel();
            plotView.Model.Series.Add(series);
            //plotView.BackColor = System.Drawing.Color.Red;
            plotView.Size = new System.Drawing.Size(640, 480);

            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Size = new System.Drawing.Size(800, 600);
            form.Controls.Add(plotView);
            form.Show();

            for (int k = 0;;k++)
            {
                int numEpisodes = 100;
                for (int i = 0; i < numEpisodes; i++)
                    agent.TrainOneEpisode(startState, adversaryTrain);

                int numPlays = 1000;
                int numWons = 0;
                int numDraws = 0;
                for (int i = 0; i < numPlays; i++)
                {
                    TicTacToe.Symbol won = agent.Play(startState, adversaryTest);
                    if (won == TicTacToe.Symbol.X)
                        numWons++;
                    else if (won == TicTacToe.Symbol.None)
                        numDraws++;
                }

                series.Points.Add(new DataPoint(k, numPlays - numWons - numDraws));
                plotView.InvalidatePlot(true);
                Console.WriteLine($"{numWons}/{numDraws}/{numPlays}");
                //Console.ReadLine();
                System.Windows.Forms.Application.DoEvents();
            }

            //Console.ReadLine();

            //
        }
    }
}
