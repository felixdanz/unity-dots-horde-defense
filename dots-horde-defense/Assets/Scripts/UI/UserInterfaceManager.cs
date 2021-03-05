using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    public static UserInterfaceManager Instance { get; private set; }
    
    [SerializeField] private BuildMenu buildMenu;
    [SerializeField] private DebugConsole debugConsole;
    
    
    private void Awake()
    {
        #region singleton

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
		
        Instance = this;

        #endregion
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            buildMenu.gameObject.SetActive(!buildMenu.gameObject.activeSelf);
        
        if (Input.GetKeyDown(KeyCode.Hash))
            debugConsole.gameObject.SetActive(!debugConsole.gameObject.activeSelf);
    }
}
