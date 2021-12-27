namespace FrostweepGames.WebGLPUNVoice
{
    /// <summary>
    /// Collects all constants in one class
    /// </summary>
    public class Constants 
    {
        /// <summary>
        /// How long will be recorded voice
        /// </summary>
        public const int RecordingTime = 1;

        /// <summary>
        /// Default sample rate of microphone
        /// </summary>
        public const int SampleRate = 44100;

        /// <summary>
        /// Size of block that sends over network
        /// </summary>
        public const int ChunkSize = SampleRate / 2;

        /// <summary>
        /// Code of network event that uses for voice data transition
        /// </summary>
        public const byte VoiceEventCode = 199;
    }
}