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

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Clase que sirve como base para proporcionar funcionalidad a las distintas armas
    /// de megaman
    /// </summary>
    public abstract class MegamanWeapon
    {
        #region Campos

        /// <summary>
        /// Timer para la reproducción de animación de disparo de megaman
        /// </summary>
        public Timer ShotAnimationTimer;

        /// <summary>
        /// Referencia a la instancia de megaman
        /// </summary>
        protected Megaman megaman;

        /// <summary>
        /// Bandera para evitar disparos continuos
        /// </summary>
        private bool pressedShoot;

        /// <summary>
        /// Posibles acciones a realizar en caso de ocurrir una colisión con algún objeto
        /// </summary>
        public enum CollisionActions
        {
            Deactivate,
            Reflect,
            None
        };

        #endregion

        #region Delegados y eventos

        /// <summary>
        /// Delegado en el que se basan los eventos de la clase MegamanWeapon
        /// </summary>
        /// <param name="weapon">Arma que provoca el evento</param>
        public delegate void MegamanWeaponHandler(MegamanWeapon weapon);

        /// <summary>
        /// Evento que ocurre antes de realizar un nuevo disparo
        /// </summary>
        public event MegamanWeaponHandler BeforeShoot;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica si el arma puede realizar un nuevo disparo
        /// </summary>
        protected virtual bool CanShoot
        {
            get
            {
                return !megaman.IsSliding && ShotCount < MaxShotCount 
                    && !megaman.Camera.IsOnTransition && !megaman.IsOnDamage
                    && !megaman.IsDeath;
            }
        }

        /// <summary>
        /// Indica si los proyectiles pueden colisionar con otros objetos
        /// </summary>
        protected virtual bool ProjectilesCanCollide
        {
            get
            {
                return !megaman.IsDeath;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Contructor que incializa el arma
        /// </summary>
        /// <param name="megaman">Referencia a la instancia megaman</param>
        public MegamanWeapon(Megaman megaman)
        {
            this.megaman = megaman;

            this.pressedShoot = false;

            this.ShotAnimationTimer = new Timer(350);
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar la actualización
        /// deacuerdo al tipo de arma
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            // Se actualiza el timer de la animación
            ShotAnimationTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            if (InputManager.ButtonDown(InputManager.Buttons.Shoot))
            {
                if (!pressedShoot)
                {
                    if (CanShoot)
                    {
                        // Invocación del evento antes de disparo
                        if (BeforeShoot != null)
                        {
                            BeforeShoot.Invoke(this);
                        }

                        // Se realiza el nuevo disparo
                        Shoot();

                        ShotAnimationTimer.Restart();
                    }

                    pressedShoot = true;
                }
            }
            else
            {
                pressedShoot = false;
            }
        }

        #endregion

        #region Miembros abstractos

        /// <summary>
        /// Método que deben de implementar la clases derivadas para realizar el dbujado
        /// deacuerdo al tipo de arma
        /// </summary>
        /// <param name="spritebatch"></param>
        public abstract void Draw(SpriteBatch spritebatch);

        /// <summary>
        /// Métoto que deben de imprimentar las clases derivadas para determinar si otro objeto
        /// colisiona con el proyetil(es) que disapara el tipo de arma
        /// </summary>
        /// <param name="other">Objeto con el cual se quiere revisar colisiones</param>
        /// <param name="action">Acción a realizar en caso de ocurrir una colisión</param>
        /// <returns>true en caso de ocurrir una colisión</returns>
        public abstract bool Intersects(Rectangle other, CollisionActions action);

        /// <summary>
        /// Propiedad que deben de implementar las clases derivadas para determinar
        /// la cantidad de daño que realiza el arma
        /// </summary>
        public abstract int Damage { get; }

        /// <summary>
        /// Propiedad que deben de implementar las clases derivadas para determinar
        /// la cantidad máxima posible de disparos que pueden estar activos
        /// </summary>
        protected abstract int MaxShotCount { get; }

        /// <summary>
        /// Propiedad que deben de implementar las clases derivadas para determinar
        /// la cantidad de disparos activos
        /// </summary>
        protected abstract int ShotCount { get; }

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar la lógica
        /// propia de disparos del tipo de arma
        /// </summary>
        protected abstract void Shoot();

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar la lógica
        /// propia para desactivar todos los disparos del arma
        /// </summary>
        public abstract void DeactivateAllShots();

        #endregion
    }
}
