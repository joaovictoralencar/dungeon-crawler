using System.Collections;
using UnityEngine;
using DungeonCrawler.Core.Combat;
using INab.UI;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyHealth : Health
    {
        [SerializeField] ProceduralProgressBar _healthBar;

        Coroutine _hideHealthBar;

        private void Start()
        {
            // _healthBar.gameObject.SetActive(false);
            _healthBar.BarFill(CurrentHealth / MaxHealth);
        }

        private void OnEnable()
        {
            Damaged += OnDamaged;
            Died += OnDied;
        }

        private void OnDisable()
        {
            Damaged -= OnDamaged;
            Died -= OnDied;
        }

        private void OnDied()
        {
            if (_hideHealthBar != null)
                StopCoroutine(_hideHealthBar);
            _healthBar.gameObject.SetActive(false);
        }

        private void OnDamaged(float damage)
        {
            _healthBar.gameObject.SetActive(true);
            if (_hideHealthBar != null)
                StopCoroutine(_hideHealthBar);
            _hideHealthBar = StartCoroutine(HideHealthBar());

            _healthBar.BarLoss(damage / MaxHealth);
        }

        IEnumerator HideHealthBar()
        {
            yield return new WaitForSeconds(5f);
            _healthBar.gameObject.SetActive(false);
        }
    }
}