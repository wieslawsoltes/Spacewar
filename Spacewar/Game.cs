using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using Avalonia.Controls.Shapes;

namespace Spacewar
{
    public class Game : Canvas
    {
        private Spaceship _player1;
        private Spaceship _player2;
        private Missile _missile1;
        private Missile _missile2;
        private Ellipse _planet;
        private TextBlock _gameOverText;
        private const double GRAVITY = 1.0; // gravitational constant

        public Game()
        {
            // Create the planet
            _planet = new Ellipse { Width = 100, Height = 100, Fill = Brushes.Gray };
            Children.Add(_planet);

            // Create the player spaceships
            _player1 = new Spaceship
            {
                Position = new Point(50, 50), Mass = 1.0, Velocity = new Vector(0, 0), Direction = 0
            };
            _player2 = new Spaceship
            {
                Position = new Point(450, 50), Mass = 1.0, Velocity = new Vector(0, 0), Direction = 0
            };

            // Add the spaceships to the window
            Children.Add(_player1);
            Children.Add(_player2);

            // Add the game over text
            _gameOverText = new TextBlock
            {
                Text = "",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            Children.Add(_gameOverText);
            
            // Start the game loop
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.0 / 60.0); // 60 FPS
            timer.Tick += GameLoop;
            timer.Start();
        }
        
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            // Make the game board focusable
            this.Focusable = true;
            // Give keyboard focus to the game board
            this.Focus();
        }
        
        private void GameLoop(object sender, EventArgs e)
        {
            // Update the game state
            _player1.Update(_planet, GRAVITY);
            _player2.Update(_planet, GRAVITY);

            if (_missile1 != null)
            {
                _missile1.Update(_planet, GRAVITY);

                if (_missile1.IsOutOfBounds())
                {
                    Children.Remove(_missile1);
                    _missile1 = null;
                }
                else if (_missile1.CollidesWith(_player2))
                {
                    Children.Remove(_missile1);
                    _missile1 = null;
                    _player2.IsDestroyed = true;
                    Children.Remove(_player2);
                }
            }

            if (_missile2 != null)
            {
                _missile2.Update(_planet, GRAVITY);

                if (_missile2.IsOutOfBounds())
                {
                    Children.Remove(_missile2);
                    _missile2 = null;
                }
                else if (_missile2.CollidesWith(_player1))
                {
                    Children.Remove(_missile2);
                    _missile2 = null;
                    _player1.IsDestroyed = true;
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
                // Stop the game loop
                DispatcherTimer timer = (DispatcherTimer)sender;
                timer.Stop();

                // Update the game over text
                if (_player1.IsDead() && _player2.IsDead())
                {
                    _gameOverText.Text = "It's a draw!";
                }
                else if (_player1.IsDead())
                {
                    _gameOverText.Text = "Player 2 wins!";
                }
                else if (_player2.IsDead())
                {
                    _gameOverText.Text = "Player 1 wins!";
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                _player1.TurnLeft();
            }
            else if (e.Key == Key.Right)
            {
                _player1.TurnRight();
            }
            else if (e.Key == Key.Up)
            {
                _player1.Accelerate();
            }
            else if (e.Key == Key.Space)
            {
                _missile1 = _player1.FireMissile();
                Children.Add(_missile1);
            }

            if (e.Key == Key.A)
            {
                _player2.TurnLeft();
            }
            else if (e.Key == Key.D)
            {
                _player2.TurnRight();
            }
            else if (e.Key == Key.W)
            {
                _player2.Accelerate();
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
        public Vector Velocity { get; set; }
        public double Direction { get; set; }
        public double Mass { get; set; }
        public bool IsDestroyed { get; set; }

        public Spaceship()
        {
            Direction = 0;
            Mass = 1.0;
            IsDestroyed = false;
        }

        public void Update(Ellipse planet, double gravity)
        {
            // Calculate the gravitational force acting on the spaceship
            Vector r = new Vector(Position.X - planet.Width / 2, Position.Y - planet.Height / 2);
            Vector force;
            if (r.Length == 0)
            {
                // Set the force vector to a default value
                force = new Vector(0, 0);
            }
            else
            {
                // Calculate the force vector
                force = r * (-gravity * Mass / Math.Pow(r.Length, 3));
            }

            // Update the velocity and position of the spaceship
            Velocity += force;
            Position += Velocity;

            // Clamp the spaceship's position to the window bounds
            Position = new Point(
                Math.Max(0, Math.Min(Position.X, Width)),
                Math.Max(0, Math.Min(Position.Y, Height)));
        }

        public void TurnLeft()
        {
            Direction -= 0.1;
        }

        public void TurnRight()
        {
            Direction += 0.1;
        }

        public void Accelerate()
        {
            Velocity += new Vector(0.1 * Math.Cos(Direction), 0.1 * Math.Sin(Direction));
        }

        public Missile FireMissile()
        {
            return new Missile
            {
                Position = Position,
                Velocity = Velocity + new Vector(20 * Math.Cos(Direction), 20 * Math.Sin(Direction)),
                Mass = 0.1
            };
        }

        public bool IsDead()
        {
            return IsDestroyed;
        }
        
        public override void Render(DrawingContext context)
        {
            // Push a translate transform onto the drawing context's transform stack
            using var _ = context.PushPostTransform(Matrix.CreateTranslation(Position.X, Position.Y));
            
            context.DrawLine(new Pen(Brushes.Red, 2), new Point(0, 0),
                new Point(20 * Math.Cos(Direction), 20 * Math.Sin(Direction)));
        }
    }

    public class Missile : Control
    {
        public Point Position { get; set; }
        public Vector Velocity { get; set; }
        public double Mass { get; set; }

        public Missile()
        {
            Mass = 0.1;
        }

        public void Update(Ellipse planet, double gravity)
        {
            // Calculate the gravitational force acting on the missile
            Vector r = new Vector(Position.X - planet.Width / 2, Position.Y - planet.Height / 2);
            Vector force = r * (-gravity * Mass / Math.Pow(r.Length, 3));

            // Update the velocity and position of the missile
            Velocity += force;
            Position += Velocity;
        }

        public bool IsOutOfBounds()
        {
            return Position.X < 0 || Position.X > Width || Position.Y < 0 || Position.Y > Height;
        }

        public bool CollidesWith(Spaceship spaceship)
        {
            // Create a bounding rectangle for the missile
            Rect missileRect = new Rect(Position.X - 2, Position.Y - 2, 4, 4);

            // Create a bounding rectangle for the spaceship
            Rect spaceshipRect = new Rect(spaceship.Position.X, spaceship.Position.Y, spaceship.Width, spaceship.Height);

            // Check if the rectangles intersect
            return missileRect.Intersects(spaceshipRect);
        }

        public override void Render(DrawingContext context)
        {
            // Push a translate transform onto the drawing context's transform stack
            using var _ = context.PushPostTransform(Matrix.CreateTranslation(Position.X, Position.Y));
            
            context.DrawEllipse(Brushes.Yellow, null, new Point(0, 0), 2, 2);
        }
    }
}
