using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System.Diagnostics;

namespace SpeechToText
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {


            await SPTLibrary.RecognitionWithPullAudioStreamAsync();

        }

    }
}
