#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Megaman_Final_Fight.HelperClasses;
using Megaman_Final_Fight.PrimaryClasses;
using Megaman_Final_Fight.PrimaryClasses.Platforms;
using Megaman_Final_Fight.PrimaryClasses.Enemies;

using xTile;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

#endregion

namespace Megaman_Final_Fight.GlobalClasses
{
    /// <summary>
    /// Clase para almacenar toda la información global del juego
    /// </summary>
    public static class Global
    {
        #region Campos

        public static bool DrawBoundingBoxes;

        // Variables para el mapa
        public static Map CurrentMap;
        public static xTile.Dimensions.Rectangle Viewport;
        public static XnaDisplayDevice MapDisplayDevice;
        public static Rectangle RoomRectangle;

        // Distintas capas del mapa
        public static Layer LevelLayer;
        public static Layer CameraLayer;
        public static Layer MovingPlatformsLayer;
        public static Layer EnemiesLayer;

        public static Rectangle GameSafeArea;

        #endregion

        #region Inicialización

        /// <summary>
        /// Método que inicializa los campos globales
        /// </summary>
        public static void Initialize(Game1 game)
        {
            DrawBoundingBoxes = false;

            // Inicialización de servicios para el desplegue del mapa
            Viewport = new xTile.Dimensions.Rectangle(0, 0, game.WindowWidth, game.WindowHeight);
            MapDisplayDevice = new XnaDisplayDevice(game.Content, game.GraphicsDevice);

            RoomRectangle = Rectangle.Empty;

            GameSafeArea = game.GraphicsDevice.Viewport.TitleSafeArea;
        }

        #endregion

        #region Carga de mapa

        /// <summary>
        /// Método que carga un mapa
        /// </summary>
        /// <param name="path">Ubicación del archivo del mapa</param>
        /// <param name="content">ContentManager para cargar el contenido</param>
        public static void LoadMap(string path, ContentManager content)
        {
            CurrentMap = content.Load<Map>(path);
            CurrentMap.LoadTileSheets(MapDisplayDevice);

            // Se guardan las referencias a las capas
            LevelLayer = CurrentMap.GetLayer("Level");
            CameraLayer = CurrentMap.GetLayer("Camera");
            MovingPlatformsLayer = CurrentMap.GetLayer("Moving Platforms");
            EnemiesLayer = CurrentMap.GetLayer("Enemies");

            CameraLayer.Visible = false;
            MovingPlatformsLayer.Visible = false;
            EnemiesLayer.Visible = false;

            CheckpointManager.ResetCheckpoints();
            ScanLevelLayer();
            ScanMovingPlatformsLayer();
            ScanEnemiesLayer();
        }

        #endregion

        #region Escaneo de tiles

        /// <summary>
        /// Método que se encarga de escanear el nivel para encontrar tiles detectores como
        /// incio de nivel.
        /// </summary>
        private static void ScanLevelLayer()
        {
            int type;
            Vector2 position;

            for (int x = 0; x < LevelLayer.LayerWidth; x++)
            {
                for (int y = 0; y < LevelLayer.LayerHeight; y++)
                {
                    // Se obtiene el valor de tileindex del tile
                    type = GameTiles.CheckTile(LevelLayer, x * GameTiles.TileWidth, y * GameTiles.TileHeight);

                    if (type != -1)
                    {
                        // Se calcula la posición del tile
                        position = new Vector2(x * GameTiles.TileWidth, y * GameTiles.TileHeight);

                        // Dependiendo del tipo de tile se crea el objeto correpondiente
                        switch (type)
                        {
                            // Inicio de nivel
                            case GameTiles.LevelStart:
                                CheckpointManager.AddCheckpoint(position);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Método que se encarga de escanear el nivel para encontrar tiles detectores como
        /// incio de nivel.
        /// </summary>
        private static void ScanMovingPlatformsLayer()
        {
            int type;
            Vector2 position;

            for (int x = 0; x < MovingPlatformsLayer.LayerWidth; x++)
            {
                for (int y = 0; y < MovingPlatformsLayer.LayerHeight; y++)
                {
                    // Se obtiene el valor de tileindex del tile
                    type = GameTiles.CheckTile(MovingPlatformsLayer, x * GameTiles.TileWidth, y * GameTiles.TileHeight);

                    if (type != -1)
                    {
                        // Se calcula la posición del tile
                        position = new Vector2(x * GameTiles.TileWidth, y * GameTiles.TileHeight);

                        MovingPlatform platform = null;

                        // Dependiendo del tipo de tile se crea el objeto correpondiente
                        switch (type)
                        {
                            case GameTiles.UpMovingPlatform:
                                platform = new UpPlatform(Game1.megaman, position);
                                break;

                            case GameTiles.DownMovingPlatform:
                                platform = new DownPlatform(Game1.megaman, position);
                                break;

                            case GameTiles.SliderPlatform_Right:
                                platform = new SliderPlatform(Game1.megaman, position, SliderPlatform.Directions.Right);
                                break;

                            case GameTiles.SliderPlatform_Left:
                                platform = new SliderPlatform(Game1.megaman, position, SliderPlatform.Directions.Left);
                                break;

                            case GameTiles.BombPlatform_Up_0:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Up, 0);
                                break;

                            case GameTiles.BombPlatform_Up_1:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Up, 1);
                                break;

                            case GameTiles.BombPlatform_Up_2:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Up, 2);
                                break;

                            case GameTiles.BombPlatform_Up_3:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Up, 3);
                                break;

                            case GameTiles.BombPlatform_Up_4:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Up, 4);
                                break;

                            case GameTiles.BombPlatform_Up_5:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Up, 5);
                                break;

                            case GameTiles.BombPlatform_Left_0:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Left, 0);
                                break;

                            case GameTiles.BombPlatform_Left_1:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Left, 1);
                                break;

                            case GameTiles.BombPlatform_Left_2:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Left, 2);
                                break;

                            case GameTiles.BombPlatform_Left_3:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Left, 3);
                                break;

                            case GameTiles.BombPlatform_Left_4:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Left, 4);
                                break;

                            case GameTiles.BombPlatform_Left_5:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Left, 5);
                                break;

                            case GameTiles.BombPlatform_Right_0:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Right, 0);
                                break;

                            case GameTiles.BombPlatform_Right_1:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Right, 1);
                                break;

                            case GameTiles.BombPlatform_Right_2:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Right, 2);
                                break;

                            case GameTiles.BombPlatform_Right_3:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Right, 3);
                                break;

                            case GameTiles.BombPlatform_Right_4:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Right, 4);
                                break;

                            case GameTiles.BombPlatform_Right_5:
                                platform = new BombPlatform(Game1.megaman, position, BombPlatform.Orientations.Right, 5);
                                break;

                            case GameTiles.SpikePlatform_Horizontal:
                                platform = new SpikePlatform(Game1.megaman, position, SpikePlatform.Orientation.Horizontal);
                                break;

                            case GameTiles.SpikePlatform_Vertical:
                                platform = new SpikePlatform(Game1.megaman, position, SpikePlatform.Orientation.Vertical);
                                break;

                            case GameTiles.FloatingPlatform_Right:
                                platform = new FloatingPlatform(Game1.megaman, position, 
                                    FloatingPlatform.Orientations.Horizontal, FloatingPlatform.Directions.Right);
                                break;

                            case GameTiles.FloatingPlatform_Left:
                                platform = new FloatingPlatform(Game1.megaman, position,
                                    FloatingPlatform.Orientations.Horizontal, FloatingPlatform.Directions.Left);
                                break;

                            case GameTiles.FloatingPlatform_Up:
                                platform = new FloatingPlatform(Game1.megaman, position,
                                    FloatingPlatform.Orientations.Vertical, FloatingPlatform.Directions.Up);
                                break;

                            case GameTiles.FloatingPlatform_Down:
                                platform = new FloatingPlatform(Game1.megaman, position,
                                    FloatingPlatform.Orientations.Vertical, FloatingPlatform.Directions.Down);
                                break;
                        }

                        if (platform != null)
                        {
                            Game1.ScreenObjects.Add(platform);
                            Game1.MovingPlatforms.Add(platform);
                        }
                    }
                }
            }
        }

        private static void ScanEnemiesLayer()
        {
            int type;
            Vector2 position;

            for (int x = 0; x < LevelLayer.LayerWidth; x++)
            {
                for (int y = 0; y < LevelLayer.LayerHeight; y++)
                {
                    // Se obtiene el valor de tileindex del tile
                    type = GameTiles.CheckTile(EnemiesLayer, x * GameTiles.TileWidth, y * GameTiles.TileHeight);

                    if (type != -1)
                    {
                        // Se calcula la posición del tile
                        position = new Vector2(x * GameTiles.TileWidth, y * GameTiles.TileHeight);

                        Enemy enemie = null;

                        // Dependiendo del tipo de tile se crea el objeto correpondiente
                        switch (type)
                        {
                            case GameTiles.Enem_Met:
                                enemie = new Met(Game1.megaman, position);
                                break;
                            case GameTiles.Enem_Sensor_Left:
                                enemie = new Sensor(Game1.megaman, position, CollisionObject.Directions.Left);
                                break;
                            case GameTiles.Enem_Sensor_Right:
                                enemie = new Sensor(Game1.megaman, position, CollisionObject.Directions.Right);
                                break;
                            case GameTiles.Enem_StationaryCannon:
                                enemie = new StationaryCannon(Game1.megaman, position);
                                break;
                        }

                        if (enemie != null)
                        {
                            Game1.ScreenObjects.Add(enemie);
                            Game1.Enemies.Add(enemie);
                        }
                    }
                }
            }
        }

        #endregion

        #region Actualización y dibujado del mapa

        /// <summary>
        /// Método que actualiza el mapa
        /// </summary>
        /// <param name="timeInterval">Intervalo de tiempo transcurrido entre frames</param>
        public static void UpdateMap(int timeInterval)
        {
            CurrentMap.Update(timeInterval);
        }

        /// <summary>
        /// Método que dibuja el mapa
        /// </summary>
        public static void DrawMap()
        {
            CurrentMap.Draw(MapDisplayDevice, Viewport);
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que determina si otro rectángulo se encuentra dentro de la pantalla
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true en caso de que el rectángulo esté dentro de la pantalla</returns>
        public static bool ScreenContainsRectangle(Rectangle other)
        {
            if (other.Right < Viewport.X)
            {
                return false;
            }
            else if (other.Left > Viewport.X + Viewport.Width)
            {
                return false;
            }
            else if (other.Bottom < Viewport.Y)
            {
                return false;
            }
            else if (other.Top > Viewport.Y + Viewport.Height)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Método que determina si otro rectángulo se encuentra dentro del mismo cuarto que megaman
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true en caso de que el rectángulo esté dentro del cuarto</returns>
        public static bool RoomContainsRectangle(Rectangle other)
        {
            return RoomRectangle.Intersects(other);
        }

        #endregion
    }
}
