using System;
using System.Collections.Generic;

using System.Xml; // To save and load using XML

namespace LevelEditor
{
    struct LoadLevelInfo
    {
        public bool success;
        public Tile[] tiles;
        public LevelProperties level;
    }

    class LevelXML
    {
        private static void CreateXML(string path)
        {
            // Create the directory (if nonexistant) to be used to load and save data
            if (!System.IO.Directory.Exists(".../.../LevelData"))
                System.IO.Directory.CreateDirectory(".../.../LevelData");

            // Creat the level data file if nonexistant
            if (!System.IO.File.Exists(path))
            {
                XmlDocument doc = new XmlDocument();

                XmlNode xmlRoot = doc.CreateElement("Root");
                doc.AppendChild(xmlRoot);

                // Level properties
                XmlNode nodeLevelProperties = doc.CreateElement("LevelProperties");
                nodeLevelProperties.AppendChild(doc.CreateElement("MapWidth"));
                nodeLevelProperties.AppendChild(doc.CreateElement("MapHeight"));
                nodeLevelProperties.AppendChild(doc.CreateElement("TileWidth"));
                nodeLevelProperties.AppendChild(doc.CreateElement("TileSheetPath"));

                // Game properties
                XmlNode nodeGameProperties = doc.CreateElement("GameProperties");
                nodeGameProperties.AppendChild(doc.CreateElement("Lives"));
                nodeGameProperties.AppendChild(doc.CreateElement("Health"));
                nodeGameProperties.AppendChild(doc.CreateElement("DamageSpikes"));
                nodeGameProperties.AppendChild(doc.CreateElement("DamageSawBots"));
                nodeGameProperties.AppendChild(doc.CreateElement("DamageTurretBots"));
                nodeGameProperties.AppendChild(doc.CreateElement("Timer"));

                // Append level and game properies to the root
                xmlRoot.AppendChild(nodeLevelProperties);
                xmlRoot.AppendChild(nodeGameProperties);

                // Layers
                for (int i = 0; i < 3; i++)
                    xmlRoot.AppendChild(doc.CreateElement("Layer" + (i + 1).ToString()));

                doc.Save(path);
            }
        }

        public static void SaveXML(string path, List<Tile>[] layerTiles, LevelProperties levelProperties)
        {
            // Creation check/process
            CreateXML(path);


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path); // no check needed as "CreateXML()" does this..


            XmlNode elmRoot = xmlDoc.ChildNodes[0];

            // Level Properties
            elmRoot.ChildNodes[0].ChildNodes[0].InnerText = levelProperties.mapWidth.ToString();
            elmRoot.ChildNodes[0].ChildNodes[1].InnerText = levelProperties.mapHeight.ToString();
            elmRoot.ChildNodes[0].ChildNodes[2].InnerText = levelProperties.tileWidth.ToString();
            elmRoot.ChildNodes[0].ChildNodes[3].InnerText = levelProperties.tileSheetPath;

            // Game Properties
            elmRoot.ChildNodes[1].ChildNodes[0].InnerText = levelProperties.generalGameProperties.lives.ToString();
            elmRoot.ChildNodes[1].ChildNodes[1].InnerText = levelProperties.generalGameProperties.health.ToString();
            elmRoot.ChildNodes[1].ChildNodes[2].InnerText = levelProperties.generalGameProperties.damageSpikes.ToString();
            elmRoot.ChildNodes[1].ChildNodes[3].InnerText = levelProperties.generalGameProperties.damageSawBot.ToString();
            elmRoot.ChildNodes[1].ChildNodes[4].InnerText = levelProperties.generalGameProperties.damageTurretBot.ToString();
            elmRoot.ChildNodes[1].ChildNodes[5].InnerText = levelProperties.generalGameProperties.timer.ToString();

            // 2 because only want to clear layers layers
            for (int i = 2; i < elmRoot.ChildNodes.Count; i++) 
                elmRoot.ChildNodes[i].RemoveAll();


            // Save each tile to their appropriate layer
            for(int layer = 0; layer < layerTiles.Length; layer++)
            {
                for (int i = 0; i < layerTiles[layer].Count; i++)
                {
                    XmlElement elmTile = xmlDoc.CreateElement("Tile");

                    // Tile data
                    XmlElement elmTileMapIdx = xmlDoc.CreateElement("TileMapIdx");
                    elmTileMapIdx.InnerText = layerTiles[layer][i].tileMapIdx.ToString();

                    XmlElement elmSpriteSheetIdx = xmlDoc.CreateElement("SpriteSheetIdx");
                    elmSpriteSheetIdx.InnerText = layerTiles[layer][i].spriteSheetIdx.ToString();

                    elmTile.AppendChild(elmTileMapIdx);
                    elmTile.AppendChild(elmSpriteSheetIdx);

                    elmRoot.ChildNodes[2 + layer].AppendChild(elmTile);
                }
            }
         

            xmlDoc.Save(path);
        }

        public static LoadLevelInfo LoadXML(string path)
        {
            LoadLevelInfo loadLevelInfo = new LoadLevelInfo();
            loadLevelInfo.success = true;

            // Check if it is a valid path
            if (!System.IO.File.Exists(path))
            {
                loadLevelInfo.success = false;
                return loadLevelInfo;
            }


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path); // check already done so it must be valid


            // Level Properties
            LevelProperties level = new LevelProperties();
            XmlNodeList levelPropertiesNode = xmlDoc.GetElementsByTagName("LevelProperties");

            if (levelPropertiesNode.Count == 0)
                loadLevelInfo.success = false;

            for(int i = 0; i < levelPropertiesNode.Count; i++)
            {
                foreach(XmlNode child in levelPropertiesNode[i].ChildNodes)
                {
                    if(child.Name == "MapWidth")
                        level.mapWidth = Convert.ToInt32(child.InnerText);
                    if (child.Name == "MapHeight")
                        level.mapHeight = Convert.ToInt32(child.InnerText);
                    if (child.Name == "TileWidth")
                        level.tileWidth = Convert.ToInt32(child.InnerText);
                    if (child.Name == "TileSheetPath")
                        level.tileSheetPath = child.InnerText;
                }
            }

            // Game Properties
            level.generalGameProperties = new GeneralGameProperties();
            XmlNodeList gamePropertiesNode = xmlDoc.GetElementsByTagName("GameProperties");

            if (gamePropertiesNode.Count == 0)
                loadLevelInfo.success = false;

            for (int i = 0; i < gamePropertiesNode.Count; i++)
            {
                foreach (XmlNode child in gamePropertiesNode[i].ChildNodes)
                {
                    if (child.Name == "Lives")
                        level.generalGameProperties.lives = Convert.ToInt32(child.InnerText);
                    if (child.Name == "Health")
                        level.generalGameProperties.health = Convert.ToInt32(child.InnerText);
                    if (child.Name == "DamageSpikes")
                        level.generalGameProperties.damageSpikes = Convert.ToInt32(child.InnerText);
                    if (child.Name == "DamageSawBots")
                        level.generalGameProperties.damageSawBot = Convert.ToInt32(child.InnerText);
                    if (child.Name == "DamageTurretBots")
                        level.generalGameProperties.damageTurretBot = Convert.ToInt32(child.InnerText);
                    if (child.Name == "Timer")
                        level.generalGameProperties.timer = Convert.ToInt32(child.InnerText);
                }
            }

            // Layers/Tiles
            Tile[] tiles;
            XmlNodeList[] layerNodeList = new XmlNodeList[3];
            layerNodeList[0] = xmlDoc.GetElementsByTagName("Layer1");
            layerNodeList[1] = xmlDoc.GetElementsByTagName("Layer2");
            layerNodeList[2] = xmlDoc.GetElementsByTagName("Layer3");

            // If any of the layers have no nodes then it must have failed, they should all have the same (above 0 or else nothing to load anyways)
            if (layerNodeList[0].Count == 0 || layerNodeList[1].Count == 0 || layerNodeList[2].Count == 0)
            {
                tiles = new Tile[0];
                loadLevelInfo.success = false;
            }
            else
            {
                // Create a tile array with all of the saved information assigned correctly.
                tiles = new Tile[layerNodeList[0][0].ChildNodes.Count + layerNodeList[1][0].ChildNodes.Count + layerNodeList[2][0].ChildNodes.Count];
                for (int layerIdx = 0, infoIdx = 0; layerIdx < layerNodeList.Length; layerIdx++)
                {
                    int x = 0, y = 0;
                    for (int i = 0; i < layerNodeList[layerIdx][0].ChildNodes.Count; i++, x++, infoIdx++)
                    {
                        if (x == level.mapWidth)
                        {
                            x = 0;
                            y++;
                        }

                        tiles[infoIdx] = new Tile();
                        tiles[infoIdx].SetPosition(x, y);
                        tiles[infoIdx].tileMapIdx = Convert.ToInt32(layerNodeList[layerIdx][0].ChildNodes[i].ChildNodes[0].InnerText);
                        tiles[infoIdx].spriteSheetIdx = Convert.ToInt32(layerNodeList[layerIdx][0].ChildNodes[i].ChildNodes[1].InnerText);
                        tiles[infoIdx].canvasIdx = layerIdx;
                    }
                }
            }


            loadLevelInfo.tiles = tiles;
            loadLevelInfo.level = level;
            return loadLevelInfo;
        }
    }
}
