#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Megaman_Final_Fight.HelperClasses
{
    /// <summary>
    /// Permite crear objetos que actuan como contadores de tiempo
    /// </summary>
    public struct Timer
    {
        #region Campos

        public int Counter;
        public bool IsActive;
        public int StartTime;
        public bool Enabled;

        public delegate void TimerStopedHandler();
        public event TimerStopedHandler TimerStoped;
        public event TimerStopedHandler TimerReachedZero;

        #endregion

        #region Propiedades

        /// <summary>
        /// Regresa el porcentaje de tiempo transcurrido del contador, entre más
        /// cercano a 1 indica que ha transcurrudo más tiempo y que está a punto de
        /// terminar el conteo.
        /// </summary>
        public float ElapsedTimePercentage
        {
            get
            {
                if (!IsActive)
                {
                    return 0f;
                }
                else
                {
                    return 1f - ((float)Counter / (float)StartTime);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa el contador
        /// </summary>
        /// <param name="startTime">Tiempo en el que inicia el contador</param>
        public Timer(int startTime)
        {
            this.Counter = 0;
            this.IsActive = false;
            this.StartTime = startTime;
            this.Enabled = true;

            TimerStoped = null;
            TimerReachedZero = null;
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Inicia el contador sólo si el contador ya ha terminado
        /// </summary>
        public void Start()
        {
            if (!IsActive && Enabled)
            {
                Counter = StartTime;
                IsActive = true;
            }
        }

        /// <summary>
        /// Reinicia el contador sin importar si ha terminado o no
        /// </summary>
        public void Restart()
        {
            if (Enabled)
            {
                Counter = StartTime;
                IsActive = true;
            }
        }

        /// <summary>
        /// Detiene el contador
        /// </summary>
        public void Stop()
        {
            if (IsActive && Enabled)
            {
                Counter = 0;
                IsActive = false;

                if (TimerStoped != null)
                {
                    TimerStoped.Invoke();
                }
            }
        }

        /// <summary>
        /// Método que actualiza el contador
        /// </summary>
        /// <param name="miliseconds"></param>
        public void Update(int miliseconds)
        {
            if (IsActive && Enabled)
            {
                Counter -= miliseconds;

                if (Counter <= 0)
                {
                    Stop();

                    if (TimerReachedZero != null)
                    {
                        TimerReachedZero.Invoke();
                    }
                }
            }
        }

        #endregion
    }
}
