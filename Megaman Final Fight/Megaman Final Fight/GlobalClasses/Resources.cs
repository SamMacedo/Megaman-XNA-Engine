#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using AnimationSystem;

#endregion

namespace Megaman_Final_Fight.GlobalClasses
{
    /// <summary>
    /// Clase para almacenar los recursos globales
    /// </summary>
    public static class Resources
    {
        #region Campos

        public static Texture2D T2D_Pixel;
        public static Texture2D T2D_Megaman_Buster_Small;
        public static Texture2D T2D_Megaman_Buster_Mid;
        public static Texture2D T2D_Megaman_Buster_Full;
        public static Texture2D T2D_SliderSolidPlatform;
        public static Texture2D T2D_SliderPlatformBall;
        public static Texture2D T2D_BombPlatformNumbersSheet;
        public static Texture2D T2D_MegamanHealthBarContainer;
        public static Texture2D T2D_HealthBar;
        public static Texture2D T2D_Megaman_DamageFlash;
        public static Texture2D T2D_Met_Shot;
        public static Texture2D T2D_StationaryCannon_Base;
        public static Texture2D T2D_StationaryCannon_Barrel;

        public static SoundEffect SFX_Megaman_Hit;
        public static SoundEffect SFX_Megaman_Death;
        public static SoundEffect SFX_Megaman_BusterCannon_Small;
        public static SoundEffect SFX_Megaman_Land;
        public static SoundEffect SFX_Megaman_Spawn_Land;
        public static SoundEffect SFX_Megaman_Health_Recover;
        public static SoundEffect SFX_UpMovingPlatform;
        public static SoundEffect SFX_DownMovingPlatform;
        public static SoundEffect SFX_Enemy_Hit;
        public static SoundEffect SFX_Enemy_Reflect;
        public static SoundEffect SFX_Enemy_Shot1;

        public static SpriteFont FNT_font1;

        public static AnimationSet ANIM_UpPlatform;
        public static AnimationSet ANIM_DownPlatform;
        public static AnimationSet ANIM_SliderPlatform;
        public static AnimationSet ANIM_BombPlatform;
        public static AnimationSet ANIM_SpikePlatform;
        public static AnimationSet ANIM_FloatingPlatform;
        public static AnimationSet ANIM_ENEM_Met;
        public static AnimationSet ANIM_ENEM_Sensor;

        public static Song Music1;

        #endregion

        #region Métodos

        /// <summary>
        /// Método que carga los recursos globales
        /// </summary>
        /// <param name="content">ContentManager para cargar el contenido</param>
        public static void LoadResources(ContentManager content)
        {
            // Texturas
            T2D_Pixel = content.Load<Texture2D>(@"Graphics\Other\Pixel");
            T2D_Megaman_Buster_Small = content.Load<Texture2D>(@"Graphics\Projectiles\Megaman\Buster_Small");
            T2D_SliderSolidPlatform = content.Load<Texture2D>(@"Graphics\Animations\Moving Platforms\SliderSolidPlatform");
            T2D_SliderPlatformBall = content.Load<Texture2D>(@"Graphics\Animations\Moving Platforms\SliderPlatformBall");
            T2D_BombPlatformNumbersSheet = content.Load<Texture2D>(@"Graphics\Animations\Moving Platforms\BombPlatformNumbers");
            T2D_MegamanHealthBarContainer = content.Load<Texture2D>(@"Graphics\HealthBars\MegamanContainer");
            T2D_HealthBar = content.Load<Texture2D>(@"Graphics\HealthBars\Bar");
            T2D_Megaman_DamageFlash = content.Load<Texture2D>(@"Graphics\Animations\Megaman\MegamanDamageFlash");
            T2D_Met_Shot = content.Load<Texture2D>(@"Graphics\Projectiles\Enemies\Met_Shot");
            T2D_StationaryCannon_Base = content.Load<Texture2D>(@"Graphics\Animations\Enemies\StationaryCannon_Base");
            T2D_StationaryCannon_Barrel = content.Load<Texture2D>(@"Graphics\Animations\Enemies\StationaryCannon_Barrel");

            // Sonidos
            SFX_Megaman_Hit = content.Load <SoundEffect>(@"Sounds\Megaman\Hit");
            SFX_Megaman_Death = content.Load<SoundEffect>(@"Sounds\Megaman\Death");
            SFX_Megaman_BusterCannon_Small = content.Load<SoundEffect>(@"Sounds\Megaman\Weapons\Shot");
            SFX_Megaman_Land = content.Load<SoundEffect>(@"Sounds\Megaman\Land");
            SFX_Megaman_Spawn_Land = content.Load<SoundEffect>(@"Sounds\Megaman\Spawn_Land");
            SFX_Megaman_Health_Recover = content.Load<SoundEffect>(@"Sounds\Megaman\Health_Recover");
            SFX_UpMovingPlatform = content.Load<SoundEffect>(@"Sounds\Moving Platforms\UpMovingPlatform");
            SFX_DownMovingPlatform = content.Load<SoundEffect>(@"Sounds\Moving Platforms\DownMovingPlatform");
            SFX_Enemy_Hit = content.Load<SoundEffect>(@"Sounds\Enemies\Hit");
            SFX_Enemy_Reflect = content.Load<SoundEffect>(@"Sounds\Enemies\Reflect");
            SFX_Enemy_Shot1 = content.Load<SoundEffect>(@"Sounds\Enemies\Shots\Enemy_Shot1");

            // Fuentes
            FNT_font1 = content.Load<SpriteFont>(@"Fonts\font1");

            // Animaciones
            ANIM_UpPlatform = content.Load<AnimationSet>(@"Graphics\Animations\Moving Platforms\UpPlatform");
            ANIM_DownPlatform = content.Load<AnimationSet>(@"Graphics\Animations\Moving Platforms\DownPlatform");
            ANIM_SliderPlatform = content.Load<AnimationSet>(@"Graphics\Animations\Moving Platforms\SliderPlatform");
            ANIM_BombPlatform = content.Load<AnimationSet>(@"Graphics\Animations\Moving Platforms\BombPlatform");
            ANIM_SpikePlatform = content.Load<AnimationSet>(@"Graphics\Animations\Moving Platforms\SpikePlatform");
            ANIM_FloatingPlatform = content.Load<AnimationSet>(@"Graphics\Animations\Moving Platforms\FloatingPlatform");
            ANIM_ENEM_Met = content.Load<AnimationSet>(@"Graphics\Animations\Enemies\Met");
            ANIM_ENEM_Sensor = content.Load<AnimationSet>(@"Graphics\Animations\Enemies\Sensor");

            // Música
            Music1 = content.Load<Song>(@"Music\Music1");
        }

        #endregion
    }
}
