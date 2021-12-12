namespace Minesweeper
{
    class Tile
    {
        public int value { get; set; }
        public bool isBomb { get; set; }

        public bool isFlagged { get; set; }
        public bool isDiscovered { get; set; }

        public Tile()
        {
            value = 0;
            isBomb = false;
            isDiscovered = false;
            isFlagged = false;
        }

        public Tile(int value, bool isBomb)
        {
            this.value = value;
            this.isBomb = isBomb;
            this.isDiscovered = false;
            this.isFlagged = false;
        }

        public string ToString(int i, int j)
        {
            return i.ToString() + j.ToString() + " | " + "Value: " + value.ToString() + ", Bomb: " + isBomb.ToString();
        }
    }
}
