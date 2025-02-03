using Snake;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Media;


namespace SnakeRL
{
    public static class State
    {
        static Random rnd = new Random();
        public static int gridWidth = ((int)GameHandler.Width) / 40;
        public static int gridHeight = ((int)GameHandler.Height) / 40;

        public static void Train()
        {
            Particle food = new Particle(gridWidth, gridHeight,Brushes.White);
            Particle snake = new Particle(gridWidth, gridHeight, Brushes.White);
            var snakeState = (int)(snake.PositionX * gridWidth + snake.PositionY);
            int goal = (int)(food.PositionX * gridWidth + gridHeight);
            double gamma = 0.5;
            double learnRate = 0.5;
            int maxEpochs = 1000;
            Console.WriteLine("Begin Q-learning maze demo");
            Console.WriteLine("Setting up maze and rewards");
            int ns = gridWidth * gridHeight;
            int[][] FT = CreateEnv(ns);
            double[][] R = CreateReward(ns,food, FT);
            double[][] Q = CreateQuality(ns);
            Console.WriteLine("Analyzing maze using Q-learning");
            Train(FT, R, Q, goal, gamma, learnRate, maxEpochs);
            Console.WriteLine("Done. Q matrix: ");
            //Print(Q);
            Console.WriteLine($"Using Q to walk from cell {snakeState} to {goal}");
            Walk(snakeState, goal, Q);
            Console.WriteLine("End demo");
            Console.ReadLine();
        }
        public static int[][] CreateEnv(int ns)
        {
            int[][] FT = new int[ns][];
            for (int i = 0; i < ns; ++i)
                FT[i] = new int[ns];

            for (int state = 0; state < ns; state++)
            {
                int x = state % gridWidth;
                int y = state / gridWidth;

                // Check and add possible moves (up, down, left, right)
                if (y > 0) FT[state][state - gridWidth] = 1; // Up
                if (y < gridHeight - 1) FT[state][state + gridWidth] = 1; // Down
                if (x > 0) FT[state][state - 1] = 1; // Left
                if (x < gridWidth - 1) FT[state][state + 1] = 1; // Right
            }
            return FT;
        }

        public static double[][] CreateReward(int ns, Particle food, int[][] FT)
        {
            double[][] R = new double[ns][];
            var foodState = (int)(food.PositionX * gridWidth + food.PositionY);
            for (int i = 0; i < ns; ++i)
                R[i] = new double[ns];
            for (int state = 0;state < ns; state++)
            {
                for (int action = 0;action < ns; action++)
                {
                    if (FT[state][action] == 1)
                    {
                        R[state][action] = (state == foodState) ? 100 : -1;
                    }
                    else
                    {
                        R[state][action] = -100;
                    }
                }
            }
            return R;
        }

        public static double[][] CreateQuality(int ns)
        {
            double[][] Q = new double[ns][];
            for (int i = 0; i < ns; ++i)
                Q[i] = new double[ns];
            return Q;
        }

        public static void Train(int[][] FT, double[][] R, double[][] Q ,int goal, double gamma, double lrnRate, int maxEpochs)
        {
            for (int epoch = 0; epoch < maxEpochs; ++epoch)
            {
                int currState = rnd.Next(0, R.Length);
                int steps = 0;
                while (true)
                {
                    int nextState = GetRandNextState(currState, FT);
                    List<int> possNextNextStates = GetPossNextStates(nextState, FT);
                    double maxQ = double.MinValue;
                    for (int j = 0; j < possNextNextStates.Count; ++j)
                    {
                        int nns = possNextNextStates[j];  // short alias
                        double q = Q[nextState][nns];
                        if (q > maxQ) maxQ = q;
                    }
                    Q[currState][nextState] =
                         ((1 - lrnRate) * Q[currState][nextState]) +
                         (lrnRate * (R[currState][nextState] + (gamma * maxQ)));
                    currState = nextState;
                    steps++;
                    if (currState == goal) break;
                }
                Console.WriteLine($"Finished episode {epoch} in {steps} steps");
            }
        }

        public static int GetRandNextState(int s, int[][] FT)
        {
            List<int> possNextStates = GetPossNextStates(s, FT);
            int ct = possNextStates.Count;
            int idx = rnd.Next(0, ct);
            return possNextStates[idx];
        }

        public static List<int> GetPossNextStates(int s, int[][] FT)
        {
            List<int> result = new List<int>();
            for (int j = 0; j < FT.Length; ++j)
                if (FT[s][j] == 1) result.Add(j);
            return result;
        }

        public static void Walk(int start, int goal, double[][] Q)
        {
            int curr = start; int next;
            Console.Write(curr + "->");
            while (curr != goal)
            {
                next = ArgMax(Q[curr]);
                Console.Write(next + "->");
                curr = next;
            }
            Console.WriteLine("done");
        }

        public static int ArgMax(double[] vector)
        {
            double maxVal = vector[0]; int idx = 0;
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > maxVal)
                {
                    maxVal = vector[i]; idx = i;
                }
            }
            return idx;
        }

        static void Print(double[][] Q)
        {
            int ns = Q.Length;
            Console.WriteLine("[0] [1] . . [11]");
            for (int i = 0; i < ns; ++i)
            {
                for (int j = 0; j < ns; ++j)
                {
                    Console.Write(Q[i][j].ToString("F2") + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
