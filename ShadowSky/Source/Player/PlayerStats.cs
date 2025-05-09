using System;

namespace ShadowSky.Source.Player
{
    public class PlayerStats
    {
        // 🩺 Vitales
        public float Health { get; private set; } = 100f;
        public float Temperature { get; private set; } = 36.5f; // temperatura corporal normal

        // 🍔 Necesidades básicas
        public float Hunger { get; private set; } = 100f;
        public float Thirst { get; private set; } = 100f;
        public float Energy { get; private set; } = 100f;

        // 🏃 Estado físico
        public float Stamina { get; private set; } = 100f;
        public float Fatigue { get; private set; } = 0f;

        // 🧠 Estado mental
        public float Sanity { get; private set; } = 100f;
        public float Mood { get; private set; } = 100f;

        // 🦠 Estado de salud
        public float Infection { get; private set; } = 0f;

        public bool IsDead => Health <= 0;

        public void Update(float deltaTime)
        {
            // Degeneración progresiva
            Hunger -= 2f * deltaTime;
            Thirst -= 3f * deltaTime;
            Energy -= 1f * deltaTime;
            Fatigue += 1f * deltaTime;
            Stamina -= 2f * deltaTime;

            Mood -= 0.5f * deltaTime;
            Sanity -= (Hunger < 30 || Thirst < 30) ? 1.5f * deltaTime : 0.2f * deltaTime;

            Temperature = Math.Clamp(Temperature + 0.01f * deltaTime, 35f, 39f);
            Infection += 0.1f * deltaTime;

            // Penalizaciones por extremos
            if (Hunger <= 0 || Thirst <= 0)
                Health -= 5f * deltaTime;

            if (Fatigue > 80 || Stamina < 20)
                Health -= 2f * deltaTime;

            if (Infection >= 100)
                Health -= 10f * deltaTime;

            // Clamps
            ClampAll();
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
        }

        // Acciones
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
    }
}
