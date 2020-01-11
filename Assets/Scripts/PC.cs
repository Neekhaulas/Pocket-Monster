using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PC : File
{
    const uint GFModelPackageMagic = 0x15122117;
    const uint GFMaterialPackageMagic = 0x15041213;
    const uint GFMotionPackageMagic = 0x00060000;
    const uint GFResourcePackageMagic = 0x00010000;

    string magic;
    List<File> files;

    public PC()
    {

    }

    public void load(Reader reader)
    {
        Debug.Log(reader.available);
        Reader origin = reader.subreader();

        magic = origin.readString(2);
        Regex regex = new Regex(@"\A[A-Z]{2}\z");
        if(!regex.IsMatch(magic))
        {
            throw new System.Exception("Invalid magic for PC: " + magic);
        }

        ushort count = reader.readUint16();
        Debug.Log(count);

        List<uint> offsets = new List<uint>();
        int looper = 0;
        while(looper <= count)
        {
            offsets.Add(reader.readUInt32());
            looper++;
        }

        List<Reader> readerFiles = new List<Reader>();
        int index = 0;
        while(index < count)
        {
            uint offset = offsets[index];
            Debug.Log(offsets.Count);
            Debug.Log(offsets[index]);
            Debug.Log(offsets[index + 1]);
            uint length = offsets[index + 1] - offsets[index];
            readerFiles.Add(origin.subreader(offset, length));
            index++;
        }

        foreach(Reader fileReader in readerFiles)
        {
            string type = guessFileType(reader);
            File result = null;
            switch (type)
            {
                case "model": { result = new Model(reader); break; }
                /*case "motion": { result = new Motion(reader); break; }
                case "shader": { result = new Shader(reader); break; }
                case "texture": { result = new Texture(reader); break; }
                case "meta": { result = new Meta(reader); break; }
                case "package": { result = new Package(reader); break; }
                case "resource": { result = Resource(reader); break; }*/
                case "pc":
                    {
                        PC newPC = new PC();
                        newPC.load(reader);
                        result = newPC;
                        break;
                    }
                case "empty": { break; }
                case "unknown": { throw new Exception("Unknown file format"); }
                default:
                    {
                        throw new Exception("Invalid file format");
                    }
            }
            this.files.Add(result);
        }
    }

    public string guessFileType(Reader reader)
    {
        if(reader.available == 0)
        {
            return "empty";
        }
        else if(reader.available >= 2)
        {
            uint magic = reader.readUInt32();
            string magicString = reader.readString(2);
            switch(magic)
            {
                case GFModelPackageMagic: { return "model"; }
                case GFMaterialPackageMagic:
                    {
                        // TODO: check shader or texture
                        uint sectionName = reader.subreader(0x8, 0).readUInt32();
                        if (sectionName == 0)
                        {
                            return "shader";
                        }
                        return "texture";
                    }
                case GFMotionPackageMagic: { return "motion"; }
                case GFResourcePackageMagic: { return "package"; }
                default: { break; }
            }
            switch (magicString)
            {
                case "PC": { return "pc"; }
                case "PS": { return "pc"; }
                default:
                    {
                        // if (/[A-Z]{2}/.test(magicString)) {
                        //     return magicString.toLowerCase();
                        // }
                        break;
                    }
            }
        }

        string boneName = reader.getString(0x20);
        Regex regex = new Regex(@"\A[A-Z0-9]+\z");
        if (regex.IsMatch(boneName)) {
            return "meta";
        }

        return "resource";
    }
}
