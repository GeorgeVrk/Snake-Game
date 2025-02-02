using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLearning
{
    public static class QLearningMem
    {
        // Grid dimensions
        public static int gridWidth = 10; // Example: 10x10 grid
        public static int gridHeight = 10;
        public static int ns = gridWidth * gridHeight; // Total number of states

        // Q-learning parameters
        public static double gamma = 0.9; // Discount factor
        public static double lrnRate = 0.1; // Learning rate
        public static double epsilon = 1.0; // Exploration rate
        public static double epsilonDecay = 0.995; // Exploration decay rate
        public static double epsilonMin = 0.01; // Minimum exploration rate

        // Q-table (sparse representation)
        public static Dictionary<(int state, int action), double> Q = new Dictionary<(int, int), double>();

        // Transition function (valid moves)
        public static Dictionary<(int state, int nextState), bool> FT = new Dictionary<(int, int), bool>();

        // Random number generator
        public static Random rnd = new Random();

        // Actions: 0 = Up, 1 = Down, 2 = Left, 3 = Right
        public enum Action { Up = 0, Down = 1, Left = 2, Right = 3 }

        // Initialize the transition function (valid moves)
        public static void InitializeFT()
        {
            for (int i = 0; i < ns; i++)
            {
                // Allow moving to adjacent states
                if (i % gridWidth != 0) // Not on the leftmost edge
                    FT[(i, i - 1)] = true; // Left
                if ((i + 1) % gridWidth != 0) // Not on the rightmost edge
                    FT[(i, i + 1)] = true; // Right
                if (i - gridWidth >= 0) // Not on the top edge
                    FT[(i, i - gridWidth)] = true; // Up
                if (i + gridWidth < ns) // Not on the bottom edge
                    FT[(i, i + gridWidth)] = true; // Down
            }
        }

        // Get the next state based on the current state and action
        public static int GetNextState(int currState, int action)
        {
            int nextState = currState;
            switch (action)
            {
                case (int)Action.Up: nextState -= gridWidth; break;
                case (int)Action.Down: nextState += gridWidth; break;
                case (int)Action.Left: nextState -= 1; break;
                case (int)Action.Right: nextState += 1; break;
            }
            return FT.ContainsKey((currState, nextState)) ? nextState : currState; // Ensure valid transition
        }

        // Get the reward for a state-action pair
        public static double GetReward(int state, int action, int goal)
        {
            int nextState = GetNextState(state, action);
            if (nextState == goal) // Reached goal
                return 100.0;
            else if (state == nextState) // Invalid move (hit wall)
                return -10.0;
            else // Valid move
                return -0.1;
        }

        // Get the best action for a state based on Q-values
        public static int GetBestAction(int state)
        {
            double bestQValue = double.MinValue;
            int bestAction = -1;

            for (int action = 0; action < 4; action++) // 4 actions
            {
                double qValue = Q.ContainsKey((state, action)) ? Q[(state, action)] : 0.0; // Default to 0 if not found
                if (qValue > bestQValue)
                {
                    bestQValue = qValue;
                    bestAction = action;
                }
            }

            return bestAction;
        }

        // Train the Q-learning agent
        public static void Train(int start, int goal, int maxEpochs)
        {
            InitializeFT(); // Initialize valid transitions

            for (int epoch = 0; epoch < maxEpochs; epoch++)
            {
                int currState = start;
                int steps = 0;

                while (true)
                {
                    int action;
                    if (rnd.NextDouble() < epsilon) // Explore: choose a random action
                        action = rnd.Next(0, 4);
                    else // Exploit: choose the best action
                        action = GetBestAction(currState);

                    int nextState = GetNextState(currState, action);
                    double reward = GetReward(currState, action, goal);

                    // Update Q-value
                    double maxQ = double.MinValue;
                    for (int a = 0; a < 4; a++) // Find max Q-value for next state
                    {
                        double qValue = Q.ContainsKey((nextState, a)) ? Q[(nextState, a)] : 0.0;
                        if (qValue > maxQ)
                            maxQ = qValue;
                    }

                    double oldQ = Q.ContainsKey((currState, action)) ? Q[(currState, action)] : 0.0;
                    Q[(currState, action)] = (1 - lrnRate) * oldQ + lrnRate * (reward + gamma * maxQ);

                    // Debug print
                    Console.WriteLine($"Epoch: {epoch}, State: {currState}, Action: {action}, Next State: {nextState}, Reward: {reward}, Q-Value: {Q[(currState, action)]}");

                    currState = nextState;
                    steps++;

                    // Check termination conditions
                    if (currState == goal)
                    {
                        Console.WriteLine($"Goal reached in epoch {epoch} after {steps} steps.");
                        break;
                    }

                    if (steps > 1000) // Prevent infinite loops
                    {
                        Console.WriteLine($"Max steps exceeded in epoch {epoch}.");
                        break;
                    }
                }

                // Decay exploration rate
                epsilon = Math.Max(epsilon * epsilonDecay, epsilonMin);
            }
        }

        // Simulate the learned policy
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

        // Main method
        public static void Train()
        {
            int start = 0; // Starting state (top-left corner)
            int goal = gridWidth * gridHeight - 1; // Goal state (bottom-right corner)

            Console.WriteLine("Starting training...");
            Train(start, goal, 1000); // Train for 1000 epochs
            Console.WriteLine("Training complete!");

            Console.WriteLine("Simulating learned policy...");
            Simulate(start, goal);
        }
    }
}
