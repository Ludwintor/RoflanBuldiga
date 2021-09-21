using System;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class Delay
    {
        public static void New(float time, Action action)
        {
            Timer timer = new Timer(time * 1000f);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => 
            {
                action();
                timer.Dispose(); 
            };
            timer.Start();
        }
    }
}
