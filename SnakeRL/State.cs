using Snake;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Media;

namespace SnakeRL
{
    public class State
    {
        public static Random rnd = new Random();
        public static int gridWidth = 20;
        public static int gridHeight = 20;
        public static int ns = gridWidth * gridHeight; 
        public int[][] FT = new int[ns][];
        public double[][] Q = new double[ns][]; 
        public double[][] R = new double[ns][]; 
        public double gamma = 0.8; 
        public double learningRate = 0.01; 
        public double epsilon = 1.0; 
        public double minExplorationRate = 0.01; 
        public double explorationDecayRate = 0.995; 

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

        public double[][] CreateReward(int ns, int goal, int[][] FT)
        {
            double[][] R = new double[ns][];
            for (int i = 0; i < ns; i++) R[i] = new double[4];

            for (int i = 0; i < ns; ++i)
            {
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
                        if (nextState == goal)
                        {
                            R[nextState][j] = 1000;
                        }
                        else
                            R[i][j] = -1;
                    }
                    else
                    {
                        R[i][j] = -100;
                    }
                }
            }

            return R;
        }

        public double[][] LoadQTable(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("File not found: " + filename);
                return null;
            }

            string json = File.ReadAllText(filename);
            double[][] Q = JsonSerializer.Deserialize<double[][]>(json);
            Console.WriteLine("Q-table loaded from " + filename);
            return Q;
        }

        public void SaveQTable(string filename, double[][] Q)
        {
        string json = JsonSerializer.Serialize(Q);
        File.WriteAllText(filename, json);
        Console.WriteLine("Q-table saved to " + filename);
        }



        public void Train()
        {
            Particle snake = new Particle(gridWidth, gridHeight, Brushes.White);
            snake.PositionX = 13;
            snake.PositionY = 5;
            int snakeState = (int)(snake.PositionY * gridWidth + snake.PositionX);
            Particle food = new Particle(gridWidth, gridHeight, Brushes.White);
            food.PositionX = 18;
            food.PositionY = 18;
            int goal = (int)(food.PositionY * gridWidth + food.PositionX);
            int[][] FT = CreateEnviroment(ns);
            double[][] R = CreateReward(ns, goal, FT);
            double[][] Q = CreateQuality(ns);
            Train(FT, R, Q, goal, gamma, learningRate, 8000, epsilon);
            Console.WriteLine($"Walking from state {snakeState} to state {goal}");
            Thread.Sleep(1000);
            Walk(snakeState, goal, Q, FT);
            Console.ReadKey();
        }

        public void Train(int[][] FT, double[][] R, double[][] Q, int goal, double gamma, double lrnRate, int maxEpochs, double epsilon)
        {
            for (int epoch = 0; epoch < maxEpochs; ++epoch)
            {
                int steps = 0;
                int currState = rnd.Next(0, R.Length); 
                int action;
                int nextState = currState;
                while (true)
                {
                    
                    List<int> possNextStates = GetPossNextStates(currState, FT);

                    
                    if (rnd.NextDouble() < epsilon)
                    {
                        action = rnd.Next(0, 4); 
                    }
                    else
                    {
                        action = ArgMax(Q[currState]);
                    }

                    if (action == 0 && currState >= gridWidth)  // Move Up
                        nextState = currState - gridWidth;
                    else if (action == 1 && (currState + 1) % gridWidth != 0)  // Move Right
                        nextState = currState + 1;
                    else if (action == 2 && currState < ns - gridWidth)  // Move Down
                        nextState = currState + gridWidth;
                    else if (action == 3 && currState % gridWidth != 0)  // Move Left
                        nextState = currState - 1;

                    
                    double maxQ = double.MinValue;
                    for (int j = 0; j < Q[nextState].Length; ++j)
                    {
                        double q = Q[nextState][j];
                        if (q > maxQ)
                        {
                            maxQ = q;
                        }
                    }

                    
                    Q[currState][action] = ((1 - lrnRate) * Q[currState][action]) + (lrnRate * (R[currState][action] + (gamma * maxQ)));

                   
                    currState = nextState;
                    steps++;

                   
                    if (currState == goal) break;
                }
                Console.WriteLine($"Finished episode {epoch} in {steps} steps.");
                if (epsilon > 0.1)
                {
                    epsilon *= 0.99; 
                }
            }
        }


        public void Walk(int start, int goal, double[][] Q, int[][] FT)
        {
            int curr = start;
            Console.Write(curr + "->");

            while (curr != goal)
            {
                
                int bestAction = ArgMax(Q[curr]);

                
                int nextState = curr;

                if (bestAction == 0 && curr >= gridWidth)  // Move Up
                    nextState = curr - gridWidth;
                else if (bestAction == 1 && (curr + 1) % gridWidth != 0)  // Move Right
                    nextState = curr + 1;
                else if (bestAction == 2 && curr < ns - gridWidth)  // Move Down
                    nextState = curr + gridWidth;
                else if (bestAction == 3 && curr % gridWidth != 0)  // Move Left
                    nextState = curr - 1;

               
                List<int> possibleNextStates = GetPossNextStates(curr, FT);
                if (!possibleNextStates.Contains(nextState))
                {
                    Console.WriteLine($"Blocked at {curr}, retrying...");
                    break;  
                }

                Console.Write(nextState + "->");
                curr = nextState;
            }

            Console.WriteLine($"{goal} done!");
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