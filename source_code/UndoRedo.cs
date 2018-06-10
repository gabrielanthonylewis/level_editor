using System.Collections.Generic;

namespace LevelEditor
{
    struct UndoRedoInfo
    {
        public bool success;
        public Tile tile;
    }

    class UndoRedo
    {
        static Stack<Tile> _Redo_TilePlacement = new Stack<Tile>();
        static Stack<Tile> _Undo_TilePlacement = new Stack<Tile>();

        // Add the tile to the Undo stack so when undo
        // is pressed the tile will revert back to its previous state
        public static void TilePlacement_Add(Tile tile)
        {
            Tile newTile = new Tile();
            newTile.canvasIdx = tile.canvasIdx;
            newTile.SetPosition(tile.position.x, tile.position.y);
            newTile.tileMapIdx = tile.tileMapIdx;
            newTile.spriteSheetIdx = tile.spriteSheetIdx;
            _Undo_TilePlacement.Push(newTile);
        }

        // Undo the last tile to its previous state
        public static UndoRedoInfo TilePlacement_Undo(List<Tile>[] layerTiles, int mapWidth)
        {
            UndoRedoInfo undoRedoInfo = new LevelEditor.UndoRedoInfo();
            undoRedoInfo.success = false;

            // If there is nothing to undo, return
            if (_Undo_TilePlacement.Count <= 0)
                return undoRedoInfo;

            Tile tile = _Undo_TilePlacement.Pop();

            // Push the current tile onto the Redo stack in the case where the user wants to revert back again
            Tile originalTile = new Tile();
            originalTile.SetPosition(tile.position.x, tile.position.y);
            originalTile.canvasIdx = tile.canvasIdx;
            originalTile.tileMapIdx = layerTiles[originalTile.canvasIdx][originalTile.position.x + originalTile.position.y * mapWidth].tileMapIdx;
            originalTile.spriteSheetIdx = layerTiles[originalTile.canvasIdx][originalTile.position.x + originalTile.position.y * mapWidth].spriteSheetIdx;
            _Redo_TilePlacement.Push(originalTile);


            undoRedoInfo.success = true;
            undoRedoInfo.tile = tile;
            return undoRedoInfo;
        }

        public static UndoRedoInfo TilePlacement_Redo(List<Tile>[] layerTiles, int mapWidth)
        {
            UndoRedoInfo undoRedoInfo = new LevelEditor.UndoRedoInfo();
            undoRedoInfo.success = false;

            // If there is nothing to redo, return
            if (_Redo_TilePlacement.Count <= 0)
                return undoRedoInfo;

            Tile tile = _Redo_TilePlacement.Pop();

            // Push the current tile onto the Undo stack in the case where the user wants to revert forwards again
            Tile originalTile = new Tile();
            originalTile.SetPosition(tile.position.x, tile.position.y);
            originalTile.canvasIdx = tile.canvasIdx;
            originalTile.tileMapIdx = layerTiles[originalTile.canvasIdx][originalTile.position.x + originalTile.position.y * mapWidth].tileMapIdx;
            originalTile.spriteSheetIdx = layerTiles[originalTile.canvasIdx][originalTile.position.x + originalTile.position.y * mapWidth].spriteSheetIdx;
            _Undo_TilePlacement.Push(originalTile);

            undoRedoInfo.success = true;
            undoRedoInfo.tile = tile;
            return undoRedoInfo;
        }

       public static void Reset()
        {
            // Reset the stacks back to their original state
            _Redo_TilePlacement.Clear();
            _Undo_TilePlacement.Clear();
        }
    }
}
