using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using static HSD_Utils;

public class TempNumberManager : MonoBehaviour
{
    private float currTime;
    TextMeshProUGUI textObj;

    void Awake() {
        textObj = GetComponent<TextMeshProUGUI>();
    }

    public void Init(string txt, Color col, float duration, bool fade) {
        textObj.text = txt;
        textObj.color = col;
        StartCoroutine(FadeOut(duration, fade));
    }

    private IEnumerator FadeOut(float duration, bool fade) {
        if (fade) {
            currTime = duration;
            while (currTime > 0) {
                textObj.color = ChangeColorAlpha(textObj.color, Mathf.Clamp01(currTime / duration));

                currTime -= Time.deltaTime;
                yield return null;
            }
        }
        else {
            yield return new WaitForSeconds(duration);
        }
        Destroy(gameObject);
    }
}
