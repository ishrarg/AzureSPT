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


        }
        private void Recognizer_Recognized(object? sender, SpeechRecognitionEventArgs e)
        {

            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                if (e.Result.Text.Length > 0)
                {
                    txtLongText.Invoke(() => { txtLongText.Text += e.Result.Text; });
                }
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            txtLongText.Text = "";
            string filename = "harvard.wav";
            //var result = await SPTLibrary.RecognizeOneLineStream("ae04421fc7ff4e6a837a8c4f53197300", "eastus", File.OpenRead(filename));
            //textBox1.Text = result.Text;

            var rx = await SPTLibrary.RecognitionWithPullAudioStreamAsync().ConfigureAwait(true);

            //txtLongText.Text = rx.Text;

            //await SPTLibrary.RecognizeLongStream("ae04421fc7ff4e6a837a8c4f53197300", "eastus", File.OpenRead(filename), Recognizer_Recognized);

            Stream input = File.OpenRead(filename);
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                var resultX = await SPTLibrary.RecognizeLongByteArray("ae04421fc7ff4e6a837a8c4f53197300", "eastus", ms.ToArray(), Recognizer_Recognized);
                MessageBox.Show(resultX.Text);

            }


        }
    }
}
