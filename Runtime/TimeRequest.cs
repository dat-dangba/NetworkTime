using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeRequest : MonoBehaviour
{
    private int countRequest;
    private bool isRequested;
    private bool isRaiseAction;

    private readonly List<string> ntps = new()
    {
        "pool.ntp.org",
        "time.google.com",
        "time.cloudflare.com",
        "time.windows.com",
    };

    public static event Action OnTimeRequestSuccess;

    public void Request(bool isUseLocalTime)
    {
        if (isRequested)
        {
            return;
        }

        countRequest = 0;
        if (Application.internetReachability != NetworkReachability.NotReachable
            && !isUseLocalTime)
        {
            RequestNetWorkTime();
        }
        else
        {
            Invoke(nameof(GetTimeOffline), 0.5f);
        }
    }

    private void GetTimeOffline()
    {
        InitTime(DateTime.UtcNow);
    }

    private void RequestNetWorkTime()
    {
        if (isRequested)
        {
            return;
        }

        try
        {
            NtpClient client = new(ntps[countRequest]);
            using (client)
            {
                //Get Time Online
                DateTime dt = client.GetNetworkTime();
                InitTime(dt);
            }
        }
        catch (Exception)
        {
            countRequest++;
            if (countRequest >= ntps.Count)
            {
                //Get Time Offline
                InitTime(DateTime.UtcNow);
            }
            else
            {
                Invoke(nameof(RequestNetWorkTime), 1);
            }
        }
    }

    private void InitTime(DateTime dateTime)
    {
        // TimeManager.Instance.Init(TimeManager.Instance.GetTotalSeconds(dateTime));
        isRequested = true;
        OnTimeRequestSuccess?.Invoke();
    }
}