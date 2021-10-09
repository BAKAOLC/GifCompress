using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public static class GIFsicle
{
    const string ExeFile = "GIFsicle.exe";

    static bool CheckResource()
        => File.Exists(ExeFile);

    static void ReleaseResource()
        => File.WriteAllBytes(ExeFile, GifCompress.Resource.gifsicle);

    static string GetTempFileName()
        => Guid.NewGuid().ToString().Replace("-", string.Empty) + ".temp";

    static async Task<Stream> Work(string param)
    {
        if (!CheckResource())
            ReleaseResource();
        return await Task.Run(() =>
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo(ExeFile, param)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            };
            process.StartInfo = startInfo;
            process.Start();
            var ms = new MemoryStream();
            var buffer = new byte[2048];
            int osize;
            while ((osize = process.StandardOutput.BaseStream.Read(buffer, 0, 2048)) > 0)
                ms.Write(buffer, 0, osize);
            process.WaitForExit();
            return ms;
        });
    }

    public static async Task<Stream> Compress(string path)
        => await Work($"--lossy \"{path}\"");

    public static async Task<Stream> Compress(Stream stream)
    {
        var filename = GetTempFileName();
        SaveStreamToFile(stream, filename);
        var result = await Work(filename);
        File.Delete(filename);
        return result;
    }

    public static void SaveStreamToFile(Stream stream, string path)
    {
        var fs = File.OpenWrite(path);
        var buffer = new byte[2048];
        int osize;
        stream.Seek(0, SeekOrigin.Begin);
        while ((osize = stream.Read(buffer, 0, 2048)) > 0)
            fs.Write(buffer, 0, osize);
        fs.Close();
        fs.Dispose();
    }
}