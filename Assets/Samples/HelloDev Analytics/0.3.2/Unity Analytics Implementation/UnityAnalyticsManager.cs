using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HelloDev.Analytics.Data;
using HelloDev.Logging;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;
using UnityEngine.SceneManagement;
using Logger = HelloDev.Logging.Logger;

namespace HelloDev.Analytics
{
    public class UnityAnalyticsManager : AnalyticsManagerBase
    {
        private const string ConsentShownKey = "HelloDev.Analytics.ConsentShown";
        private const string ConsentKey = "HelloDev.Analytics.Consent";
        private const string PlayerIdKey = "HelloDev.Analytics.PlayerId";

        #region Events

        public static Action ConsentShareData;
        public static Action ConsentRejectData;

        public static Action ShowConsentUI;
        public static Action OnConsentSharingData;
        public static event Action<IAnalytics, GameObject> InstanceCreated;

        #endregion

        private static UnityAnalyticsManager _instance;

        #region Life Cycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InstanceCreated?.Invoke(this, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            _instance = null;
        }

        public void Start()
        {
            if (Initialized) return;
            PlayerId = PlayerPrefs.GetString(PlayerIdKey, Application.identifier);
            EssentialData = new AnalyticsEssentialData
            {
                PlayerName = PlayerId,
                Language = Application.systemLanguage.ToString(),
                IsMultiplayer = false,
                PlayerLevel = 1,
                GameScene = SceneManager.GetActiveScene().name
            }.ToDictionary();

            if (PlayerPrefs.GetInt(ConsentShownKey, 0) == 1)
            {
                if (GetPlayerConsentStatus())
                    _ = InitializeIfNot(true);
                return;
            }

            ConsentShareData += OnConsentShareData;
            ConsentRejectData += OnConsentRejectData;
            ShowConsentUI?.Invoke();
        }

        #endregion

        #region Private Methods

        private async void OnConsentShareData()
        {
            try
            {
                SaveConsentStatus(true);
                PlayerPrefs.SetInt(ConsentShownKey, 1);
                PlayerPrefs.Save();

                await InitializeIfNot(true);

                ConsentShareData -= OnConsentShareData;
                ConsentRejectData -= OnConsentRejectData;
                OnConsentSharingData?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError(AnalyticsLogSystem, $"Failed to accept analytics consent: {e.Message}");
            }
        }

        private void OnConsentRejectData()
        {
            try
            {
                SaveConsentStatus(false);
                PlayerPrefs.SetInt(ConsentShownKey, 1);
                PlayerPrefs.Save();

                ConsentShareData -= OnConsentShareData;
                ConsentRejectData -= OnConsentRejectData;
            }
            catch (Exception e)
            {
                Logger.LogError(AnalyticsLogSystem, $"Failed to reject analytics consent: {e.Message}");
            }
        }

        #endregion

        #region SDK Initialization (unchanged)

        protected override async UniTask InitializeCore()
        {
            await UnityServices.InitializeAsync().AsUniTask();
        }

        public override bool CheckInitialized()
        {
            return UnityServices.State == ServicesInitializationState.Initialized;
        }

        protected override bool IsInitializing()
        {
            return UnityServices.State == ServicesInitializationState.Initializing;
        }

        #endregion

        #region SDK Events Hooks (unchanged)

        protected override void HookManagerEvents()
        {
        }

        protected override void UnHookManagerEvents()
        {
        }

        #endregion

        #region SDK Consent Storage

        protected override void SaveConsentStatus(bool consent)
        {
            PlayerPrefs.SetInt(ConsentKey, consent ? 1 : 0);
            PlayerPrefs.Save();
        }

        protected override bool LoadConsentStatus()
        {
            return PlayerPrefs.GetInt(ConsentKey, 0) == 1;
        }

        #endregion

        #region SDK Data Collection (unchanged)

        protected override void StartDataCollection(string playerId)
        {
            UnityServices.ExternalUserId = playerId;
            EndUserConsent.SetConsentState(new ConsentState
            {
                AnalyticsIntent = ConsentStatus.Granted
            });
        }

        protected override void StopDataCollection()
        {
            EndUserConsent.SetConsentState(new ConsentState
            {
                AnalyticsIntent = ConsentStatus.Denied
            });
        }

        protected override void RequestDataDeletion()
        {
            EndUserConsent.SetConsentState(new ConsentState
            {
                AnalyticsIntent = ConsentStatus.Denied
            });
            AnalyticsService.Instance.RequestDataDeletion();
        }

        #endregion

        #region SDK Record Event (unchanged)

        protected override void RecordEvent(string eventName, Dictionary<string, object> data)
        {
            var gameEvent = new CustomEvent(eventName);
            foreach (var kvp in data)
                gameEvent.Add(kvp.Key, kvp.Value);
            AnalyticsService.Instance.RecordEvent(gameEvent);
            AnalyticsService.Instance.Flush();
        }

        public void RecordEvent(AnalyticsCustomEvent customEvent)
        {
            foreach (var kvp in EssentialData) customEvent.Add(kvp.Key, kvp.Value);
            AnalyticsService.Instance.RecordEvent(customEvent);
            Logger.Log(AnalyticsLogSystem, FormatEventLog(customEvent.EventName, customEvent.InternalData));
            AnalyticsService.Instance.Flush();
        }

        #endregion
    }
}