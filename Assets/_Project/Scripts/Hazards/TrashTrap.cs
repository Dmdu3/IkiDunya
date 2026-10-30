using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Takan çöp (Trash layer, Is Trigger collider). Kaplumbağa tarafı türüne bakıp ne olacağına karar verir.</summary>
    [RequireComponent(typeof(Collider))]
    public class TrashTrap : MonoBehaviour
    {
        public enum Turu { CopYigini, Ag, Halka, TekerlekIzi }

        [SerializeField] private Turu tur = Turu.CopYigini;

        public Turu Tur => tur;
    }
}
