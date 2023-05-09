using Microsoft.CognitiveServices.Speech;

namespace TestAzureSpeechToTextConsole
{
    public static class SPTLibrary5
    {
        private static SpeechConfig? GetSpeechConfig()
        {
            var speechConfig = SpeechConfig.FromSubscription("ae04421fc7ff4e6a837a8c4f53197300", "eastus");
            return speechConfig;
        }
        private static SpeechConfig? GetSpeechConfigError()
        {
            var speechConfig = SpeechConfig.FromSubscription("errorfdsfdsfdsfdsfds", "eastus");
            return speechConfig;
        }

        public static async Task<SpeechRecognitionCustomResult> FromByteArray(byte[] byteArray)
        {
            //this is working now. I changed the way of writing byte array to memory stream. 
            MemoryStream stream = new MemoryStream(byteArray);
            
            return await FromStream(stream);
        }

        public static async Task<SpeechRecognitionCustomResult> FromStream(Stream stream)
        {
            SpeechRecognitionCustomResult result = new SpeechRecognitionCustomResult();
            var config = GetSpeechConfig();
            //var config = GetSpeechConfigError();

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (var audioInput = AudioHelper.OpenWavFile(stream))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognizer.Recognizing += Recognizer_Recognizing;

                    recognizer.Recognized += (s, e) =>
                    {
                        CopyResultObject(e.Result, result);
                    };
                    recognizer.Recognized += Recognizer_Recognized;

                    recognizer.Canceled += (s, e) =>
                    {
                        CopyResultObject(e.Result, result);

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

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            return result;
        }

        private static void CopyResultObject(SpeechRecognitionResult result, SpeechRecognitionCustomResult resultCopy)
        {
            Console.WriteLine("CopyResultObject: " + result.Reason + " " + resultCopy.Reason);
            if (result.Reason == ResultReason.Canceled)
            {
                if (resultCopy.Reason == ResultReason.RecognizedSpeech)
                {
                    return;
                }
            }

            var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(result);
            resultCopy.ResultId = result.ResultId;
            resultCopy.OffsetInTicks = result.OffsetInTicks;
            resultCopy.DetectedLanguage = autoDetectSourceLanguageResult.Language;

            resultCopy.Reason = result.Reason;
            resultCopy.Text += result.Text;
            resultCopy.TotalDuration += result.Duration;

            if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                resultCopy.ErrorDetails = cancellation.ErrorDetails;
                resultCopy.CancelReason = cancellation.Reason;

                if (cancellation.Reason == CancellationReason.Error)
                {
                    resultCopy.ErrorCode = cancellation.ErrorCode;
                }
            }
        }

        private static void Recognizer_Recognized(object? sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
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
}
