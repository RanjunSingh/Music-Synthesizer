using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthie
{
    public class SineWave : AudioNode
    {
        //Section: SineWave
        private double freq;
        private double amp;
        private double phase;

        public double Frequency { get => freq; set => freq = value; }

        public SineWave()
        {
            phase = 0;
            amp = 0.1;
            freq = 440;
        }


        public override void Start()
        {
            phase = 0;
        }

        public override bool Generate()
        {
            frame[0] = amp * Math.Sin(phase * 2 * Math.PI);
            frame[1] = frame[0];

            phase += freq * samplePeriod;

            return true;
        }

    }
}
