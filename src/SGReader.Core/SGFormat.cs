namespace SGReader.Core
{
    /// <summary>Fixed layout sizes and version quirks from the Impressions SG format.</summary>
    internal static class SGFormat
    {
        public const int HeaderSize = 80;
        public const int IndexSize = 600;
        public const int IndexEntryCount = 300;
        public const int BitmapRecordSize = 200;
        public const int BitmapFileNameLength = 65;
        public const int BitmapCommentLength = 51;
        public const int BitmapTrailingUnknownBytes = 64;

        // SG2 header "file size" values used by the original tools for version checks.
        public const uint Sg2NormalFileSize = 74480;
        public const uint Sg2EnemyFileSize = 522680;

        public static int MaxBitmapSlots(SGFileVersion version) => version switch
        {
            SGFileVersion.SG2FormatDemo => 50,
            SGFileVersion.SG2Format => 100,
            SGFileVersion.SG3Format => 200,
            SGFileVersion.SG3FormatWithAlphaMask => 200,
            _ => throw new System.ArgumentOutOfRangeException(nameof(version), version, null)
        };
    }
}
