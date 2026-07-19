using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Equipo de máx. 3: "conectar, no coleccionar". Liberar una para reclutar otra.
    public class TeamManager : MonoBehaviour
    {
        public const int MaxTeam = 3;
        public float swapCooldown = 2f;

        [SerializeField] List<CreatureInstance> _team = new List<CreatureInstance>();

        public IReadOnlyList<CreatureInstance> Team => _team;
        public int ActiveIndex { get; private set; }
        public CreatureInstance Active => _team.Count > 0 ? _team[ActiveIndex] : null;

        float _nextSwapAt;
        public bool CanSwap => _team.Count > 1 && Time.time >= _nextSwapAt;

        public void SwapNext()
        {
            if (!CanSwap) return;
            ActiveIndex = (ActiveIndex + 1) % _team.Count;
            _nextSwapAt = Time.time + swapCooldown;
            // TODO(Fase 4): intercambiar el modelo/controller de la criatura activa.
        }

        public bool TryAdd(CreatureInstance c)
        {
            if (c == null || _team.Count >= MaxTeam) return false; // liberar para reclutar
            _team.Add(c);
            return true;
        }

        public bool Release(int index)
        {
            if (index < 0 || index >= _team.Count) return false;
            _team.RemoveAt(index);
            if (ActiveIndex >= _team.Count) ActiveIndex = Mathf.Max(0, _team.Count - 1);
            return true;
        }
    }
}
