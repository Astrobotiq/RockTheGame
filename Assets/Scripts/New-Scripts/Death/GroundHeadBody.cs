using System;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asit dışı kafanın kök nesnesinde durur ve çarpışmaları havuza iletir (squash için).
    /// <see cref="GroundHeadPool"/> tarafından ekilir.
    /// </summary>
    public class GroundHeadBody : MonoBehaviour
    {
        public Action<Collision2D> Hit;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Hit?.Invoke(collision);
        }
    }
}
