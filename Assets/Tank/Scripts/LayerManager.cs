using UnityEngine;
using System.Collections;

public class LayerManager : MonoBehaviour {

	static public int blueLayer = 15;
	static public int goldLayer = 16;
	static public int whiteLayer = 17;

	static public LayerMask GetEnemyLayer(Team team) {
		LayerMask mask = 0;
		switch (team) {
		case Team.Blue:
			mask = (1 << whiteLayer) | (1 << goldLayer);
			break;
		case Team.Gold:
			mask = (1 << blueLayer) | (1 << whiteLayer);
			break;
		case Team.White:
			mask = (1 << blueLayer) | (1 << goldLayer);
			break;
		default:
			break;
		}


		return mask;
	}
}
