namespace Lab12;

class Maze
{
    public Tile[,] Grid;
    public int Rows;
    public int Cols;

    public Maze(string[] lines)
    {
        Rows = lines.Length;
        Cols = lines[0].Length;
        Grid = new Tile[Rows, Cols];

        int r = 0;
        while (r < Rows)
        {
            int c = 0;
            while (c < Cols)
            {
                Grid[r, c] = new Tile(lines[r][c]);
                c++;
            }
            r++;
        }
    }

    public char Get(int r, int c)
    {
        return Grid[r, c].Symbol;
    }

    public void Set(int r, int c, char s)
    {
        Grid[r, c].Symbol = s;
    }
}
