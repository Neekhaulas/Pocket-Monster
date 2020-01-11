using System;
using System.Collections.Generic;

public class Model : File
{
    public Model(Reader reader)
    {
        magic = reader.getUint32();

        uint unitCount = reader.getUint32();

        reader.skipPadding(0x10, 0);

        Section section = new Section(reader, "gfmodel");
        long offset = reader.index;

        HashNameTable shaderPacks = new HashNameTable(reader);
        HashNameTable textureNames = new HashNameTable(reader);
    }

}

public class HashNameTable
{
    List<(uint, string)> values;

    public HashNameTable(Reader reader)
    {
        uint count = reader.readUInt32();
        values = new List<(uint, string)>();
        int looper = 0;
        while (looper < count)
        {
            values.Add((reader.readUInt32(), reader.readString(0x40)));

            looper++;
        }
    }
}