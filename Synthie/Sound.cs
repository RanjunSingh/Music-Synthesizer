using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Synthie
{
    public enum SoundFileTypes { WAV = 1, MP3 = 2 }

    public class Sound
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

        public float Duration { get => (float)cachedSamples.Length / (format.SampleRate * format.Channels); }
        public string Filename { get => filename; }
        public WaveFormat Format { get => format; set => format = value; }

        public int Channels
        {
            get { 
                if (format != null) { 
                    return format.Channels; 
                } else 
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
        public Sound()
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
        public Sound(int sampleRate, int channels, float duration = 0.5f)
        {
            format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            cachedSamples = new float[(int)(sampleRate * duration * channels)];
        }

        /// <summary>
        /// Constructor for a a Sound object loaded from a file
        /// </summary>
        /// <param name="path">path to file</param>
        public Sound(string path)
        {
            Open(path);
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

        /// <summary>
        /// Open a file to stream output.
        /// While there is a file type, only WAV is currently implemented.
        /// </summary>
        /// <param name="path">file name to save as</param>
        /// <param name="format"> the format of hte sound</param>
        /// <param name="type">The file type to output</param>
        public void OpenSaveStream(string path, WaveFormat format, SoundFileTypes type = SoundFileTypes.WAV)
        {
            OpenSaveStream(path, format.SampleRate, format.Channels, type);
        }

        /// <summary>
        /// Open a file to stream output.
        /// While there is a file type, only WAV is currently implemented.
        /// </summary>
        /// <param name="path">file name to save as</param>
        /// <param name="sampleRate"> defaults to 44100</param>
        /// <param name="channels">number of channels</param>
        /// <param name="type">The file type to output</param>
        public void OpenSaveStream(string path, int sampleRate = 44100, int channels = 2, SoundFileTypes type = SoundFileTypes.WAV)
        {
            Close();
            switch (type)
            {
                case SoundFileTypes.WAV:  //WAVE
                    format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
                    soundFileFormat = SoundFileTypes.WAV;

                    writerStream = new WaveFileWriter(path, format);
                    break;

                default:
                    MessageBox.Show("Only WAV's can be streamed", "Saving Error");
                    break;
            }
        }

        /// <summary>
        /// Gets the next sample frame and auto advances. Work with both streaming and 
        /// fully loaded sound files.
        /// </summary>
        /// <returns>Gets the next sample frame.</returns>
        public float[] ReadNextFrame()
        {
            if (bytesPerFrame != 0)
            {
                byte[] temp = new byte[bytesPerFrame];
                readerStream.Read(temp, 0, bytesPerFrame);
                return ByteToFloat(temp);
            }
            else
            {
                float[] result = new float[format.Channels];
                Array.Copy(cachedSamples, lastReadSampleIndex, result, 0, format.Channels);
                lastReadSampleIndex += format.Channels;
                return result;
            }
        }

        /// <summary>
        /// Open a file to stream read. Currently only works properly (and testes) with Wave files.
        /// </summary>
        /// <param name="path">file name to save as</param>
        public bool OpenReadStream(string path)
        {
            Close();
            try
            {
                //open file
                readerStream = new WaveFileReader(path);

                //save format
                format = readerStream.WaveFormat;
                bytesPerFrame = format.BitsPerSample / 8 * format.Channels;
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
        /// Save the file to the last saved location
        /// </summary>
        /// <returns>true if saved successfully</returns>
        public bool Save()
        {
            return SaveAs(lastFile, lastFileType);
        }

        /// <summary>
        /// Save the sound file at the given path. with a given integer type. 
        /// This int is converted to a SoundFileType enum (Wav and MP3 currently.)
        /// </summary>
        /// <param name="path">location to save</param>
        /// <param name="type">file format enum integer value</param>
        /// <returns></returns>
        public bool SaveAs(string path, int type)
        {
            switch (type)
            {
                case 1:  //WAVE
                    return SaveAs(path, SoundFileTypes.WAV);

                case 2: //MP3
                    return SaveAs(path, SoundFileTypes.MP3);
            }
            return false;
        }

        /// <summary>
        /// Save the file to the given saved location, with the given format
        /// </summary>
        /// <param name="path">path to save at</param>
        /// <param name="type">the desired sound format </param>
        /// <returns></returns>
        public bool SaveAs(string path, SoundFileTypes type)
        {
            if (Samples == null)
            {
                MessageBox.Show("No sound samples available", "Saving Error");
                return false;
            }
            try
            {
                switch (type)
                {
                    case SoundFileTypes.WAV:  //WAVE
                        WaveFormat f = WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate, format.Channels);
                        using (WaveFileWriter writer = new WaveFileWriter(path, f))
                        {
                            if(readerStream != null)
                            {
                                readerStream.CopyTo(writer);
                                readerStream.Seek(0, SeekOrigin.Begin);
                            }
                            else
                                writer.WriteSamples(Samples, 0, Samples.Length);
                        }
                        break;

                    case SoundFileTypes.MP3: //MP3
                                             //confirm codec exists
                        var mediaType = MediaFoundationEncoder.SelectMediaType(
                            AudioSubtypes.MFAudioFormat_MP3,
                            new WaveFormat(format.SampleRate, format.Channels),
                            format.SampleRate);

                        if (mediaType != null) //mp3 encoding supported
                        {
                            using (MediaFoundationEncoder enc = new MediaFoundationEncoder(mediaType))
                            {
                                //converts back to bytes, and put in provider for pulling samples when writing
                                byte[] tt = FloatToByte(Samples);
                                IWaveProvider provider = new RawSourceWaveStream(
                                   new MemoryStream(tt), format);

                                //use the Microsoft media foundation API to save the file.
                                MediaFoundationApi.Startup();
                                enc.Encode(path, provider);
                                MediaFoundationApi.Shutdown();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Your computer does not support encoding mp3. Download a codec", "Saving Error");
                        }
                        break;
                }

                //save file location for ease of later saving
                lastFile = path;
                lastFileType = type;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Saving Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Write the next frame of a streamed output file.
        /// </summary>
        /// <param name="frame">The next frame to save</param>
        public void WriteStreamSample(float[] frame)
        {
            if (bytesPerFrame != 0)
            {
                for (int c = 0; c < format.Channels; c++)
                {
                    cachedSamples[lastWriteSampleIndex + c] = frame[c];
                }
                lastWriteSampleIndex += format.Channels;
            }
            else
            {
                switch (soundFileFormat)
                {
                    case SoundFileTypes.WAV:  //WAVE
                        writerStream.WriteSamples(frame, 0, frame.Length);
                        break;
                }
            }
        }

        #endregion File open/close operations

        #region Playback functions

        /// <summary>
        /// Sound cleanup
        /// </summary>
        /// <param name="sender">the object the trigger the event</param>
        /// <param name="args">details about the event</param>
        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputPlayDevice.Dispose();
            outputPlayDevice = null;
        }

        /// <summary>
        /// Plays the raw samples to the speakers
        /// </summary>
        public void BasicPlay()
        {
            //if output is not running
            if (outputPlayDevice == null)
            {
                if (readerStream != null)
                {
                    //make the output varaiable
                    outputPlayDevice = new WaveOutEvent();

                    //give it a method to call when done (for memory release)
                    outputPlayDevice.PlaybackStopped += OnPlaybackStopped;

                    //initalize the output and play
                    outputPlayDevice.Init(readerStream);
                    outputPlayDevice.Play();
                }
                else {
                    //playback needs the raw bytes, in a provider, so convert
                    byte[] tt = FloatToByte(Samples);
                    IWaveProvider provider = new RawSourceWaveStream(
                                       new MemoryStream(tt), format);

                    //make the output varaiable
                    outputPlayDevice = new WaveOutEvent();

                    //give it a method to call when done (for memory release)
                    outputPlayDevice.PlaybackStopped += OnPlaybackStopped;

                    //initalize the output and play
                    outputPlayDevice.Init(provider);
                    outputPlayDevice.Play();
                }
            }
            else
            {
                //if paused, restart
                if (outputPlayDevice.PlaybackState == PlaybackState.Paused)
                {
                    outputPlayDevice.Play();
                }
            }
        }

       
        /// <summary>
        /// Pause the sound playback
        /// </summary>
        public void Pause()
        {
            outputPlayDevice.Pause();
        }

        /// <summary>
        /// Stop the playback and release memeory
        /// </summary>
        public void Stop()
        {
            if (outputPlayDevice != null)
                outputPlayDevice.Stop();
        }

        #endregion Playback functions
    }
}