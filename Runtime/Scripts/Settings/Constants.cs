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
        /// Amount of microphone channels
        /// </summary>
        public const int Channels = 2;

        /// <summary>
        /// Default sample rate of microphone
        /// </summary>
        public const int SampleRate = 16000;

        /// <summary>
        /// Chunk duration in ms
        /// </summary>
        public const int ChunkDuration = 150;

        /// <summary>
        /// Size of block that sends over network
        /// </summary>
        public const int ChunkSize = (SampleRate * Channels * ChunkDuration) / 1000;

        /// <summary>
        /// Code of network event that uses for voice data transition
        /// </summary>
        public const byte VoiceEventCode = 199;

    }
}