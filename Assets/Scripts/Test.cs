using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Test : MonoBehaviour
{
    public struct PokemonStruct
    {
        public int id;
        public string name;
        public int file;
        public bool hasGenderDifference;
        public bool hasExtrasModels;
        public byte modelsCount;
        public List<PokemonModel> models;
    }

    public struct PokemonModel
    {
        public int file;
        public byte natural;
        public byte decoration;
        public bool issue;
    }

    public List<string> PokemonsNames = new List<string>();
    public PokemonStruct[] Pokemons;

    // Start is called before the first frame update
    void Start()
    {
        string line;

        using (StreamReader file = new StreamReader("./Assets/Resources/national.txt"))
        {
            while ((line = file.ReadLine()) != null)
            {
                PokemonsNames.Add(line);
            }
            Pokemons = new PokemonStruct[PokemonsNames.Count];
        }

        using (FileStream fileStream = new FileStream("./Assets/Resources/file_00000.bin", FileMode.Open))
        {
            using(BinaryReader reader = new BinaryReader(fileStream))
            {
                Reader readerPkm = new Reader(reader, 0, reader.BaseStream.Length);

                var id = 0;

                while (id < PokemonsNames.Count)
                {
                    var fileNumber = readerPkm.readUint16();
                    var count = readerPkm.readUint8();
                    var flags = readerPkm.readUint8();
                    Pokemons[id] = new PokemonStruct
                    {
                        id = id + 1,
                        name = PokemonsNames[id],
                        file = fileNumber,
                        hasGenderDifference = (flags & 0x2) != 0,
                        hasExtrasModels = (flags & 0x4) != 0,
                        modelsCount = count,
                        models = new List<PokemonModel>(),
                    };
                    id++;
                }

                id = 0;
                var file = 0;
                var offset = 0;
                while(readerPkm.index < readerPkm.end)
                {
                    var pokemon = Pokemons[id];
                    byte[] flags = { readerPkm.readUint8(), readerPkm.readUint8() };
                    pokemon.models.Add(new PokemonModel
                    {
                        file = pokemon.file + pokemon.models.Count,
                        natural = flags[0],
                        decoration = flags[1],
                        issue = false
                    });
                    file++;
                    offset++;
                    if(pokemon.file + pokemon.modelsCount <= file)
                    {
                        Debug.Log("Loaded " + PokemonsNames[id]);
                        pokemon.modelsCount = 0;
                        id++;
                        offset = 0;
                    }
                }
            }
        }
        loadPokemon(0, 0);
    }

    public void loadPokemon(int id, int offset)
    {
        var pokemon = Pokemons[id - 1];

        int origin = (pokemon.file + offset) * 9 + 1;

        List<PC> files = new List<PC>();

        var tuples = new List<(string, byte)>
        {
            ("model", 4),
            ("textures.normal", 4),
            ("textures.shiny", 4),
            ("textures.shadow", 4),
            ("motions.fighting", 1),
            ("motions.pet", 1),
            ("motions.map", 1),
            ("motions.acting", 1),
            ("extra", 8),
        };

        for(int i = 0; i < tuples.Count; i++)
        {
            loadPackage(pokemon, offset, origin + i, tuples[i].Item2);
        }
    }

    public void loadPackage(PokemonStruct pokemon, int offset, int file, byte flag)
    {
        PC finalPC = new PC();

        List<int> ids = new List<int>();
        ids.Add(file);
        int newOffset = 0;
        while((pokemon.models[offset + newOffset].natural & flag) != 0)
        {
            --newOffset;
        }
        if(newOffset != 0)
        {
            ids.Add(file + 9 * newOffset);
        }

        foreach(int id in ids)
        {
            PC pc = new Loader(getFileName(id)).load(null);
        }
    }

    public string getFileName(int id)
    {
        string number = "00000" + id.ToString();
        number = number.Substring(number.Length - 5, 5);
        return "file_" + number + ".pc";
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
