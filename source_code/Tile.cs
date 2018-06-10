namespace LevelEditor
{
    struct Position
    {
        public int x;
        public int y;
    };

    // Stores all of the information relevent to the tile
    // NOTE: The reason for the public getters and setters is because public variables are 
    // considered bad practice, so I dont access them directly.
    class Tile
    {
        private bool _isCollidable = false;
        public bool isCollidable { get { return _isCollidable; } set { _isCollidable = value; } }

        private int _tileMapIdx = -1;
        public int tileMapIdx { get { return _tileMapIdx; } set { _tileMapIdx = value; } }

        private int _spriteSheetIdx = -1;
        public int spriteSheetIdx { get { return _spriteSheetIdx; } set { _spriteSheetIdx = value; } }

        private int _canvasIdx = -1;
        public int canvasIdx { get { return _canvasIdx; } set { _canvasIdx = value; } }

        private Position _position;
        public Position position {  get { return _position; } }
        public void SetPosition(int x, int y) { _position.x = x; _position.y = y; }
    }
}
