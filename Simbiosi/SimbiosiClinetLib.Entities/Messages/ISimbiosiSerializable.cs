using SimbiosiClientLib.Entities.Streams;

namespace SimbiosiClientLib.Entities.Messages
{
    public interface ISimbiosiSerializable
    {
        void Write(SimbiosiStreamWriter writer, bool whenErrorReturnBack);

        void Read(SimbiosiStreamReader reader, bool whenErrorReturnBack);

    }
}
