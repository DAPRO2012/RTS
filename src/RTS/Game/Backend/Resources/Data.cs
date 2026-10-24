using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;

public class Data
{
    protected readonly Dictionary<Identifier, List<Identifier>> _data = [];

    public static string GetPath(string rootDir, Identifier path) =>
        System.IO.Path.Combine(
            rootDir,
            path.GetNamespace(),
            path.GetPath()
        );
}