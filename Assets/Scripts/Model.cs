using System;
public class Model : File
{
    public Model(Reader reader)
    {
        magic = reader.getUint32();

        uint unitCount = reader.getUint32();

        reader.skipPadding(0x10, 0);

        Section section = new Section(reader, "gfmodel"); 

    }
}
