using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading; 


namespace DailyTimer
{

    public delegate void DurationCallback(string duration);

    enum TIMER_STATE
    {
        TS_START = 0,
        TS_END,
        TS_UPDATE
    };
          
    class Timer
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,string val,string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,string key,string def, StringBuilder retVal,
                    int size,string filePath);

        private const string fileName       = "D:\\Timer.ini";
        private const string Duration       = "Duration";
        private const string Duration_Tick  = "Duration_Tick";
        private const string Tick           = "Tick"; // current time - in terms of tick count

        private DurationCallback    m_updateCallback;
        private TIMER_STATE         m_timerState;

        
        public Timer(DurationCallback callbackDelegate)
        {
            m_updateCallback = callbackDelegate;
            m_timerState = TIMER_STATE.TS_START;
        }
   
        public void StartLogging()
        {
            while (true)
            {
                LogTime(m_timerState);
                // 1 sec = 1000 milliseconds
                Thread.Sleep(1000 * 60 * 5); // 5 minutes
                m_timerState = TIMER_STATE.TS_UPDATE;
            }
        }
      
        public void LogTime(TIMER_STATE state)
        {
            long dPrevTick = readVal(Tick);

            DateTime    now     = DateTime.Now;
            string      dateVal = now.ToShortDateString();
   
            long ticks = now.Ticks;

            switch (state)
            {
                case TIMER_STATE.TS_START:
                    {
                        // set tick count to start time
                        WritePrivateProfileString(dateVal, Tick, ticks.ToString(), fileName);
                        //update textbox control for duration value
                        UpdateControl();
                    }
                    break;

                case TIMER_STATE.TS_UPDATE:
                case TIMER_STATE.TS_END:
                    {
                        TimeSpan prvTickTimeSpan = new TimeSpan(dPrevTick);
                        TimeSpan diff = new TimeSpan(now.Subtract(prvTickTimeSpan).Ticks);

                        //update duration
                        long        duration        = readVal(Duration_Tick);
                        TimeSpan    currDuration    = diff.Add(new TimeSpan(duration));

                        string sDuration = currDuration.ToString();
                        if (sDuration.IndexOf('.') > -1)
                            sDuration = sDuration.Remove(sDuration.IndexOf('.'));

                        m_updateCallback(sDuration); // generate callback for Form1 to upate its textbox*/
       
                        WritePrivateProfileString(dateVal, Duration, sDuration, fileName); //total duration
                        WritePrivateProfileString(dateVal, Duration_Tick, currDuration.Ticks.ToString(), fileName); //duration tick count
                        WritePrivateProfileString(dateVal, Tick, ticks.ToString(), fileName); // Current time/tick count
                    }
                    break;
            }
        }

        private void UpdateControl()
        {
            long duration = readVal(Duration_Tick);
            TimeSpan currDuration = new TimeSpan(duration);

            string sDuration = currDuration.ToString();
            if (sDuration.IndexOf('.') > -1)
                sDuration = sDuration.Remove(sDuration.IndexOf('.'));
            m_updateCallback(sDuration); // generate callback for Form1 to upate its textbox
        }

        private long readVal(string key)
        {
            DateTime now = DateTime.Now;
            string dateVal = now.ToShortDateString();

            StringBuilder retVal = new StringBuilder(50);
            int size = 50;

            int nRes = GetPrivateProfileString(dateVal, key, "", retVal, size, fileName);
            if (0 == nRes)
                return nRes;
            else
                return Convert.ToInt64(retVal.ToString());
        }


    }
}
