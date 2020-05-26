using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class WallRandom : MonoBehaviour {

	// I used gradient to easily adjust the percentages of walls with each style
	public Gradient gradient;
	public List<Sprite> sprites;

	void Start() {
		Color c = gradient.Evaluate(Random.Range(0.0f, 1.0f));
		Sprite s = sprites[0];
		if (c.Equals(Color.red))
			s = sprites[0];
		if (c.Equals(Color.green))
			s = sprites[1];
		if (c.Equals(Color.blue))
			s = sprites[2];
		if (c.Equals(new Color(1, 1, 0)))
			s = sprites[3];
		GetComponent<SpriteRenderer>().sprite = s;
	}
}
