using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subTitleText;

    public void SetItem(Item item) { 
        itemImage.sprite = item.imageFile;
        titleText.text = item.title;
        subTitleText.text = item.subTitle;
    }

    public static explicit operator Cell(GameObject v)
    {
        throw new NotImplementedException();
    }
}
