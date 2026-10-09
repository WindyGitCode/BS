using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BS.UI
{
    /// <summary>
    /// UI 悬停/按下缩放效果。
    /// 挂在任意带 RectTransform 的 UI 对象上即可，无需改动面板代码。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverScaler : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("缩放")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float pressScale = 0.96f;
        [SerializeField] private float duration = 0.12f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        [Header("行为")]
        [Tooltip("游戏暂停(Time.timeScale=0)时是否仍然播放")]
        [SerializeField] private bool ignoreTimeScale = true;
        [Tooltip("按钮不可交互时不播放")]
        [SerializeField] private bool skipWhenNotInteractable = true;

        private RectTransform _rect;
        private Selectable _selectable;
        private Vector3 _baseScale;
        private Tween _tween;
        private bool _hovering;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _baseScale = _rect.localScale;      // 记录原始缩放，做到与预制体设置无关
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanPlay()) return;
            _hovering = true;
            Play(_baseScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            Play(_baseScale);                    // 离开一律还原，避免状态残留
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanPlay()) return;
            Play(_baseScale * pressScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CanPlay()) return;
            Play(_hovering ? _baseScale * hoverScale : _baseScale);
        }

        private bool CanPlay()
        {
            return !(skipWhenNotInteractable
                     && _selectable != null
                     && !_selectable.interactable);
        }

        private void Play(Vector3 target)
        {
            // 坑 1：先精准杀掉自己的上一个 tween。
            // 不要用 transform.DOKill()，那会误杀面板自身的淡入淡出等动画。
            _tween?.Kill();

            _tween = _rect.DOScale(target, duration)
    .SetEase(ease)
    .SetLink(gameObject)          // 保留：面板被 Destroy 时自动清理
    .SetUpdate(ignoreTimeScale);  // 保留：暂停时仍播放
        }

        private void OnDisable()
        {
            // 悬停状态下被隐藏时还原，避免下次显示还停留在放大状态
            _tween?.Kill();
            _tween = null;
            _hovering = false;
            if (_rect != null) _rect.localScale = _baseScale;
        }
    }
}