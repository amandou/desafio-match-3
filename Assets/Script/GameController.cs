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
                if (x > 1
                    && newBoard[y][x].type == newBoard[y][x - 1].type
                    && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
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
        Debug.Log("SwapTile");
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<List<int>> matchedTiles = CreateMatchedTiles(newBoard);

        List<List<int>> matchedTileList = FindMatches(newBoard, matchedTiles);

        int i = 0;
        Debug.Log("result "+ HasMatch(matchedTileList));
        bool hasMatches;
        CleanLinesIfPossible(fromX,  fromY,  toX, toY, newBoard, matchedTiles);
        CleanColunmsIfPossible(fromX, fromY, toX, toY, newBoard, matchedTiles);

        do
        {
            hasMatches = HasMatch(FindMatches(newBoard, matchedTiles));
            if (!hasMatches) continue;

            var specialTilesAdded = new List<AddedTileInfo>();
            onUpdateScore?.Invoke();

            var matchedPosition = CleanMatchedTiles(newBoard, matchedTiles, specialTilesAdded);

            Dictionary<int, MovedTileInfo> movedTiles = new Dictionary<int, MovedTileInfo>();
            List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
            DroppingTiles(newBoard, matchedPosition, movedTiles, movedTilesList);

            List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
            FillBoard(newBoard, addedTiles);

            BoardSequence sequence = new BoardSequence
            {
                matchedPosition = matchedPosition,
                movedTiles = movedTilesList,
                addedTiles = addedTiles,
                newSpecialTiles = specialTilesAdded
            };

            boardSequences.Add(sequence);
            matchedTiles = CreateMatchedTiles(newBoard);
        } while (hasMatches);

        _boardTiles = newBoard;
        return boardSequences;
    }
    private void CleanLinesIfPossible(int fromX, int fromY, int toX, int toY, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        if (newBoard[fromY][fromX].type == (int)TileTypes.LineBreaker)
        {
            CleanLine(fromY, matchedTiles);
        }
        else if (newBoard[toY][toX].type == (int)TileTypes.LineBreaker)
        {
            CleanLine(toY, matchedTiles);
        }
    }

    private void CleanColunmsIfPossible(int fromX, int fromY, int toX, int toY, List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        if (newBoard[fromY][fromX].type == (int)TileTypes.ColumnBreaker)
        {
            CleanColunm(fromY-1, matchedTiles);
        }
        else if (newBoard[toY][toX].type == (int)TileTypes.ColumnBreaker)
        {
            CleanColunm(toY-1, matchedTiles);
        }
    }


    private List<List<int>> CreateMatchedTiles(List<List<Tile>> newBoard)
    {
        Debug.Log("CreateMatchedTiles");
        List<List<int>> matchedTiles = new List<List<int>>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            matchedTiles.Add(new List<int>(newBoard[y].Count));
            for (int x = 0; x < newBoard.Count; x++)
            {
                matchedTiles[y].Add(0);
            }
        }
        return matchedTiles;
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

    private static bool HasMatch(List<List<int>> list)
    {
        Debug.Log("Has Match");
        for (int y = 0; y < list.Count; y++)
            for (int x = 0; x < list[y].Count; x++)
                if (list[y][x] != 0)
                    return true;
        return false;
    }

    private static List<List<int>> FindMatches(List<List<Tile>> newBoard, List<List<int>> matchedTiles)
    {
        Debug.Log("Find Matches");
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
    private static void FindHorizontalMatch(List<List<Tile>> newBoard, List<List<int>> matchedTiles, int x, int y)
    {
        if (x > 2)
        {
            if (newBoard[y][x].type == newBoard[y][x - 1].type &&
                newBoard[y][x - 1].type == newBoard[y][x - 2].type &&
                newBoard[y][x - 2].type == newBoard[y][x - 3].type)
            {
                VerifyValidTile(newBoard, matchedTiles, x, y, -(int)TileTypes.LineBreaker);
                VerifyValidTile(newBoard, matchedTiles, x-1, y, 1);
                VerifyValidTile(newBoard, matchedTiles, x-2, y, 1);
                VerifyValidTile(newBoard, matchedTiles, x-3, y, 1);
                return;
            }
        }

        if (x > 1)
        {

            if (newBoard[y][x].type == newBoard[y][x - 1].type
                && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
            {
                VerifyValidTile(newBoard, matchedTiles, x , y, 1);
                VerifyValidTile(newBoard, matchedTiles, x - 1, y, 1);
                VerifyValidTile(newBoard, matchedTiles, x - 2, y, 1);
                return;
                
            }
        }
    }

    private static void FindVerticalMatch(List<List<Tile>> newBoard, List<List<int>> matchedTiles, int x, int y)
    {
        if (y > 2)
        {
            if (newBoard[y][x].type == newBoard[y - 1][x].type &&
                newBoard[y - 1][x].type == newBoard[y - 2][x].type &&
                newBoard[y - 2][x].type == newBoard[y - 3][x].type)
            {
                VerifyValidTile(newBoard, matchedTiles, x, y, -(int)TileTypes.ColumnBreaker);
                VerifyValidTile(newBoard, matchedTiles, x, y - 1, 1);
                VerifyValidTile(newBoard, matchedTiles, x, y - 2, 1);
                VerifyValidTile(newBoard, matchedTiles, x, y - 3, 1);
                return;
            }
        }

        if (y > 1)
        {
            if (newBoard[y][x].type == newBoard[y - 1][x].type &&
                newBoard[y - 1][x].type == newBoard[y - 2][x].type)
            {
                VerifyValidTile(newBoard, matchedTiles, x, y, 1);
                VerifyValidTile(newBoard, matchedTiles, x, y - 1, 1);
                VerifyValidTile(newBoard, matchedTiles, x, y - 2, 1);
                return;
            }
        }
    }

    private static void VerifyValidTile(List<List<Tile>> newBoard, List<List<int>> matchedTiles, int x, int y, int value)
    {
        if (matchedTiles[y][x] > -1)
        {
            matchedTiles[y][x] = value;
        }
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

    private List<Vector2Int> CleanMatchedTiles(List<List<Tile>> newBoard, List<List<int>> matchedTiles, List<AddedTileInfo> newSpecialTiles)
    {
        Debug.Log("CleanMatchedTiles");

        List<Vector2Int> matchedPosition = new List<Vector2Int>();

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                var tileType = matchedTiles[y][x];
                if (tileType != 0)
                {
                    matchedPosition.Add(new Vector2Int(x, y));
                    if (tileType < 0)
                    {
                        newBoard[y][x] = new Tile { id = _tileCount++, type = tileType };
                        newSpecialTiles.Add(new AddedTileInfo
                        {
                            position = new Vector2Int(x, y),
                            type = -newBoard[y][x].type
                        });
                    }
                    else
                    {
                        newBoard[y][x] = new Tile { id = -1, type = -1 };
                    }
                }
            }
        }
        return matchedPosition;
    }

    private void DroppingTiles(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition, Dictionary<int, MovedTileInfo> movedTiles, List<MovedTileInfo> movedTilesList)
    {
        Debug.Log("DroppingTiles");

        for (int i = 0; i < matchedPosition.Count; i++)
        {
            int x = matchedPosition[i].x;
            int y = matchedPosition[i].y;

            if (newBoard[y][x].type != -1) continue;

            if (y > 0)
            {
                for (int j = y; j > 0; j--)
                {
                    Tile movedTile = newBoard[j - 1][x];
                    newBoard[j][x] = movedTile;
                    if (movedTile.type != -1)
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
                else if (newBoard[y][x].type < -1)
                {
                    newBoard[y][x].type = -newBoard[y][x].type;
                    Debug.Log("newBoard[y][x].type " + newBoard[y][x].type);
                }
            }
        }
    }

}
