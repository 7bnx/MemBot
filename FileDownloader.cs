namespace MemBot
{
  internal class FileDownloader
  {
    public static async Task<byte[]> FromUrl(string url)
    {
      var client = new HttpClient();
      var response = await client.GetAsync(url);

      using var stream = await response.Content.ReadAsStreamAsync();
      using var ms = new MemoryStream();
      await stream.CopyToAsync(ms);
      return ms.ToArray();
    }
  }
}
