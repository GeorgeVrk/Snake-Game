using Snake;
using System;
using System.Collections.Generic;

namespace SnakeRL
{
    public static class State
    {
        public static int gridWidth = (int)GameHandler.Width;
        public static int gridHeight = (int)GameHandler.Height;
        public static Dictionary<int, List<int>> FT = new Dictionary<int, List<int>>();
        public static Dictionary<int, double> R = new Dictionary<int, double>();
        public static Dictionary<(int state, int action), double> Q = new Dictionary<(int, int), double>();

        public static void SetState()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    int state = y * gridWidth + x;
                    FT[state] = new List<int>();

                    if (x > 0) FT[state].Add((int)Directions.LEFT);
                    if (x < gridWidth - 1) FT[state].Add((int)Directions.RIGHT);
                    if (y > 0) FT[state].Add((int)Directions.UP);
                    if (y < gridHeight - 1) FT[state].Add((int)Directions.DOWN);
                }
            }
        }

        public static void SetReward()
        {
            foreach (var state in FT.Keys)
                R[state] = -0.1; 

            int foodState = (int)(GameHandler.foodHandler.Food[0].PositionY * gridWidth + GameHandler.foodHandler.Food[0].PositionX);
            R[foodState] = 10.0; 

            foreach (var segment in GameHandler.Tail)
            {
                int snakeState = (int)(segment.PositionY * gridWidth + segment.PositionX);
                R[snakeState] = -10.0; 
            }
        }

        public static void Train(int episodes, double alpha, double gamma, double epsilon)
        {
            GameHandler.StartGame();
            Random rnd = new Random();
            for (int episode = 0; episode < episodes; episode++)
            {
                int x = gridWidth / 2;
                int y = gridHeight / 2;
                int state = y * gridWidth + x;

                while (true)
                {
                    int action = (rnd.NextDouble() < epsilon) ? rnd.Next(0, 4) : GetBestAction(state);
                    GameHandler.compHandler.ChangeDirection((Directions)action);
                    Console.WriteLine(action);
                    int newX = x, newY = y;
                    switch ((Directions)action)
                    {
                        case Directions.UP: if (y > 0) newY--; break;
                        case Directions.DOWN: if (y < gridHeight - 1) newY++; break;
                        case Directions.LEFT: if (x > 0) newX--; break;
                        case Directions.RIGHT: if (x < gridWidth - 1) newX++; break;
                    }
                    int newState = newY * gridWidth + newX;

                    double maxQ = GetMaxQ(newState);
                    double reward = R.ContainsKey(newState) ? R[newState] : -0.1;

                    if (!Q.ContainsKey((state, action))) Q[(state, action)] = 0;
                    Q[(state, action)] = (1 - alpha) * Q[(state, action)] + alpha * (reward + gamma * maxQ);

                    state = newState;
                    x = newX;
                    y = newY;

                    if (reward == 10.0 || reward == -10.0) break;
                }
            }
        }

        public static int GetBestAction(int state)
        {
            if (!FT.ContainsKey(state)) return 0; 

            double maxQ = double.MinValue;
            int bestAction = 0;

            foreach (int action in FT[state])
            {
                if (Q.ContainsKey((state, action)) && Q[(state, action)] > maxQ)
                {
                    maxQ = Q[(state, action)];
                    bestAction = action;
                }
            }
            return bestAction;
        }

        public static double GetMaxQ(int state)
        {
            if (!FT.ContainsKey(state)) return 0; 

            double maxQ = double.MinValue;
            foreach (int action in FT[state])
            {
                if (Q.ContainsKey((state, action)) && Q[(state, action)] > maxQ)
                {
                    maxQ = Q[(state, action)];
                }
            }
            return maxQ == double.MinValue ? 0 : maxQ;
        }
    }
}
