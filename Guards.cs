namespace Lab12;

class Guard : Entity
{
    public int Direction;

    public Guard(int r, int c) : base('%', r, c)
    {
        Direction = 0;
    }
}
