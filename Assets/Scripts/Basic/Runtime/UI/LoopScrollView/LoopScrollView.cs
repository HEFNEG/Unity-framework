using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Cooper.UI {


    [RequireComponent(typeof(ScrollRect))]
    public class LoopScrollView : UIElement {
        private RectTransform Content => scrollView.content;
        [SerializeField] private ScrollRect scrollView;

        private int preCursor;
        public int PreCursor => preCursor;

        private int lastCursor;
        public int LastCursor => lastCursor;

        private Vector2 minOffset;
        public Vector2 MinOffset => minOffset;
        private Vector2 maxOffset;
        public Vector2 MaxOffset => MaxOffset;


        protected Vector2 preAnchorPostion;


        [SerializeField] private Align alignment;
        [SerializeField] private Vector2 space;
        public Vector2 Space => space;
        [SerializeField] private Vector2 padding;
        public Vector2 Padding => padding;

        public Align Alignment => alignment;

        private IList dataArray;
        private List<ScrollItem> tempDataArray;
        private List<ScrollItem> itemList;

        private void Awake() {
            itemList = new List<ScrollItem>();
            scrollView = GetComponent<ScrollRect>();
            tempDataArray = new List<ScrollItem>();
        }

        private void OnEnable() {
            minOffset = (Vector2.zero - RectTransform.pivot) * RectTransform.sizeDelta;
            maxOffset = (Vector2.one - RectTransform.pivot) * RectTransform.sizeDelta;
        }

        private void Start() {
            scrollView.horizontal = alignment == Align.Horizontal;
            scrollView.vertical = alignment == Align.Vertical;

            Content.anchorMin = new Vector2(0f, 1f);
            Content.anchorMax = new Vector2(0f, 1f);
            Content.pivot = new Vector2(0f, 1f);

            var tempData = new List<int>(100);
            for(int i = 0; i < 100; i++) {
                tempData.Add(i);
            }
            SetData(tempData);
        }

        private void Update() {
            if(preAnchorPostion != Content.anchoredPosition) {
                var deltaPos = Content.anchoredPosition - preAnchorPostion;
                preAnchorPostion = Content.anchoredPosition;

                switch(alignment) {
                    case Align.Vertical:
                        if(deltaPos.y > 0) {
                            TryMoveNext();
                        }else if(deltaPos.y < 0) {
                            TryMoveLast();
                        }
                        break;
                    case Align.Horizontal:
                        if(deltaPos.x < 0) {
                            TryMoveNext();
                        } else if(deltaPos.x > 0) {
                            TryMoveLast();
                        }
                        break;
                }
            }
        }

        public void SetData<T>(List<T> list) {
            dataArray = list;

            ReArrange();
        }

        private void ElementMatch() {
            itemList.Clear();
            int childCount = Content.childCount;
            for(int i = 0; i < childCount; i++) {
                var child = Content.GetChild(i);
                var childRectTransform = child as RectTransform;
                childRectTransform.pivot = new Vector2(0f, 1f);
                childRectTransform.anchorMax = new Vector2(0f, 1f);
                childRectTransform.anchorMin = new Vector2(0f, 1f);

                itemList.Add(child.GetComponent<ScrollItem>());
            }
        }

        [ContextMenu("ReArrange")]
        protected void ReArrange() {
            Content.anchoredPosition = Vector2.zero;
            if(dataArray == null) {
                // 生成临时测试数据
                dataArray = new List<int>();
                for(int i = 0; i < 100; i++) {
                    dataArray.Add(i);
                }
            }

            ElementMatch();
            // 计算 Content 总长度
            CalculateContentSize();
            preCursor = lastCursor = 0;
            Arrange();
        }

        protected void Arrange() {
            lastCursor = preCursor;
            var offset = GetItemOffset(preCursor);

            switch(alignment) {
                case Align.Vertical:
                    offset.x = 0f;
                    offset += new Vector2(padding.x, -padding.y);
                    for(int i = 0; i < itemList.Count; i++) {
                        var item = itemList[i];
                        item.RectTransform.anchoredPosition = offset;

                        if(dataArray != null && lastCursor < dataArray.Count) {
                            item.gameObject.SetActiveEx(true);
                            item.Show(dataArray[lastCursor++]);
                        } else {
                            item.gameObject.SetActiveEx(false);
                        }

                        offset.y -= itemList[i].RectTransform.sizeDelta.y + Space.y;
                    }
                    break;
                case Align.Horizontal:
                    offset.x = 0f;
                    offset += new Vector2(padding.x, -padding.y);
                    for(int i = 0; i < itemList.Count; i++) {
                        var item = itemList[i];
                        item.RectTransform.anchoredPosition = offset;

                        if(dataArray != null && lastCursor < dataArray.Count) {
                            item.gameObject.SetActiveEx(true);
                            item.Show(dataArray[lastCursor++]);
                        } else {
                            item.gameObject.SetActiveEx(false);
                        }

                        offset.x += itemList[i].RectTransform.sizeDelta.x + Space.x;
                    }
                    break;
            }
        }

        private void CalculateContentSize() {
            Vector2 contentSize = Vector2.zero;
            for(int i = 0; i < dataArray.Count; i++) {
                contentSize += GetItemSize(i) + Space;
            }

            switch(alignment) {
                case Align.Vertical:
                    Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentSize.y);
                    break;
                case Align.Horizontal:
                    Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentSize.x);
                    break;
            }
        }

        private void TryMoveNext() {
            tempDataArray.Clear();
            for(int i = 0; i < itemList.Count; i++) {
                tempDataArray.Add(itemList[i]);
            }

            for(int i = 0; i < tempDataArray.Count; i++) {
                var item = tempDataArray[i];
                var minPosition = item.RectTransform.GetMinPositionInRectTransform(RectTransform);
                var maxPosition = item.RectTransform.GetMaxPositionInRectTransform(RectTransform);
                bool able;
                switch(alignment) {
                    case Align.Vertical:
                        able = minPosition.y > maxOffset.y && lastCursor < dataArray.Count;
                        if(able) {
                            itemList.Remove(item);
                            var lastItemRect = itemList[^1].RectTransform;
                            item.RectTransform.anchoredPosition = lastItemRect.anchoredPosition - new Vector2(0f, itemList[^1].SizeDelta.y + Space.y);
                            item.Show(dataArray[lastCursor++]);
                            preCursor++;
                            itemList.Add(item);
                        }
                        break;
                    case Align.Horizontal:
                        able = maxPosition.x < minOffset.x && lastCursor < dataArray.Count;
                        if(able) {
                            itemList.Remove(item);
                            var lastItemRect = itemList[^1].RectTransform;
                            item.RectTransform.anchoredPosition = lastItemRect.anchoredPosition + new Vector2(itemList[^1].SizeDelta.x + Space.x, 0f);
                            item.Show(dataArray[lastCursor++]);
                            preCursor++;
                            itemList.Add(item);
                        }
                        break;
                }
            }
        }

        private void TryMoveLast() {
            tempDataArray.Clear();
            for(int i = 0; i < itemList.Count; i++) {
                tempDataArray.Add(itemList[i]);
            }

            for(int i = tempDataArray.Count - 1; i >= 0; i--) {
                var item = tempDataArray[i];
                var minPosition = item.RectTransform.GetMinPositionInRectTransform(RectTransform);
                var maxPosition = item.RectTransform.GetMaxPositionInRectTransform(RectTransform);
                bool able;
                switch(alignment) {
                    case Align.Vertical:
                        able = maxPosition.y < minOffset.y && preCursor > 0;
                        if(able) {
                            itemList.Remove(item);
                            var firstItemRect = itemList[0].RectTransform;
                            item.RectTransform.anchoredPosition = firstItemRect.anchoredPosition + new Vector2(0f, item.SizeDelta.y + Space.y);
                            item.Show(dataArray[--preCursor]);
                            --lastCursor;
                            itemList.Insert(0, item);
                        }
                        break;
                    case Align.Horizontal:
                        able = minPosition.x > maxOffset.x && preCursor > 0;
                        if(able) {
                            itemList.Remove(item);
                            var firstItemRect = itemList[0].RectTransform;
                            item.RectTransform.anchoredPosition = firstItemRect.anchoredPosition - new Vector2(item.SizeDelta.x + Space.x, 0f);
                            item.Show(dataArray[--preCursor]);
                            --lastCursor;
                            itemList.Insert(0, item);
                        }
                        break;
                }
            }
        }

        protected virtual Vector2 GetItemSize(int index) {
            return itemList[0].SizeDelta;
        }

        protected virtual Vector2 GetItemOffset(int index) {
            return index * itemList[0].SizeDelta;
        }

        public enum Align {
            Horizontal,
            Vertical,
        }
    }
}
