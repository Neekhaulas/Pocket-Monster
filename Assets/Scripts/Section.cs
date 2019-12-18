using System;
public class Section 
{
    string magic;
    uint length;

    public Section(Reader reader, string expected)
    {
        magic = reader.readString(8);
        if(!magic.Equals(expected))
        {
            throw new Exception("Invalid magic " + magic + ", expected: " + expected);
        }
        length = reader.readUInt32();
        if(reader.readInt32() != -1)
        {
            throw new Exception("Invalid section padding, expected -1");
        }
    }
}
