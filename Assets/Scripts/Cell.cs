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

    public int Index { get; private set; }
    public void SetItem(Item item,int index = 0) { 
        itemImage.sprite = item.imageFile;
        titleText.text = item.title;
        subTitleText.text = item.subTitle;
        Index = index; 
    }

    public static explicit operator Cell(GameObject v)
    {
        throw new NotImplementedException();
    }
}
