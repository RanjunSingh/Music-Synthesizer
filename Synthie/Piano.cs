using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthie
{
    public class Piano : Instrument
    {
        //piano Instrument      
        //private SineWave sinewave = new SineWave();
        private PianoNote pianoNote; 

        //instead of a sinewave creating the frames, they will have to be loaded in from the sample files. 

        private double duration;
        private double time;
        private double amp;

        private double noteDuration;

        private AR ar;

        //public double Frequency { get => sinewave.Frequency; set => sinewave.Frequency = value; }
        //public int Bpm { get => bpm; set => bpm = value; }

        public Piano()
        {
            duration = 0.1;
            pianoNote = new PianoNote();
            ar = new AR();
        }

        public override void Start()
        {
            //sinewave.SampleRate = SampleRate;
            //sinewave.Start();
            time = 0;

            noteDuration = duration * (1 * 60 / bpm);
            // Tell the AR object it gets its samples from 
            // the sine wave object.
            //ar.Source = sinewave;
            ar.Source = pianoNote;
            

            //pianoNote.Format.SampleRate = SampleRate;
            ar.NoteDuration = noteDuration; 
            ar.Start();
        }


        //streaming the audio. 
        public override bool Generate()
        {
            // Tell the component to generate an audio sample
            //sinewave.Generate();
            ar.Generate();
            // Read the component's sample and make it our resulting frame.
            frame[0] = ar.Frame(0) * amp;
            frame[1] = ar.Frame(1) * amp;

            // Update time
            time += samplePeriod;

            // noteDuration = duration * (1 * 60 / bpm );
            // We return true until the time reaches the duration.
            return time < noteDuration;
        }

        //Let the instrument set the note. 
        
        public override void SetNote(Note note)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;

            string path = projectDirectory + "\\res\\" + note.Pitch;
          //  Console.WriteLine(path);

            duration = note.Count;
            amp = note.Amp;
            pianoNote.Open(path); //get the audio file of the pitch
        } 


    }
}
