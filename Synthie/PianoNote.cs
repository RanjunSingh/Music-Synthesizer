using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace Synthie
{
    class PianoNote : AudioNode
    {

        #region Properties

        private int bytesPerFrame = 0;
        private WaveFileReader readerStream;
        private int lastWriteSampleIndex = 0;
        private int lastReadSampleIndex = 0;
        private AudioFileReader audioFile;
        private string filename;
        private WaveFormat format;
        private string lastFile = "temp.wav";
        private SoundFileTypes lastFileType = SoundFileTypes.WAV;
        private WaveOutEvent outputPlayDevice;
        private float[] cachedSamples;
        private SoundFileTypes soundFileFormat;
        private WaveFileWriter writerStream;

        private double phase;
        private double time;
        private int i;

        public float Duration { get => (float)cachedSamples.Length / (format.SampleRate * format.Channels); }
        public string Filename { get => filename; }
        public WaveFormat Format { get => format; set => format = value; }

        public int Channels
        {
            get
            {
                if (format != null)
                {
                    return format.Channels;
                }
                else
                    return 0;
            }
        }

        public int SampleRate
        {
            get
            {
                if (format != null)
                {
                    return format.SampleRate;
                }
                else
                    return 0;
            }
        }

        public float[] Samples
        {
            get => cachedSamples;
            set => cachedSamples = value;
        }

        public int SampleCount
        {
            get { return (int)(Duration * format.Channels * format.SampleRate); }
        }

        #endregion Properties

        /// <summary>
        /// Constructor for a default, 0.5 seconds of silence with a 44100 sample rate and mono sound.
        /// </summary>
        public PianoNote()
        {
            format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            cachedSamples = new float[22050];
        }

        /// <summary>
        /// Constructor for a sound with a the given sample rate and channels.
        /// Create 0.5s of silence by defalt
        /// </summary>
        /// <param name="sampleRate">sample rate</param>
        /// <param name="channels">channels</param>
        /// <param name="duration">duration in seconds (defaults to 0.5)</param>
        public PianoNote(int sampleRate, int channels, float duration = 0.5f)
        {
            format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            cachedSamples = new float[(int)(sampleRate * duration * channels)];
        }

        /// <summary>
        /// Constructor for a a Sound object loaded from a file
        /// </summary>
        /// <param name="path">path to file</param>
        public PianoNote(string path)
        {
            Open(path);
        }

        //helper function to resize the cachedSamples array
        public void resize(int size)
        {
            cachedSamples = new float[size];
        }

        #region Conversion Helper Functions

        /// <summary>
        /// Converts a raw byte sound data into a raw float sound data.
        /// Warning: the float data will only convert cleanly if the loaded sound bytes are in float format.
        /// If they are not, coversion will complete, and conversion back is possible, but min and max value may be inaccurate.
        /// This can be checked with the WaveFormat. IEEE will work properly.
        /// </summary>
        /// <param name="input">raw byte sound data</param>
        /// <returns>a raw float sound data</returns>
        public float[] ByteToFloat(byte[] input)
        {
            var floatArray2 = new float[input.Length / 4];
            Buffer.BlockCopy(input, 0, floatArray2, 0, input.Length);
            return floatArray2;
        }

        /// <summary>
        /// Converts a raw byte sound data into a raw short sound data.
        /// Warning: the short data will only convert cleanly if the loaded sound bytes are in short format.
        /// If they are not, coversion will complete, and conversion back is possible, but min and max value may be inaccurate.
        /// This can be checked with the WaveFormat. PCM16 will work properly.
        /// </summary>
        /// <param name="input">raw byte sound data</param>
        /// <returns>a raw short sound data</returns>
        public short[] ByteToShort(byte[] input)
        {
            short[] sdata = new short[(int)Math.Ceiling(input.Length / 2.0)];
            Buffer.BlockCopy(input, 0, sdata, 0, input.Length);
            return sdata;
        }

        /// <summary>
        /// Converts a raw float sound data into a raw byte sound data.
        /// </summary>
        /// <param name="input">raw float sound data</param>
        /// <returns>a raw byte sound data</returns>
        public byte[] FloatToByte(float[] input)
        {
            var byteArray = new byte[input.Length * 4];
            Buffer.BlockCopy(input, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        /// <summary>
        /// Converts a raw short sound data into a raw byte sound data.
        /// </summary>
        /// <param name="input">raw short sound data</param>
        /// <returns>a raw byte sound data</returns>
        public byte[] ShortToByte(short[] input)
        {
            byte[] result = new byte[input.Length];

            for (int i = 0; i < input.Length / 2; i++)
            {
                byte[] temp = BitConverter.GetBytes(input[i]);
                result[i * 2 + 0] = temp[0];
                result[i * 2 + 1] = temp[1];
            }
            return result;
        }

        #endregion Conversion Helper Functions

        #region File open/close operations

        /// <summary>
        /// Release data from program
        /// </summary>
        public void Close()
        {
            format = null;
            bytesPerFrame = 0;
            lastReadSampleIndex = 0;

            if (outputPlayDevice != null)
            {
                outputPlayDevice.Dispose();
                outputPlayDevice = null;
            }

            if (writerStream != null)
            {
                writerStream.Dispose();
                writerStream.Close();
                writerStream = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile.Close();
                audioFile = null;
            }

            if (readerStream != null)
            {
                readerStream.Dispose();
                readerStream.Close();
                readerStream = null;
            }
        }

        /// <summary>
        /// Closes the current sound, and opens a sound file and load in the raw 
        /// data for later editing.
        /// Currently supports IEEE format (most WAVs and MP3s). Other formats may 
        /// complete, but the value may be incorrect.
        /// </summary>
        /// <param name="path">path to a sound file</param>
        /// <returns>true if opened successfully</returns>
        public bool Open(string path)
        {
            Close();
            try
            {
                //open file
                audioFile = new AudioFileReader(path);

                //save format
                format = audioFile.WaveFormat;

                //convert from raw bytes to floats
                byte[] temp = new byte[audioFile.Length];
                audioFile.Read(temp, 0, (int)audioFile.Length);
                Samples = ByteToFloat(temp);
                filename = path;

                if (format.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    MessageBox.Show("Sound file not a float format. Values may be incorrect", "Loading problem");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Loading Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Opens a resource sound file
        /// </summary>
        /// <param name="resourceStream">the sound resource</param>
        /// <returns>true if successful</returns>
        public bool Open(UnmanagedMemoryStream resourceStream)
        {
            //helper function for fast opening
            try
            {
                //open file
                WaveFileReader wave = new WaveFileReader(resourceStream);
                Wave16ToFloatProvider provider = new Wave16ToFloatProvider(wave);

                //save format
                format = provider.WaveFormat;

                //convert from raw bytes to floats
                byte[] temp = new byte[wave.Length];
                provider.Read(temp, 0, (int)wave.Length);
                Samples = ByteToFloat(temp);


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Loading Error");
                return false;
            }
            return true;
        }

        #endregion
        public override bool Generate()
        {
            //throw new NotImplementedException();
            //  frame[0] = cachedSamples[phase];
            // frame[1] = frame[0];

            // phase += freq * samplePeriod;


            if (i < cachedSamples.Length)
            {
                frame[0] = cachedSamples[i];
                frame[1] = frame[0];
            }

            i++;

           return true;

        }

        public override void Start()
        {
            // throw new NotImplementedException();
            i = 0;
        }

    }
   
}

