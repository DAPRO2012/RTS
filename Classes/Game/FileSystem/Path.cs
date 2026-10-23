using System;
using System.IO;
using System.Linq;

public class Path(string path)
{
    private string _path = path;
    public string GetPath() => _path;

    public Path(params string[] paths) : this(System.IO.Path.Combine(paths)) { }
    public Path(params Path[] paths) : this(System.IO.Path.Combine([.. paths.Select(x => x.GetPath())])) { }

    public bool IsDirectory() => Directory.Exists(GetPath());
    public bool IsFile() => File.Exists(GetPath());

    public void CreateDirectory()
    {
        if (IsFile())
            throw new Exception("File Already Exists.");
        else
            Directory.CreateDirectory(GetPath());
    }

    public void CreateFile()
    {
        if (IsDirectory())
            throw new Exception("Directory Already Exists.");
        else
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(GetPath()));
            File.Create(GetPath()).Close();
    }

    public void Remove()
    {
        if (IsDirectory()) Directory.Delete(GetPath());
        if (IsFile()) File.Delete(GetPath());
    }

    public string[] ListDir() =>
        [.. Directory.EnumerateFiles(GetPath())];

    public static Path Combine(params string[] paths) => new Path(paths);
    public static Path Combine(params Path[] paths) => new Path(paths);


    public Path ToAbsolute() => new Path(System.IO.Path.GetFullPath(GetPath()));

    public override string ToString() => GetPath();
}