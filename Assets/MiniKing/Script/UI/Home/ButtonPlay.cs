using System.Collections;
using System.Collections.Generic;
using MiniKing.Script.Constant;
using MiniKing.Script.Procedure.Scene;
using Spring.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void LoadGame() {
        SpringContext.GetBean<ProcedureChangeScene>().ChangeScene(SceneEnum.Level1);
    }
}
