#nullable enable

using System;
using System.Linq;
using System.Text.Json.Nodes;

public class Identifier
{
    public static readonly string DefaultNamespace = "RTS";

    private readonly string _namespace;
    private readonly string _path;

    public Identifier(string id)
    {
        string[] splitId = id.Split(":", 2);
        _namespace = splitId[0];
        _path = splitId[1];
    }
    public Identifier(string[] strings)
    {
        _namespace = strings[0];
        _path = string.Join("/", strings.Skip(1));
    }
    public Identifier(string @namespace, string path)
    {
        _namespace = @namespace;
        _path = path;
    }

    public static Identifier fromNamespaceAndPath(string @namespace, string path)
    {
        return new Identifier(@namespace, path);
    }

    public static Identifier fromDefaultNamespace(string path)
    {
        return new Identifier(DefaultNamespace, path);
    }

    public string GetNamespace() => _namespace;
    public string GetPath() => _path;
    public override string ToString() => $"{_namespace}:{_path}";

    public Identifier WithPath(string newPath) => new(GetNamespace(), newPath);
    public Identifier WithPrefix(string prefix) => WithPath(prefix + GetPath());
    public Identifier WithSuffix(string suffix) => WithPath(GetPath() + suffix);

    public Path ResolveAgainst(Path root)
    {
        Path fullPath = new(System.IO.Path.GetFullPath(System.IO.Path.Combine(root.GetPath(), GetNamespace(), GetPath())));
        Path fullRoot = new(System.IO.Path.GetFullPath(root.GetPath()));
        if (!fullPath.GetPath().StartsWith(fullRoot.GetPath()))
            throw new Exception(
                string.Format("Identifier \"%s\" tried to access path \"%s\" from root \"%s\"", this, fullPath, fullRoot)
            );
        else
            return fullPath;
    }

    public bool Equals(Identifier other) => _namespace == other._namespace && _path == other._path;
    public override bool Equals(object obj) => obj is Identifier other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_namespace, _path);

    public static readonly Codec<Identifier> CODEC = new IdentifierCodec(); 

    private sealed class IdentifierCodec : Codec<Identifier>
    {
        public override DataResult<JsonNode> Encode(Identifier value)
        {
            if (value == null)
                return new DataResult<JsonNode>.Error<JsonNode>("Identifier Has No Value", Optional.Empty<JsonNode>());
            return new DataResult<JsonNode>.Success<JsonNode>(JsonValue.Create(value.ToString()));
        }
        public override DataResult<Identifier> Decode(JsonNode? input)
        {
            if (input is JsonValue value && value.TryGetValue(out string? result)
                && result is not null)
                return new DataResult<Identifier>.Success<Identifier>(new Identifier(result));

            return new DataResult<Identifier>.Error<Identifier>("Expected a string.", Optional.Empty<Identifier>());
        }
    }
}