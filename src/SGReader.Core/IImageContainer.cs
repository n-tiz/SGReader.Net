using System.Collections.Generic;

namespace SGReader.Core
{
    public interface IImageContainer
    {
        SGImage GetImageById(int imageId);

        IReadOnlyList<SGImage> Images { get; }
    }
}