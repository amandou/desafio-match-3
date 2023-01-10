using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController
{
    private List<List<Tile>> _boardTiles;
    private List<int> _tilesTypes;
    private int _tileCount;
    
    public static event Action onUpdateScore;


    public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
    {
        
        _tilesTypes = new List<int> { (int)TileTypes.Yellow, (int)TileTypes.Blue, (int)TileTypes.Green, (int)TileTypes.Orange };
        _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
        return _boardTiles;
    }

    public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {

                if (x > 2 &&
                    newBoard[y][x].type == newBoard[y][x - 1].type &&
                    newBoard[y][x - 1].type == newBoard[y][x - 2].type &&
                    newBoard[y][x - 2].type == newBoard[y][x - 3].type)
                {
                    return true;
                }

                if (x > 1
                    && newBoard[y][x].type == newBoard[y][x - 1].type
                    && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
                {
                    return true;
                }

                if (y > 2
                    && newBoard[y][x].type == newBoard[y - 1][x].type
                    && newBoard[y - 1][x].type == newBoard[y - 2][x].type
                    && newBoard[y - 2][x].type == newBoard[y - 3][x].type)
                {
                    return true;
                }

                if (y > 1
                    && newBoard[y][x].type == newBoard[y - 1][x].type
                    && newBoard[y - 1][x].type == newBoard[y - 2][x].type)
                {
                    return true;
                }

            }
        }
        return false;
    }

    public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<List<bool>> matchedTiles;

        while (HasMatch(matchedTiles = FindMatches(newBoard)))
        {
            List<Vector2Int> matchedPosition = new List<Vector2Int>();
            CleanMatchedTiles(newBoard, matchedPosition, matchedTiles);

            onUpdateScore?.Invoke();

            Dictionary<int, MovedTileInfo> movedTiles = new Dictionary<int, MovedTileInfo>();
            List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
            DroppingTiles(newBoard, matchedPosition, movedTiles, movedTilesList);

            List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
            FillBoard(newBoard, addedTiles);

            BoardSequence sequence = new BoardSequence
            {
                matchedPosition = matchedPosition,
                movedTiles = movedTilesList,
                addedTiles = addedTiles
            };
            boardSequences.Add(sequence);
        }

        _boardTiles = newBoard;
        return boardSequences;
    }

    private static void CleanLine(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int y)
    {
        for (int i = 0; i < newBoard[y].Count; i++)
        {
            matchedTiles[y][i] = true;
        }
    }

    private static void CleanColunm(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int x)
    {
        for (int i = 0; i < newBoard[x].Count; i++)
        {
            matchedTiles[i][x] = true;
        }
    }

    private static bool HasMatch(List<List<bool>> list)
    {
        for (int y = 0; y < list.Count; y++)
            for (int x = 0; x < list[y].Count; x++)
                if (list[y][x])
                    return true;
        return false;
    }

    private static List<List<bool>> FindMatches(List<List<Tile>> newBoard)
    {
        List<List<bool>> matchedTiles = new List<List<bool>>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            matchedTiles.Add(new List<bool>(newBoard[y].Count));
            for (int x = 0; x < newBoard.Count; x++)
            {
                matchedTiles[y].Add(false);
            }
        }

        int rows = newBoard.Count;

        for (int y = 0; y < rows; y++)
        {
            int colums = newBoard[y].Count;
            for (int x = 0; x < colums; x++)
            {
                FindHorizontalMatch(newBoard, matchedTiles, x, y);

                FindVerticalMatch(newBoard, matchedTiles, x, y);
            }
        }

        return matchedTiles;
    }

    private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
    {
        List<List<Tile>> newBoard = new List<List<Tile>>(boardToCopy.Count);
        for (int y = 0; y < boardToCopy.Count; y++)
        {
            newBoard.Add(new List<Tile>(boardToCopy[y].Count));
            for (int x = 0; x < boardToCopy[y].Count; x++)
            {
                Tile tile = boardToCopy[y][x];
                newBoard[y].Add(new Tile { id = tile.id, type = tile.type });
            }
        }

        return newBoard;
    }

    private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
    {
        List<List<Tile>> board = new List<List<Tile>>(height);
        _tileCount = 0;
        for (int y = 0; y < height; y++)
        {
            board.Add(new List<Tile>(width));
            for (int x = 0; x < width; x++)
            {
                board[y].Add(new Tile { id = -1, type = -1 });
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                List<int> noMatchTypes = new List<int>(tileTypes.Count);

                for (int i = 0; i < tileTypes.Count; i++)
                {
                    noMatchTypes.Add(_tilesTypes[i]);
                }

                AvoidHorizontalMatch(noMatchTypes, board, x, y);

                AvoidVerticalMatch(noMatchTypes, board, x, y);

                board[y][x].id = _tileCount++;
                board[y][x].type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
            }
        }

        return board;
    }

    private void AvoidHorizontalMatch(List<int> noMatchTypes, List<List<Tile>> board, int x, int y)
    {
        if (x > 1)
        {
            var middleTile = board[y][x - 1].type;
            var leftTile = board[y][x - 2].type;
            if (middleTile == leftTile)
            {
                noMatchTypes.Remove(middleTile);
            }
        }

    }

    private void AvoidVerticalMatch(List<int> noMatchTypes, List<List<Tile>> board, int x, int y)
    {
        if (y > 1)
        {
            var middleTile = board[y - 1][x].type;
            var upperTile = board[y - 2][x].type;
            if (middleTile == upperTile)
            {
                noMatchTypes.Remove(middleTile);
            }
        }
    }

    private static void FindHorizontalMatch(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int x, int y)
    {
        if (x > 2)
        {
            if (newBoard[y][x].type == newBoard[y][x - 1].type &&
                newBoard[y][x - 1].type == newBoard[y][x - 2].type &&
                newBoard[y][x - 2].type == newBoard[y][x - 3].type)
            {
                CleanLine(newBoard, matchedTiles, y);
                return;
            }
        }

        if (x > 1)
        {

            if (newBoard[y][x].type == newBoard[y][x - 1].type 
                && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
            {
                matchedTiles[y][x] = true;
                matchedTiles[y][x - 1] = true;
                matchedTiles[y][x - 2] = true;
            }
        }
    }

    private static void FindVerticalMatch(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int x, int y)
    {
        if (y > 2)
        {
            if (newBoard[y][x].type == newBoard[y - 1][x].type &&
                newBoard[y - 1][x].type == newBoard[y - 2][x].type &&
                newBoard[y - 2][x].type == newBoard[y - 3][x].type 
                )
            {
                CleanColunm(newBoard, matchedTiles, x);
               
            }
        }

        if (y > 1)
        {
            if (newBoard[y][x].type == newBoard[y - 1][x].type &&
                newBoard[y - 1][x].type == newBoard[y - 2][x].type)
            {
               
                matchedTiles[y][x] = true;
                matchedTiles[y - 1][x] = true;
                matchedTiles[y - 2][x] = true;
            }
        }

    }

    private void CleanMatchedTiles(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition, List<List<bool>> matchedTiles)
    {
        Debug.Log("CleanMatchedTiles");
        for (int y = 0; y < newBoard.Count; y++)
        {
            int count = 0;
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (matchedTiles[y][x])
                {
                    count++;
                    matchedPosition.Add(new Vector2Int(x, y));
                    newBoard[y][x] = new Tile { id = -1, type = -1 };
                }
            }
        }
    }

    private void DroppingTiles(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition, Dictionary<int, MovedTileInfo> movedTiles, List<MovedTileInfo> movedTilesList)
    {
        Debug.Log("DroppingTiles");
        for (int i = 0; i < matchedPosition.Count; i++)
        {
            int x = matchedPosition[i].x;
            int y = matchedPosition[i].y;
            if (y > 0)
            {
                for (int j = y; j > 0; j--)
                {
                    Tile movedTile = newBoard[j - 1][x];
                    newBoard[j][x] = movedTile;
                    if (movedTile.type > -1)
                    {
                        if (movedTiles.ContainsKey(movedTile.id))
                        {
                            movedTiles[movedTile.id].to = new Vector2Int(x, j);
                        }
                        else
                        {
                            MovedTileInfo movedTileInfo = new MovedTileInfo
                            {
                                from = new Vector2Int(x, j - 1),
                                to = new Vector2Int(x, j)
                            };
                            movedTiles.Add(movedTile.id, movedTileInfo);
                            movedTilesList.Add(movedTileInfo);
                        }
                    }
                }

                newBoard[0][x] = new Tile
                {
                    id = -1,
                    type = -1
                };
            }
        }
    }

    private void FillBoard(List<List<Tile>> newBoard, List<AddedTileInfo> addedTiles)
    {
        Debug.Log("FillBoard");
        for (int y = newBoard.Count - 1; y > -1; y--)
        {
            for (int x = newBoard[y].Count - 1; x > -1; x--)
            {
                if (newBoard[y][x].type == -1)
                {
                    int tileType = Random.Range(0, _tilesTypes.Count);
                    Tile tile = newBoard[y][x];
                    tile.id = _tileCount++;
                    tile.type = _tilesTypes[tileType];
                    addedTiles.Add(new AddedTileInfo
                    {
                        position = new Vector2Int(x, y),
                        type = tile.type
                    });
                }
            }
        }
    }

}
