using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToText
{
    public static class SPTLibrary
    {
        static string filename = "harvard.wav";
        async static Task FromFile(SpeechConfig speechConfig)
        {
            using var audioConfig = AudioConfig.FromWavFileInput(filename);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            var result = await speechRecognizer.RecognizeOnceAsync();
            Console.WriteLine($"RECOGNIZED: Text={result.Text}");
        }

        public static List<SpeechRecognitionResult> Results = new List<SpeechRecognitionResult>();
        public static async Task<SpeechRecognitionCustomResult> RecognitionWithPullAudioStreamAsync()
        {
            SpeechRecognitionCustomResult result = new SpeechRecognitionCustomResult();
            Results.Clear();
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechConfig.FromSubscription("ae04421fc7ff4e6a837a8c4f53197300", "eastus");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create an audio stream from a wav file.
            // Replace with your own audio file name.
            using (var audioInput = Helper.OpenWavFile(filename))
            {
                // Creates a speech recognizer using audio stream input.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognizer.Recognizing += Recognizer_Recognizing;
                    
                    recognizer.Recognized += (s, e) =>
                    {
                        result.Reason = e.Result.Reason;
                        result.Text += e.Result.Text;
                        result.TotalDuration += e.Result.Duration;

                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Results.Add(e.Result);
                        }
                    };
                    recognizer.Recognized += Recognizer_Recognized;

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\nSession stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    //var result = await recognizer.RecognizeOnceAsync();
                    //MessageBox.Show(result.Text);
                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            return result;
        }

        /// <summary>
        /// Recognizes a WAV stream object using the specified subscription key and region.
        /// </summary>
        /// <param name="SubscriptionKey">The subscription key.</param>
        /// <param name="region">The region.</param>
        /// <param name="WavStreamObject">The WAV stream object.</param>
        /// <returns>The speech recognition result.</returns>
        public static async Task<SpeechRecognitionResult> RecognizeOneLineStream(string SubscriptionKey, string region, Stream WavStreamObject)
        {
            var config = SpeechConfig.FromSubscription(SubscriptionKey, region);
            BinaryReader reader = new BinaryReader(WavStreamObject);
            using (var audioInput = Helper.OpenWavFile(reader))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognizer.Recognized += Recognizer_Recognized;

                    var result = await recognizer.RecognizeOnceAsync();
                    return result;
                }
            }
        }

        public static async Task<List<SpeechRecognitionResult>> RecognizeLongStream(string SubscriptionKey, string region, Stream WavStreamObject, EventHandler<SpeechRecognitionEventArgs> Recognizer_Recognized)
        {
            var config = SpeechConfig.FromSubscription(SubscriptionKey, region);

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            BinaryReader reader = new BinaryReader(WavStreamObject);
            using (var audioInput = Helper.OpenWavFile(reader))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Results.Add(e.Result);
                        }
                    };
                    recognizer.Recognized += Recognizer_Recognized;
                   await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(true);
                    Task.WaitAny(new[] { stopRecognition.Task });
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            return Results;

        }

        public class SpeechRecognitionCustomResult
        {
            public string Text { get; set; }
            public ResultReason Reason { get; set; }
            public TimeSpan TotalDuration { get; set; }
        }
        public static async Task<SpeechRecognitionCustomResult> RecognizeLongByteArray(string SubscriptionKey, string region, byte[] myByteArray
            , EventHandler<SpeechRecognitionEventArgs> Recognizer_Recognized)
        {
            var config = SpeechConfig.FromSubscription(SubscriptionKey, region);

            SpeechRecognitionCustomResult result = new SpeechRecognitionCustomResult();

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            MemoryStream WavStreamObject = new MemoryStream(myByteArray);

            BinaryReader reader = new BinaryReader(WavStreamObject);
            using (var audioInput = Helper.OpenWavFile(reader))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognizer.Recognized += (s, e) =>
                    {
                        result.Reason = e.Result.Reason;
                        result.Text += e.Result.Text;
                        result.TotalDuration += e.Result.Duration;

                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Results.Add(e.Result);
                        }
                    };
                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\nSession stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };
                    recognizer.Recognized += Recognizer_Recognized;
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            
            return result;

        }

        async static Task FromStreamChristina(SpeechConfig speechConfig)
        {
            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create a push stream
            using (var pushStream = AudioInputStream.CreatePushStream())
            {
                using (var audioInput = AudioConfig.FromStreamInput(pushStream))
                {
                    // Creates a speech recognizer using audio stream input.
                    using (var recognizer = new SpeechRecognizer(speechConfig, audioInput))
                    {
                        // Subscribes to events.
                        recognizer.Recognizing += (s, e) =>
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        };

                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                                if (e.Result.Text.Length > 0)
                                    MessageBox.Show(e.Result.Text);
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                            }
                        };

                        recognizer.Canceled += (s, e) =>
                        {
                            Console.WriteLine($"CANCELED: Reason={e.Reason}");

                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                Console.WriteLine($"CANCELED: Did you update the subscription info?");
                            }

                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (s, e) =>
                        {
                            Console.WriteLine("\nSession started event.");
                        };

                        recognizer.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine("\nSession stopped event.");
                            Console.WriteLine("\nStop recognition.");
                            stopRecognition.TrySetResult(0);
                        };

                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                        // open and read the wave file and push the buffers into the recognizer
                        using (BinaryAudioStreamReader reader = Helper.CreateWavReader(filename))
                        {
                            byte[] buffer = new byte[1000];
                            while (true)
                            {
                                var readSamples = reader.Read(buffer, (uint)buffer.Length);
                                if (readSamples == 0)
                                {
                                    break;
                                }
                                pushStream.Write(buffer, readSamples);
                            }
                        }
                        pushStream.Close();

                        // Waits for completion.
                        // Use Task.WaitAny to keep the task rooted.
                        Task.WaitAny(new[] { stopRecognition.Task });

                        // Stops recognition.
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private static void Recognizer_Canceled(object? sender, SpeechRecognitionCanceledEventArgs e)
        {
            Console.WriteLine($"CANCELED: Reason={e.Reason}");

            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
            }

        }

        private static void Recognizer_Recognized(object? sender, SpeechRecognitionEventArgs e)
        {

            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Results.Add(e.Result);
                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");

            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
            }
        }

        private static void Recognizer_Recognizing(object? sender, SpeechRecognitionEventArgs e)
        {
            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");

        }
    }
    public class Helper
    {
        public static AudioConfig OpenWavFile(string filename, AudioProcessingOptions audioProcessingOptions = null)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filename));
            return OpenWavFile(reader, audioProcessingOptions);
        }

        public static AudioConfig OpenWavFile(BinaryReader reader, AudioProcessingOptions audioProcessingOptions = null)
        {
            AudioStreamFormat format = readWaveHeader(reader);
            return (audioProcessingOptions == null)
                    ? AudioConfig.FromStreamInput(new BinaryAudioStreamReader(reader), format)
                    : AudioConfig.FromStreamInput(new BinaryAudioStreamReader(reader), format, audioProcessingOptions);
        }

        public static BinaryAudioStreamReader CreateWavReader(string filename)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filename));
            // read the wave header so that it won't get into the in the following readings
            AudioStreamFormat format = readWaveHeader(reader);
            return new BinaryAudioStreamReader(reader);
        }

        public static BinaryAudioStreamReader CreateBinaryFileReader(string filename)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filename));
            return new BinaryAudioStreamReader(reader);
        }

        public static AudioStreamFormat readWaveHeader(BinaryReader reader)
        {
            // Tag "RIFF"
            char[] data = new char[4];
            reader.Read(data, 0, 4);
            Trace.Assert((data[0] == 'R') && (data[1] == 'I') && (data[2] == 'F') && (data[3] == 'F'), "Wrong wav header");

            // Chunk size
            long fileSize = reader.ReadInt32();

            // Subchunk, Wave Header
            // Subchunk, Format
            // Tag: "WAVE"
            reader.Read(data, 0, 4);
            Trace.Assert((data[0] == 'W') && (data[1] == 'A') && (data[2] == 'V') && (data[3] == 'E'), "Wrong wav tag in wav header");

            // Tag: "fmt"
            reader.Read(data, 0, 4);
            Trace.Assert((data[0] == 'f') && (data[1] == 'm') && (data[2] == 't') && (data[3] == ' '), "Wrong format tag in wav header");

            // chunk format size
            var formatSize = reader.ReadInt32();
            var formatTag = reader.ReadUInt16();
            var channels = reader.ReadUInt16();
            var samplesPerSecond = reader.ReadUInt32();
            var avgBytesPerSec = reader.ReadUInt32();
            var blockAlign = reader.ReadUInt16();
            var bitsPerSample = reader.ReadUInt16();

            // Until now we have read 16 bytes in format, the rest is cbSize and is ignored for now.
            if (formatSize > 16)
                reader.ReadBytes((int)(formatSize - 16));

            // Handle optional LIST chunk.
            // tag: "LIST"
            reader.Read(data, 0, 4);
            if (data[0] == 'L' && data[1] == 'I' && data[2] == 'S' && data[3] == 'T')
            {
                var listChunkSize = reader.ReadUInt32();
                reader.ReadBytes((int)listChunkSize);
                reader.Read(data, 0, 4);
            }

            // Second Chunk, data
            // tag: "data"
            Trace.Assert((data[0] == 'd') && (data[1] == 'a') && (data[2] == 't') && (data[3] == 'a'), "Wrong data tag in wav");
            // data chunk size
            int dataSize = reader.ReadInt32();

            // now, we have the format in the format parameter and the
            // reader set to the start of the body, i.e., the raw sample data
            return AudioStreamFormat.GetWaveFormatPCM(samplesPerSecond, (byte)bitsPerSample, (byte)channels);
        }
    }
    public sealed class BinaryAudioStreamReader : PullAudioInputStreamCallback
    {
        private System.IO.BinaryReader _reader;

        /// <summary>
        /// Creates and initializes an instance of BinaryAudioStreamReader.
        /// </summary>
        /// <param name="reader">The underlying stream to read the audio data from. Note: The stream contains the bare sample data, not the container (like wave header data, etc).</param>
        public BinaryAudioStreamReader(System.IO.BinaryReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Creates and initializes an instance of BinaryAudioStreamReader.
        /// </summary>
        /// <param name="stream">The underlying stream to read the audio data from. Note: The stream contains the bare sample data, not the container (like wave header data, etc).</param>
        public BinaryAudioStreamReader(System.IO.Stream stream)
            : this(new System.IO.BinaryReader(stream))
        {
        }

        /// <summary>
        /// Reads binary data from the stream.
        /// </summary>
        /// <param name="dataBuffer">The buffer to fill</param>
        /// <param name="size">The size of data in the buffer.</param>
        /// <returns>The number of bytes filled, or 0 in case the stream hits its end and there is no more data available.
        /// If there is no data immediate available, Read() blocks until the next data becomes available.</returns>
        public override int Read(byte[] dataBuffer, uint size)
        {
            return _reader.Read(dataBuffer, 0, (int)size);
        }

        /// <summary>
        /// This method performs cleanup of resources.
        /// The Boolean parameter <paramref name="disposing"/> indicates whether the method is called from <see cref="IDisposable.Dispose"/> (if <paramref name="disposing"/> is true) or from the finalizer (if <paramref name="disposing"/> is false).
        /// Derived classes should override this method to dispose resource if needed.
        /// </summary>
        /// <param name="disposing">Flag to request disposal.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _reader.Dispose();
            }

            disposed = true;
            base.Dispose(disposing);
        }


        private bool disposed = false;
    }

}
