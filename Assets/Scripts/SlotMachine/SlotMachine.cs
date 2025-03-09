
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Slot Reels Configuration")]
    [SerializeField] private MoveUp[] slotReels;

    [Header("UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private TextMeshProUGUI betAmountText;
    [SerializeField] private TextMeshProUGUI winAmountText;
    [SerializeField] private TextMeshProUGUI avialbeAmountText;

    [Header("Bet Configuration")]
    [SerializeField] private int betIncrement = 10;
    [SerializeField] private int minBet = 10;

    private bool isSpinning = false;
    private int betAmount;
    [SerializeField]private int availableAmount;


    private void Start()
    {
        avialbeAmountText.text = availableAmount.ToString();
        betAmount = 10;
    }
    /// <summary>
    /// Starts spinning all slot reels.
    /// </summary>
    public void StartSpinning()
    {
        if (isSpinning || availableAmount-betAmount <0) return;

        isSpinning = true;
        availableAmount -= betAmount;
        foreach (MoveUp reel in slotReels)
        {
            reel.StartSpinning();
        }
    }

    /// <summary>
    /// Checks if all reels match after spinning and determines the outcome.
    /// </summary>
    public void CheckWinCondition()
    {
        if (!isSpinning) return;

        // Stop all reels
        foreach (MoveUp reel in slotReels)
        {
            reel.StopSpinning();
        }

        // Verify if all reels have the same value
        for (int i = 1; i < slotReels.Length; i++)
        {
            if (slotReels[i].SelectedPoint != slotReels[i - 1].SelectedPoint)
            {
             
               StartCoroutine(ActivatePanelAfterSometime(lossPanel));
                return;
            }
        }
        winAmountText.text = "You Won " + betAmount * 2;
        StartCoroutine(ActivatePanelAfterSometime(winPanel));

        availableAmount += betAmount * 2;

    }

    IEnumerator ActivatePanelAfterSometime(GameObject obj)
    {
    
        yield return new WaitForSeconds(1f);
        
        obj.SetActive(true);
        avialbeAmountText.text = availableAmount.ToString();
        isSpinning = false;
    }

    /// <summary>
    /// Increases the bet amount, ensuring it does not exceed the available balance.
    /// </summary>
    public void IncreaseBet()
    {
        if (isSpinning)
            return;
        if (betAmount + betIncrement <= availableAmount)
        {
            betAmount += betIncrement;
            UpdateBetUI();
        }
    }

    /// <summary>
    /// Decreases the bet amount, ensuring it does not go below the minimum bet.
    /// </summary>
    public void DecreaseBet()
    {
        if (isSpinning)
            return;
        if (betAmount > minBet)
        {
            betAmount -= betIncrement;
            UpdateBetUI();
        }
    }

    /// <summary>
    /// Updates the bet amount text in the UI.
    /// </summary>
    private void UpdateBetUI()
    {
        betAmountText.text = betAmount.ToString("00");
    }
}