using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour {

	public GameObject mainMenu;
	public GameObject ui;
	public GameObject gameOver;
	public Image step0Image;
	public Image step1Image;
	public Image step2Image;
	public Image step3Image;
	public Image stepDotsImage;
	public TextMeshProUGUI levelText;
	public TextMeshProUGUI keysText;
	public TextMeshProUGUI potionsText;
	public TextMeshProUGUI treasuresText;

	void Start() {
		mainMenu.SetActive(true);
		ui.SetActive(false);
		gameOver.SetActive(false);
	}


	void Update() {
		// Get the references
		PlayerController player = FindObjectOfType<PlayerController>();
		GameManager gameManager = GameManager.Instance;

		if (player) {
			// Change the texts
			levelText.text = "Level: " + gameManager.currentLevel;
			keysText.text = "Keys: " + player.keys + "/3";
			potionsText.text = "Potions: " + player.potions;
			treasuresText.text = "Treasures: " + player.treasure;

			// Remaining turns
			int r = player.remainingTurns;
			step0Image.gameObject.SetActive(r > 0);
			step1Image.gameObject.SetActive(r > 1);
			step2Image.gameObject.SetActive(r > 2);
			step3Image.gameObject.SetActive(r > 3);
			stepDotsImage.gameObject.SetActive(r > 4);
		}

		// On game over screen
		if (gameOver.activeSelf) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				mainMenu.SetActive(false);
				ui.SetActive(true);
				gameOver.SetActive(false);
				GameManager.Instance.Restart();
			} else if (Input.GetKeyDown(KeyCode.Escape)) {
				Application.Quit();
			}
		}

		// On main menu screen
		if (mainMenu.activeSelf) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				mainMenu.SetActive(false);
				ui.SetActive(true);
				GameManager.Instance.GenerateLevel();
			} else if (Input.GetKeyDown(KeyCode.Escape)) {
				Application.Quit();
			}
		}
	}

	public void GameOver() {
		ui.SetActive(false);
		gameOver.SetActive(true);
	}
}
