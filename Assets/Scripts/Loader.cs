using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

public class Loader
{
    string files;

    public Loader(string files)
    {

        this.files = files.Substring(0);
    }

    public PC load(PC pc)
    {
        if (pc == null)
        {
            pc = new PC();
        }

        using (FileStream fileStream = new FileStream("./Assets/Resources/" + files, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                pc.load(new Reader(reader, 0, reader.BaseStream.Length));
            }
        }

        return pc;
    }
}

public class Reader
{
    public BinaryReader buffer;
    public long begin = 0;
    public long end;
    public long index;

    public long available { get { return this.end - this.index; } }

    public Reader(BinaryReader buffer, long offset, long length)
    {
        this.buffer = buffer;
        this.begin = offset;
        this.end = this.buffer.BaseStream.Length;
        Debug.Log(end);
        if (this.begin + length < this.end)
        {
            this.end = this.begin + length;
        }

        this.index = this.begin;
    }

    public string getString(long length)
    {
        if (this.index >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }

        if (length == 0) {
            length = 0;
            while ((this.index + length < this.end) &&
                   (this.index + length < this.buffer.BaseStream.Length))
            {
                this.buffer.BaseStream.Seek(this.index + length, SeekOrigin.Begin);
                if(getUint8() != 0)
                {
                    ++length;
                }
            }
        }

        List<Byte> byteList = new List<Byte>();

        while(length > 0)
        {
            byteList.Add(readUint8());
            length--;
        }
        return Encoding.ASCII.GetString(byteList.ToArray());
    }

    public string readString(long length)
    {
        string str = getString(length);
        if (length != 0) {
            this.index += length;
        } else
        {
            this.index += str.Length + 1;
        }

        return str;
    }

    public byte getUint8()
    {
        if(this.index >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadByte();
    }

    public ushort getUint16()
    {
        if (this.index + 1 >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadUInt16();
    }

    public uint getUint32()
    {
        if (this.index + 3 >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadUInt32();
    }

    public sbyte getInt8()
    {
        if (this.index >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadSByte();
    }

    public short getInt16()
    {
        if (this.index + 1 >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadInt16();
    }

    public int getInt32()
    {
        if (this.index + 3 >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadInt32();
    }

    public byte readUint8()
    {
        byte uint8 = this.getUint8();
        index++;
        return uint8;
    }

    public ushort readUint16()
    {
        ushort uint16 = this.getUint16();
        index += 2;
        return uint16;
    }

    public uint readUInt32()
    {
        uint uint32 = this.getUint32();
        index += 4;
        return uint32;
    }

    public sbyte readInt8()
    {
        sbyte int8 = this.getInt8();
        index++;
        return int8;
    }

    public short readInt16()
    {
        short int16 = this.getInt16();
        index += 2;
        return int16;
    }

    public int readInt32()
    {
        int int32 = this.getInt32();
        index += 4;
        return int32;
    }

    public float getFloat32()
    {
        if (this.index + 3 >= this.end)
        {
            throw new Exception("Reader has reach the end of buffer");
        }
        this.buffer.BaseStream.Seek(index, SeekOrigin.Begin);
        return this.buffer.ReadSingle();
    }

    public float readFloat32()
    {
        float float32 = this.getFloat32();
        index += 4;
        return float32;
    }

    public Reader subreader(long offset, long length)
    {
        if(length <= 0)
        {
            length = this.end - this.index - offset;
        }
        if (length > 0)
        {
            if (this.index >= this.end)
            {
                throw new Exception("Reader has reach the end of buffer");
            }
            if (this.index + offset >= this.end)
            {
                throw new Exception("The offset is out of the range of the buffer");
            }
            if (this.index + offset + length > this.end)
            {
                throw new Exception("The end of the slice is out of the range of the buffer");
            }
        }
        return new Reader(this.buffer, this.index + offset, length);
    }

    public Reader subreader()
    {
        return subreader(0, 0);
    }

    public void skipPadding(long baseSkip, byte[] expected)
    {
        while((this.index - this.begin) % baseSkip == 1)
        {
            byte value = this.readUint8();
            for(int i = 0; i < expected.Length; i++)
            {
                if(expected[i] == value)
                {
                    throw new Exception("Expected " + expected[i] + ", but got " + value);
                }
            }
        }
    }

    public void skipPadding(long baseSkip, byte expected)
    {
        skipPadding(baseSkip, new[] { expected });
    }

    public void skip(long offset, byte[] expected)
    {
        int looper = 0;
        while (looper < offset)
        {
            byte value = readUint8();
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] == value)
                {
                    throw new Exception("Expected " + expected[i] + ", but got " + value);
                }
            }
            ++looper;
        }
    }

    public void skip(long offset, byte expected)
    {
        skip(offset, new[] { expected });
    }
}