namespace MemBot
{
  internal interface IMediaFactory
  {
    MemMedia Create(MemMedia.Types type);

    MemMedia Create(string extension);
  }
}
