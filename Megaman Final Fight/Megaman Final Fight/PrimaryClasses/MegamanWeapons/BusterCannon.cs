#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

using AnimationSystem;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses.MegamanWeapons
{
    public sealed class BusterCannon : MegamanWeapon
    {
        #region Campos

        // Arreglo de disparos de megaman
        private Shot[] shots;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica la cantidad máxima de disparos posibles
        /// </summary>
        protected override int MaxShotCount
        {
            get { return 3; }
        }

        /// <summary>
        /// Indica la cantidad de disparos que se encuentran activos
        /// </summary>
        protected override int ShotCount
        {
            get 
            {
                int count = 0;

                for (int i = 0; i < shots.Length; i++)
                {
                    if (shots[i].Active)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Indica la cantidad de daño que realiza el buster
        /// </summary>
        public override int Damage
        {
            get { return shots[0].Damage; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa el BusterCannon
        /// </summary>
        /// <param name="megaman">Referencia a la instancia megaman</param>
        public BusterCannon(Megaman megaman)
            : base(megaman)
        {
            // Inicialización del arreglo de disparos
            shots = new Shot[MaxShotCount];

            // Asignación de valores al arreglo de disparos
            for (int i = 0; i < shots.Length; i++)
            {
                shots[i] = new Shot();
            }
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza la lógica de los disparos del buster cannon
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Actualización de cada uno de los disparos
            foreach (Shot shot in shots)
            {
                shot.Update();
            }

            base.Update(gameTime);
        }

        #endregion

        #region Dibujado

        /// <summary>
        /// Método que dibuja los disparos del busterCannon
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void Draw(SpriteBatch spritebatch)
        {
            // Dibujado de cada uno de los disparos
            foreach (Shot shot in shots)
            {
                shot.Draw(spritebatch);
            }
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Métoto que revisa si alguno de los proyectiles del arma intersecta con otro objeto
        /// </summary>
        /// <param name="other">Objeto con el cual se quiere revisar colisiones</param>
        /// <param name="action">Acción a realizar en caso de ocurrir una colisión</param>
        /// <returns>true en caso de ocurrir una colisión</returns>
        public override bool Intersects(Rectangle other, CollisionActions action)
        {
            if (ProjectilesCanCollide)
            {
                foreach (Shot shot in shots)
                {
                    if (shot.Intersects(other))
                    {
                        if (action == CollisionActions.Deactivate)
                        {
                            shot.Deactivate();
                        }
                        else if (action == CollisionActions.Reflect)
                        {
                            if (shot.Angle == 0)
                            {
                                shot.Angle = 135f;
                            }
                            else
                            {
                                shot.Angle = 45f;
                            }

                            shot.CanCollide = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Método que se encarga de realizar un nuevo disparo de busterCannon
        /// </summary>
        protected override void Shoot()
        {
            // Se busca un lugar disponible en el arreglo, en caso de encontrarlo entonces
            // se realiza un nuevo disaparo
            for (int i = 0; i < shots.Length; i++)
            {
                if (!shots[i].Active)
                {
                    shots[i].Activate(megaman.ShotDrawPosition, megaman.FacingAngle);

                    Resources.SFX_Megaman_BusterCannon_Small.Play();

                    return;
                }
            }
        }

        /// <summary>
        /// Método que se encarga de desactivar todos los disparos del buster
        /// </summary>
        public override void DeactivateAllShots()
        {
            for (int i = 0; i < shots.Length; i++)
            {
                if (shots[i].Active)
                {
                    shots[i].Deactivate();
                }
            }

            ShotAnimationTimer.Stop();
        }

        #endregion
    }

    #region Clase auxiliar "Shot"

    class Shot : Projectile
    {
        // Constantes del proyectil
        private const int DefaultWidth = 30;
        private const int DefaultHeight = 22;
        private const float DefaultSpeed = 17f;
        private const int DefaultDamage = 5;

        /// <summary>
        /// Constructor que inicializa el proyectil
        /// </summary>
        public Shot()
            : base(DefaultSpeed, DefaultDamage, DefaultWidth, DefaultHeight, Color.Yellow)
        {
        }

        /// <summary>
        /// Método que dibuja el proyectil
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch, Resources.T2D_Megaman_Buster_Small);
        }
    }

    #endregion
}
