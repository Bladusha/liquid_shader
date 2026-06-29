using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lab.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class HydrodynamicWaterButtonAnimator : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISelectHandler,
        IDeselectHandler
    {
        private const string DefaultWaveResourcePath = "UI/button_primary_water_wave_overlay";
        private const string ClipName = "WaterMotionClip";
        private const string WaveName = "WaterMotionWave";
        private const float PressedScale = 0.96f;
        private const float ClipInset = 3f;
        private const float WaveHorizontalBleed = 160f;

        [SerializeField] private Sprite waveSprite;
        [SerializeField] private string waveResourcePath = DefaultWaveResourcePath;
        [SerializeField, Min(0.1f)] private float waveImpulseDuration = 0.82f;
        [SerializeField, Min(0f)] private float hoverWaveTravel = 30f;
        [SerializeField, Min(0f)] private float pressWaveTravel = 48f;
        [SerializeField, Range(0f, 1f)] private float hoverWaveAlpha = 0.74f;
        [SerializeField, Range(0f, 1f)] private float pressWaveAlpha = 0.95f;
        [SerializeField, Min(0.01f)] private float returnSpeed = 9f;
        [SerializeField, Min(0.01f)] private float scaleSpeed = 14f;

        private Button button;
        private RectTransform rootRect;
        private RectTransform waveRect;
        private Image waveImage;
        private Vector3 baseScale;
        private float waveX;
        private float waveStartX;
        private float waveTargetX;
        private float waveTimer = 1f;
        private float waveAlpha;
        private WavePhase wavePhase = WavePhase.Idle;
        private bool hovered;
        private bool pressed;
        private bool selected;

        public static void EnsureOn(Button target)
        {
            if (target == null)
            {
                return;
            }

            Image targetImage = target.targetGraphic as Image;
            if (targetImage == null || targetImage.sprite == null)
            {
                return;
            }

            if (!targetImage.sprite.name.Contains("button_primary_water_frame"))
            {
                return;
            }

            HydrodynamicWaterButtonAnimator animator = target.GetComponent<HydrodynamicWaterButtonAnimator>();
            if (animator == null)
            {
                animator = target.gameObject.AddComponent<HydrodynamicWaterButtonAnimator>();
            }

            animator.EnsureVisuals();
        }

        public static void EnsureInChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                EnsureOn(buttons[i]);
            }
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            rootRect = transform as RectTransform;
            baseScale = transform.localScale;
            EnsureVisuals();
        }

        private void OnEnable()
        {
            if (rootRect == null)
            {
                rootRect = transform as RectTransform;
            }

            baseScale = transform.localScale;
            EnsureVisuals();
        }

        private void OnDisable()
        {
            hovered = false;
            pressed = false;
            selected = false;
            wavePhase = WavePhase.Idle;
            waveTimer = waveImpulseDuration;
            waveX = 0f;
            waveAlpha = 0f;

            if (waveRect != null)
            {
                waveRect.anchoredPosition = Vector2.zero;
            }

            if (waveImage != null)
            {
                SetWaveAlpha(0f);
            }

            transform.localScale = baseScale;
        }

        private void Update()
        {
            if (button == null || rootRect == null)
            {
                return;
            }

            EnsureVisuals();

            float dt = Time.unscaledDeltaTime;
            if (waveRect != null)
            {
                UpdateWaveMotion(dt);
            }

            float targetScale = pressed && button.interactable ? PressedScale : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale * targetScale, 1f - Mathf.Exp(-scaleSpeed * dt));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanAnimate())
            {
                return;
            }

            hovered = true;
            TriggerWave(hoverWaveTravel);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanAnimate())
            {
                return;
            }

            pressed = true;
            TriggerWave(pressWaveTravel);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!CanAnimate())
            {
                return;
            }

            selected = true;
            TriggerWave(hoverWaveTravel * 0.72f);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            pressed = false;
        }

        private bool CanAnimate()
        {
            return button != null && button.interactable && isActiveAndEnabled;
        }

        private void TriggerWave(float travel)
        {
            EnsureVisuals();
            waveStartX = waveX;
            waveTargetX = waveX + travel;
            waveTimer = 0f;
            wavePhase = WavePhase.Forward;
        }

        private void UpdateWaveMotion(float dt)
        {
            if (wavePhase == WavePhase.Forward)
            {
                waveTimer += dt;
                float t = Mathf.Clamp01(waveTimer / waveImpulseDuration);
                waveX = Mathf.LerpUnclamped(waveStartX, waveTargetX, EaseOutCubic(t));

                if (t >= 1f)
                {
                    StartReturnWave();
                }
            }
            else if (wavePhase == WavePhase.Return)
            {
                waveTimer += dt;
                float t = Mathf.Clamp01(waveTimer / waveImpulseDuration);
                waveX = Mathf.LerpUnclamped(waveStartX, waveTargetX, EaseOutCubic(t));

                if (t >= 1f)
                {
                    waveX = 0f;
                    wavePhase = WavePhase.Idle;
                }
            }
            else
            {
                waveX = 0f;
            }

            float targetAlpha = 0f;
            if (pressed && button.interactable)
            {
                targetAlpha = pressWaveAlpha;
            }
            else if ((hovered || selected) && button.interactable)
            {
                targetAlpha = hoverWaveAlpha;
            }

            waveAlpha = Mathf.Lerp(waveAlpha, targetAlpha, 1f - Mathf.Exp(-returnSpeed * dt));
            waveRect.anchoredPosition = new Vector2(waveX, pressed ? -1.5f : 0f);
            SetWaveAlpha(waveAlpha);
        }

        private void StartReturnWave()
        {
            waveStartX = waveX;
            waveTargetX = 0f;
            waveTimer = 0f;
            wavePhase = WavePhase.Return;
        }

        private void EnsureVisuals()
        {
            if (rootRect == null)
            {
                rootRect = transform as RectTransform;
            }

            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (rootRect == null || button == null)
            {
                return;
            }

            if (waveSprite == null && !string.IsNullOrWhiteSpace(waveResourcePath))
            {
                waveSprite = Resources.Load<Sprite>(waveResourcePath);
            }

            if (waveSprite == null)
            {
                return;
            }

            Image sourceImage = button.targetGraphic as Image;
            if (sourceImage == null || sourceImage.sprite == null)
            {
                return;
            }

            RectTransform clipRect = transform.Find(ClipName) as RectTransform;
            if (clipRect == null)
            {
                GameObject clipObject = new GameObject(ClipName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
                clipObject.transform.SetParent(transform, false);
                clipObject.transform.SetAsFirstSibling();
                clipRect = clipObject.GetComponent<RectTransform>();

                Image clipImage = clipObject.GetComponent<Image>();
                clipImage.raycastTarget = false;

                Mask mask = clipObject.GetComponent<Mask>();
                mask.showMaskGraphic = false;
            }

            Stretch(clipRect, 0f, ClipInset);

            Image maskImage = clipRect.GetComponent<Image>();
            if (maskImage != null)
            {
                maskImage.sprite = sourceImage.sprite;
                maskImage.type = sourceImage.type;
                maskImage.pixelsPerUnitMultiplier = sourceImage.pixelsPerUnitMultiplier;
                maskImage.raycastTarget = false;
                maskImage.color = Color.white;
            }

            if (waveRect == null)
            {
                waveRect = clipRect.Find(WaveName) as RectTransform;
            }

            if (waveRect == null)
            {
                GameObject waveObject = new GameObject(WaveName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                waveObject.transform.SetParent(clipRect, false);
                waveRect = waveObject.GetComponent<RectTransform>();
                waveImage = waveObject.GetComponent<Image>();
            }
            else if (waveImage == null)
            {
                waveImage = waveRect.GetComponent<Image>();
            }

            if (waveImage == null)
            {
                return;
            }

            Stretch(waveRect, WaveHorizontalBleed, 0f);
            waveImage.sprite = waveSprite;
            waveImage.type = Image.Type.Simple;
            waveImage.raycastTarget = false;
            waveImage.preserveAspect = false;
            SetWaveAlpha(waveAlpha);
        }

        private void SetWaveAlpha(float alpha)
        {
            if (waveImage == null)
            {
                return;
            }

            Color color = Color.white;
            color.a = alpha;
            waveImage.color = color;
        }

        private static void Stretch(RectTransform rect, float horizontalBleed, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset - horizontalBleed, inset);
            rect.offsetMax = new Vector2(horizontalBleed - inset, -inset);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static float EaseOutCubic(float t)
        {
            float inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }

        private enum WavePhase
        {
            Idle,
            Forward,
            Return
        }
    }
}
