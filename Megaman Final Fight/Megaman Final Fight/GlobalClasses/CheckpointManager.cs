#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Megaman_Final_Fight.PrimaryClasses;
using Megaman_Final_Fight.HelperClasses;

#endregion

namespace Megaman_Final_Fight.GlobalClasses
{
    /// <summary>
    ///  Clase para administrar los checkpoints
    /// </summary>
    public static class CheckpointManager
    {
        // Lista que guarda los checkpoints alcanzados
        private static List<Vector2> Checkpoints;

        /// <summary>
        /// Regresa la posición del último checkpoint
        /// </summary>
        public static Vector2 LastCheckpoint
        {
            get { return Checkpoints[Checkpoints.Count - 1]; } 
        }

        /// <summary>
        /// Método que inicializa los campos
        /// </summary>
        public static void Initialize()
        {
            Checkpoints = new List<Vector2>();
        }

        /// <summary>
        /// Método que limpia el listado de checkpoints
        /// </summary>
        public static void ResetCheckpoints()
        {
            Checkpoints.Clear();
        }

        /// <summary>
        /// Método que agrega un nuevo checkpoint
        /// </summary>
        /// <param name="newCheckpoint">Posición del nuevo checkpoint</param>
        public static void AddCheckpoint(Vector2 newCheckpoint)
        {
            if (!ContainsCheckpoint(newCheckpoint))
            {
                Checkpoints.Add(newCheckpoint);
            }
        }

        /// <summary>
        /// Método que se encarga de revisar si megaman alcanza un nuevo checkpoint
        /// </summary>
        /// <param name="megaman"></param>
        public static void SearchNewCheckpoints(Megaman megaman)
        {
            // Se obtiene la posición central de megaman
            int x = (int)megaman.Center.X; 
            int y = (int)megaman.Center.Y;

            // Se revisa si megaman toca un tile de checkpoint, en caso de ser así se agrega
            // el nuevo checkpoint al listado
            if (GameTiles.CheckTile(Global.LevelLayer, x, y) == GameTiles.Checkpoint)
            {
                Vector2 newCheckpoint = GetCheckpointPosition(x, y);
                AddCheckpoint(newCheckpoint);
            }
        }

        /// <summary>
        /// Método que obtiene la posición del tile inferior de la fila de tiles de checkpoints
        /// colocados en el mapa
        /// </summary>
        /// <param name="x">Posición inicial en X</param>
        /// <param name="y">Posición inicial en Y</param>
        /// <returns></returns>
        private static Vector2 GetCheckpointPosition(int x, int y)
        {
            bool found = false;
            int type;

            // Mientras no se encuentre un tile que sea diferente a un tile checkpoint entonces 
            // se continúa avanzando en Y hacia abajo
            do
            {
                type = GameTiles.CheckTile(Global.LevelLayer, x, y + GameTiles.TileHeight);

                if (type != GameTiles.Checkpoint)
                {
                    found = true;
                }
                else
                {
                    y += GameTiles.TileHeight;
                }
            }
            while (!found);

            // Se obtiene la posición exacta del tile
            x = x / GameTiles.TileWidth * GameTiles.TileWidth;
            y = y / GameTiles.TileHeight * GameTiles.TileHeight;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Método que determina si el checkpoint ya se ecuentra en el listado de checkpoints
        /// </summary>
        /// <param name="newCheckpoint"></param>
        /// <returns></returns>
        private static bool ContainsCheckpoint(Vector2 newCheckpoint)
        {
            bool found = false;

            foreach (Vector2 checkpoint in Checkpoints)
            {
                if (newCheckpoint.X == checkpoint.X 
                    && newCheckpoint.Y == checkpoint.Y)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }
    }
}
