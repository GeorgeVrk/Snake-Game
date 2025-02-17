﻿using System;
using System.Windows;
using System.Windows.Input;

namespace Snake
{
    public class ComponentHandler
    {
        private Window _window;
        private Directions? direction;
        public event Action<Directions?> OnDirectionChanged;
        public ComponentHandler(Window window) 
        { 
            _window = window;
        }

        public void MonitorMovement()
        {
             _window.KeyDown += Movement;
        }

        private void Movement(object sender, KeyEventArgs e)
        {
            Directions? newDirections = null;
            switch (e.Key)
            {
                case Key.W:
                    if (direction != Directions.DOWN)
                    {
                        newDirections = Directions.UP;
                    }
                    break;
                case Key.S:
                    if (direction != Directions.UP)
                    {
                        newDirections = Directions.DOWN;
                    }
                    break;
                case Key.D:
                    if (direction != Directions.LEFT)
                    {
                        newDirections = Directions.RIGHT;
                    }
                    break;
                case Key.A:
                    if (direction != Directions.RIGHT)
                    {
                        newDirections = Directions.LEFT;
                    }
                    break;
                default:
                    newDirections = null;
                    break;
            }

            if (newDirections.HasValue && newDirections != direction)
            {
                direction = newDirections.Value;
                OnDirectionChanged?.Invoke(direction.Value);
            }
        }

        public void ChangeDirection(Directions? d)
        {
            direction = d.Value;
            OnDirectionChanged?.Invoke(direction.Value);
        }
    }
}
