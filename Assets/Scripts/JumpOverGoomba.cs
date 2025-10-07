using UnityEngine;
using TMPro;

public class JumpOverGoomba : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    [System.NonSerialized] public int score = 0;

    void Start()
    {
        if (scoreText) scoreText.text = "Score: 0";
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText) scoreText.text = "Score: " + score;
        Debug.Log($"Score: {score}");
    }
}
