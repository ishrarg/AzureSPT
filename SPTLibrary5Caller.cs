namespace TestAzureSpeechToTextConsole
{
    public class SPTLibrary5Caller
    {        
        public static async Task RunExampleStream()
        {
            var stream = File.OpenRead(Const.FilePath); //this would come from an uploaded file
            var rx = await SPTLibrary5.FromStream(stream).ConfigureAwait(true);
            rx.Debug();
        }
        public static async Task RunExampleByteArray()
        {
            var byteArray = File.ReadAllBytes(Const.FilePath); //this would come from an uploaded file
            var rx = await SPTLibrary5.FromByteArray(byteArray).ConfigureAwait(true);
            rx.Debug();
        }        
        public static async Task RunExamples()
        {
            //sawait RunExampleStream();
            await RunExampleByteArray();
        }
    }
}
