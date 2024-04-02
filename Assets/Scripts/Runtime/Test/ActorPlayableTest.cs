using Game.Basic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ActorPlayableTest : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private string anim_config;
    private Animator animator;
    private AnimationPlayer animationPlayer;
    private void OnEnable() {
        animator = GetComponent<Animator>();
        animationPlayer = new AnimationPlayer();
        animationPlayer.Initialized(animator, anim_config);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animationPlayer.Tick();
        if(AppBootstrap.inputMgr != null) {
            Vector2 moveInput = AppBootstrap.inputMgr.GetAction("move").ReadValue<Vector2>();
            if(moveInput != Vector2.zero) {
                /*animator.SetBool("run", true);
                animator.SetFloat("x", moveInput.x);
                animator.SetFloat("y", moveInput.y);*/
                animationPlayer.Play("run");
                animationPlayer.SetValue("velocity", moveInput);
            } else {
                //animator.SetBool("run", false);
                animationPlayer.Play("idle");
            }
        }
        

    }
}
