using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DBD.NetworkTime
{
    public abstract class BaseNetworkTimeManager<INSTANCE> : MonoBehaviour
    {
        public static INSTANCE Instance { get; private set; }

        [SerializeField] private bool isUseLocalTime;

        public bool IsInit { get; private set; }

        private int countRequest;

        private DateTimeOffset dateTimeOffset;
        private DateTimeOffset startDateTimeOffset;

        private double realTimeSinceStartup = 0;

        private readonly DateTime timeStart = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // private string currentDateTimeString = "";

        private readonly List<string> ntps = new()
        {
            "pool.ntp.org",
            "time.google.com",
            "time.cloudflare.com",
            "time.windows.com",
        };

        public static event Action OnCompleted;

        #region Singleton

#if UNITY_EDITOR
        protected virtual void Reset()
        {
        }
#endif

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = GetComponent<INSTANCE>();

                Transform root = transform.root;
                if (root != transform)
                {
                    DontDestroyOnLoad(root);
                }
                else
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            if (!IsInit)
            {
                return;
            }

            dateTimeOffset = startDateTimeOffset.AddSeconds(Time.realtimeSinceStartupAsDouble - realTimeSinceStartup);

            // string dateTimeString = GetDateTimeString(GetDateTime());
            // if (currentDateTimeString == dateTimeString) return;
            // currentDateTimeString = dateTimeString;
            // OnGetNetworkTimeComplete?.Invoke();
        }

        protected virtual void FixedUpdate()
        {
        }

        public void GetNetworkTime()
        {
            if (Application.internetReachability != NetworkReachability.NotReachable && !isUseLocalTime)
            {
                RequestNetworkTime();
            }
            else
            {
                Invoke(nameof(GetTimeOffline), 0.2f);
            }

            StartCoroutine(TimeOutRequest());
        }

        private IEnumerator TimeOutRequest()
        {
            yield return new WaitForSeconds(10);
            GetTimeOffline();
        }

        private void RequestNetworkTime()
        {
            if (IsInit) return;

            try
            {
                NtpClient client = new(ntps[countRequest]);
                using (client)
                {
                    DateTime dt = client.GetNetworkTime();
                    Init(dt);
                }
            }
            catch (Exception)
            {
                countRequest++;
                Invoke(nameof(RequestNetworkTime), 1);
            }
        }

        private void GetTimeOffline()
        {
            Init(DateTime.UtcNow);
        }

        private void Init(DateTime dateTime)
        {
            StopAllCoroutines();
            // double seconds = GetTotalSeconds(dateTime);
            // startDateTimeOffset = dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)seconds);
            realTimeSinceStartup = Time.realtimeSinceStartupAsDouble;

            startDateTimeOffset = dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
            IsInit = true;
            OnCompleted?.Invoke();
        }

        public virtual int GetTotalDays(DateTime dateTime)
        {
            return (int)(dateTime - timeStart).TotalDays;
        }

        public virtual double GetTotalSeconds(DateTime dateTime)
        {
            return (dateTime - timeStart).TotalSeconds;
        }

        public virtual int GetTotalDays(TimeType type = TimeType.LOCAL)
        {
            return (int)(GetDateTime(type) - timeStart).TotalDays;
        }

        public virtual long GetTotalSeconds()
        {
            // return (GetDateTime() - timeStart).TotalSeconds;
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        public virtual DateTime GetDateTime(TimeType type = TimeType.LOCAL)
        {
            return type == TimeType.UTC ? dateTimeOffset.DateTime : dateTimeOffset.LocalDateTime;
        }

        public virtual int GetDayOfWeek(TimeType type = TimeType.LOCAL)
        {
            return (int)GetDateTime(type).DayOfWeek;
        }

        public virtual int GetDayOfMonth(TimeType type = TimeType.LOCAL)
        {
            return GetDateTime(type).Day;
        }

        public virtual int GetDayOfYear(TimeType type = TimeType.LOCAL)
        {
            return GetDateTime(type).DayOfYear;
        }

        public virtual int GetWeekOfYear(TimeType type = TimeType.LOCAL, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Calendar calendar = cultureInfo.Calendar;
            int weekOfYear = calendar.GetWeekOfYear(GetDateTime(type), cultureInfo.DateTimeFormat.CalendarWeekRule,
                firstDayOfWeek);
            return weekOfYear;
        }

        public virtual int GetMonthInYear(TimeType type = TimeType.LOCAL)
        {
            return GetDateTime(type).Month;
        }

        private string GetDateTimeString(DateTime dateTime)
        {
            return dateTime.ToString("o");
        }
    }
}