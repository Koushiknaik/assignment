using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DiceManager : MonoBehaviour
{
    [Header("Dice Settings")]
    [SerializeField] private Sprite[] diceSprites;  // Renamed for clarity
    [SerializeField] private Image firstDice, secondDice;
    [SerializeField] private Button rollButton;
    [SerializeField] private float animationDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource rollSound;

    [Header("Game Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject lossPanel;

    [Header("Win/Loss Numbers")]
    [SerializeField] private int[] winNumbers;
    [SerializeField] private int[] lossNumbers;

    private int diceValue1, diceValue2;

    private void Start()
    {
        if (rollButton != null)
        {
            rollButton.onClick.AddListener(() => StartCoroutine(RollDiceAnimation()));
        }
    }

    private IEnumerator RollDiceAnimation()
    {
        rollButton.interactable = false; // Disable button to prevent multiple rolls
        winPanel.SetActive(false);
        lossPanel.SetActive(false);

        if (rollSound != null) rollSound.Play();

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            firstDice.sprite = diceSprites[Random.Range(0, 6)];
            secondDice.sprite = diceSprites[Random.Range(0, 6)];
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        // Ensure dice values are in range (1-6)
        diceValue1 = Random.Range(1, 7);
        diceValue2 = Random.Range(1, 7);
        int totalValue = diceValue1 + diceValue2;

        firstDice.sprite = diceSprites[diceValue1 - 1]; // Adjusting for zero-based index
        secondDice.sprite = diceSprites[diceValue2 - 1];

        yield return new WaitForSeconds(0.5f);

        // Check Win/Loss Conditions
        if (System.Array.Exists(winNumbers, num => num == totalValue))
        {
            winPanel.SetActive(true);
        }
        else if (System.Array.Exists(lossNumbers, num => num == totalValue))
        {
            lossPanel.SetActive(true);
        }

        rollButton.interactable = true; // Re-enable the button after animation
    }
}
