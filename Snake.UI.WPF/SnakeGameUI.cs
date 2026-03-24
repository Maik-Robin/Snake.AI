using Game.Core;
using Snake.AI;
using Snake.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Snake.UI
{
    internal class SnakeGameUI: SnakeGame
    {
        public bool aiEnabled = false;
        //public ISnakeGameController aiController = new SnakeCheater1000();
        public ISnakeGameController aiController;
        private readonly object _lock = new();
        private Thread? _gameThread;
        private volatile bool _running;
        private int _tickIntervalMs;

        public object SyncRoot => _lock;

        public SnakeGameUI(SnakeWorld.SnakeWorldSize size, int tickIntervalMs = 120) : base(size)
        {
            _tickIntervalMs = tickIntervalMs;
        }

        public SnakeGameUI(Int32 sizeX, Int32 sizeY, int tickIntervalMs = 120) : base(sizeX,sizeY)
        {
            _tickIntervalMs = tickIntervalMs;
        }

        public new void Start()
        {
            if (_running) return;
            _running = true;
            _gameThread = new Thread(GameLoop)
            {
                IsBackground = true,
                Name = "SnakeGameLoop"
            };
            _gameThread.Start();
        }

        public new void Stop()
        {
            _running = false;
            _gameThread?.Join();
            _gameThread = null;
        }

        private void GameLoop()
        {
            var stopwatch = Stopwatch.StartNew();
            long nextTickTime = 0;

            while (_running)
            {
                long now = stopwatch.ElapsedMilliseconds;
                if (now >= nextTickTime)
                {
                    nextTickTime += _tickIntervalMs;

                    // If we fell far behind, skip ahead instead of rapid-firing ticks
                    if (nextTickTime < now)
                        nextTickTime = now + _tickIntervalMs;

                    lock (_lock)
                    {
                        if (aiEnabled)
                        {
                            var nextDirection = aiController.GetNextMove(GetGameState());
                            ChangeDirection(nextDirection);
                        }
                        Tick();
                    }

                    if (IsGameOver)
                    {
                        _running = false;
                        return;
                    }
                }
                else
                {
                    // Sleep for a short duration to avoid busy-waiting,
                    // but keep it short enough for responsive timing
                    int sleepMs = (int)(nextTickTime - now);
                    if (sleepMs > 1)
                        Thread.Sleep(sleepMs - 1);
                    else
                        Thread.Yield();
                }
            }
        }

        public void ChangeDirection(Direction2D direction)
        {
            lock (_lock)
            {
                base.ChangeDirection(direction);
            }
        }

        public void Restart()
        {
            lock (_lock)
            {
                Stop();
                ResetWorld();
                OnGameUpdated();
                Start();
            }
        }

    }
}
