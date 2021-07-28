using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthie
{
    public abstract class AudioNode
    {
        //Section: AudioNode

        protected int sampleRate;
        protected double samplePeriod;
        protected double[] frame = new double[2];

        protected double bpm;

        public int SampleRate { get => sampleRate; set => sampleRate = value; }

        public double SamplePeriod { get => samplePeriod; }

        public double Bpm { get => bpm; set => bpm = value; }



        public AudioNode()
        {
            frame[0] = 0;
            frame[1] = 0;
            sampleRate = 44100;
            samplePeriod = 1.0 / sampleRate;
            bpm = 60;
        }

        /// <summary>
        /// Access a generated audio frame
        /// </summary
        /// ><returns>the sound sample</returns>
        public double[] Frame() { return frame; }

        /// <summary>
        /// Access one channel of a generated audio frame
        /// </summary>
        /// <param name="c">the channel</param>
        /// <returns>the value of at the channel in the sound sample</returns>
        public double Frame(int c) { return frame[c]; }

        /// <summary>
        /// Cause one sample to be generated
        /// </summary>
        /// <returns>Returns true if there are remainng smaples to generate</returns>
        public abstract bool Generate();

        /// <summary>
        /// Start the node generation
        /// </summary>
        public abstract void Start();


    }
}
