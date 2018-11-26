using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MuEmu
{
    public class SubSystem
    {
        private Thread _main;
        public SubSystem Instance { get; set; }

        public SubSystem()
        {
            if (Instance != null)
                throw new Exception("Already Initialized");

            Instance = this;
            _main = new Thread(Worker);
        }

        private static void Worker()
        {

        }
    }
}
