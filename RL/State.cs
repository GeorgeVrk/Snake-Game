using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RL
{
    public class State
    {
        public static Random rnd = new Random();
        public static int gridWidth = 20;
        public static int gridHeight = 20;
        public static int ns = gridWidth * gridHeight; 
        public int[][] FT = new int[ns][]; 
        public double gamma = 0.99; 
        public double learningRate = 0.1; 
        public double epsilon = 0.5; 
        public double minExplorationRate = 0.01; 
        public double explorationDecayRate = 0.9995; 
        private static ILogger s_log = new LoggerConfiguration().WriteTo.File("Walk.txt").MinimumLevel.Verbose().CreateLogger().ForContext(typeof(State));

        public enum Action { UP, DOWN, LEFT, RIGHT }

        public int[][] CreateEnviroment(int ns)
        {
            int[][] FT = new int[ns][];
            for (int i = 0; i < ns; ++i)
            {
                FT[i] = new int[ns];
                int x = i % gridWidth;
                int y = i / gridHeight;

                if (y > 0) FT[i][i - gridHeight] = 1; // Move Up
                if (y < gridHeight - 1) FT[i][i + gridHeight] = 1; // Move Down
                if (x > 0) FT[i][i - 1] = 1; // Move Left
                if (x < gridWidth - 1) FT[i][i + 1] = 1; // Move right
            }
            return FT;
        }

        public double[][] CreateReward(int ns, int goal, int[][] FT, int gridWidth)
        {
            double[][] R = new double[ns][];
            for (int i = 0; i < ns; i++)
                R[i] = new double[4];

            int goalX = goal % gridWidth;  // X position of goal
            int goalY = (goal / gridWidth);  // Y position of goal

            for (int i = 0; i < ns; ++i)
            {
                int currentX = i % gridWidth;  // X position of current state
                int currentY = i / gridWidth;  // Y position of current state

                for (int j = 0; j < 4; ++j)
                {
                    int nextState = -1;

                    switch (j)
                    {
                        case 0: // Up
                            if (i - gridWidth >= 0 && FT[i][i - gridWidth] == 1)
                                nextState = i - gridWidth;
                            break;
                        case 1: // Down
                            if (i + gridWidth < ns && FT[i][i + gridWidth] == 1)
                                nextState = i + gridWidth;
                            break;
                        case 2: // Left
                            if (i % gridWidth != 0 && FT[i][i - 1] == 1)
                                nextState = i - 1;
                            break;
                        case 3: // Right
                            if ((i + 1) % gridWidth != 0 && FT[i][i + 1] == 1)
                                nextState = i + 1;
                            break;
                    }

                    if (nextState != -1)
                    {
                        int nextX = nextState % gridWidth;
                        int nextY = nextState / gridWidth;

                        if (nextState == goal)
                        {
                            R[nextState][j] = 500000; // Big reward for reaching goal
                        }
                        else
                        {
                            R[i][j] -= 1; // Default penalty for movement

                            // Manhattan Distance Check: Reward moves that reduce distance
                            int currentDist = Math.Abs(currentX - goalX) + Math.Abs(currentY - goalY);
                            int nextDist = Math.Abs(nextX - goalX) + Math.Abs(nextY - goalY);

                            if (nextDist < currentDist)
                            {
                                R[i][j] += ((currentDist - nextDist) / 20) * 10; // Reward for moving toward goal
                            }
                            else
                            {
                                R[i][j] -= ((nextDist - currentDist) / 25) * 15;
                            }
                        }
                    }
                    else
                    {
                        R[i][j] = -50; // High penalty for invalid move
                    }
                }
            }

            return R;
        }


        public void Train()
        {
            int[][] FT = CreateEnviroment(ns);
            double[][] Q = CreateQuality(ns);
            List<Action> actions = new List<Action>();
            for (int i = 0; i < ns; i++)
            {
                for (int j = 0; j < ns; j++)
                {
                    Train(FT, Q, j, i, gamma, learningRate, 8000, epsilon);
                    Walk(i, j, Q, FT, out actions);
                }
            }
            Console.ReadKey();
        }

        public List<Action> Train(int state, int goal)
        {
            int[][] FT = CreateEnviroment(ns);
            double[][] Q = CreateQuality(ns);
            Train(FT, Q, goal, state, gamma, learningRate, 8000, epsilon);
            List<Action> actions = new List<Action>();
            Walk(state, goal, Q, FT, out actions);
            return actions;

        }

        public void Train(int[][] FT, double[][] Q, int goal, int state, double gamma, double lrnRate, int maxEpochs, double epsilon)
        {
            for (int epoch = 0; epoch < maxEpochs; ++epoch)
            {
                double[][] R = CreateReward(ns, goal, FT, gridWidth); // Create reward table for this goal

                int currState = state; // Start from a random state
                int nextState = currState;
                int steps = 0;

                while (true)
                {
                    List<int> possNextStates = GetPossNextStates(currState, FT);

                    // Epsilon-Greedy Action Selection
                    int action = (rnd.NextDouble() < epsilon) ? rnd.Next(0, 4) : ArgMax(Q[currState]);

                    // Determine next state based on action
                    if (action == 0 && currState >= gridWidth) nextState = currState - gridWidth;  // Move Up
                    else if (action == 1 && (currState + 1) % gridWidth != 0) nextState = currState + 1; // Move Right
                    else if (action == 2 && currState < ns - gridWidth) nextState = currState + gridWidth; // Move Down
                    else if (action == 3 && currState % gridWidth != 0) nextState = currState - 1; // Move Left

                    // Get max Q-value for the next state
                    double maxQ = Q[nextState].Max();

                    // Q-Learning Update Rule
                    if (nextState != -1 && possNextStates.Contains(nextState))
                    {
                        maxQ = Q[nextState].Max(); // Find best Q-value for next state

                        // Update Q-table correctly
                        Q[currState][action] = (1 - lrnRate) * Q[currState][action] +
                                               lrnRate * (R[currState][action] + gamma * maxQ);
                    }
                    else
                    {
                        Q[currState][action] = -1000; // Big penalty for impossible moves
                    }



                    currState = nextState;
                    steps++;

                    // Stop if goal is reached
                    if (currState == goal) break;
                }

                epsilon = Math.Max(0.1, epsilon * explorationDecayRate);
            }
        }



        public int Walk(int start, int goal, double[][] Q, int[][] FT, out List<Action> actions)
        {
            Console.WriteLine($"Walking from state {start} to state {goal}");
            int curr = start;
            actions = new List<Action>();
            Console.Write(curr + "->");
            s_log.Information(curr + "->");

            while (curr != goal)
            {
                
                int bestAction = ArgMax(Q[curr]);
                int nextState = curr;

                if (bestAction == 0 && curr >= gridWidth)
                {  // Move Up
                    nextState = curr - gridWidth;
                    actions.Add(Action.UP);
                }
                else if (bestAction == 1 && (curr + 1) % gridWidth != 0)
                {  // Move Right
                    nextState = curr + 1;
                    actions.Add(Action.RIGHT);
                }
                else if (bestAction == 2 && curr < ns - gridWidth)
                {  // Move Down
                    nextState = curr + gridWidth;
                    actions.Add(Action.DOWN);
                }
                else if (bestAction == 3 && curr % gridWidth != 0)
                {  // Move Left
                    nextState = curr - 1;
                    actions.Add(Action.LEFT);
                }

                Console.Write(nextState + "->");
                s_log.Information(nextState + "->");
                curr = nextState;
            }
            if (curr == goal)
            {
                Console.WriteLine($"{goal} done!");
                s_log.Information($"done!");
            }
            return 0;
        }

        public int ArgMax(double[] vector)
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

        public double[][] CreateQuality(int ns)
        {
            double[][] Q = new double[ns][];
            for (int i = 0; i < ns; ++i)
                Q[i] = new double[4];
            return Q;
        }

        public List<int> GetPossNextStates(int s, int[][] FT)
        {
            List<int> result = new List<int>();
            for (int j = 0; j < FT.Length; ++j)
                if (FT[s][j] == 1) result.Add(j);
            return result;
        }
    }
}