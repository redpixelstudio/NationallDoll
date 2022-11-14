// Anthony Lee 11010841
// Main meat of the project
// Load's the given audio file into memory
// Plays/Pauses the audio file
// Performs analysis on the audio file

//Library from https://github.com/Teh-Lemon/Onset-Detection

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
// using NAudio.Wave;

namespace RhythmGameStarter
{
    class AudioAnalysis
    {
        // // Audio stream fed into the sound playback device
        // //BlockAlignReductionStream stream;
        // // Instance of sound playback device
        // public WaveOutEvent outputDevice;

        // Fast Fourier Transform library
        FFT fft;

        /// <summary>
        /// Raw audio data
        /// </summary>
        // public AudioFileReader PCMStream { get; set; }

        // Onset Detection
        OnsetDetection onsetDetection;
        public float[] OnsetsFound { get; set; }
        public float TimePerSample { get; set; }

        private AudioClip clip;

        // Constructor
        public AudioAnalysis(AudioClip clip)
        {
            this.clip = clip;
            SetUpFFT();
        }

        public static float getTimeFromIndex(AudioClip clip, int index)
        {
            int spectrumSampleSize = 1024;
            return ((1f / (float)clip.frequency) * index) * spectrumSampleSize;
        }

        public void DetectOnsets(float sensitivity = 1.5f)
        {
            int spectrumSampleSize = 1024;

            onsetDetection = new OnsetDetection(clip, spectrumSampleSize);
            var multiChannelData = new float[clip.samples * clip.channels];
            clip.GetData(multiChannelData, 0);

            //To Mono data
            float[] preProcessedSamples = new float[clip.samples];
            int numProcessed = 0;
            float combinedChannelAverage = 0f;
            for (int i = 0; i < multiChannelData.Length; i++)
            {
                combinedChannelAverage += multiChannelData[i];

                // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                if ((i + 1) % clip.channels == 0)
                {
                    preProcessedSamples[numProcessed] = combinedChannelAverage / clip.channels;
                    numProcessed++;
                    combinedChannelAverage = 0f;
                }
            }

            //Loop and add to calculation
            int iterations = clip.samples / spectrumSampleSize;
            // Debug.Log(iterations);
            for (int i = 0; i < iterations; i++)
            {
                float[] sampleChunk = new float[spectrumSampleSize];
                Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);
                onsetDetection.AddFlux(sampleChunk);
            }

            // Find peaks
            onsetDetection.FindOnsets(sensitivity);
        }

        public void NormalizeOnsets(int type)
        {
            onsetDetection.NormalizeOnsets(type);
        }

        public float[] GetOnsets()
        {
            return onsetDetection.Onsets;
        }

        public float GetTimePerSample()
        {
            return onsetDetection.TimePerSample();
        }

        #region Internals

        // Read in a sample and convert it to mono
        float[] ReadMonoPCM(float[] output)
        {
            // If stereo, convert to mono
            if (clip.channels == 2)
            {
                return ConvertStereoToMono(output);
            }
            else
            {
                return output;
            }
        }

        // Averages the 2 channels into 1
        float[] ConvertStereoToMono(float[] input)
        {
            float[] output = new float[input.Length / 2];
            int outputIndex = 0;

            float leftChannel = 0.0f;
            float rightChannel = 0.0f;

            // Go through each pair of samples
            // Average out the pair
            // Save to output
            for (int i = 0; i < input.Length; i += 2)
            {
                leftChannel = input[i];
                rightChannel = input[i + 1];

                // Average the two channels
                output[outputIndex] = (leftChannel + rightChannel) / 2;
                outputIndex++;
            }

            return output;
        }

        // Starts up the Fast Fourier Transform class
        void SetUpFFT()
        {
            fft = new FFT();

            //Determine how phase works on the forward and inverse transforms. 
            // (0, 1) default
            // (1, -1) for signal processing
            fft.A = 0;
            fft.B = 1;
        }

        #endregion
    }
}
