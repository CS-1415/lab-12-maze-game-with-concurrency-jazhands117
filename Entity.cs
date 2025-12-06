namespace Lab12;

class Entity
{
    public int Row;
    public int Col;
    public char Symbol;

    public Entity(char symbol, int row, int col)
    {
        Symbol = symbol;
        Row = row;
        Col = col;
    }
}
