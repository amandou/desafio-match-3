using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTiles : MonoBehaviour
{
    public static void ValidateSpecialTilesMaches(Vector2Int from, Vector2Int to, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        CleanLinesIfPossible(from, to, newBoard, matchedTiles);
        CleanColunmsIfPossible(from, to, newBoard, matchedTiles);
        ExplodeIfPossible(from, to, newBoard, matchedTiles);
        DestroyColorIfPossible(from, to, newBoard, matchedTiles);
    }

    private static void DestroyColorIfPossible(Vector2Int from, Vector2Int to, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        if (newBoard[from.y][from.x].type == (int)TileTypes.ColorDestroyer)
        {
            int matchedTileType = newBoard[to.y][to.x].type;
            DestroyColors(newBoard, matchedTiles, matchedTileType);
        }
        else if (newBoard[to.y][to.x].type == (int)TileTypes.ColorDestroyer)
        {
            int matchedTileType = newBoard[from.y][from.x].type;
            DestroyColors(newBoard, matchedTiles, matchedTileType);
        }
    }

    private static void CleanLinesIfPossible(Vector2Int from, Vector2Int to, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        if (newBoard[from.y][from.x].type == (int)TileTypes.LineBreaker)
        {
            CleanLine(from.y, matchedTiles);
        }
        else if (newBoard[to.y][to.x].type == (int)TileTypes.LineBreaker)
        {
            CleanLine(to.y, matchedTiles);
        }
    }

    private static void CleanColunmsIfPossible(Vector2Int from, Vector2Int to, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        if (newBoard[from.y][from.x].type == (int)TileTypes.ColumnBreaker)
        {
            CleanColunm(from.x, matchedTiles);
        }
        else if (newBoard[to.y][to.x].type == (int)TileTypes.ColumnBreaker)
        {
            CleanColunm(to.x, matchedTiles);
        }
    }
    private static void CleanLine(int y, List<List<int>> matchedTiles)
    {
        for (int i = 0; i < matchedTiles[y].Count; i++)
        {
            matchedTiles[y][i] = 1;
        }
    }

    private static void CleanColunm(int x, List<List<int>> matchedTiles)
    {
        for (int i = 0; i < matchedTiles[x].Count; i++)
        {
            matchedTiles[i][x] = 1;
        }
    }

    private static void ExplodeIfPossible(Vector2Int from, Vector2Int to, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        if (newBoard[from.y][from.x].type == (int)TileTypes.Bomb)
        {
            Explode(matchedTiles, from.x, from.y);
        }
        else if (newBoard[to.y][to.x].type == (int)TileTypes.Bomb)
        {
            Explode(matchedTiles, to.x, to.y);
        }
    }

    private static void Explode(List<List<int>> matchedTiles, int bombX, int bombY)
    {
        var radius = 3;
        var minY = Mathf.Max(0, bombY - radius);
        var minX = Mathf.Max(0, bombX - radius);
        var maxY = Mathf.Min(matchedTiles.Count, bombY + radius);
        var maxX = Mathf.Min(matchedTiles[bombY].Count, bombX + radius);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                if ((Mathf.Pow(y - bombY, 2) + Mathf.Pow(x - bombX, 2)) > Mathf.Pow(radius, 2)) continue;
                matchedTiles[y][x] = 1;
            }
        }
    }

    private static void DestroyColors(List<List<Tile>> newBoard, List<List<int>> matchedTiles, int matchedTileType)
    {
        int rows = newBoard.Count;
        for (int y = 0; y < rows; y++)
        {
            int columns = newBoard[y].Count;
            for (int x = 0; x < columns; x++)
            {
                if (matchedTileType == newBoard[y][x].type)
                    matchedTiles[y][x] = 1;
            }
        }
    }

}
