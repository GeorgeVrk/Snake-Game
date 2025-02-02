using Snake;
using System;
using System.Collections.Generic;
using Serilog;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace SnakeRL
{
    public static class State
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        public static int gridWidth = (int)GameHandler.Width;
        public static int gridHeight = (int)GameHandler.Height;
        public static int ns = gridHeight * gridWidth;
        public static Dictionary<(int state, int action), double> R = new Dictionary<(int state, int action), double>();
        public static Dictionary<(int state, int nextState), bool> FT = new Dictionary<(int i, int j), bool>();
        public static Dictionary<(int state, int action), double> Q = new Dictionary<(int, int), double>();

        public static Random rnd = new Random();
        public static double gamma = 0.95;
        public static double lrnRate = 0.2;
        public static double epsilon = 1.0;
        public static double epsilonDecay = 0.998;
        public static double epsilonMin = 0.01;

        public enum Action { Up = 0, Down = 1, Left = 2, Right = 3 }

        public static Dictionary<(int state, int action), double> SetReward(Particle foodState)
        {
            int state = (int)foodState.PositionY * gridWidth + (int)(foodState.PositionX);
            R.Add((state, 5), 100);
            return R;
        }

        public static double GetReward(int currState, int action, int goal)
        {
            int nextState = GetNextState(currState, action);

            if (nextState == goal)
                return 100.0;

            if (currState == goal)
                return 100.0;

            int currX = currState % gridWidth, currY = currState / gridWidth;
            int goalX = goal % gridWidth, goalY = goal / gridWidth;
            int currDist = Math.Abs(currX - goalX) + Math.Abs(currY - goalY);

            int modThreshold = 1000;
            int modValue = currDist % modThreshold;

            if (modValue == 0)
                return 10.0; 
            else if (modValue < 150)
                return 0.5; 
            else
                return -1.0; 
        }

        public static void InitializeFT()
        {
            for (int i = 0; i < ns; i++)
            {
                int x = i % gridWidth;
                int y = i / gridWidth;

                // Left
                if (x > 0)
                    FT[(i, i - 1)] = true;

                // Right
                if (x < gridWidth - 1)
                    FT[(i, i + 1)] = true;

                // Up
                if (y > 0)
                    FT[(i, i - gridWidth)] = true;

                // Down
                if (y < gridHeight - 1)
                    FT[(i, i + gridWidth)] = true;
            }
        }

        public static void Train()
        {
            Particle par = new Particle(gridHeight,gridWidth, Brushes.White);
            Particle foodPar = new Particle(gridHeight, gridWidth, Brushes.White);

            int start = (int)(par.PositionX * gridWidth + par.PositionY);
            int goal = (int)(499 * gridWidth + 500);

            Console.WriteLine("Starting training...");
            Train(start, goal, 1000);
            Console.WriteLine("Training complete!");

            Console.WriteLine("Simulating learned policy...");
            Simulate(start, goal);
        }


        public static void Train(int start, int goal, int maxEpochs)
        {
            InitializeFT();

            for (int epoch = 0; epoch < maxEpochs; epoch++)
            {
                int currState = start;
                int steps = 0;

                while (true)
                {
                    int action;
                    if (rnd.NextDouble() < epsilon) 
                        action = rnd.Next(0, 4);
                    else 
                        action = GetBestAction(currState);

                    int nextState = GetNextState(currState, action);
                    double reward = GetReward(currState, action, goal);
                  
                    double maxQ = 0.0;
                    for (int a = 0; a < 4; a++) 
                    {
                        double qValue = Q.ContainsKey((nextState, a)) ? Q[(nextState, a)] : 0.0;
                        if (qValue > maxQ)
                            maxQ = qValue;
                    }

                    double oldQ = Q.ContainsKey((currState, action)) ? Q[(currState, action)] : 0.0;
                    Q[(currState, action)] = oldQ + lrnRate * (reward + gamma * maxQ - oldQ);

                    
                    Console.WriteLine($"Epoch: {epoch}, State: {currState}, Action: {action}, Next State: {nextState}, Reward: {reward}, Q-Value: {Q[(currState, action)]}, Goal State: {goal}");

                    currState = nextState;
                    steps++;

                    
                    if (currState == goal)
                    {
                        Console.WriteLine($"Goal reached in epoch {epoch} after {steps} steps.");
                        break;
                    }
                }

                
                epsilon = Math.Max(epsilon * epsilonDecay, epsilonMin);
            }
        }

        public static void Simulate(int start, int goal)
        {
            int currState = start;
            Console.Write($"Path: {currState}");

            while (currState != goal)
            {
                int action = GetBestAction(currState);
                int nextState = GetNextState(currState, action);

                Console.Write($" -> {nextState}");
                currState = nextState;
            }

            Console.WriteLine("\nSimulation complete!");
            Console.ReadKey();
        }

        public static int GetNextState(int currState, int action)
        {
            int x = currState % gridWidth;
            int y = currState / gridWidth;

            switch (action)
            {
                case (int)Action.Up: y--; break;
                case (int)Action.Down: y++; break;
                case (int)Action.Left: x--; break;
                case (int)Action.Right: x++; break;
            }

      
            x = Clamp(x, 0, gridWidth - 1);
            y = Clamp(y, 0, gridHeight - 1);

            return y * gridWidth + x;
        }


        public static int GetBestAction(int state)
        {
            double bestQValue = double.MinValue;
            int bestAction = -1;

            for (int action = 0; action < 4; action++) 
            {
                double qValue = Q.ContainsKey((state, action)) ? Q[(state, action)] : 0.0;
                if (qValue > bestQValue)
                {
                    bestQValue = qValue;
                    bestAction = action;
                }
            }

            return bestAction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                //ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

    }
}

