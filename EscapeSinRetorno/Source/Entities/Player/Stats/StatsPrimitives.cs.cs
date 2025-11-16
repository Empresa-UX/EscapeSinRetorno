using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EscapeSinRetorno.Source.Systems.Stats
{
    public enum DamageType { Physical, Poison, Bleed, True }

    public readonly struct DamageRequest
    {
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly bool IgnoresArmor;

        public DamageRequest(float amount, DamageType type = DamageType.Physical, bool ignoresArmor = false)
        {
            Amount = MathF.Max(0, amount);
            Type = type;
            IgnoresArmor = ignoresArmor;
        }
    }

    public sealed class Stat
    {
        public float Current { get; private set; }
        public float Max { get; private set; }
        public float RegenPerSec { get; set; }
        public float Min { get; set; } = 0f;

        public Stat(float max, float current, float regenPerSec = 0f)
        {
            Max = MathF.Max(1f, max);
            Current = MathHelper.Clamp(current, Min, Max);
            RegenPerSec = regenPerSec;
        }

        public void SetMax(float max, bool keepRatio = true)
        {
            max = MathF.Max(1f, max);
            if (keepRatio)
            {
                var r = Max <= 0f ? 0f : Current / Max;
                Max = max;
                Current = MathHelper.Clamp(max * r, Min, Max);
            }
            else
            {
                Max = max;
                Current = MathHelper.Clamp(Current, Min, Max);
            }
        }

        public void Set(float v) => Current = MathHelper.Clamp(v, Min, Max);
        public void Add(float v) => Set(Current + v);
        public void Sub(float v) => Set(Current - v);

        public void Update(float dt)
        {
            if (Math.Abs(RegenPerSec) > 0.0001f)
                Add(RegenPerSec * dt);
        }

        public float Ratio => Max <= 0f ? 0f : Current / Max;
        public bool IsZero => Current <= Min + 0.0001f;
        public bool IsFull => Current >= Max - 0.0001f;
        public bool IsLow(float pct) => Ratio <= pct;
    }

    public struct VisualEffectState
    {
        public float VignetteIntensity;   // 0..1
        public float DistortionIntensity; // 0..1
        public float Desaturate;          // 0..1
        public bool IsDead;
    }

    public sealed class StatsConfig
    {
        public float HealthMax { get; set; } = 100f;
        public float HealthRegen { get; set; } = 0.6f;

        public float StaminaMax { get; set; } = 120f;
        public float StaminaRegen { get; set; } = 15f;

        public float HungerMax { get; set; } = 100f;
        public float HungerDrainPerSec { get; set; } = 0.6f;

        public float ThirstMax { get; set; } = 100f;
        public float ThirstDrainPerSec { get; set; } = 1.0f;

        public float SanityMax { get; set; } = 100f;
        public float SanityDrainPerSec { get; set; } = 0.18f;

        public float ArmorPercent { get; set; } = 0.2f;

        public float HungerZeroHealthDps { get; set; } = 2.0f;
        public float ThirstZeroHealthDps { get; set; } = 3.0f;

        public float LowHealthPct { get; set; } = 0.25f;
        public float LowSanityPct { get; set; } = 0.2f;

        public float StaminaExhaustionPct { get; set; } = 0.10f; // umbral visible de agotamiento
    }

    public interface IStatusEffect
    {
        string Id { get; }
        bool IsFinished { get; }
        void OnApply(PlayerStats stats);
        void Update(PlayerStats stats, float dt);
        void OnRemove(PlayerStats stats);
    }

    public sealed class BleedingEffect : IStatusEffect
    {
        public string Id => "bleeding";

        private float _tickTimer;
        private readonly float _tickInterval;
        private readonly float _damagePerTick;
        private float _duration;

        public bool IsFinished => _duration <= 0f;

        public BleedingEffect(float duration = 12f, float tickInterval = 1.5f, float dmgPerTick = 2f)
        {
            _duration = duration;
            _tickInterval = MathF.Max(0.1f, tickInterval);
            _damagePerTick = MathF.Max(0f, dmgPerTick);
        }

        public void OnApply(PlayerStats stats) { }

        public void Update(PlayerStats stats, float dt)
        {
            _duration -= dt;
            _tickTimer += dt;

            if (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                stats.ApplyDamage(new DamageRequest(_damagePerTick, DamageType.Bleed, true));
            }
        }

        public void OnRemove(PlayerStats stats) { }
    }

    public sealed class PoisonEffect : IStatusEffect
    {
        public string Id => "poison";

        private float _timer;
        private readonly float _duration;
        private readonly float _dps;

        public bool IsFinished => _timer >= _duration;

        public PoisonEffect(float duration = 10f, float dps = 1.25f)
        {
            _duration = duration;
            _dps = MathF.Max(0f, dps);
        }

        public void OnApply(PlayerStats stats) { }

        public void Update(PlayerStats stats, float dt)
        {
            _timer += dt;
            stats.ApplyDamage(new DamageRequest(_dps * dt, DamageType.Poison, true));
            stats.HealthRegenBlocked = true; // veneno bloquea curación natural
        }

        public void OnRemove(PlayerStats stats)
        {
            stats.HealthRegenBlocked = false;
        }
    }

    public sealed class RegenerationEffect : IStatusEffect
    {
        public string Id => "regen";

        private float _timer;
        private readonly float _duration;
        private readonly float _bonusRegen;

        public bool IsFinished => _timer >= _duration;

        public RegenerationEffect(float duration = 6f, float bonusRegen = 2.5f)
        {
            _duration = duration;
            _bonusRegen = bonusRegen;
        }

        public void OnApply(PlayerStats stats)
        {
            stats.TempHealthRegenBonus += _bonusRegen;
        }

        public void Update(PlayerStats stats, float dt)
        {
            _timer += dt;
        }

        public void OnRemove(PlayerStats stats)
        {
            stats.TempHealthRegenBonus -= _bonusRegen;
        }
    }

    public sealed class PlayerStats
    {
        public readonly Stat Health;
        public readonly Stat Stamina;
        public readonly Stat Hunger;
        public readonly Stat Thirst;
        public readonly Stat Sanity;

        public float ArmorPercent { get; private set; }

        public bool IsDead { get; private set; }
        public bool HealthRegenBlocked { get; set; }
        public float TempHealthRegenBonus { get; set; }

        // 🔹 MODO DIOS (para comandos de chat)
        public bool DebugGodMode { get; set; } = false;

        private readonly StatsConfig _cfg;
        private readonly Dictionary<string, IStatusEffect> _effects = new();

        public event Action OnDeath;
        public event Action<float> OnHealthChanged;
        public event Action<string> OnEffectApplied;
        public event Action<string> OnEffectRemoved;

        public PlayerStats(StatsConfig cfg = null)
        {
            _cfg = cfg ?? new StatsConfig();

            Health = new Stat(_cfg.HealthMax, _cfg.HealthMax, _cfg.HealthRegen);
            Stamina = new Stat(_cfg.StaminaMax, _cfg.StaminaMax, _cfg.StaminaRegen);
            Hunger = new Stat(_cfg.HungerMax, _cfg.HungerMax, -_cfg.HungerDrainPerSec);
            Thirst = new Stat(_cfg.ThirstMax, _cfg.ThirstMax, -_cfg.ThirstDrainPerSec);
            Sanity = new Stat(_cfg.SanityMax, _cfg.SanityMax, -_cfg.SanityDrainPerSec);

            ArmorPercent = MathHelper.Clamp(_cfg.ArmorPercent, 0f, 0.9f);
        }

        public void Tick(float dt, bool isSprinting, out VisualEffectState fx)
        {
            // Si ya está muerto, no seguir drenando ni aplicando efectos.
            if (IsDead)
            {
                fx = new VisualEffectState
                {
                    IsDead = true,
                    VignetteIntensity = 1f,
                    DistortionIntensity = 0f,
                    Desaturate = 0f
                };
                return;
            }

            float prevHealth = Health.Current;

            // Salud: regen (bloqueada por veneno)
            float baseRegen = Health.RegenPerSec;
            Health.RegenPerSec = HealthRegenBlocked ? 0f : _cfg.HealthRegen + TempHealthRegenBonus;
            Health.Update(dt);
            Health.RegenPerSec = baseRegen;

            // Estamina: SOLO drena corriendo (sprint real)
            if (isSprinting && Stamina.Current > 0.5f)
                Stamina.Sub(25f * dt);
            else
                Stamina.Update(dt);

            // Supervivencia
            Hunger.Update(dt);
            Thirst.Update(dt);
            Sanity.Update(dt);

            if (Hunger.IsZero)
                ApplyDamage(new DamageRequest(_cfg.HungerZeroHealthDps * dt, DamageType.True, true));

            if (Thirst.IsZero)
                ApplyDamage(new DamageRequest(_cfg.ThirstZeroHealthDps * dt, DamageType.True, true));

            // Efectos
            if (_effects.Count > 0)
            {
                var list = new List<IStatusEffect>(_effects.Values);
                foreach (var e in list)
                {
                    e.Update(this, dt);
                    if (e.IsFinished)
                        RemoveEffect(e.Id);
                }
            }

            if (!IsDead && Health.IsZero)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }

            if (Math.Abs(prevHealth - Health.Current) > 0.0001f)
                OnHealthChanged?.Invoke(Health.Current);

            fx = new VisualEffectState
            {
                IsDead = IsDead,
                VignetteIntensity = Health.IsLow(_cfg.LowHealthPct)
                    ? MathHelper.Lerp(0.2f, 1f, 1f - Health.Ratio / _cfg.LowHealthPct)
                    : 0f,
                DistortionIntensity = Sanity.IsLow(_cfg.LowSanityPct)
                    ? MathHelper.Lerp(0.1f, 0.85f, 1f - Sanity.Ratio / _cfg.LowSanityPct)
                    : 0f,
                Desaturate = _effects.ContainsKey("poison") ? 0.35f : 0f
            };
        }

        public void ApplyDamage(DamageRequest req)
        {
            if (IsDead || req.Amount <= 0f || DebugGodMode)
                return;

            float dmg = req.Amount;
            if (req.Type == DamageType.Physical && !req.IgnoresArmor)
                dmg *= (1f - ArmorPercent);

            Health.Sub(dmg);

            if (!IsDead && Health.IsZero)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Health.Add(amount);
        }

        public void AddEffect(IStatusEffect effect)
        {
            if (_effects.TryGetValue(effect.Id, out var existing))
            {
                existing.OnRemove(this);
                _effects.Remove(effect.Id);
            }

            _effects.Add(effect.Id, effect);
            effect.OnApply(this);
            OnEffectApplied?.Invoke(effect.Id);
        }

        public void RemoveEffect(string id)
        {
            if (_effects.TryGetValue(id, out var e))
            {
                e.OnRemove(this);
                _effects.Remove(id);
                OnEffectRemoved?.Invoke(id);
            }
        }

        public void ConsumeFood(float calories = 25f, float sanityBoost = 2f)
        {
            Hunger.Add(calories);
            Sanity.Add(sanityBoost);
        }

        public void Drink(float water = 30f)
        {
            Thirst.Add(water);
        }

        public void UseBandage()
        {
            RemoveEffect("bleeding");
            Heal(10f);
        }

        public void UseAntidote()
        {
            RemoveEffect("poison");
        }

        public void UseTonic()
        {
            AddEffect(new RegenerationEffect());
        }

        public void SetArmorPercent(float pct)
        {
            ArmorPercent = MathHelper.Clamp(pct, 0f, 0.9f);
        }
    }
}
