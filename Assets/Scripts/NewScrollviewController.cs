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
    /// 필요한 만큼 셀 생성
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
    /// 화면에 출력
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
    /// 현재 보여질 Cell 인덱스를 반환하는 메서드
    /// </summary>
    /// <returns>가장 위에 표시될 Cell 인덱스, 가장 아래에 표시될 Cell 인덱스</returns>
    private (int startIndex, int endIndex) GetVisibleIndexRange() {
        var visibleRect = new Rect(
                _scrollRect.content.anchoredPosition.x,
                _scrollRect.content.anchoredPosition.y,
                _rectTransform.rect.width,
                _rectTransform.rect.height
            );
        //스크롤 위치에 따른 시작 인덱스 계산
        var startIndex = Mathf.FloorToInt(visibleRect.y / cellHeight);

        //화면에 보이게 될 Cell 개수 계산
        int visibleCount = Mathf.CeilToInt(visibleRect.height / cellHeight);

        //버퍼 추가
        startIndex = Mathf.Max(0, startIndex - 1); // startIndex가 0보다 크면 startIndex -1, 아니면 0
        visibleCount += 2;

        return (startIndex,startIndex + visibleCount-1);
    }

    /// <summary>
    ///  특정 상황에서 화면에 보여질 수 있는지 인덱스를 판단하는 메소드
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
            //올라가는 중
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
            //내려가는 중
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
