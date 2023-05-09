using Microsoft.CognitiveServices.Speech;

namespace TestAzureSpeechToTextConsole
{
    public class SpeechRecognitionCustomResult
    {
        public string? Text { get; set; }
        public ResultReason Reason { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public long OffsetInTicks { get; set; }
        public string? DetectedLanguage { get; set; }
        public string? ResultId { get; set; }

        //All relating to cancels
        public CancellationReason CancelReason { get; internal set; }
        public CancellationErrorCode ErrorCode { get; internal set; }
        public string? ErrorDetails { get; set; }


        public void Debug()
        {
            Console.WriteLine("Text: " + Text);
            Console.WriteLine("TotalDuration: " + TotalDuration);
            Console.WriteLine("OffsetInTicks: " + OffsetInTicks);
            Console.WriteLine("DetectedLanguage: " + DetectedLanguage);
            Console.WriteLine("ResultId: " + ResultId);
            
            Console.WriteLine("Reason: " + Reason);

            Console.WriteLine("CancelReason: " + CancelReason);
            Console.WriteLine("ErrorCode: " + ErrorCode);
            Console.WriteLine("ErrorDetails: " + ErrorDetails);
        }
    }
}
