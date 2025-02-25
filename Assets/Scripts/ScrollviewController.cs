using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ObjectPool))]
public class NeScrollviewController : MonoBehaviour
{

    ///content에 content size filter를 설정했을 때의 코드입니다
    [SerializeField] private Transform ContentTransform;

    private ScrollRect _scrollRect;
    private RectTransform _rectTransform;
    private ObjectPool _objectPool;
    private List<Item> _items;
    private Sprite[] _sprites;
    private int nextImageIndex;
    private int beforeImageIndex;
    private float previousScrollPosition = 0f;
    private List<GameObject> _poolingObjects;
    private const int cellHeight = 300;



    private void Awake()
    {
        _objectPool = GetComponent<ObjectPool>();
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = GetComponent<RectTransform>();
        _sprites = Resources.LoadAll<Sprite>("Items_12");
        _poolingObjects = new List<GameObject>();

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
        for (int i = 0; i < 8; i++)
        {
            var obj = _objectPool.GetObject();
            _poolingObjects.Add(obj);
            SetCellData(_poolingObjects[i], i);
            nextImageIndex = i;
        }
        var contentSizeDelta = _scrollRect.content.sizeDelta;
        contentSizeDelta.y = _items.Count * cellHeight;
        _scrollRect.content.sizeDelta = contentSizeDelta;

    }
    /// <summary>
    /// 화면에 출력
    /// </summary>
    private void SetCellData(GameObject cellObject, int imgIndex)
    {
        cellObject.GetComponent<Cell>().SetItem(_items[imgIndex]);
    }
    private void LoadAllData()
    {
        /*  _items = new List<Item>{

              new Item{imageFile = _sprites[0], title = "Title 1", subTitle = "Subtitle 1"},
              new Item{imageFile = _sprites[1], title = "Title 2", subTitle = "Subtitle 2"},
              new Item{imageFile = _sprites[2], title = "Title 3", subTitle = "Subtitle 3"},
              new Item{imageFile = _sprites[3], title = "Title 4", subTitle = "Subtitle 4"}

          };*/
        _items = new List<Item>();
        for (int i = 0; i < 54; i++)
        {
            _items.Add(new Item { imageFile = _sprites[i], title = $"Title {i + 1}", subTitle = $"Subtitle {i + 1}" });
        }
    }

    public void OnValueChanged(Vector2 Value)
    {
        float currentScrollPosition = Value.y;
        var yPosition = _scrollRect.content.anchoredPosition.y;
        var totalHeight = _rectTransform.rect.height; ;

        if (currentScrollPosition < previousScrollPosition) 
        {
            if (Mathf.Abs(yPosition) > cellHeight)
            {
                if (nextImageIndex >= 52) { return; }
                _objectPool.ReturnObject(_poolingObjects[0]);
                _poolingObjects.RemoveAt(0);
                _scrollRect.content.anchoredPosition = Vector2.zero;
                var cellObject = _objectPool.GetObject();
                SetCellData(cellObject, ++nextImageIndex);
                beforeImageIndex = nextImageIndex - 8;
                cellObject.transform.SetAsLastSibling();
                _poolingObjects.Add(cellObject);
            }
        }
        else if (currentScrollPosition > previousScrollPosition)
        {
            if (yPosition < 0)
            {
                if (beforeImageIndex <= 0) { return; }
                var cellObject = _objectPool.GetObject();
                cellObject.transform.SetAsFirstSibling();
                SetCellData(cellObject, --beforeImageIndex);
                _scrollRect.content.anchoredPosition = new Vector2(0, cellHeight);
                _poolingObjects.Insert(0, cellObject);
                nextImageIndex = beforeImageIndex + 8;
                _objectPool.ReturnObject(_poolingObjects[_poolingObjects.Count - 1]);
                _poolingObjects.RemoveAt(_poolingObjects.Count - 1);
            }
        }
        previousScrollPosition = currentScrollPosition;
    }
}
