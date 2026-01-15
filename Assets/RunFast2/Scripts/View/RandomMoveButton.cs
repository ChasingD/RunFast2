using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RunFast2.Scripts.View
{
    public class RandomMoveButton : MonoBehaviour
    {
        private enum MoveMode
        {
            Bouncing,   // DVD Logo 风格反弹
            Teleporting // 随机闪现
        }

        [Header("General Settings")]
        public float MinModeDuration = 3.0f; // 某种模式最少持续多久
        public float MaxModeDuration = 6.0f; // 某种模式最多持续多久

        [Header("Bouncing Settings (DVD Style)")]
        public float MoveSpeed = 300f; // 移动速度

        [Header("Teleport Settings")]
        public float TeleportInterval = 0.4f; // 闪现间隔时间

        // Runtime State
        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private MoveMode _currentMode;
        private float _modeTimer;
        private float _currentModeDuration;

        // Bouncing State
        private Vector2 _velocity;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;

        // Teleport State
        private float _teleportTimer;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (transform.parent != null)
            {
                _parentRect = transform.parent.GetComponent<RectTransform>();
            }
        }

        private void OnEnable()
        {
            // 每次激活时重置状态
            CalculateBounds();
            SwitchMode(MoveMode.Bouncing); // 默认先开始反弹
            
            // 给一个随机初始速度方向
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _velocity = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized * MoveSpeed;
        }

        private void Update()
        {
            if (_rectTransform == null || _parentRect == null) return;

            // 1. 模式切换计时
            _modeTimer += Time.deltaTime;
            if (_modeTimer >= _currentModeDuration)
            {
                // 切换到另一种模式
                MoveMode nextMode = _currentMode == MoveMode.Bouncing ? MoveMode.Teleporting : MoveMode.Bouncing;
                SwitchMode(nextMode);
            }

            // 2. 执行当前模式逻辑
            switch (_currentMode)
            {
                case MoveMode.Bouncing:
                    UpdateBouncing();
                    break;
                case MoveMode.Teleporting:
                    UpdateTeleporting();
                    break;
            }
        }

        private void SwitchMode(MoveMode newMode)
        {
            _currentMode = newMode;
            _modeTimer = 0f;
            _currentModeDuration = Random.Range(MinModeDuration, MaxModeDuration);
            
            // 切换模式时的初始化
            if (newMode == MoveMode.Teleporting)
            {
                _teleportTimer = TeleportInterval; // 立即触发一次闪现
            }
            else if (newMode == MoveMode.Bouncing)
            {
                // 确保速度不为0
                if (_velocity == Vector2.zero)
                {
                    _velocity = new Vector2(1, 1).normalized * MoveSpeed;
                }
            }
        }

        private void UpdateBouncing()
        {
            // 移动
            Vector2 pos = _rectTransform.anchoredPosition;
            pos += _velocity * Time.deltaTime;

            // 边界检测与反弹 (DVD Logo 逻辑)
            bool bounced = false;

            // X轴检测
            if (pos.x < _minPosition.x)
            {
                pos.x = _minPosition.x;
                _velocity.x = Mathf.Abs(_velocity.x); // 强制向右
                bounced = true;
            }
            else if (pos.x > _maxPosition.x)
            {
                pos.x = _maxPosition.x;
                _velocity.x = -Mathf.Abs(_velocity.x); // 强制向左
                bounced = true;
            }

            // Y轴检测
            if (pos.y < _minPosition.y)
            {
                pos.y = _minPosition.y;
                _velocity.y = Mathf.Abs(_velocity.y); // 强制向上
                bounced = true;
            }
            else if (pos.y > _maxPosition.y)
            {
                pos.y = _maxPosition.y;
                _velocity.y = -Mathf.Abs(_velocity.y); // 强制向下
                bounced = true;
            }

            _rectTransform.anchoredPosition = pos;
        }

        private void UpdateTeleporting()
        {
            _teleportTimer += Time.deltaTime;
            if (_teleportTimer >= TeleportInterval)
            {
                _teleportTimer = 0f;
                TeleportToRandomPosition();
            }
        }

        private void TeleportToRandomPosition()
        {
            float x = Random.Range(_minPosition.x, _maxPosition.x);
            float y = Random.Range(_minPosition.y, _maxPosition.y);
            _rectTransform.anchoredPosition = new Vector2(x, y);
        }

        private void CalculateBounds()
        {
            if (_parentRect == null || _rectTransform == null) return;

            // 获取父物体的矩形大小
            Rect parentRect = _parentRect.rect;
            // 获取按钮自身的矩形大小
            Rect btnRect = _rectTransform.rect;

            // 计算活动范围 (假设锚点在中心)
            // 最小X = 父物体左边界 + 按钮宽度的一半
            // 最大X = 父物体右边界 - 按钮宽度的一半
            // 这样按钮就不会超出父物体
            
            // 注意：anchoredPosition 是相对于锚点的。
            // 这里假设父物体和按钮的锚点都是 MiddleCenter (0.5, 0.5)
            // 如果不是，计算会复杂一些。为了通用性，我们基于 rect 的宽高计算相对位移。

            float halfBtnWidth = btnRect.width * 0.5f;
            float halfBtnHeight = btnRect.height * 0.5f;

            _minPosition = new Vector2(parentRect.xMin + halfBtnWidth, parentRect.yMin + halfBtnHeight);
            _maxPosition = new Vector2(parentRect.xMax - halfBtnWidth, parentRect.yMax - halfBtnHeight);
        }
    }
}