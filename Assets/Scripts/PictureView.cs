using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PictureView : MonoBehaviour
{
    [SerializeField] private Image imageFrame;
    [SerializeField] private RawImage rawImagePicture;
    [SerializeField] private GameObject buttonPrev;
    [SerializeField] private GameObject buttonNext;
    [SerializeField] private Image imageButtonPrev;
    [SerializeField] private Image imageButtonNext;
    [SerializeField] private Image imagePieceNumber;
    [SerializeField] private TextMeshProUGUI textMeshProPieceNumber;
    [SerializeField] private Image imageTitle;
    [SerializeField] private TextMeshProUGUI textMeshProTitle;
    [SerializeField] private TextMeshProUGUI textMeshProAuthor;

    private int _basePositionX;
    private bool _isActiveImageButtonPrev;
    private bool _isActiveImageButtonNext;

    public void Initialize(PictureInfo pictureInfo)
    {
        _basePositionX = (int) transform.localPosition.x;
        var imageFrameSizeX = pictureInfo.texture.width + 6;
        var imageFrameSizeY = pictureInfo.texture.height + 8;
        imageFrame.rectTransform.sizeDelta = new Vector2(imageFrameSizeX, imageFrameSizeY);
        rawImagePicture.rectTransform.sizeDelta = new Vector2(pictureInfo.texture.width, pictureInfo.texture.height);
        rawImagePicture.texture = pictureInfo.texture;
        var imageButtonPrevPositionX = pictureInfo.texture.width / -2 - 10;
        var imageButtonNextPositionX = pictureInfo.texture.width / 2 + 10;
        buttonPrev.transform.localPosition = new Vector3(imageButtonPrevPositionX, 0.0f, 0.0f);
        buttonNext.transform.localPosition = new Vector3(imageButtonNextPositionX, 0.0f, 0.0f);

        textMeshProPieceNumber.text = (pictureInfo.sizeX * pictureInfo.sizeY).ToString();
        textMeshProTitle.text = pictureInfo.title;
        textMeshProAuthor.text = pictureInfo.author;
    }

    public void SetActiveImageButtonPrev(bool isActive)
    {
        _isActiveImageButtonPrev = isActive;
    }

    public void SetActiveImageButtonNext(bool isActive)
    {
        _isActiveImageButtonNext = isActive;
    }

    public void ShowImageButtons()
    {
        if (_isActiveImageButtonPrev)
        {
            imageButtonPrev.DOFade(1, 0.2f);
            buttonPrev.gameObject.SetActive(true);
        }

        if (_isActiveImageButtonNext)
        {
            imageButtonNext.DOFade(1, 0.2f);
            buttonNext.gameObject.SetActive(true);
        }
    }

    public void HideImageButtons()
    {
        imageButtonPrev.DOFade(0, 0.2f).OnComplete(() => { buttonPrev.gameObject.SetActive(false); });
        imageButtonNext.DOFade(0, 0.2f).OnComplete(() => { buttonNext.gameObject.SetActive(false); });
    }

    public void EscapeToLeft()
    {
        transform.DOLocalMoveX(_basePositionX - 320, 2.0f, true).SetEase(Ease.InQuad);
    }

    public void EscapeToRight()
    {
        transform.DOLocalMoveX(_basePositionX + 320, 2.0f, true).SetEase(Ease.InQuad);
    }

    public void ReturnEscape()
    {
        transform.DOLocalMoveX(_basePositionX, 2.0f, true).SetEase(Ease.OutQuad);
    }

    public void ShowDetails()
    {
        imagePieceNumber.gameObject.SetActive(true);
        imagePieceNumber.rectTransform.DOAnchorPosY(-12, 0.3f);
        imageTitle.gameObject.SetActive(true);
        textMeshProAuthor.gameObject.SetActive(true);
    }

    public void HideDetails()
    {
        imagePieceNumber.rectTransform.DOAnchorPosY(14, 0.3f).OnComplete(() =>
        {
            imagePieceNumber.gameObject.SetActive(false);
        });
        imageTitle.gameObject.SetActive(false);
        textMeshProAuthor.gameObject.SetActive(false);
    }
    
    public void Focus(float duration)
    {
        imageFrame.transform.DOLocalMoveY(8.0f, duration, true).SetEase(Ease.OutBack);
    }

    public void ReturnFocus(float duration)
    {
        imageFrame.transform.DOLocalMoveY(22.0f, duration, true).SetEase(Ease.InBack);
    }

    public void ClearPicture()
    {
        
        var texture2d = new Texture2D(rawImagePicture.texture.width, rawImagePicture.texture.height,
            TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point
        };
        var pixels = texture2d.GetPixels();
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0.1f, 0.125f, 0.125f);
        }

        texture2d.SetPixels(pixels);
        texture2d.Apply();
        rawImagePicture.texture = texture2d;
        

        //rawImagePicture.color = new Color(0, 0, 0, 0);
    }

    public void DrawPictureBySaveData(PictureInfo picture, PicturePieceSaveInfo saveData)
    {
        var texture2d = new Texture2D(rawImagePicture.texture.width, rawImagePicture.texture.height,
            TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point
        };
        for (var i = 0; i < saveData.pieceMap.Length; i++)
        {
            var x = i % picture.texture.width;
            var y = i / picture.texture.width;
            if (saveData.pieces[saveData.pieceMap[i]].belongCode == 2)
            {
                texture2d.SetPixel(x, y, picture.texture.GetPixel(x, y));
            }
            else
            {
                texture2d.SetPixel(x, y, new Color(0.1f, 0.125f, 0.125f));
            }
        }
        
        texture2d.Apply();
        rawImagePicture.texture = texture2d;
    }

    public void DrawPicture(PictureInfo picture)
    {
        rawImagePicture.texture = picture.texture;
    }
}

