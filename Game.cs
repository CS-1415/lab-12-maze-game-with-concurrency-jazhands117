using System.Diagnostics;
namespace Lab12;

class Game
{
    Maze Maze;
    Player Player;
    List<Guard> Guards;
    int Score;
    int CoinsRemaining;
    bool Running;
    object LockObj = new object();
    Stopwatch Timer;
    CancellationTokenSource TokenSource;

    public Game()
    {
        string[] rows = File.ReadAllLines("maze.txt");
        Maze = new Maze(rows);
        Guards = new List<Guard>();
        Score = 0;
        Running = true;
        Timer = new Stopwatch();
        TokenSource = new CancellationTokenSource();

        int r = 0;
        while (r < Maze.Rows)
        {
            int c = 0;
            while (c < Maze.Cols)
            {
                char t = Maze.Get(r, c);
                if (t == '^')
                    CoinsRemaining++;
                if (t == '%')
                {
                    Guards.Add(new Guard(r, c));
                    Maze.Set(r, c, ' ');
                }
                if (t == '@')
                    Player = new Player(r, c);

                c++;
            }
            r++;
        }

        if (Player == null)
            Player = new Player(1, 1);
    }

    public void Run()
    {
        Console.SetCursorPosition(0, 0);
        Timer.Start();

        foreach (Guard g in Guards)
        {
            Thread t = new Thread(() => GuardLoop(g, TokenSource.Token));
            t.Start();
        }

        Thread renderer = new Thread(() =>
        {
            while (Running)
            {
                Draw();
                Thread.Sleep(50); 
            }
        });
        renderer.IsBackground = true;
        renderer.Start();

        Draw();

        while (Running)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Escape)
                Running = false;
            else
                HandleKey(key.Key);
        }

        TokenSource.Cancel();
        EndGame("Exited. Thanks for playing!");
    }

    void HandleKey(ConsoleKey key)
    {
        int nr = Player.Row;
        int nc = Player.Col;

        if (key == ConsoleKey.W) 
            nr--;
        if (key == ConsoleKey.S) 
            nr++;
        if (key == ConsoleKey.A)   
            nc--;
        if (key == ConsoleKey.D) 
            nc++;

        TryMovePlayer(nr, nc);
}

    void TryMovePlayer(int r, int c)
    {
        if (r < 0) return;
        if (c < 0) return;
        if (r >= Maze.Rows) return;
        if (c >= Maze.Cols) return;

        char t = Maze.Get(r, c);
        if (t == '*') return;

        if (t == '^')
        {
            Score += 100;
            CoinsRemaining--;
            Maze.Set(r, c, " "[0]);
            if (CoinsRemaining == 0)
                OpenGate();
        }

        if (t == '$')
        {
            Score += 200;
            Maze.Set(r, c, " "[0]);
        }

        if (t == '#')
        {
            Running = false;
            Timer.Stop();
            EndGame("You Win!");
            return;
        }

        lock (LockObj)
        {
            Player.Row = r;
            Player.Col = c;

            foreach (Guard g in Guards)
            {
                if (g.Row == r && g.Col == c)
                {
                    Running = false;
                    EndGame("You Lose! Caught by a guard.");
                    return;
                }
            }
        }
    }

    void OpenGate()
    {
        int r = 9;
        int c = 24;
        int i = -1;
        while (i <= 1)
        {
            Maze.Set(r + i, c, " "[0]);
            i++;
        }
    }

    void GuardLoop(Guard g, CancellationToken tok)
    {
        Random rnd = new Random();

        while (!tok.IsCancellationRequested)
        {
            Thread.Sleep(300);
            int dir = g.Direction;

            int nr = g.Row;
            int nc = g.Col;

            if (dir == 0) 
                nr--;
            if (dir == 1) 
                nr++;
            if (dir == 2) 
                nc--;
            if (dir == 3)  
                nc++;

            bool moved = false;

            lock (LockObj)
            {
                if (nr >= 0 && nr < Maze.Rows && nc >= 0 && nc < Maze.Cols)
                {
                    char t = Maze.Get(nr, nc);
                    if (t != '*' && t != '#')
                    {
                        g.Row = nr;
                        g.Col = nc;
                        moved = true;
                    }
                }

                if (g.Row == Player.Row && g.Col == Player.Col)
                {
                    Running = false;
                    EndGame("You Lose");
                    return;
                }
            }

            if (!moved)
            {
                int newDir = dir + 1;
                if (newDir > 3) newDir = 0;
                g.Direction = newDir;
            }
        }
    }

    void Draw()
    {
        lock (LockObj)
        {
            Console.SetCursorPosition(0, 0);

            int r = 0;
            while (r < Maze.Rows)
            {
                int c = 0;
                while (c < Maze.Cols)
                {
                    bool drawn = false;

                    if (Player.Row == r && Player.Col == c)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write('@');
                        drawn = true;
                    }

                    if (!drawn)
                    {
                        bool guardHere = false;

                        foreach (Guard g in Guards)
                    { 
                            if (g.Row == r && g.Col == c)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write('%');
                                guardHere = true;
                                break;
                            }
                        }

                        if (!guardHere)
                        {
                            char t = Maze.Get(r, c);

                            if (t == '*')
                                Console.ForegroundColor = ConsoleColor.Cyan;
                            else if (t == '^')
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            else if (t == '$')
                                Console.ForegroundColor = ConsoleColor.Blue;
                            else
                                Console.ForegroundColor = ConsoleColor.Gray;

                            Console.Write(t);
                        }
                    }

                    c++;
                }

                Console.WriteLine();
                r++;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Score: " + Score);
            Console.WriteLine("Coins: " + CoinsRemaining);
            Console.WriteLine("Time: " + Timer.Elapsed.ToString(@"mm\:ss"));
        }
    }

    void EndGame(string msg)
    {
        Console.Clear();
        Console.WriteLine(msg);
        Console.WriteLine("Final Score: " + Score);
        Console.WriteLine("Time: " + Timer.Elapsed.ToString(@"mm\:ss"));
        Environment.Exit(0);
    }
}