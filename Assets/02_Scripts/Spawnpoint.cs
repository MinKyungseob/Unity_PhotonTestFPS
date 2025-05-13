using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
   [SerializeField] private GameObject grphics;

   private void Awake()
   {
      grphics.SetActive(false);
   }
}
