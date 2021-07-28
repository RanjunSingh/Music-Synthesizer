using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthie
{
    public class AR
    {
        private double attack;
        private double release;

        private AudioNode source;
        private int sampleRate;

        protected double[] frame = new double[2];

        private double noteDuration;

        private double time = 0;
        public AudioNode Source { get => source; set => source = value; }
        public int SampleRate { get => sampleRate; set => sampleRate = value; }

        public double NoteDuration { get => noteDuration; set => noteDuration = value; }

        public double Frame(int c) { return frame[c]; }


        public AR()
        {
            attack = 0.05;
            release = 0.05;
        }

        public void Start()
        {
            //noteDuration = 0.1;
            attack = 0.05;
            release = 0.05;
            time = 0;
        }

        public bool Generate()
        {
            // source.Frame[0] = source.Amp * Math.Sin(phase * 2 * Math.PI);
            //  frame[1] = frame[0];

            
            // phase += freq * samplePeriod;
            
            //Call generate for the source SineWave
            source.Generate();

            //Create a ramp in/out depending on the time and the attack/release. 
            if(time < attack)
            {
                frame[0] = (time / attack) * source.Frame(0);
            }
            else if(noteDuration - time < release)
            {
                frame[0] = ((noteDuration - time) / release) * source.Frame(0);
            }
            else
            {
                frame[0] = source.Frame(0);
            }
            frame[1] = frame[0];

            time += source.SamplePeriod;

            return true;
        }

    }
}
