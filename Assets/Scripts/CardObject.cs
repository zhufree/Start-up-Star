using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardObject : MonoBehaviour
{
    public Card cardItem;
    public Image cardImage;
    public TMP_Text cardName;
    public TMP_Text cardDesc;
    public bool selected;

    public void SetItem(Card item) {
        if (item != null) {
            cardItem = item;
            cardImage.sprite = Resources.Load<Sprite>(item.image);
            cardName.text = item.name;
            cardDesc.text = item.description.ToString();
        } else {
            cardImage.sprite = null;
            cardName.text = "";
            cardDesc.text = "";
        }
    }

    public void ItemOnClicked() {
      if (selected) {
        BattleController.Instance.UseCard(this);
      } else {
        selected = true;
      }
    }
}