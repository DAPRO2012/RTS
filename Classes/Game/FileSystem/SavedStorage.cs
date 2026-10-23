using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

public class SavedStorage
{
    private readonly Path _path;
    public Path Path => _path;

    public SavedStorage(Path path)
    {
        path.CreateDirectory();
        _path = path;
        Console.WriteLine("Path: " + path);
    }

    public Path GetDataFile(Identifier id)
    {
        Path path = id.WithSuffix(".dat").ResolveAgainst(_path);
        if (!path.ToAbsolute().GetPath().StartsWith(_path.ToAbsolute().GetPath())) {
            throw new Exception("SavedDataStorage attempted file access outside of data directory: {}" + path);
        } else {
            return path;
        }
    }

    public JsonNode ReadJsonFromDisk(Path file)
    {
        Path path = new(System.IO.Path.Combine(_path.GetPath(), file.GetPath()));

        using FileStream fileStream = File.OpenRead(path.GetPath());
        return JsonNode.Parse(fileStream);
    }
    public DataResult<T> ReadCodecFromDisk<T>(Path file, Codec<T> codec) => codec.Decode(ReadJsonFromDisk(file));

    public void WriteJsonToDisk(Path file, JsonNode node)
    {
        Path path = new(System.IO.Path.Combine(_path.GetPath(), file.GetPath()));
        path.CreateFile();
        File.WriteAllText(path.GetPath(), node.ToJsonString());
    }
    public void WriteCodecToDisk<T>(Path file, T obj, Codec<T> codec)
    {
        DataResult<JsonNode> result = codec.Encode(obj);
        if (result is DataResult<JsonNode>.Error<JsonNode> err)
            throw new Exception(err.Message);
        else
            WriteJsonToDisk(file, result.Result().GetValue());
    }
}