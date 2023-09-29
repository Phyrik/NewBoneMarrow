using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomingLoadingScreenBehaviour : MonoBehaviour
{
    public RectTransform leftImage;
    public RectTransform rightImage;
    public RectTransform topImage;
    public RectTransform bottomImage;
    public RectTransform mainCharacterWindow;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        topImage.offsetMin = new Vector2(topImage.offsetMin.x,
            360f + (mainCharacterWindow.anchoredPosition.y + mainCharacterWindow.rect.height / 2f));
        rightImage.offsetMin = new Vector2(mainCharacterWindow.anchoredPosition.x + mainCharacterWindow.rect.width / 2f,
            rightImage.offsetMin.y);
        leftImage.offsetMax =
            new Vector2(-(480f - (mainCharacterWindow.anchoredPosition.x - mainCharacterWindow.rect.width / 2f)),
                leftImage.offsetMax.y);
        bottomImage.offsetMax = new Vector2(bottomImage.offsetMax.x,
            (mainCharacterWindow.anchoredPosition.y - mainCharacterWindow.rect.height / 2f));
    }
}
