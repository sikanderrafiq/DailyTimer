using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading; 

namespace DailyTimer
{
    public partial class Form1 : Form
    {
        private delegate void SetTextDelegate(string text);

        Timer  m_timer;
        Thread m_Thread;
        bool bSuspended = false;

        public Form1()
        {
            InitializeComponent();
            
        }
  
        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;

            // disable resizing 
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            btnStart.Enabled = false;

            m_timer = new Timer(new DurationCallback(this.UpdateText));
            m_Thread = new Thread(new ThreadStart(m_timer.StartLogging));
            m_Thread.Start();
        }
      
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) 
        {
            StopTimer();
        }
  
        public void UpdateText(string sDuration)
        {
            this.SetText(sDuration);
        }
  
        private void SetText(string text)
        {
            if (this.txtDuration.InvokeRequired)
            {
                SetTextDelegate txtDelegate = new SetTextDelegate(SetText);
                this.Invoke(txtDelegate, new object[] { text });
            }
            else
            {
                this.txtDuration.Text = text;
            }
        }

        private void StopTimer()
        {
            if (m_timer != null)
            {
                m_timer.LogTime(TIMER_STATE.TS_END);

                if (bSuspended) {
                    m_Thread.Resume();
                }

                m_Thread.Abort();
                m_Thread.Join();
            }
        }

        private void SuspendTimer()
        {
            if (m_timer != null)
            {
                m_timer.LogTime(TIMER_STATE.TS_END);
                m_Thread.Suspend();
                bSuspended = true;
            }
        }

        private void ResumeTime()
        {
            if (m_timer != null)
            {
                m_timer.LogTime(TIMER_STATE.TS_START);
                m_Thread.Resume();
                bSuspended = false;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SuspendTimer();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            ResumeTime();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }
    }
}
