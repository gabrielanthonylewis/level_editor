using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

using Microsoft.Win32; // For save and open dialog

namespace LevelEditor
{
    // Keyboard Commands
    public static class Command
    {
        public static RoutedCommand UndoCommand = new RoutedCommand();
        public static RoutedCommand RedoCommand = new RoutedCommand();
        public static RoutedCommand SaveCommand = new RoutedCommand();
    }

    public class GeneralGameProperties
    {
        public int lives = 3;
        public int health = 100;
        public int damageSpikes = 100;
        public int damageSawBot = 50;
        public int damageTurretBot = 10;
        public int timer = -1; // if -1 then there is no timer
    }

    struct LevelProperties
    {
        public int mapWidth;
        public int mapHeight;
        public int tileWidth;
        public string tileSheetPath;
        public GeneralGameProperties generalGameProperties;
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Tile>[] _layerTiles = new List<Tile>[3]; // The actual data of each tile on each of the three avaliable layers.
        private List<CroppedBitmap>[] _spriteSheets = new List<CroppedBitmap>[3]; // for the three sprite sheet tabs
        private LevelProperties _level = new LevelProperties(); // the level specfic properties
   
        private string _path = ""; // the current path of the level data
        private int _prevTileIdx = -1; // the previous tile idx selected
        private const int _tileRenderXY = 32; // The rendered size of the tiles (used for better viewing if the sprites are small)

        // Getters
        public int GetMapWidth() { return _level.mapWidth; }
        public int GetMapHeight() { return _level.mapHeight; }

        // Setters
        public void SetGeneralGameProperties(GeneralGameProperties val) { _level.generalGameProperties = val; }
        public void SetMapWidth(int val) { _level.mapWidth = val; }
        public void SetMapHeight(int val) { _level.mapHeight = val; }
        public void SetTileWidth(int val) { _level.tileWidth = val; }
        public void SetPath(string path) { _path = path; }
 

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (App.startWindow != null)
            {
                App.startWindow.intendedClose = true;
                App.startWindow.Close();
            }

            if (App.gamePropertiesWindow != null)
                App.gamePropertiesWindow.Close();
        }

        #region Initialisation
        public void Initialise(string tileSheetPath)
        {
            // Initialise the game properties 
            if(_level.generalGameProperties == null)
                _level.generalGameProperties = new GeneralGameProperties();
            

            // Initialise each sprite sheet
            _level.tileSheetPath = tileSheetPath;

            for (int i = 0; i < _spriteSheets.Length; i++)
                _spriteSheets[i] = new List<CroppedBitmap>();
          
            InitialiseSpriteSheet(_level.tileSheetPath, 0);
            InitialiseSpriteSheet(".../.../images/sheetEnemies.png", 1);
            InitialiseSpriteSheet(".../.../images/sheetOther.png", 2);


            // Set the layer width and height for every layer
            int width = _level.mapWidth * _tileRenderXY;
            int height = _level.mapHeight * _tileRenderXY;
            _Layer0.Width = width;
            _Layer0.Height = height;
            _Layer1.Width = width;
            _Layer1.Height = height;
            _Layer2.Width = width;
            _Layer2.Height = height;
            _Layer3.Width = width;
            _Layer3.Height = height;
            _LayerGrid.Width = width;
            _LayerGrid.Height = height;

            // Clear all of the tiles from every layer
            _Layer0.Children.Clear();
            _Layer1.Children.Clear();
            _Layer2.Children.Clear();
            _Layer3.Children.Clear();
            _LayerGrid.Children.Clear();
            _LayerDraw.Children.Clear();

            // Clear the layer tile information stored
            for (int i = 0; i < _layerTiles.Length; i++)
                _layerTiles[i] = new List<Tile>();


            // Fill each layer with empty tiles..  
            int x = 0, y = 0;
            for (int i = 0; i < _level.mapWidth * _level.mapHeight; i++, x++)
            {
                if (x == _level.mapWidth)
                {
                    x = 0;
                    y++;
                }

                Thickness marginThickness = new Thickness(x * _tileRenderXY, y * _tileRenderXY, 0, 0);
                _Layer0.Children.Add(CreateTileRect(new SolidColorBrush(Colors.White), marginThickness));
                _Layer1.Children.Add(CreateTileRect(new SolidColorBrush(Colors.Transparent), marginThickness));
                _Layer2.Children.Add(CreateTileRect(new SolidColorBrush(Colors.Transparent), marginThickness));
                _Layer3.Children.Add(CreateTileRect(new SolidColorBrush(Colors.Transparent), marginThickness));
                _LayerDraw.Children.Add(CreateTileRect(new SolidColorBrush(Colors.Transparent), marginThickness));

                // Add the tile information to each layerTile layer
                for (int layer = 0; layer < _layerTiles.Length; layer++)
                {
                    Tile newTile = new Tile();
                    newTile.tileMapIdx = -1;
                    newTile.SetPosition(x, y);
                    newTile.canvasIdx = layer;
                    newTile.spriteSheetIdx = -1;
                    _layerTiles[layer].Add(newTile);
                }
            }

            InitialiseGrid();

            // Reset the undo and redo stacks
            UndoRedo.Reset();
        }
        private void InitialiseSpriteSheet(string tileSheetPath, int spriteSheetIdx)
        {
            // Initialise the appropriate sprite sheet
            if (spriteSheetIdx == 0)
                _ListView_TileSet.Items.Clear();
            else if (spriteSheetIdx == 1)
                _Enemies_TileSheet.Items.Clear();
            else if (spriteSheetIdx == 2)
                _Other_Tilesheet.Items.Clear();

            _spriteSheets[spriteSheetIdx].Clear();

            if (!System.IO.File.Exists(tileSheetPath))
                return;

            // Get the spriteSheet from the path
            BitmapSource tileSet = new BitmapImage(new Uri(@tileSheetPath, UriKind.RelativeOrAbsolute));

            for (int y = 0; y + _level.tileWidth <= tileSet.PixelHeight; y += _level.tileWidth)
            {
                for (int x = 0; x + _level.tileWidth <= tileSet.PixelWidth; x += _level.tileWidth)
                {
                    // Divide the sprite sheet into tiles
                    System.Windows.Int32Rect rect = new System.Windows.Int32Rect(x, y, _level.tileWidth, _level.tileWidth);
                    CroppedBitmap cb = new CroppedBitmap(tileSet, rect);

                    _spriteSheets[spriteSheetIdx].Add(cb);

                    // Create the sprite image
                    Image newImg = new Image();
                    newImg.Source = cb;         
                    newImg.Width = _tileRenderXY;
                    newImg.Height = _tileRenderXY; 

                    // Add it to the appropriate sprite/tile sheet tab
                    if (spriteSheetIdx == 0)
                        _ListView_TileSet.Items.Add(newImg);
                    else if (spriteSheetIdx == 1)
                        _Enemies_TileSheet.Items.Add(newImg);
                    else if (spriteSheetIdx == 2)
                        _Other_Tilesheet.Items.Add(newImg);
                }
            }


            if (_ListView_TileSet.Items.Count > 0)
                _ListView_TileSet.SelectedIndex = 0;
            else if (_Enemies_TileSheet.Items.Count > 0)
                _Enemies_TileSheet.SelectedIndex = 0;
            else if (_Other_Tilesheet.Items.Count > 0)
                _Other_Tilesheet.SelectedIndex = 0;

        }
        private void InitialiseGrid()
        {
            _LayerGrid.Children.Clear();

            // Horiz lines
            for (int i = 0; i <= _level.mapHeight; i++)
            {
                Line gridLine = new Line();

                gridLine.Stroke = System.Windows.Media.Brushes.Black;
                gridLine.Fill = new SolidColorBrush(Colors.Black);

                gridLine.X1 = 0;
                gridLine.Y1 = i * _tileRenderXY;

                gridLine.X2 = _Layer1.Width;
                gridLine.Y2 = i * _tileRenderXY;

                _LayerGrid.Children.Add(gridLine);
            }
            // Vertical lines
            for (int i = 0; i <= _level.mapWidth; i++)
            {
                Line gridLine = new Line();

                gridLine.Stroke = System.Windows.Media.Brushes.Black;
                gridLine.Fill = new SolidColorBrush(Colors.Black);

                gridLine.X1 = i * _tileRenderXY;
                gridLine.Y1 = 0;

                gridLine.X2 = i * _tileRenderXY;
                gridLine.Y2 = _Layer1.Height;

                _LayerGrid.Children.Add(gridLine);
            }
        }

        // Helper function to create Rectangle
        private Rectangle CreateTileRect(SolidColorBrush fillBrush, Thickness marginThickness)
        {
            Rectangle temp = new Rectangle();
            temp.Fill = fillBrush;
            temp.HorizontalAlignment = HorizontalAlignment.Left;
            temp.VerticalAlignment = VerticalAlignment.Top;
            temp.Width = _tileRenderXY;
            temp.Height = _tileRenderXY;
            temp.Margin = marginThickness;
            return temp;
        }
        #endregion

        #region Edit Tab
        private void UndoCommandExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            Undo_Click(null, null);
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            _Edit_Menu.Visibility = Visibility.Collapsed;

            // Get the Tile from the undo stack and draw it if successful

            LevelEditor.UndoRedoInfo undoRedoInfo = UndoRedo.TilePlacement_Undo(_layerTiles, _level.mapWidth);

            if (undoRedoInfo.success == false)
                return;

            DrawTile(undoRedoInfo.tile);
        }

        private void RedoCommandExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            Redo_Click(null, null);
        }
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            _Edit_Menu.Visibility = Visibility.Collapsed;

            // Get the Tile from the redo stack and draw it if successful
            LevelEditor.UndoRedoInfo undoRedoInfo = UndoRedo.TilePlacement_Redo(_layerTiles, _level.mapWidth);

            if (undoRedoInfo.success == false)
                return;

            DrawTile(undoRedoInfo.tile);
        }

        private void GameProperties_Click(object sender, RoutedEventArgs e)
        {
            // Show the game properties dialog window
            App.gamePropertiesWindow.Activate();
            App.gamePropertiesWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            App.gamePropertiesWindow.Visibility = Visibility.Visible;

            // update the window's information
            App.gamePropertiesWindow.RefreshInfo(_level.generalGameProperties);

            _Edit_Menu.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region New Tab
        private void MenuItem_New_Click(object sender, RoutedEventArgs e)
        {
            // Open the 'New' dialog window
            App.startWindow.Activate();
            App.startWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            App.startWindow.Visibility = Visibility.Visible;

            _Level_Menu.Visibility = Visibility.Collapsed;
        }

        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            MenuItem_Save_Click(null, null);
        }
        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            // If attempting to save but the current level is new, use Save As
            if (_path == "")
                MenuItem_SaveAs_Click(null, null);
            else // otherwise overwrite the file
                LevelXML.SaveXML(_path, _layerTiles, _level);

            _Level_Menu.Visibility = Visibility.Collapsed;
        }
        private void MenuItem_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Level data (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Saved to the selected path
                _path = saveFileDialog.FileName;
                LevelXML.SaveXML(_path, _layerTiles, _level);
            }

            _Level_Menu.Visibility = Visibility.Collapsed;
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Level data (*.xml)|*.xml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                _path = openFileDialog.FileName;

                // Use the XML loader to get the tile information and general game properties stored.
                LevelEditor.LoadLevelInfo loadLevelInfo = LevelXML.LoadXML(_path);
                if (loadLevelInfo.success == true)
                {
                    // Assign the new information
                    _level.mapHeight = loadLevelInfo.level.mapHeight;
                    _level.mapWidth = loadLevelInfo.level.mapWidth;
                    _level.tileWidth = loadLevelInfo.level.tileWidth;
                    _level.generalGameProperties = loadLevelInfo.level.generalGameProperties;

                    // Reinitialise the level
                    Initialise(loadLevelInfo.level.tileSheetPath);

                    // Draw all of the tiles
                    for (int i = 0; i < loadLevelInfo.tiles.Length; i++)
                        DrawTile(loadLevelInfo.tiles[i]);
                }
                else
                    System.Windows.MessageBox.Show("Error: Failed to load level.");
            }

            _Level_Menu.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Draw Tile
        internal void Draw(System.Windows.Point point, int idx)
        {
            int x = (int)point.X / _tileRenderXY;
            int y = (int)point.Y / _tileRenderXY;

            // Return if off screen
            if (y >= _level.mapHeight || x >= _level.mapWidth)
                return;

            int currCanvas = 0;
            if (_Layer2_RadioButton.IsChecked == true)
                currCanvas = 1;
            else if (_Layer3_RadioButton.IsChecked == true)
                currCanvas = 2;

            // The same tile is already present so no need to draw.
             if (_layerTiles[currCanvas][(x + y * _level.mapWidth)].tileMapIdx == idx &&
                _layerTiles[currCanvas][(x + y * _level.mapWidth)].spriteSheetIdx == _TabControl_SpriteSheets.SelectedIndex)
            {
                return;
            }
          
            // Push Tile onto Undo stack
            UndoRedo.TilePlacement_Add(_layerTiles[currCanvas][(x + y * _level.mapWidth)]);

            _layerTiles[currCanvas][(x + y * _level.mapWidth)].tileMapIdx = idx;
            _layerTiles[currCanvas][(x + y * _level.mapWidth)].spriteSheetIdx = _TabControl_SpriteSheets.SelectedIndex;

            DrawTile(_layerTiles[currCanvas][(x + y * _level.mapWidth)]);
        }
        private void DrawTile(Tile tile)
        {
            if (tile.position.y >= _level.mapHeight || tile.position.x >= _level.mapWidth)
                return;

            int idx = tile.position.x + tile.position.y * _level.mapWidth;

            // Get the current canvas
            Canvas currCanvas = _Layer1;
            if (tile.canvasIdx == 1)
                currCanvas = _Layer2;
            else if (tile.canvasIdx == 2)
                currCanvas = _Layer3;

            currCanvas.Children.RemoveAt(idx);

            // If there is a sprite draw it
            if (tile.tileMapIdx >= 0 && tile.spriteSheetIdx >= 0)
            {
                Image image = new Image();
                image.Source = _spriteSheets[tile.spriteSheetIdx][tile.tileMapIdx];
                image.Width = _tileRenderXY;
                image.Height = _tileRenderXY;
                image.Margin = new Thickness(tile.position.x * image.Width, tile.position.y * image.Height, 0, 0);
                currCanvas.Children.Insert(idx, image);
            }
            else // otherwise draw nothing (erasing the tile)
            {
                tile.tileMapIdx = -1;
                tile.spriteSheetIdx = -1;

                Rectangle rect = new Rectangle();
                rect.Fill = new SolidColorBrush(Colors.Transparent);
                rect.Width = _tileRenderXY;
                rect.Height = _tileRenderXY;
                rect.Margin = new Thickness(tile.position.x * rect.Width, tile.position.y * rect.Height, 0, 0);
                currCanvas.Children.Insert(idx, rect);
            }

            // Update the layerTiles data (the actual reference to all of the tiles)
            if (tile.canvasIdx < _layerTiles.Length && tile.canvasIdx >= 0)
            {
                _layerTiles[tile.canvasIdx][idx].tileMapIdx = tile.tileMapIdx;
                _layerTiles[tile.canvasIdx][idx].spriteSheetIdx = tile.spriteSheetIdx;
            }
        }
        #endregion

        #region Mouse handling for Tile placement
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                MoveMouse(sender, e);
        }
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                int x = (int)e.GetPosition(_LayerDraw).X / _tileRenderXY;
                int y = (int)e.GetPosition(_LayerDraw).Y / _tileRenderXY;

                int tileIdx = (x + y * _level.mapWidth);

                // Prevent drawing in the same tile
                if (tileIdx != _prevTileIdx)
                {
                    MoveMouse(sender, e);

                    _prevTileIdx = tileIdx;
                }
            }
        }
        private void MoveMouse(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Draw depending on the spritesheet tab selected
                if (_TabControl_SpriteSheets.SelectedIndex == 0)
                    Draw(e.GetPosition(_LayerDraw), _ListView_TileSet.SelectedIndex);
                else if (_TabControl_SpriteSheets.SelectedIndex == 1)
                    Draw(e.GetPosition(_LayerDraw), _Enemies_TileSheet.SelectedIndex);
                else if (_TabControl_SpriteSheets.SelectedIndex == 2)
                    Draw(e.GetPosition(_LayerDraw), _Other_Tilesheet.SelectedIndex);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                // Erase Tile
                Draw(e.GetPosition(_LayerDraw), -1);
            }
        }
        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            _prevTileIdx = -1;
        }
        #endregion


        // Grid visibility button
        private void ShowGrid_Click(object sender, RoutedEventArgs e)
        {
            if (_LayerGrid.Visibility == Visibility.Visible)
                _LayerGrid.Visibility = Visibility.Hidden;
            else if (_LayerGrid.Visibility == Visibility.Hidden)
                _LayerGrid.Visibility = Visibility.Visible;

            _View_Menu.Visibility = Visibility.Collapsed;
        }

        #region Layer visibility buttons
        private void Layer1_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_Layer1.Visibility == Visibility.Visible)
                _Layer1.Visibility = Visibility.Hidden;
            else if (_Layer1.Visibility == Visibility.Hidden)
                _Layer1.Visibility = Visibility.Visible;
        }
        private void Layer2_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_Layer2.Visibility == Visibility.Visible)
                _Layer2.Visibility = Visibility.Hidden;
            else if (_Layer2.Visibility == Visibility.Hidden)
                _Layer2.Visibility = Visibility.Visible;
        }
        private void Layer3_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_Layer3.Visibility == Visibility.Visible)
                _Layer3.Visibility = Visibility.Hidden;
            else if (_Layer3.Visibility == Visibility.Hidden)
                _Layer3.Visibility = Visibility.Visible;
        }
        #endregion

        #region Main Tab buttons
        private void Level_Menu_Click(object sender, RoutedEventArgs e)
        {
            if (_Level_Menu.Visibility == Visibility.Collapsed)
            {
                _Level_Menu.Visibility = Visibility.Visible;

                _Edit_Menu.Visibility = Visibility.Collapsed;
                _View_Menu.Visibility = Visibility.Collapsed;
            }
            else if (_Level_Menu.Visibility == Visibility.Visible)
                _Level_Menu.Visibility = Visibility.Collapsed;
        }
        private void Edit_Menu_Click(object sender, RoutedEventArgs e)
        {
            if (_Edit_Menu.Visibility == Visibility.Collapsed)
            {
                _Edit_Menu.Visibility = Visibility.Visible;

                _Level_Menu.Visibility = Visibility.Collapsed;
                _View_Menu.Visibility = Visibility.Collapsed;
            }
            else if (_Edit_Menu.Visibility == Visibility.Visible)
                _Edit_Menu.Visibility = Visibility.Collapsed;
        }
        private void View_Menu_Click(object sender, RoutedEventArgs e)
        {
            if (_View_Menu.Visibility == Visibility.Collapsed)
            {
                _View_Menu.Visibility = Visibility.Visible;

                _Level_Menu.Visibility = Visibility.Collapsed;
                _Edit_Menu.Visibility = Visibility.Collapsed;
            }
            else if (_View_Menu.Visibility == Visibility.Visible)
                _View_Menu.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
