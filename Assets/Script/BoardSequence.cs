using System.Collections.Generic;
using UnityEngine;

public class BoardSequence
{
    public List<Vector2Int> matchedPosition;
    public List<AddedTileInfo> addedTiles;
    public List<AddedTileInfo> newSpecialTiles;
    public List<MovedTileInfo> movedTiles;

    public override string ToString()
    {
        string log;
        log = "matchedPosition: \n";
        for (int i = 0; i < matchedPosition.Count; i++)
        {
            log += $"{matchedPosition[i]}, ";
        }

        log += "\naddedTiles: \n";
        for (int i = 0; i < addedTiles.Count; i++)
        {
            log += $"{addedTiles[i].position}, ";
        }

        log += "\nspecialTiles: \n";
        for (int i = 0; i < newSpecialTiles.Count; i++)
        {
            log += $"{newSpecialTiles[i].position}, ";
        }

        log += $"\nmovedTiles: {movedTiles.Count}\n";
        for (int i = 0; i < movedTiles.Count; i++)
        {
            log += $"{movedTiles[i].from} - {movedTiles[i].to}, ";
        }

        //log = $"matchedPosition: {matchedPosition.Count} - addedTiles: {addedTiles.Count} - movedTiles: {movedTiles.Count}";
        return log;
    }
}