using System.IO;

namespace Easy.Admin.Services
{
    public interface IFileUpload
    {
        string PutObject(string key, Stream content);
    }
}
