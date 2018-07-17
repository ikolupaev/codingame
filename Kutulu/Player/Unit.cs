class Unit
{
    public Vector2D Pos = new Vector2D();
    public string UnitType;
    public int Id;
    public int Param0;
    public int Param1;
    public int Param2;
    public int Distance;

    public int Sanity => Param0;
}
