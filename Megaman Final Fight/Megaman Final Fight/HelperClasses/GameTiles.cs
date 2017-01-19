#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using xTile.Tiles;
using xTile.Layers;

using Megaman_Final_Fight.GlobalClasses;

#endregion

namespace Megaman_Final_Fight.HelperClasses
{
    /// <summary>
    /// Clase para almacenar los distintos tipos de tiles que contiene un mapa
    /// </summary>
    public static class GameTiles
    {
        // Constantes del tamaño de los tiles
        public static int TileWidth = 64;
        public static int TileHeight = 64;

        // Se guardan los tipos de tiles con su respectivo valor de tileindex
        public const int Ladder = 1;
        public const int Solid = 2;
        public const int LeftMovingTile = 3;
        public const int RightMovingTile = 4;
        public const int LevelStart = 5;
        public const int Checkpoint = 0;

        public const int CameraLimit = 3;
        public const int RightCameraTransitionTile = 4;
        public const int LeftCameraTransitionTile = 1;
        public const int UpCameraTransitionTile = 2;
        public const int DownCameraTransitionTile = 0;

        // Plataformas
        public const int DownMovingPlatform = 12;
        public const int UpMovingPlatform = 30;

        public const int SliderPlatform_Right = 15;
        public const int SliderPlatform_Left = 14;
        public const int SliderPlatform_Rail = 7;

        public const int BombPlatform_Up_0 = 25;
        public const int BombPlatform_Up_1 = 26;
        public const int BombPlatform_Up_2 = 19;
        public const int BombPlatform_Up_3 = 27;
        public const int BombPlatform_Up_4 = 4;
        public const int BombPlatform_Up_5 = 5;

        public const int BombPlatform_Left_0 = 0;
        public const int BombPlatform_Left_1 = 1;
        public const int BombPlatform_Left_2 = 8;
        public const int BombPlatform_Left_3 = 9;
        public const int BombPlatform_Left_4 = 2;
        public const int BombPlatform_Left_5 = 10;

        public const int BombPlatform_Right_0 = 3;
        public const int BombPlatform_Right_1 = 11;
        public const int BombPlatform_Right_2 = 16;
        public const int BombPlatform_Right_3 = 24;
        public const int BombPlatform_Right_4 = 17;
        public const int BombPlatform_Right_5 = 18;

        public const int FloatingPlatform_Right = 21;
        public const int FloatingPlatform_Up = 28;
        public const int FloatingPlatform_Down = 6;
        public const int FloatingPlatform_Left = 13;
        public const int FloatingPlatform_Limit = 20;

        public const int SpikePlatform_Vertical = 29;
        public const int SpikePlatform_Horizontal = 22;

        // Enemigos
        public const int Enem_Met = 0;
        public const int Enem_Sensor_Left = 1;
        public const int Enem_Sensor_Right = 2;
        public const int Enem_StationaryCannon = 3;

        /// <summary>
        /// Método que revisa el tipo de tile que se encuentra en cierta posición
        /// </summary>
        /// <param name="layer">Capa en la que se encuentra el tile</param>
        /// <param name="x">Posición en el eje X</param>
        /// <param name="y">Posición en el eje Y</param>
        /// <returns>Regresa el tipo de tile, regresa -1 en caso de no exister nungun tile en la 
        /// posición indicada</returns>
        public static int CheckTile(Layer layer, int x, int y)
        {
            Tile tile = layer.Tiles[x / TileWidth, y / TileHeight];

            if (tile != null)
            {
                return tile.TileIndex;
            }

            return -1;
        }

        /// <summary>
        /// Método que determina si el tile en la posición indicada es un tope de escalera
        /// </summary>
        /// <param name="x">Posición en X</param>
        /// <param name="y">Posición en Y</param>
        /// <returns>Regresa true en caso de ser un tope de escalera</returns>
        public static bool IsTopLadderTile(int x, int y)
        {
            return CheckTile(Global.LevelLayer, x, y - TileHeight) != Ladder;
        }
    }
}
