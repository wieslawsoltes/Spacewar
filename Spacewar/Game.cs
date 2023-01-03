using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace Spacewar
{
    public class Game : Canvas
    {
        private readonly DispatcherTimer _timer;
        private Spaceship _player1;
        private Spaceship _player2;
        private Missile _missile1;
        private Missile _missile2;

        public Game()
        {
            // Create the player spaceships
            _player1 = new Spaceship { Position = new Point(50, 50) };
            _player2 = new Spaceship { Position = new Point(450, 50) };

            // Add the spaceships to the window
            Children.Add(_player1);
            Children.Add(_player2);

            // Start the game loop
            // Create the timer and set its interval
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 60.0); // 60 FPS
            _timer.Tick += GameLoop;
            _timer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Update the game state
            _player1.Update();
            _player2.Update();

            if (_missile1 != null)
            {
                _missile1.Update();

                if (_missile1.IsOutOfBounds())
                {
                    Children.Remove(_missile1);
                    _missile1 = null;
                }
                else if (_missile1.CollidesWith(_player2))
                {
                    Children.Remove(_missile1);
                    _missile1 = null;
                    Children.Remove(_player2);
                }
            }

            if (_missile2 != null)
            {
                _missile2.Update();

                if (_missile2.IsOutOfBounds())
                {
                    Children.Remove(_missile2);
                    _missile2 = null;
                }
                else if (_missile2.CollidesWith(_player1))
                {
                    Children.Remove(_missile2);
                    _missile2 = null;
                    Children.Remove(_player1);
                }
            }

            // Invalidate the visuals of the spaceships and missiles
            _player1.InvalidateVisual();
            _player2.InvalidateVisual();
            if (_missile1 != null) _missile1.InvalidateVisual();
            if (_missile2 != null) _missile2.InvalidateVisual();

            // Check if the game is over
            if (_player1.IsDead() || _player2.IsDead())
            {
                // TODO:
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                _player1.ThrustLeft();
            }
            else if (e.Key == Key.Right)
            {
                _player1.ThrustRight();
            }
            else if (e.Key == Key.Up)
            {
                _player1.ThrustForward();
            }
            else if (e.Key == Key.Space)
            {
                _missile1 = _player1.FireMissile();
                Children.Add(_missile1);
            }

            if (e.Key == Key.A)
            {
                _player2.ThrustLeft();
            }
            else if (e.Key == Key.D)
            {
                _player2.ThrustRight();
            }
            else if (e.Key == Key.W)
            {
                _player2.ThrustForward();
            }
            else if (e.Key == Key.LeftShift)
            {
                _missile2 = _player2.FireMissile();
                Children.Add(_missile2);
            }
        }
    }

    public class Spaceship : Control
    {
        public Point Position { get; set; }
        public double Direction { get; set; }
        public double Speed { get; set; }

        public Spaceship()
        {
            Direction = 0;
            Speed = 0;
        }

        public void Update()
        {
            // Update the spaceship's position based on its direction and speed
            Position = new Point(
                Position.X + Math.Cos(Direction) * Speed,
                Position.Y + Math.Sin(Direction) * Speed);

            // Clamp the spaceship's position to the window bounds
            Position = new Point(
                Math.Max(0, Math.Min(Position.X, Width)),
                Math.Max(0, Math.Min(Position.Y, Height)));
        }

        public void ThrustLeft()
        {
            Direction -= 0.1;
        }

        public void ThrustRight()
        {
            Direction += 0.1;
        }

        public void ThrustForward()
        {
            Speed += 0.1;
        }

        public Missile FireMissile()
        {
            return new Missile
            {
                Position = new Point(Position.X + 20, Position.Y),
                Direction = Direction,
                Speed = Speed + 5
            };
        }

        public bool IsDead()
        {
            return false;
        }

        public override void Render(DrawingContext context)
        {
            // Draw the spaceship body
            context.DrawEllipse(Brushes.White, null, new Point(20, 20), 20, 20);

            // Draw the spaceship thrusters
            context.DrawLine(new Pen(Brushes.Red, 2), new Point(20, 40), new Point(10, 30));
            context.DrawLine(new Pen(Brushes.Red, 2), new Point(20, 40), new Point(30, 30));
        }
    }

    public class Missile : Control
    {
        public Point Position { get; set; }
        public double Direction { get; set; }
        public double Speed { get; set; }

        public Missile()
        {
            Width = 10;
            Height = 10;
        }

        public void Update()
        {
            // Update the missile's position based on its direction and speed
            Position = new Point(
                Position.X + Math.Cos(Direction) * Speed,
                Position.Y + Math.Sin(Direction) * Speed);

            // Clamp the missile's position to the window bounds
            Position = new Point(
                Math.Max(0, Math.Min(Position.X, Width)),
                Math.Max(0, Math.Min(Position.Y, Height)));
        }
        public bool CollidesWith(Spaceship spaceship)
        {
            // Check if the missile collides with the spaceship
            return Bounds.Intersects(spaceship.Bounds);
        }

        public bool IsOutOfBounds()
        {
            // Check if the missile is out of bounds
            return Position.X < 0 || Position.X > Width || Position.Y < 0 || Position.Y > Height;
        }

        public override void Render(DrawingContext context)
        {
            // Draw the missile
            context.DrawEllipse(Brushes.Yellow, null, new Point(5, 5), 5, 5);
        }
    }
}
