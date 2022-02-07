namespace MemBot
{
  public interface IMediaFactory
  {
    Media Create(Media.Types type);

    Media Create(string extension);
  }
}
