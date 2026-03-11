using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InvestigationGame.Data;

namespace InvestigationGame.Core
{
    public static class SuspectSelector
    {
        public static List<SuspectData> PickSuspects(List<SuspectData> masterPool)
        {
            if (masterPool == null || masterPool.Count < 4)
            {
                Debug.LogWarning("Kolam utama tidak memiliki cukup tersangka (butuh minimal 4). Mengembalikan apa yang ada.");
                return masterPool?.ToList() ?? new List<SuspectData>();
            }

            var users = masterPool.Where(s => s.IsUser).ToList();
            var nonUsers = masterPool.Where(s => s.IsUser == false).ToList(); // Fixed a potential logic check while at it, but original was !s.IsUser

            if (users.Count < 1)
            {
                Debug.LogWarning("Tidak ada tersangka 'Pengguna' ditemukan di kolam utama! Memilih tersangka acak.");
                return masterPool.OrderBy(x => Random.value).Take(4).ToList();
            }

            if (nonUsers.Count < 3)
            {
                Debug.LogWarning("Tersangka 'Bukan Pengguna' tidak cukup! Memilih tersangka acak.");
                return masterPool.OrderBy(x => Random.value).Take(4).ToList();
            }

            var selectedUsers = users.OrderBy(x => Random.value).Take(1).ToList();
            var selectedNonUsers = nonUsers.OrderBy(x => Random.value).Take(3).ToList();

            var combined = new List<SuspectData>();
            combined.AddRange(selectedUsers);
            combined.AddRange(selectedNonUsers);

            // Shuffle
            return combined.OrderBy(x => Random.value).ToList();
        }
    }
}