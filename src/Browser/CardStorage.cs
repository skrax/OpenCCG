using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Proto;
using Serilog;
using HttpClient = System.Net.Http.HttpClient;

namespace OpenCCG.Browser;

[GlobalClass]
public partial class CardStorage : Node
{
    private readonly HttpClient _httpClient;

    public CreatureOutline[] Creatures = null!;
    public SpellOutline[] Spells = null!;
    public IDictionary<string, CreatureOutline> CreaturesById = null!;
    public IDictionary<string, SpellOutline> SpellsById = null!;
    private readonly TaskCompletionSource _dataReadyTsc = new();
    public Task DataReady => _dataReadyTsc.Task;

    public CardStorage()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7085");
    }

    public override async void _Ready()
    {
        Creatures = await FetchCreaturesAsync();
        Spells = await FetchSpellsAsync();
        CreaturesById = Creatures.ToDictionary(x => x.Id);
        SpellsById = Spells.ToDictionary(x => x.Id);
        
        Log.Information("finished fetching cards {CreatureCount} {SpellCount}", Creatures.Length, Spells.Length);

        _dataReadyTsc.SetResult();
    }

    private async Task<CreatureOutline[]> FetchCreaturesAsync()
    {
        var list = new List<CreatureOutline>();

        var pageNumber = 1;
        while (true)
        {
            var creatures =
                await _httpClient.GetFromJsonAsync<CreatureOutline[]>(
                    $"Creatures?orderBy=cost&pageNumber={pageNumber++}");
            if (creatures is null || !creatures.Any()) break;

            list.AddRange(creatures);
        }

        return list.ToArray();
    }

    private async Task<SpellOutline[]> FetchSpellsAsync()
    {
        var list = new List<SpellOutline>();

        var pageNumber = 1;
        while (true)
        {
            var spells =
                await _httpClient.GetFromJsonAsync<SpellOutline[]>($"Spells?orderBy=cost&pageNumber={pageNumber++}");
            if (spells is null || !spells.Any()) break;

            list.AddRange(spells);
        }

        return list.ToArray();
    }
}