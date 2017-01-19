/**********************************************************************************\
 * Proyecto: Megaman Final Fight
 * 
 * Autor: Samuel Macedo
 * 
 * Versión: 0.5
 * 
 * TODO:
 *    - Comentar clases de barra de vida, clase base enemigo y clases derivadas de enemigos
 *    - Corregir presionado de teclas izq/der al mismo tiempo durante slide 
 *      debajo de un tile
 * 
 \*********************************************************************************/

#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.PrimaryClasses;

#endregion

namespace Megaman_Final_Fight
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Texture2D mega;

        #region Campos

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public int WindowWidth = 1024;
        public int WindowHeight = 768;

        public static Megaman megaman;

        public static List<IScreenObject> ScreenObjects = new List<IScreenObject>();
        public static List<MovingPlatform> MovingPlatforms = new List<MovingPlatform>();
        public static List<Enemy> Enemies = new List<Enemy>();

        #endregion

        #region Constructor

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Ajuste de la resolución
            this.graphics.PreferredBackBufferWidth = WindowWidth;
            this.graphics.PreferredBackBufferHeight = WindowHeight;

            this.Window.Title = "Megaman";
        }

        #endregion

        #region Inicialización

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Global.Initialize(this);
            Debug.Initialize(this);
            CheckpointManager.Initialize();

            base.Initialize();
        }

        #endregion

        #region Carga de contenido

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargar recursos globales
            Resources.LoadResources(Content);

            megaman = new Megaman();
            megaman.LoadContent(Content);

            Global.LoadMap(@"Maps\Level1", Content);
            megaman.StartSpawn();

            mega = Content.Load<Texture2D>("mega");

            //MediaPlayer.Play(Resources.Music1);
            //MediaPlayer.IsRepeating = true;
            //MediaPlayer.Volume = 0.4f;
        }

        #endregion

        #region Descarga de contenido

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            InputManager.Update();

            Debug.Update(gameTime);

            Global.UpdateMap(gameTime.ElapsedGameTime.Milliseconds);

            foreach (IScreenObject screenObject in ScreenObjects)
            {
                screenObject.Update(gameTime);
            }

            megaman.Update(gameTime);

            base.Update(gameTime);
        }

        #endregion

        #region Dibujado

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            Global.DrawMap();

            foreach (IScreenObject screenObject in ScreenObjects)
            {
                screenObject.Draw(spriteBatch);
            }

            megaman.Draw(spriteBatch);

            Debug.Draw(spriteBatch);

            spriteBatch.Begin();
            spriteBatch.Draw(mega, new Vector2(370, 610), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion
    }
}
