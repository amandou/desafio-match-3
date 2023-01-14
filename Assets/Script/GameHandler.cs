using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameHandler : MonoBehaviour
{
        
    [SerializeField] private GameController gameController;

    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] public int boardWidth = 10;

    [SerializeField] public int boardHeight = 10;

    [SerializeField] public BoardView boardView;

    [SerializeField] private AudioClip swapSound;
    public static event Action<AudioClip> onPlaySound;

    private int score = 0;

    private void Awake()
    {
        gameController = new GameController();
        boardView.onTileClick += OnTileClick;
        scoreText.text = "Score \n\n"+ score;
    }

    private void Start()
    {
        List<List<Tile>> board = gameController.StartGame(boardWidth, boardHeight);
        boardView.CreateBoard(board);
    }

    private int selectedX, selectedY = -1;

    private bool isAnimating;

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) return;

        if (selectedX > -1 && selectedY > -1)
        {
            if (Mathf.Abs(selectedX - x) + Mathf.Abs(selectedY - y) > 1)
            {
                selectedX = -1;
                selectedY = -1;
            }
            else
            {
                isAnimating = true;
                onPlaySound?.Invoke(swapSound);
                boardView.SwapTiles(selectedX, selectedY, x, y).onComplete += () =>
                {
                    Vector2Int from = new Vector2Int(selectedX, selectedY);
                    Vector2Int to = new Vector2Int(x, y);
                    bool isValid = gameController.IsValidMovement(from, to);
                   
                    if (!isValid)
                    {
                        boardView.SwapTiles(x, y, selectedX, selectedY)
                        .onComplete += () => isAnimating = false;
                    }
                    else
                    {
                        List<BoardSequence> swapResult = gameController.SwapTile(selectedX, selectedY, x, y);

                        AnimateBoard(swapResult, 0, () => isAnimating = false);

                    }
                    selectedX = -1;
                    selectedY = -1;
                };
            }
        }
        else
        {
            selectedX = x;
            selectedY = y;
        }
    }

    private void AnimateBoard(List<BoardSequence> boardSequences, int i, Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        BoardSequence boardSequence = boardSequences[i];
        sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition));
        sequence.Append(boardView.CreateTile(boardSequence.newSpecialTiles));
        sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
        sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

        i++;
        if (i < boardSequences.Count)
        {
            sequence.onComplete += () => AnimateBoard(boardSequences, i, onComplete);
        }
        else
        {
            sequence.onComplete += () => onComplete();
        }
    }

    private void IncreaseScore()
    {
        score += 10;
        scoreText.text = "Score \n\n" + score;
    }

    private void OnEnable()
    {
        GameController.onUpdateScore += IncreaseScore;
    }

    private void OnDisable()
    {
        GameController.onUpdateScore -= IncreaseScore;
    }

}
