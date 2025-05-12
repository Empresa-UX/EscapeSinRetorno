using System;
using System.Collections.Generic;

namespace ShadowSky.Source.Player
{
    public class PlayerStats
    {
        public float Health { get; private set; } = 100f;
        public float Temperature { get; private set; } = 36.5f;

        public float Hunger { get; private set; } = 100f;
        public float Thirst { get; private set; } = 100f;
        public float Energy { get; private set; } = 100f;
        public float Stamina { get; private set; } = 100f;
        public float Fatigue { get; private set; } = 0f;

        public float Sanity { get; private set; } = 100f;
        public float Mood { get; private set; } = 100f;

        public float Infection { get; private set; } = 0f;
        public float Poisoned { get; private set; } = 0f;
        public float Bleeding { get; private set; } = 0f;
        public float Pain { get; private set; } = 0f;
        public float Wetness { get; private set; } = 0f;

        public float StunTimer { get; set; } = 0f;

        public bool IsDead => Health <= 0;
        public bool CanMove => !IsUnconscious && Health > 0;
        public bool CanSprint => Stamina >= 20 && !IsInPain && !IsExhausted;
        public bool CanAct => !IsUnconscious && Mood >= 30 && !IsSleeping;
        public bool CanFight => Mood >= 30 && Pain < 60 && !IsSleeping;
        public bool IsSleeping { get; private set; } = false;

        public float SpeedMultiplier
        {
            get
            {
                float mult = 1f;
                if (Hunger < 30) mult *= 0.9f;
                if (Fatigue > 70) mult *= 0.85f;
                if (Pain > 50) mult *= 0.8f;
                return mult;
            }
        }

        public bool IsStarving { get; private set; }
        public bool IsDehydrated { get; private set; }
        public bool IsExhausted { get; private set; }
        public bool IsFatigued { get; private set; }
        public bool IsUnconscious { get; private set; }

        public bool IsHypothermic { get; private set; }
        public bool IsOverheated { get; private set; }

        public bool IsSick { get; private set; }
        public bool IsBleeding { get; private set; }
        public bool IsInPain { get; private set; }

        public bool IsDepressed { get; private set; }
        public bool IsDelirious { get; private set; }

        public bool VisionBlurred { get; private set; }
        public bool VisionShaky { get; private set; }
        public bool ControlsInverted { get; private set; }

        public void Update(float dt)
        {
            DecayStats(dt);
            ApplyEnvironmentalEffects(dt);
            ApplyMentalEffects(dt);
            ApplyHealthConsequences(dt);
            ClampAll();
            UpdateStatusEffects();
        }

        private void DecayStats(float dt)
        {
            Hunger -= 2f * dt;
            Thirst -= 3f * dt;
            Energy -= 1f * dt;
            Fatigue += 1f * dt;
            Stamina -= 2f * dt;
            Mood -= 0.5f * dt;
            Sanity -= (Hunger < 30 || Thirst < 30) ? 1.5f * dt : 0.2f * dt;
            Temperature = Math.Clamp(Temperature + 0.01f * dt, 35f, 39f);
            Infection += 0.1f * dt;
        }

        private void ApplyEnvironmentalEffects(float dt)
        {
            if (Wetness > 50 && Temperature < 36f)
                Temperature -= 0.5f * dt;
            if (Wetness > 70)
                Infection += 0.3f * dt;
            if (Poisoned > 50)
            {
                Hunger -= 1.5f * dt;
                Energy -= 1.5f * dt;
                Mood -= 2f * dt;
            }
        }

        private void ApplyMentalEffects(float dt)
        {
            VisionBlurred = Sanity < 15;
            VisionShaky = Thirst < 30;
            ControlsInverted = Sanity < 15;
        }

        private void ApplyHealthConsequences(float dt)
        {
            if (Hunger <= 0 || Thirst <= 0)
                Health -= 5f * dt;
            if (Fatigue >= 100)
                IsUnconscious = true;
            if (Temperature >= 39.5f)
                Health -= 2f * dt;
            if (Temperature <= 34f)
                Fatigue += 2f * dt;
            if (Infection >= 100)
                Health -= 10f * dt;
            if (Bleeding > 0)
                Health -= 4f * dt;
            if (Pain > 80)
                StunTimer = 1f;
        }

        private void ClampAll()
        {
            Health = Math.Clamp(Health, 0, 100);
            Hunger = Math.Clamp(Hunger, 0, 100);
            Thirst = Math.Clamp(Thirst, 0, 100);
            Energy = Math.Clamp(Energy, 0, 100);
            Stamina = Math.Clamp(Stamina, 0, 100);
            Fatigue = Math.Clamp(Fatigue, 0, 100);
            Sanity = Math.Clamp(Sanity, 0, 100);
            Mood = Math.Clamp(Mood, 0, 100);
            Infection = Math.Clamp(Infection, 0, 100);
            Temperature = Math.Clamp(Temperature, 32f, 42f);
            Wetness = Math.Clamp(Wetness, 0, 100);
        }

        private void UpdateStatusEffects()
        {
            IsStarving = Hunger <= 5;
            IsDehydrated = Thirst <= 5;
            IsExhausted = Energy <= 5;
            IsFatigued = Fatigue >= 90;

            IsHypothermic = Temperature <= 34.0f;
            IsOverheated = Temperature >= 39.5f;

            IsSick = Infection >= 60 || Poisoned >= 50;
            IsBleeding = Bleeding > 0;
            IsInPain = Pain >= 60;

            IsDepressed = Mood <= 30;
            IsDelirious = Sanity <= 20;
        }

        public void SpendStamina(float amount) => Stamina = Math.Clamp(Stamina - amount, 0, 100);

        // Setters públicos para testing manual
        public void SetFatigue(float value) => Fatigue = Math.Clamp(value, 0, 100);
        public void SetThirst(float value) => Thirst = Math.Clamp(value, 0, 100);
        public void SetSanity(float value) => Sanity = Math.Clamp(value, 0, 100);
        public void SetMood(float value) => Mood = Math.Clamp(value, 0, 100);
        public void SetWetness(float value) => Wetness = Math.Clamp(value, 0, 100);

        public void Eat(float amount) => Hunger = Math.Clamp(Hunger + amount, 0, 100);
        public void Drink(float amount) => Thirst = Math.Clamp(Thirst + amount, 0, 100);

        public void Rest(float amount)
        {
            Energy = Math.Clamp(Energy + amount, 0, 100);
            Fatigue = Math.Clamp(Fatigue - amount * 0.5f, 0, 100);
            Stamina = Math.Clamp(Stamina + amount * 0.5f, 0, 100);
        }

        public void Heal(float amount) => Health = Math.Clamp(Health + amount, 0, 100);

        public void CheerUp(float amount)
        {
            Mood = Math.Clamp(Mood + amount, 0, 100);
            Sanity = Math.Clamp(Sanity + amount * 0.5f, 0, 100);
        }

        public void HealInfection(float amount) => Infection = Math.Clamp(Infection - amount, 0, 100);
        public void AdjustTemperature(float delta) => Temperature = Math.Clamp(Temperature + delta, 32f, 42f);

        public List<string> GetStatusMessages()
        {
            var messages = new List<string>();
            if (Hunger > 90) messages.Add("Estás lleno");
            else if (Hunger > 70) messages.Add("Te sientes satisfecho");
            else if (Hunger > 50) messages.Add("Tienes un poco de hambre");
            else if (Hunger > 30) messages.Add("Tienes hambre");
            else if (Hunger > 10) messages.Add("Tienes mucha hambre");
            else messages.Add("Alerta: hambre extrema");

            if (Thirst > 90) messages.Add("Estás bien hidratado");
            else if (Thirst > 70) messages.Add("No tienes sed");
            else if (Thirst > 50) messages.Add("Tienes un poco de sed");
            else if (Thirst > 30) messages.Add("Tienes sed");
            else if (Thirst > 10) messages.Add("Tienes mucha sed");
            else messages.Add("Alerta: deshidratación");

            if (Energy < 5) messages.Add("Estás exhausto");
            else if (Energy < 30) messages.Add("Estás muy cansado");
            else if (Energy < 60) messages.Add("Estás algo cansado");

            if (Fatigue > 90) messages.Add("Estás a punto de desmayarte");
            else if (Fatigue > 70) messages.Add("Estás muy fatigado");
            else if (Fatigue > 40) messages.Add("Estás algo fatigado");

            if (Sanity < 20) messages.Add("Estás delirando");
            else if (Sanity < 50) messages.Add("Sientes confusión mental");

            if (Mood < 30) messages.Add("Estás deprimido");

            if (Temperature <= 34) messages.Add("Estás con hipotermia");
            if (Temperature >= 39.5f) messages.Add("Estás con fiebre");

            if (IsSick) messages.Add("Te sientes enfermo");
            if (IsInPain) messages.Add("Sientes dolor");

            return messages;
        }
    }
}
