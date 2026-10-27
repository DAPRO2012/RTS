using System;

public enum AutoTileType
{
    None,
    Bitmask,
    Overlay
}

public record TextureData(Identifier[] TextureIdentifiers, AutoTileType Method = AutoTileType.None, int Zlayer = 0, Identifier Overlay = null)
{
    public static readonly Codec<TextureData> CODEC = RecordCodecBuilder<TextureData>.Create()
        .Field(Codec.STRING.FieldOf("method"), (td) => td.Method.ToString().ToLower())
        .Field(Identifier.CODEC.ArrayOf().FieldOf("textures"), (td) => td.TextureIdentifiers)
        .Field(Codec.INT.FieldOf("z-layer"), (td) => td.Zlayer)
        .Field(Identifier.CODEC.FieldOf("overlay"), (td) => td.Overlay)
        .Build((args) =>
        {
            Identifier overlay = args.Count < 4 ? null : (Identifier)args[3];
            int Zlayer = args.Count < 3 ? 0 : (int)args[2];

            AutoTileType method = AutoTileType.None;
            foreach (string enumString in Enum.GetNames(typeof(AutoTileType)))
            if (enumString.Equals(args[0].ToString(), StringComparison.CurrentCultureIgnoreCase))
                method = Enum.Parse<AutoTileType>(enumString);

            return new TextureData((Identifier[])args[1], method, Zlayer, overlay);
        });
}