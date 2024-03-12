using System;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;

namespace MouseAudio
{
    public partial class Form1 : Form
    {
        private WaveOutEvent outputDevice;
        private BufferedWaveProvider bufferedWaveProvider;
        private bool isMouseDown = false;
        private float lastFrequency = 0;

        public Form1()
        {
            InitializeComponent();
            MouseDown += Form1_MouseDown;
            MouseUp += Form1_MouseUp;
            outputDevice = new WaveOutEvent();
            bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1)); // Mono, 16-bit PCM
            outputDevice.Init(bufferedWaveProvider);
            outputDevice.Play();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                lastFrequency = Cursor.Position.X / (float)Screen.PrimaryScreen.Bounds.Width * 5000 + 100; // Adjust frequency range
                PlaySwooshStart();
                Thread audioThread = new Thread(AudioLoop);
                audioThread.Start();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
                PlaySwooshEnd();
            }
        }

        private void AudioLoop()
        {
            while (isMouseDown)
            {
                float frequency = Cursor.Position.X / (float)Screen.PrimaryScreen.Bounds.Width * 5000 + 100; // Adjust frequency range
                if (frequency != lastFrequency)
                {
                    lastFrequency = frequency;
                    byte[] sample = GenerateSample(frequency, 20); // Adjust duration
                    bufferedWaveProvider.AddSamples(sample, 0, sample.Length);
                }
                Thread.Sleep(10); // Adjust sleep duration
            }
        }

        private void PlaySwooshStart()
        {
            float startFrequency = 200; // Adjust start frequency
            byte[] startSample = GenerateSample(startFrequency, 100); // Adjust start duration
            bufferedWaveProvider.AddSamples(startSample, 0, startSample.Length);
        }

        private void PlaySwooshEnd()
        {
            float endFrequency = 1000; // Adjust end frequency
            byte[] endSample = GenerateSample(endFrequency, 100); // Adjust end duration
            bufferedWaveProvider.AddSamples(endSample, 0, endSample.Length);
        }

        private byte[] GenerateSample(float frequency, int milliseconds)
        {
            int sampleRate = 44100;
            int sampleCount = sampleRate * milliseconds / 1000;
            byte[] buffer = new byte[sampleCount * 2]; // 16-bit, so 2 bytes per sample
            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / sampleRate;
                double wave = Math.Sin(2 * Math.PI * frequency * t);
                short sampleValue = (short)(wave * short.MaxValue);
                buffer[i * 2] = (byte)(sampleValue & 0xff);
                buffer[i * 2 + 1] = (byte)((sampleValue >> 8) & 0xff);
            }
            return buffer;
        }
    }
}
