using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ObjectPool))]
public class NewScrollviewController : MonoBehaviour
{
    [SerializeField] private Transform ContentTransform;
    [SerializeField] private const int cellHeight = 300;

    private ScrollRect _scrollRect;
    private RectTransform _rectTransform;
    private ObjectPool _objectPool;
    private List<Item> _items;
    private LinkedList<GameObject> _visibleCells = new LinkedList<GameObject> ();
    private Sprite[] _sprites;
    private float _lastScrollYValue = 1f;
    


    private void Awake()
    {
        _objectPool = GetComponent<ObjectPool>();
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = GetComponent<RectTransform>();
        _sprites = Resources.LoadAll<Sprite>("Items_12");
    }
    private void Start()
    {
        Init();
    }
    /// <summary>
    /// �ʿ��� ��ŭ �� ����
    /// </summary>
    private void Init()
    {
        LoadAllData();
    
        var contentSizeDelta = _scrollRect.content.sizeDelta;
        contentSizeDelta.y = _items.Count * cellHeight;
        _scrollRect.content.sizeDelta = contentSizeDelta;
        
        var (startIndex, endIndex) = GetVisibleIndexRange();
        var maxEndIndex = Mathf.Min(endIndex, _items.Count - 1);
        for (int i = startIndex; i < maxEndIndex; i++)
        {
            var obj = _objectPool.GetObject();
            _visibleCells.AddLast(obj);
            SetCellData(_visibleCells.Last.Value, i);
            _visibleCells.Last.Value.transform.localPosition = new Vector3(0, -i*cellHeight,0);
        }

    }
    /// <summary>
    /// ȭ�鿡 ���
    /// </summary>
    private void SetCellData(GameObject cellObject, int imgIndex)
    {
        cellObject.GetComponent<Cell>().SetItem(_items[imgIndex],imgIndex);
    }
    private void LoadAllData()
    {
        _items = new List<Item>();
        for (int i = 0; i < 54; i++)
        {
            _items.Add(new Item { imageFile = _sprites[i], title = $"Title {i + 1}", subTitle = $"Subtitle {i + 1}" });
        }
    }

    /// <summary>
    /// ���� ������ Cell �ε����� ��ȯ�ϴ� �޼���
    /// </summary>
    /// <returns>���� ���� ǥ�õ� Cell �ε���, ���� �Ʒ��� ǥ�õ� Cell �ε���</returns>
    private (int startIndex, int endIndex) GetVisibleIndexRange() {
        var visibleRect = new Rect(
                _scrollRect.content.anchoredPosition.x,
                _scrollRect.content.anchoredPosition.y,
                _rectTransform.rect.width,
                _rectTransform.rect.height
            );
        //��ũ�� ��ġ�� ���� ���� �ε��� ���
        var startIndex = Mathf.FloorToInt(visibleRect.y / cellHeight);

        //ȭ�鿡 ���̰� �� Cell ���� ���
        int visibleCount = Mathf.CeilToInt(visibleRect.height / cellHeight);

        //���� �߰�
        startIndex = Mathf.Max(0, startIndex - 1); // startIndex�� 0���� ũ�� startIndex -1, �ƴϸ� 0
        visibleCount += 2;

        return (startIndex,startIndex + visibleCount-1);
    }

    /// <summary>
    ///  Ư�� ��Ȳ���� ȭ�鿡 ������ �� �ִ��� �ε����� �Ǵ��ϴ� �޼ҵ�
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool IsVisibleIndex(int index)
    {
        var (startIndex, endIndex) = GetVisibleIndexRange();
        endIndex = Mathf.Min(endIndex, _items.Count - 1);
        return startIndex <= index && index <= endIndex;
    }
    public void OnValueChanged(Vector2 value) {
        if (_lastScrollYValue < value.y) {
            //�ö󰡴� ��
            var firstCell = _visibleCells.First.Value.GetComponent<Cell>();
            var newFirstIndex = firstCell.Index - 1;
            if (IsVisibleIndex(newFirstIndex)) {
                var cell = ObjectPool.Instance.GetObject().GetComponent<Cell>();
                SetCellData(cell.gameObject, newFirstIndex);
                cell.transform.localPosition = new Vector3(0, -newFirstIndex * cellHeight, 0);
                _visibleCells.AddFirst(cell.gameObject);
            }
            var lastCell = _visibleCells.Last.Value.GetComponent<Cell>();

            if (!IsVisibleIndex(lastCell.Index)){ 
                ObjectPool.Instance.ReturnObject(lastCell.gameObject);
                _visibleCells.RemoveLast();
            }
        }
        else if((_lastScrollYValue > value.y)) {
            //�������� ��
            var LastCell = _visibleCells.Last.Value.GetComponent<Cell>();
            var newLastIndex = LastCell.Index + 1;
            if (IsVisibleIndex(newLastIndex))
            {
                var cell = ObjectPool.Instance.GetObject().GetComponent<Cell>();
                SetCellData(cell.gameObject, newLastIndex);
                cell.transform.localPosition = new Vector3(0, -newLastIndex * cellHeight, 0);
                _visibleCells.AddLast(cell.gameObject);
            }

            var firstCell = _visibleCells.First.Value.GetComponent<Cell>();
            if (!IsVisibleIndex(firstCell.Index))
            {
                ObjectPool.Instance.ReturnObject(firstCell.gameObject);
                _visibleCells.RemoveFirst();
            }
        }
        _lastScrollYValue = value.y;
    }
   
}
